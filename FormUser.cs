
using System.Text;

namespace WinFormsSerial
{
    public partial class FormUser : Form
    {
        private FireControlPanelEmulator _emulator;
        private readonly SerialPortManager _serialPortManager;

        public FormUser()
        {
            InitializeComponent();
            textBoxLog.Clear();

            _serialPortManager = new SerialPortManager(AddToLog);
            _emulator = new FireControlPanelEmulator(AddToLog);

            AddToLog("Program başladı.");
            //listBoxFireAlarms.Items.Clear();
            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                listBoxFireAlarms.Items.Add("Alarm " + (i + 1));
                listBoxZoneFaults.Items.Add("Zone Fault " + (i + 1));
                listBoxZoneNames.Items.Add("Zone Name " + (i + 1));
            }
            listBoxControlPanelFaults.Items.AddRange(new string[] { "Batarya yok", "Batarya zayıf", "Şebeke yok", "Şarj zayıf", "Siren1 Arıza", "Siren2 Arıza",
                "Çıkış arıza", "Toprak arıza" });

            Shown += FormUser_Shown;
            //FormUser_Shown(this, null);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _serialPortManager?.Dispose();
            base.OnFormClosing(e);
        }

        private async void FormUser_Shown(object? sender, EventArgs e)
        {
            AddToLog("Run emulator...");
            _emulator.Run();

            var detectedPort = await SerialPortManager.DetectFireControlPanelPortAsync(AddToLog);
            if (string.IsNullOrEmpty(detectedPort))
            {
                AddToLog($"Yangın alarm paneli ile iletişim kurulamadı! Panelin PC'ye bağlantısını kontrol edin.");
                textBoxFireControlPanelConnection.BackColor = Color.Red;
                textBoxFireControlPanelConnection.Text = "BAĞLANTI YOK";
            }
            else
            {
                AddToLog($"Yangın alarm paneli tespit edildi, port {detectedPort}.");
                textBoxFireControlPanelConnection.BackColor = Color.Green;
                textBoxFireControlPanelConnection.Text = "BAĞLANTI VAR";

                // Poll Fire Control Panel at 1Hz:
                AddToLog($"Panel ile bağlantı kuruluyor...");
                await _serialPortManager.ConnectAsync(detectedPort);
                AddToLog($"Panel saniyede 1 sorgulanıyor...");
                var communicationCts = new CancellationTokenSource();
                /*while (_serialPortManager.IsConnected)
                {
                    byte[] response = await _serialPortManager.ProcessPeriodicCommandsAsync(communicationCts, 1000);
                    if (response.Length > 0)
                    {
                        var responseFirstByte = response[0];
                        UpdateUI(responseFirstByte);
                    }                   
                }*/
            }
        }

        private void UpdateUI(byte responseFirstByte)
        {
            if (responseFirstByte == 0)
            {
                textBoxAlarm.BackColor = Color.White;
                textBoxAlarm.Text = "";
                listBoxFireAlarms.Items.Clear();
            }
            else
            {
                textBoxAlarm.BackColor = Color.Red;
                textBoxAlarm.Text = "ALARM";
                List<int> problemZones = FaultAlarmCommandProcessor.GetZonesWithProblems(responseFirstByte);
                //AddToLog($"Problem zones detected: {string.Join(", ", problemZones)}");
                listBoxFireAlarms.Items.Clear();

                // Convert integers to proper string format
                string[] alarmItems = problemZones.Select(zone => $"Alarm {zone + 1}").ToArray();
                listBoxFireAlarms.Items.AddRange(alarmItems);
            }
        }

        private void AddToLog(string message)
        {
            if (InvokeRequired) // We're on a different thread - marshal the call to the UI thread
            {
                BeginInvoke(() => AddToLog(message));
                return;
            }
            textBoxAlarm.BackColor = Color.Red;
            textBoxLog?.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message + Environment.NewLine); // "?" is here to prevent exception on window close, which disposes textBoxLog
            textBoxLog?.ScrollToCaret();
        }

        private void buttonSwitchToDev_Click(object sender, EventArgs e)
        {
            var formDev = new FormDeveloper();
            formDev.ShowDialog();
        }

        private async void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            AddToLog("Get zone names...");
            buttonGetZoneNames.Enabled = false;
            try
            {
                var responseBytes = await _serialPortManager.GetZoneNamesAsync();
                ParseAndDisplayZoneNames(responseBytes);
                AddToLog("Zone names obtained successfully.");
                buttonUpdateZoneNames.Enabled = true;
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
                buttonUpdateZoneNames.Enabled = false;
            }
            finally
            {
                buttonGetZoneNames.Enabled = true;
            }
        }

        private void ParseAndDisplayZoneNames(byte[] responseBytes)
        {
            listBoxZoneNames.Items.Clear();
            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                int startIndex = i * Constants.ZONE_NAME_LENGTH;
                byte[] zoneNameBytes = new byte[Constants.ZONE_NAME_LENGTH];
                Array.Copy(responseBytes, startIndex, zoneNameBytes, 0, Constants.ZONE_NAME_LENGTH);
                string zoneName = Encoding.ASCII.GetString(zoneNameBytes).TrimEnd();
                listBoxZoneNames.Items.Add(zoneName);
            }
        }
    }
}
