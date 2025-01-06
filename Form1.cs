using System;
using System.IO.Ports;
using System.Reflection;
using System.Text;

namespace WinFormsSerial
{
    public partial class Form1 : Form
    {
        private readonly SerialPortManager _serialPortManager;
        private readonly FaultAlarmCommandProcessor _faultAlarmCommandProcessor;
        private readonly ZoneNameEditor _zoneNameEditor;
        private bool _isCommunicating;
        private const string COMMUNICATE_TEXT = "Communicate 1Hz";
        private const string STOP_TEXT = "Stop";
        private FireControlPanelEmulator _emulator;
        private SerialPortEnumerator? _portEnumerator;

        public Form1()
        {
            InitializeComponent();
            InitializePortEnumerator();

            _emulator = new FireControlPanelEmulator(LogMessage);
            _serialPortManager = new SerialPortManager(LogMessage);
            _faultAlarmCommandProcessor = new FaultAlarmCommandProcessor(LogMessage);
            _zoneNameEditor = new ZoneNameEditor(listBoxZoneNames);

            Controls.Add(_zoneNameEditor.EditBoxControl);
            InitializeUI();
            // Add build date/time to form title
            var buildDate = System.Reflection.Assembly.GetExecutingAssembly()
            .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
            .FirstOrDefault(attr => attr.Key == "BuildDate")?.Value;

            this.Text = $"My Application - Built on {buildDate}";
        }

        private void InitializePortEnumerator()
        {
            _portEnumerator = new SerialPortEnumerator(LogMessage);
            _portEnumerator.PortsChanged += PortEnumerator_PortsChanged;

            // Get initial port list
            UpdatePortList(_portEnumerator.GetAvailablePorts());
        }

        private void PortEnumerator_PortsChanged(object sender, SerialPortEnumerator.SerialPortsChangedEventArgs e)
        {
            // Since this event comes from a different thread, we need to use Invoke
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePortList(e.Ports)));
                return;
            }
            UpdatePortList(e.Ports);
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
            if (InvokeRequired)
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
                        while (!_communicationCts.Token.IsCancellationRequested && _serialPortManager.IsConnected)
                        {
                            foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
                            {
                                if (_communicationCts.Token.IsCancellationRequested) break;

                                try
                                {
                                    var (response, bytesRead) = _serialPortManager.SendCommand(new[] { command });
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
                buttonConnect.Enabled = true;
                buttonCommunicate1Hz.Text = COMMUNICATE_TEXT;
                _isCommunicating = false;
            }
        }

        private async void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            LogMessage("Get zone names");
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
                    var expectedBytesLength = Constants.NB_OF_ZONES * Constants.ZONE_NAME_LENGTH;
                    var (responseBytes, readBytesLength) = _serialPortManager.SendCommand(
                        new[] { Constants.GET_ZONE_NAMES_COMMAND },
                        expectedBytesLength
                    );

                    if (readBytesLength == expectedBytesLength)
                    {
                        this.Invoke(() =>
                        {
                            ParseAndDisplayZoneNames(responseBytes);
                            LogMessage("Zone names obtained successfully.");
                            buttonUpdateZoneNames.Enabled = true;
                        });
                    }
                    else
                    {
                        LogMessage($"Expected number of bytes {expectedBytesLength} is different from received {readBytesLength}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogMessage($"Error for getting zone names command {Constants.GET_ZONE_NAMES_COMMAND}: {ex.Message}");
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

                byte[] command = BuildZoneNamesUpdateCommand();
                var (response, _) = _serialPortManager.SendCommand(command);
                var responseFirstByte = response[0];
                const byte expectedResponse = 245;
                if (responseFirstByte == expectedResponse)
                {
                    LogMessage($"Zone names update command sent successfully. Response: {responseFirstByte}");
                    _zoneNameEditor.ClearEditHistory(); // Clear edit highlight from edited lines
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
            _serialPortManager.Dispose();
            _portEnumerator?.Dispose();
            base.OnFormClosing(e);
        }

        private void buttonDetectPort_Click(object sender, EventArgs e)
        {
            string[] availablePorts = SerialPort.GetPortNames();
            LogMessage($"Checking if fire control panel is connected by sending command {Constants.IS_THERE_FIRE_ALARM} to {availablePorts.Length} availabale ports...");
            buttonDetectPort.Enabled = false;
            buttonConnect.Enabled = false;
            string detectedPort = "";

            foreach (string port in availablePorts)
            {
                LogMessage($"Checking {port}...");

                try
                {
                    using (SerialPort testPort = new SerialPort(port))
                    {
                        testPort.BaudRate = 9600;    // Match your device settings
                        testPort.DataBits = 8;
                        testPort.Parity = Parity.None;
                        testPort.StopBits = StopBits.One;
                        testPort.ReadTimeout = 300;   // Short timeout for quick scanning
                        testPort.WriteTimeout = 300;

                        testPort.Open();

                        // Clear any existing data
                        testPort.DiscardInBuffer();
                        testPort.DiscardOutBuffer();

                        // Send the "are you there?" command
                        testPort.Write(new byte[] { Constants.IS_THERE_FIRE_ALARM }, 0, 1);

                        // Read response
                        byte[] response = new byte[1];
                        int bytesRead = testPort.Read(response, 0, 1);
                        testPort.Close();

                        if (bytesRead == 1)
                        {
                            detectedPort = port;
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    // Port is either in use or not the correct one
                    continue;
                }
            }
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