using System.Text;

namespace WinFormsSerial
{
    public partial class Form1 : Form
    {
        private readonly SerialPortManager _serialPort;
        private readonly FaultAlarmCommandProcessor _faultAlarmCommandProcessor;
        private readonly ZoneNameEditor _zoneNameEditor;
        private bool _isCommunicating;
        private const string COMMUNICATE_TEXT = "Communicate 1Hz";
        private const string STOP_TEXT = "Stop";

        public Form1()
        {
            InitializeComponent();

            _serialPort = new SerialPortManager(LogMessage);
            _faultAlarmCommandProcessor = new FaultAlarmCommandProcessor(LogMessage);
            _zoneNameEditor = new ZoneNameEditor(listBoxZoneNames);

            Controls.Add(_zoneNameEditor.EditBoxControl);
            InitializeUI();
            PopulateCOMPorts();
            // Add build date/time to form title
            Text = $"Serial Connection Demo - Built: {GetBuildDateTime():yyyy-MM-dd HH:mm:ss}";
        }

        private static DateTime GetBuildDateTime()
        {
            string exePath = Application.ExecutablePath;
            FileInfo fileInfo = new FileInfo(exePath);
            return fileInfo.LastWriteTime;
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
            if (InvokeRequired)
            {
                BeginInvoke(() => LogMessage(message));
                return;
            }
            textBoxLog.AppendText(message + Environment.NewLine);
            textBoxLog.ScrollToCaret();
        }

        private void PopulateCOMPorts()
        {
            comboBoxCOMPorts.Items.Clear();
            string[] ports = _serialPort.GetAvailablePortNames();

            comboBoxCOMPorts.Items.AddRange(ports);
            btnConnect.Enabled = ports.Length > 0;

            if (ports.Length > 0)
                comboBoxCOMPorts.SelectedIndex = 0;
            else
                comboBoxCOMPorts.Text = "No COM ports!";
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;

            try
            {
                if (_serialPort.IsConnected)
                {
                    _serialPort.Disconnect();
                    UpdateUIForDisconnected();
                    return;
                }

                if (comboBoxCOMPorts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port");
                    return;
                }

                await _serialPort.ConnectAsync(comboBoxCOMPorts.SelectedItem.ToString()!);
                UpdateUIForConnected();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}");
                UpdateUIForDisconnected();
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private void UpdateUIForConnected()
        {
            btnConnect.Text = "Disconnect";
            comboBoxCOMPorts.Enabled = false;
            buttonCommunicate1Hz.Enabled = true;
            buttonGetZoneNames.Enabled = true;
        }

        private void UpdateUIForDisconnected()
        {
            btnConnect.Text = "Connect";
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
                    btnConnect.Enabled = false;
                    buttonCommunicate1Hz.Text = STOP_TEXT;

                    try
                    {
                        while (!_communicationCts.Token.IsCancellationRequested && _serialPort.IsConnected)
                        {
                            foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
                            {
                                if (_communicationCts.Token.IsCancellationRequested) break;

                                try
                                {
                                    var (response, bytesRead) = _serialPort.SendCommand(new[] { command });
                                    if (bytesRead > 0)
                                    {
                                        _faultAlarmCommandProcessor.ProcessResponse(command, response);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"Command {command} execution error: {ex.Message}");
                                    //return;
                                }

                                try
                                {
                                    await Task.Delay(1000, _communicationCts.Token); // 1Hz frequency
                                }
                                catch (OperationCanceledException)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _communicationCts.Dispose();
                        _communicationCts = null;
                    }
                }
                else
                {
                    _communicationCts?.Cancel();
                    LogMessage("Stop");
                }
            }
            finally
            {
                btnConnect.Enabled = true;
                buttonCommunicate1Hz.Text = COMMUNICATE_TEXT;
                _isCommunicating = false;
            }
        }

        private void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("Get zone names");
                var expectedLength = Constants.NB_OF_ZONES * Constants.ZONE_NAME_LENGTH;
                var (response, bytesRead) = _serialPort.SendCommand(
                    new[] { Constants.GET_ZONE_NAMES_COMMAND },
                    expectedLength
                );

                if (bytesRead == expectedLength)
                {
                    ParseAndDisplayZoneNames(response);
                    buttonUpdateZoneNames.Enabled = true;
                    LogMessage("Zone names obtained successfully.");
                }
                else
                {
                    LogMessage($"Received unexpected number of bytes: {bytesRead}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error for getting zone names command {Constants.GET_ZONE_NAMES_COMMAND}: {ex.Message}");
                buttonUpdateZoneNames.Enabled = false;
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

                byte[] command = BuildZoneNamesUpdateCommand();
                var (response, _) = _serialPort.SendCommand(command);
                var responseFirstByte = response[0];
                const byte expectedResponse = 245;
                if (responseFirstByte == expectedResponse)
                {
                    LogMessage($"Zone names update command sent successfully. Response: {responseFirstByte}");
                }
                else
                {
                    LogMessage($"Error: Zone names update expectedResponse was {expectedResponse}, got {responseFirstByte}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error for updating zone names command {Constants.SET_ZONE_NAMES_COMMAND}: {ex.Message}");
            }
        }

        private byte[] BuildZoneNamesUpdateCommand()
        {
            const int commandSize = 1 + Constants.NB_OF_ZONES * (1 + Constants.ZONE_NAME_LENGTH) + 1;
            byte[] command = new byte[commandSize];
            command[0] = Constants.SET_ZONE_NAMES_COMMAND;

            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                string zoneName = (listBoxZoneNames.Items[i]?.ToString() ?? "").PadRight(Constants.ZONE_NAME_LENGTH);
                byte[] nameBytes = Encoding.ASCII.GetBytes(zoneName);

                int nameStart = 2 + i * (1 + Constants.ZONE_NAME_LENGTH);
                command[nameStart - 1] = (byte)(200 + i + 1);
                Array.Copy(nameBytes, 0, command, nameStart, Constants.ZONE_NAME_LENGTH);
            }

            command[commandSize - 1] = 27; // End marker
            return command;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _communicationCts?.Cancel();
            _communicationCts?.Dispose();
            _isCommunicating = false;
            _serialPort.Dispose();
            base.OnFormClosing(e);
        }
    }
}