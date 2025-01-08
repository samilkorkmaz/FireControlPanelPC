
using System.Text;

namespace WinFormsSerial
{
    public partial class FormUser : Form
    {
        private FireControlPanelEmulator _emulator;
        private readonly SerialPortManager _serialPortManager;
        private readonly ZoneNameEditor _zoneNameEditor;

        public FormUser()
        {
            InitializeComponent();
            textBoxLog.Clear();

            _serialPortManager = new SerialPortManager(AddToLog);
            _emulator = new FireControlPanelEmulator(AddToLog);
            _zoneNameEditor = new ZoneNameEditor(listBoxZoneNames);
            Controls.Add(_zoneNameEditor.EditBoxControl);

            listBoxFireAlarms.Items.Clear();
            listBoxZoneFaults.Items.Clear();
            listBoxControlPanelFaults.Items.Clear();

            AddToLog("Program başladı.");
            try
            {
                var (osVersion, windowsVersion) = Utils.CheckWindowsVersion();
                AddToLog($"OS: {windowsVersion}, " +
                   $"Build Number: {osVersion.Build}, " +
                   $"Revision: {osVersion.Revision}");

                Controls.Add(_zoneNameEditor.EditBoxControl);
                /*for (int i = 0; i < Constants.NB_OF_ZONES; i++)
                {
                    listBoxFireAlarms.Items.Add("Alarm " + (i + 1));
                    listBoxZoneFaults.Items.Add("Zone Fault " + (i + 1));
                    listBoxZoneNames.Items.Add("Zone Name " + (i + 1));
                }
                listBoxControlPanelFaults.Items.AddRange(new string[] { "Batarya yok", "Batarya zayıf", "Şebeke yok", "Şarj zayıf", "Siren1 Arıza", "Siren2 Arıza",
                "Çıkış arıza", "Toprak arıza" });*/

                Shown += FormUser_Shown;
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
                Utils.DisableAllGUIControls(Controls);
            }
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
                buttonGetZoneNames.Enabled = true;
                buttonUpdateZoneNames.Enabled = true;

                // Poll Fire Control Panel at 1Hz:
                AddToLog($"Panel ile bağlantı kuruluyor...");
                await _serialPortManager.ConnectAsync(detectedPort);
                AddToLog($"Panel saniyede 1 sorgulanıyor...");
                var communicationCts = new CancellationTokenSource();
                while (_serialPortManager.IsConnected)
                {
                    //var (command, responseBytes) = await _serialPortManager.ProcessPeriodicCommandsAsync(communicationCts, 1000);
                    foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
                    {
                        if (communicationCts.Token.IsCancellationRequested) break;

                        try
                        {
                            var (responseBytes, bytesRead) = await _serialPortManager.SendCommandAsync([command]);
                            if (bytesRead > 0)
                            {
                                var responseFirstByte = responseBytes[0];
                                UpdateUI(command, responseFirstByte);
                            }
                        }
                        catch (Exception ex)
                        {
                            AddToLog($"Command {command} hatası: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void UpdateUI(byte command, byte responseFirstByte)
        {
            if (responseFirstByte == 0)
            {
                textBoxAlarm.BackColor = Color.White;
                textBoxAlarm.Text = "";
                listBoxFireAlarms.Items.Clear();

                textBoxFault.BackColor = Color.White;
                textBoxFault.Text = "";
                listBoxZoneFaults.Items.Clear();
                listBoxControlPanelFaults.Items.Clear();
            }
            else
            {
                if (command == Constants.IS_THERE_FIRE_ALARM) {
                    textBoxAlarm.BackColor = Color.Red;
                    textBoxAlarm.Text = "ALARM";
                    List<int> problemZones = FaultAlarmCommandProcessor.GetZonesWithProblems(responseFirstByte);
                    //AddToLog($"Problem zones detected: {string.Join(", ", problemZones)}");
                    listBoxFireAlarms.Items.Clear();

                    // Convert integers to proper string format
                    string[] alarmItems = problemZones.Select(zone => $"Alarm {zone + 1}").ToArray();
                    listBoxFireAlarms.Items.AddRange(alarmItems);
                }
                else if (command == Constants.IS_THERE_ZONE_LINE_FAULT)
                {
                    textBoxFault.BackColor = Color.Yellow;
                    textBoxFault.ForeColor = Color.Red;
                    textBoxFault.Text = "HATA";
                    List<int> problemZones = FaultAlarmCommandProcessor.GetZonesWithProblems(responseFirstByte);
                    listBoxZoneFaults.Items.Clear();
                    string[] faultItems = problemZones.Select(zone => $"Bölge Hatası {zone + 1}").ToArray();
                    listBoxZoneFaults.Items.AddRange(faultItems);
                }
                else if (command == Constants.IS_THERE_CONTROL_PANEL_FAULT)
                {
                    textBoxFault.BackColor = Color.Yellow;
                    textBoxFault.ForeColor = Color.Red;
                    textBoxFault.Text = "HATA";
                    var controlPanelFaults = FaultAlarmCommandProcessor.GetFireControlPanelFaults(responseFirstByte);
                    listBoxControlPanelFaults.Items.Clear();
                    foreach (var fault in controlPanelFaults)
                    {
                        listBoxControlPanelFaults.Items.AddRange(fault);
                    }
                }
            }
        }

        private void AddToLog(string message)
        {
            if (InvokeRequired) // Method called from a different thread - marshal the call to the UI thread
            {
                BeginInvoke(() => AddToLog(message));
                return;
            }
            if (!textBoxLog.IsDisposed) // To prevent exception on window close, which disposes textBoxLog
            {
                textBoxLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message + Environment.NewLine);
                textBoxLog.ScrollToCaret();
            }
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

        private void buttonUpdateZoneNames_Click(object sender, EventArgs e)
        {
            AddToLog("Update zone names");
            try
            {
                if (listBoxZoneNames.Items.Count == 0)
                {
                    AddToLog("Bölge ismi yok, lütfen önce Bölge İsimlerini Al butonuna basınız.");
                    return;
                }
                var responseFirstByte = _serialPortManager.UpdateZoneNamesAsync(listBoxZoneNames.Items.Cast<string>().ToArray());
                AddToLog($"Bölge isimlerini güncelle emri yollandı. Cevap: {responseFirstByte}");
                _zoneNameEditor.ClearEditHistory(); // Clear edit highlight from edited lines
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
            }
        }
    }
}
