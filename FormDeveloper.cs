using System.Text;

namespace WinFormsSerial
{
    public partial class FormDeveloper : Form
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly ZoneNameEditor _zoneNameEditor;
        private FireControlPanelEmulator _emulator;
        private SerialPortEnumerator _portEnumerator;

        private bool _isCommunicating;
        private const string COMMUNICATE_TEXT = "Communicate 1Hz";
        private const string STOP_TEXT = "Stop";

        public FormDeveloper()
        {
            InitializeComponent();
            SetTitle();
            _serialPortManager = new SerialPortManager(LogMessage);
            _zoneNameEditor = new ZoneNameEditor(listBoxZoneNames);
            _emulator = new FireControlPanelEmulator(LogMessage);
            _portEnumerator = new SerialPortEnumerator(LogMessage, SafeUpdatePortList);            
            SafeUpdatePortList(_portEnumerator.GetAvailablePorts());
            try
            {
                var (osVersion, windowsVersion) = Utils.CheckWindowsVersion();
                LogMessage($"OS: {windowsVersion}, " +
                   $"Build Number: {osVersion.Build}, " +
                   $"Revision: {osVersion.Revision}");
                
                Controls.Add(_zoneNameEditor.EditBoxControl);
                InitializeUI();
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                Utils.DisableAllGUIControls(Controls);
            }            
        }

        private void SetTitle()
        {
            Text = $"Fire Control Panel Dev Mode - Built on {Utils.GetBuildDate()}";
        }               

        private void SafeUpdatePortList(string[] ports)
        {
            if (InvokeRequired) // We're on a different thread - marshal the call to the UI thread
            {
                Invoke(new Action(() => UpdatePortList(ports)));
                return;
            }
            UpdatePortList(ports);
        }

        private void UpdatePortList(string[] ports)
        {
            comboBoxCOMPorts.Items.Clear();

            if (ports.Length == 0)
            {
                comboBoxCOMPorts.Items.Add("No COM ports available");
                comboBoxCOMPorts.Enabled = false;
                buttonConnect.Enabled = false;
            }
            else
            {
                comboBoxCOMPorts.Items.AddRange(ports);
                comboBoxCOMPorts.Enabled = true;
                buttonConnect.Enabled = true;
                comboBoxCOMPorts.SelectedIndex = 0; // Select first port
            }

            // You might want to update status label or other UI elements
            string message = ports.Length == 0 ?
                "No COM ports detected" :
                $"Found {ports.Length} COM port(s)";
            LogMessage(message);
        }

