using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsSerial
{
    public partial class Form1 : Form
    {
        private SerialPort? serialPort;
        private CancellationTokenSource? cts;
        private string stopText = "Stop";
        private string communicateText = "Communicate 1Hz";
        private const byte isThereFireAlarm = 35;
        private const byte isThereZoneLineFault = 36;
        private const byte isThereControlPanelFault = 37;
        private const byte resetControlPanel = 38;
        private const byte silenceBuzzer = 39;
        private const byte GET_ZONE_NAMES_COMMAND = 14;
        private const byte SET_ZONE_NAMES_COMMAND = 13;
        private const int NB_OF_ZONES = 8;
        private const int ZONE_NAME_LENGTH = 16;
        private byte[] periodicCommandsOrder = { isThereFireAlarm, isThereZoneLineFault, isThereFireAlarm, isThereControlPanelFault };
        private TextBox editBoxZoneName = new TextBox();

        public Form1()
        {
            InitializeComponent();
            PopulateCOMPorts();
            ConfigureListBoxZoneNames();
        }

        private void ConfigureListBoxZoneNames()
        {
            editBoxZoneName.Visible = false;
            editBoxZoneName.LostFocus += EditBoxZoneName_LostFocus;
            editBoxZoneName.KeyPress += EditBoxZoneName_KeyPress;
            this.Controls.Add(editBoxZoneName);
            listBoxZoneNames.MouseDoubleClick += ListBoxZoneNames_MouseDoubleClick;
        }

        private void EditBoxZoneName_LostFocus(object? sender, EventArgs e)
        {
            CommitEdit();
        }

        private void EditBoxZoneName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (editBoxZoneName.Text.Length <= 16)
                {
                    CommitEdit();
                }
                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                editBoxZoneName.Visible = false;
                e.Handled = true;
            }
            else if (editBoxZoneName.Text.Length >= ZONE_NAME_LENGTH && e.KeyChar != (char)Keys.Back)
            {
                // Reject input if already at 16 chars (but allow backspace)
                e.Handled = true;
            }
        }

        private void CommitEdit()
        {
            if (editBoxZoneName.Visible && editBoxZoneName.Text.Length <= 16)
            {
                int index = (int)editBoxZoneName.Tag;
                listBoxZoneNames.Items[index] = editBoxZoneName.Text;
                editBoxZoneName.Visible = false;
            }
        }

        private void ListBoxZoneNames_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            int iClicked = listBoxZoneNames.IndexFromPoint(e.Location);
            if (iClicked != ListBox.NoMatches)
            {
                // Position the TextBox at the clicked item
                Rectangle rect = listBoxZoneNames.GetItemRectangle(iClicked);
                editBoxZoneName.Bounds = new Rectangle(
                    listBoxZoneNames.Left + rect.Left,
                    listBoxZoneNames.Top + rect.Top,
                    rect.Width,
                    rect.Height);

                // Set the TextBox text and show it
                editBoxZoneName.Text = listBoxZoneNames.Items[iClicked].ToString();
                editBoxZoneName.Tag = iClicked;  // Store the index for later
                editBoxZoneName.Visible = true;
                editBoxZoneName.BringToFront();  // Make sure it's visible above the ListBox
                editBoxZoneName.Focus();
                editBoxZoneName.SelectAll();
            }
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
            buttonCommunicate1Hz.Enabled = false;
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    comboBoxCOMPorts.Enabled = true;
                    buttonCommunicate1Hz.Enabled = false;
                    buttonCommunicate1Hz.Text = communicateText;
                    buttonGetZoneNames.Enabled = false;
                    appendToLog("Disconnected");
                    btnConnect.Text = "Connect";
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
                buttonCommunicate1Hz.Enabled = true;
                buttonGetZoneNames.Enabled = true;
                appendToLog("Connected");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                btnConnect.Enabled = true;
                comboBoxCOMPorts.Enabled = true;
                buttonGetZoneNames.Enabled = false;
                buttonUpdateZoneNames.Enabled = false;
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

        private void SendCommandAndProcessResponse(byte[] command)
        {
            if (serialPort == null)
                throw new InvalidOperationException("Serial port is not initialized");

            if (!serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not open");

            try
            {
                // Send command
                serialPort.Write(command, 0, command.Length);
                appendToLog($"Command '{command}' sent.");

                // Wait for response
                Thread.Sleep(500);

                byte[] bufferRead = new byte[NB_OF_ZONES * ZONE_NAME_LENGTH];
                int bytesToRead;
                int bytesRead;

                if (command[0] == GET_ZONE_NAMES_COMMAND)
                {
                    bytesToRead = NB_OF_ZONES * ZONE_NAME_LENGTH;
                }
                else
                {
                    bytesToRead = 1;
                }

                bytesRead = serialPort.Read(bufferRead, 0, bytesToRead);

                if (bytesRead > 0)
                {
                    ProcessResponse(command, bufferRead);
                }
                else
                {
                    appendToLog("No response from the control panel.");
                }
            }
            catch (Exception ex)
            {
                string codes = string.Join(", ", command.Select(c => (int)c));
                appendToLog("Error while communicating! command ASCII code: " + codes + " Exception: " + ex.Message);
            }
        }

        private static string getZoneNumbers(byte response)
        {
            string zones = "";
            if ((response & 0b00000001) != 0) zones += "1 ";
            if ((response & 0b00000010) != 0) zones += "2 ";
            if ((response & 0b00000100) != 0) zones += "3 ";
            if ((response & 0b00001000) != 0) zones += "4 ";
            if ((response & 0b00010000) != 0) zones += "5 ";
            if ((response & 0b00100000) != 0) zones += "6 ";
            if ((response & 0b01000000) != 0) zones += "7 ";
            if ((response & 0b10000000) != 0) zones += "8 ";
            return zones;
        }

        private void parseZoneNames(byte[] buffer)
        {
            const int expectedZoneNameLength = NB_OF_ZONES * ZONE_NAME_LENGTH;
            if (buffer.Length != expectedZoneNameLength)
            {
                throw new ArgumentOutOfRangeException("Zone name byte array size " + buffer.Length + " not " + expectedZoneNameLength);
            }
            listBoxZoneNames.Items.Clear();
            for (int i = 0; i < NB_OF_ZONES; i++)
            {
                int iStart = i * ZONE_NAME_LENGTH;
                byte[] subset = new byte[ZONE_NAME_LENGTH];
                Array.Copy(buffer, iStart, subset, 0, ZONE_NAME_LENGTH);
                string zoneName = System.Text.Encoding.ASCII.GetString(subset);
                listBoxZoneNames.Items.Add(zoneName);
            }
        }

        private void ProcessResponse(byte[] command, byte[] bufferRead)
        {
            byte firstByteRead = bufferRead[0];
            switch (command[0])
            {
                case isThereFireAlarm:
                    if (firstByteRead == 0b00000000) appendToLog("No fire alarm.");
                    else appendToLog($"Fire alarm at zones: {getZoneNumbers(firstByteRead)}");
                    break;
                case isThereZoneLineFault:
                    if (firstByteRead == 0b00000000) appendToLog("No zone line fault.");
                    else appendToLog($"Zone line fault at zones: {getZoneNumbers(firstByteRead)}");
                    break;
                case isThereControlPanelFault:
                    appendToLog("control panel fault conditions:");
                    if ((firstByteRead & 0b00000001) != 0) appendToLog("Batarya yok");
                    if ((firstByteRead & 0b00000010) != 0) appendToLog("Batarya zayıf");
                    if ((firstByteRead & 0b00000100) != 0) appendToLog("Şebeke yok");
                    if ((firstByteRead & 0b00001000) != 0) appendToLog("Şarj zayıf");
                    if ((firstByteRead & 0b00010000) != 0) appendToLog("Siren Arıza");
                    if ((firstByteRead & 0b00100000) != 0) appendToLog("Çıkış arıza");
                    if ((firstByteRead & 0b01000000) != 0) appendToLog("Toprak arıza");
                    if (firstByteRead == 0b00000000) appendToLog("No control panel fault.");
                    break;
                case GET_ZONE_NAMES_COMMAND:
                    parseZoneNames(bufferRead);
                    break;
                case SET_ZONE_NAMES_COMMAND:
                    appendToLog("Set zone names command sent.");
                    break;
                case resetControlPanel:
                    appendToLog("Control panel reset command sent.");
                    break;
                case silenceBuzzer:
                    appendToLog("Buzzer silence command sent.");
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

        private void buttonCommunicate1Hz_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonCommunicate1Hz.Text == communicateText)
                {
                    btnConnect.Enabled = false;
                    buttonCommunicate1Hz.Text = stopText;
                    foreach (byte command in periodicCommandsOrder)
                    {
                        SendCommandAndProcessResponse(new byte[] { command });
                    }
                }
                else
                {
                    btnConnect.Enabled = true;
                    buttonCommunicate1Hz.Text = communicateText;
                }
            }
            finally
            {
                btnConnect.Enabled = true;
                buttonCommunicate1Hz.Text = communicateText;
            }
        }

        private void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            appendToLog("GetZoneNames clicked");
            SendCommandAndProcessResponse(new byte[] { GET_ZONE_NAMES_COMMAND });
            /*for (int i = 0; i < NB_OF_ZONES; i++)
            {
                listBoxZoneNames.Items.Add("zone " + i);
            }*/
            buttonUpdateZoneNames.Enabled = listBoxZoneNames.Items.Count != 0;
        }

        private void buttonUpdateZoneNames_Click(object sender, EventArgs e)
        {
            appendToLog("UpdateZoneNames clicked");
            if (listBoxZoneNames.Items.Count == 0)
            {
                appendToLog("listBoxZoneNames.Items.Count == 0, you must first get zone names from panel!");
                return;
            }
            const int commandSize = 1 + NB_OF_ZONES * (1 + ZONE_NAME_LENGTH) + 1;
            byte[] command = new byte[commandSize];
            command[0] = SET_ZONE_NAMES_COMMAND;
            for (int iZone = 0; iZone < NB_OF_ZONES; iZone++)
            {
                string? zoneName = listBoxZoneNames.Items[iZone].ToString();
                if (zoneName == null)
                {
                    throw new ArgumentNullException("Zone " + iZone + " name null!");
                }
                else
                {
                    zoneName = zoneName.PadRight(ZONE_NAME_LENGTH, ' ');
                }
                byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(zoneName);
                int iNameStart = 2 + iZone * (1 + ZONE_NAME_LENGTH);
                command[iNameStart - 1] = (byte)(200 + iZone + 1);
                Array.Copy(nameBytes, 0, command, iNameStart, ZONE_NAME_LENGTH);
            }
            command[commandSize - 1] = 27;
            //appendToLog(System.Text.Encoding.ASCII.GetString(command));
            SendCommandAndProcessResponse(command);
        }
    }
}