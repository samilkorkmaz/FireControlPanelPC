using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsSerial
{
    public partial class Form1 : Form
    {
        private SerialPort? serialPort;
        private CancellationTokenSource? cts;
        private string stopText = "Stop";
        private string communicateText = "Communicate";
        private const string isThereFireAlarm = "#";
        private const string isThereZoneLineFault = "$";
        private const string isThereZControlPanelFault = "%";
        private const string resetControlPanel = "&";
        private const string silenceBuzzer = "'";
        private string[] periodicCommandsOrder = { isThereFireAlarm, isThereZoneLineFault, isThereFireAlarm, isThereZControlPanelFault };

        public Form1()
        {
            InitializeComponent();
            PopulateCOMPorts();
        }

        private void PopulateCOMPorts()
        {
            comboBoxCOMPorts.Items.Clear();
            string[] availableSerialPorts = SerialPort.GetPortNames();
            foreach (string port in availableSerialPorts)
            {
                comboBoxCOMPorts.Items.Add(port);
            }
            if (comboBoxCOMPorts.Items.Count > 0)
            {
                comboBoxCOMPorts.SelectedIndex = 0;
                btnConnect.Enabled = true;
            }
            else
            {
                comboBoxCOMPorts.Text = "No COM ports!";
                btnConnect.Enabled = false;
            }
        }

        private void appendToLog(string message)
        {
            textBoxLog.AppendText(message + Environment.NewLine);
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            buttonCommunicate.Enabled = false;
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    btnConnect.Text = "Connect";
                    comboBoxCOMPorts.Enabled = true;
                    buttonCommunicate.Enabled = false;
                    buttonCommunicate.Text = communicateText;
                    appendToLog("Disconnected");
                    return;
                }

                if (comboBoxCOMPorts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port");
                    return;
                }

                serialPort = new SerialPort
                {
                    PortName = comboBoxCOMPorts.SelectedItem.ToString(),
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                // Create a cancellation token with timeout
                cts = new CancellationTokenSource();
                double connectionTimout_s = 3;
                cts.CancelAfter(TimeSpan.FromSeconds(connectionTimout_s));

                // Attempt to open the port with timeout
                await Task.Run(() =>
                {
                    try
                    {
                        serialPort.Open();
                    }
                    catch (Exception) when (cts.Token.IsCancellationRequested)
                    {
                        throw new TimeoutException("Connection attempt timed out after " + connectionTimout_s + " seconds");
                    }
                }, cts.Token);

                btnConnect.Text = "Disconnect";
                comboBoxCOMPorts.Enabled = false;
                buttonCommunicate.Enabled = true;
                appendToLog("Connected");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                btnConnect.Enabled = true;
                comboBoxCOMPorts.Enabled = true;
                appendToLog("Connection failed");

                // Clean up if connection failed
                if (serialPort != null)
                {
                    serialPort.Dispose();
                    serialPort = null;
                }
            }
            finally
            {
                // Clean up the cancellation token source
                if (cts != null)
                {
                    cts.Dispose();
                    cts = null;
                }
                btnConnect.Enabled = true;
            }
        }

        private void SendCommandAndProcessResponse(string command)
        {
            if (serialPort == null)
                throw new InvalidOperationException("Serial port is not initialized");

            if (!serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not open");

            try
            {
                // Send command
                serialPort.Write(command);
                appendToLog($"Command '{command}' sent.");

                // Wait for response
                Thread.Sleep(500);

                byte[] buffer = new byte[1];
                int bytesRead = serialPort.Read(buffer, 0, 1);

                if (bytesRead > 0)
                {
                    byte responseByte = buffer[0];
                    ProcessResponse(command, responseByte);
                }
                else
                {
                    appendToLog("No response from the control panel.");
                }
            }            
            catch (Exception ex)
            {
                appendToLog("Error while communicating! command: " + command + " Exception: " + ex.Message);
            }
        }

        private void ProcessResponse(string command, byte response)
        {
            switch (command)
            {
                case isThereFireAlarm:
                    if (response == 0b00000000) appendToLog("No fire alarm.");
                    else appendToLog($"Fire alarm at zone: {response}");
                    break;
                case isThereZoneLineFault:
                    if (response == 0b00000000) appendToLog("No zone line fault.");
                    else appendToLog($"Zone Line Fault at zone: {response}");
                    break;
                case isThereZControlPanelFault:
                    appendToLog("control panel fault conditions:");
                    if ((response & 0b00000001) != 0) appendToLog("Batarya yok");
                    if ((response & 0b00000010) != 0) appendToLog("Batarya zayıf");
                    if ((response & 0b00000100) != 0) appendToLog("Şebeke yok");
                    if ((response & 0b00001000) != 0) appendToLog("Şarj zayıf");
                    if ((response & 0b00010000) != 0) appendToLog("Siren Arıza");
                    if ((response & 0b00100000) != 0) appendToLog("Çıkış arıza");
                    if ((response & 0b01000000) != 0) appendToLog("Toprak arıza");
                    if (response == 0b00000000) appendToLog("No control panel fault.");
                    break;
                case resetControlPanel:
                    appendToLog("TODO: Control panel reset command acknowledged.");
                    break;
                case silenceBuzzer:
                    appendToLog("TODO: Buzzer silence command acknowledged.");
                    break;
                default:
                    appendToLog("ERROR: Unknown command: " + command);
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }
            cts?.Dispose();
            base.OnFormClosing(e);
        }

        private void buttonCommunicate_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonCommunicate.Text == communicateText)
                {
                    btnConnect.Enabled = false;
                    buttonCommunicate.Text = stopText;
                    foreach (string command in periodicCommandsOrder)
                    {
                        SendCommandAndProcessResponse(command);
                    }
                }
                else
                {
                    btnConnect.Enabled = true;
                    buttonCommunicate.Text = communicateText;
                }
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }
    }
}