        private void InitializeUI()
        {
            buttonCommunicate1Hz.Text = COMMUNICATE_TEXT;
            buttonUpdateZoneNames.Enabled = false;
            buttonGetZoneNames.Enabled = false;
            buttonCommunicate1Hz.Enabled = false;
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired) // We're on a different thread - marshal the call to the UI thread
            {
                BeginInvoke(() => LogMessage(message));
                return;
            }
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.ScrollToCaret();
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = false;
            try
            {
                if (_serialPortManager.IsConnected)
                {
                    _serialPortManager.Disconnect();
                    UpdateUIForDisconnected();
                    return;
                }

                if (comboBoxCOMPorts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port");
                    return;
                }

                await _serialPortManager.ConnectAsync(comboBoxCOMPorts.SelectedItem.ToString()!);
                UpdateUIForConnected();
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                UpdateUIForDisconnected();
            }
            finally
            {
                buttonConnect.Enabled = true;
            }
        }

        private void UpdateUIForConnected()
        {
            buttonConnect.Text = "Disconnect";
            comboBoxCOMPorts.Enabled = false;
            buttonCommunicate1Hz.Enabled = true;
            buttonGetZoneNames.Enabled = true;
        }

        private void UpdateUIForDisconnected()
        {
            buttonConnect.Text = "Connect";
            comboBoxCOMPorts.Enabled = true;
            buttonCommunicate1Hz.Enabled = false;
            buttonGetZoneNames.Enabled = false;
            buttonUpdateZoneNames.Enabled = false;
            _isCommunicating = false;
            buttonCommunicate1Hz.Text = COMMUNICATE_TEXT;
        }

        private CancellationTokenSource? _communicationCts;

        private async void buttonCommunicate1Hz_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isCommunicating)
                {
                    LogMessage("Start communicating at 1Hz...");
                    _isCommunicating = true;
                    _communicationCts = new CancellationTokenSource();
                    buttonConnect.Enabled = false;
                    buttonCommunicate1Hz.Text = STOP_TEXT;

                    try
                    {
                        if (_serialPortManager != null)
                        {
                            while (!_communicationCts.Token.IsCancellationRequested && _serialPortManager.IsConnected)
                            {
                                await _serialPortManager.ProcessPeriodicCommandsAsync(_communicationCts);
                            }
                        }
                        else
                        {
                            LogMessage("ERROR: _serialPortManager == null");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage(ex.Message);
                    }
                    finally
                    {
                        _communicationCts.Dispose();
                        _communicationCts = null;
                        LogMessage("Stopped");
                    }
                }
                else
                {
                    _communicationCts?.Cancel();
                    LogMessage("User requested stop...");
                }
            }
            finally
            {
                buttonConnect.Enabled = true;
                buttonCommunicate1Hz.Text = COMMUNICATE_TEXT;
                _isCommunicating = false;
            }
        }

        private async void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            LogMessage("Get zone names...");
            /*for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                listBoxZoneNames.Items.Add("zone " + i);
                buttonUpdateZoneNames.Enabled = true;
            }*/
            buttonGetZoneNames.Enabled = false;
            try
            {
                await Task.Run(() => // Use async/await pattern to prevent button clicks in rapid succession
                {
                    var responseBytes = _serialPortManager.GetZoneNames();
                    Invoke(() =>
                    {
                        ParseAndDisplayZoneNames(responseBytes);
                        LogMessage("Zone names obtained successfully.");
                        buttonUpdateZoneNames.Enabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                buttonUpdateZoneNames.Enabled = false;
            }
            finally
            {
                buttonGetZoneNames.Enabled = true;
            }
        }

        private void ParseAndDisplayZoneNames(byte[] buffer)
        {
            listBoxZoneNames.Items.Clear();

            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                int startIndex = i * Constants.ZONE_NAME_LENGTH;
                byte[] zoneNameBytes = new byte[Constants.ZONE_NAME_LENGTH];
                Array.Copy(buffer, startIndex, zoneNameBytes, 0, Constants.ZONE_NAME_LENGTH);

                string zoneName = Encoding.ASCII.GetString(zoneNameBytes).TrimEnd();
                listBoxZoneNames.Items.Add(zoneName);
            }
        }

        private void buttonUpdateZoneNames_Click(object sender, EventArgs e)
        {
            LogMessage("Update zone names");
            try
            {
                if (listBoxZoneNames.Items.Count == 0)
                {
                    LogMessage("No zone names to update. Please get zone names first.");
                    return;
                }
                var responseFirstByte = _serialPortManager.UpdateZoneNames(listBoxZoneNames.Items.Cast<string>().ToArray());
                LogMessage($"Zone names update command sent successfully. Response: {responseFirstByte}");
                _zoneNameEditor.ClearEditHistory(); // Clear edit highlight from edited lines
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _communicationCts?.Cancel();
            _communicationCts?.Dispose();
            _isCommunicating = false;
            _serialPortManager?.Dispose();
            _portEnumerator?.Dispose();
            base.OnFormClosing(e);
        }

        private void buttonDetectPort_Click(object sender, EventArgs e)
        {
            buttonDetectPort.Enabled = false;
            buttonConnect.Enabled = false;
            var detectedPort = _serialPortManager.DetectFirePanelPort();
            if (string.IsNullOrEmpty(detectedPort))
            {
                LogMessage($"No fire control panel detected!");
            }
            else
            {
                LogMessage($"Fire control panel at {detectedPort}");
                int index = comboBoxCOMPorts.FindString(detectedPort);
                if (index != -1)
                {
                    comboBoxCOMPorts.SelectedIndex = index;
                }
            }
            buttonDetectPort.Enabled = true;
            buttonConnect.Enabled = true;

            /*int i = comboBoxCOMPorts.FindString("COM4");
            if (i != -1)
            {
                comboBoxCOMPorts.SelectedIndex = i;
            }*/
        }

        private async void checkBoxEmulate_CheckedChanged(object sender, EventArgs e)
        {            
            checkBoxEmulate.Enabled = false;  // Prevent rapid toggling
            try
            {
                if (checkBoxEmulate.Checked == true)
                {
                    LogMessage("Run emulator...");
                    _emulator.Run();
                }
                else
                {
                    await _emulator.StopAsync();
                }
            }
            finally
            {
                checkBoxEmulate.Enabled = true;
            }
        }
    }
}