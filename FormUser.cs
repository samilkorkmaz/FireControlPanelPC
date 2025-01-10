using NLog;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace FireControlPanelPC
{
    public partial class FormUser : Form
    {
        private FireControlPanelEmulator _emulator;
        private readonly SerialPortManager _serialPortManager;
        private readonly ZoneNameEditor _zoneNameEditor;

        private CancellationTokenSource? _periodicCommandsCts;
        private Task? _periodicCommandsTask;
        private int _pollingPeriod_ms = 1000;
        private int _writeReadDelay_ms = 300;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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

        private Task StartPeriodicCommandsAsync()
        {
            _periodicCommandsCts = new CancellationTokenSource();
            _periodicCommandsTask = RunPeriodicCommandsAsync(_periodicCommandsCts.Token);
            return Task.CompletedTask; // a lightweight way to return a completed task without allocating a new one. We need to return a Task to maintain the async method signature
        }

        private async Task PausePeriodicCommandsAsync()
        {
            if (_periodicCommandsCts != null)
            {
                _periodicCommandsCts.Cancel();
                if (_periodicCommandsTask != null)
                {
                    try
                    {
                        await _periodicCommandsTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancelling
                    }
                }
                _periodicCommandsCts = null;
            }
        }

        private async Task RunPeriodicCommandsAsync(CancellationToken ct)
        {
            AddToLog($"Panel saniyede 1 sorgulanıyor...");
            while (!ct.IsCancellationRequested && _serialPortManager.IsConnected)
            {
                foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
                {
                    if (ct.IsCancellationRequested) break;

                    try
                    {
                        var (responseBytes, bytesRead) = await _serialPortManager.SendCommandWithTimeoutAsync([command], 1, _pollingPeriod_ms);
                        if (bytesRead > 0)
                        {
                            var responseFirstByte = responseBytes[0];
                            UpdateUI(command, responseFirstByte);
                        }
                        await Task.Delay(_pollingPeriod_ms, ct); // Add delay after each command
                    }
                    catch (Exception ex)
                    {
                        AddToLog($"Command {command} hatası: {ex.Message}");
                    }
                }
            }
        }

        private bool _isFormClosing;  // Track cleanup state
        private bool _isSerialPortManagerDisposed;   // Track if we've already disposed
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            // Proceed with normal closing if:
            // - We're already in the closing process, or
            // - We've already disposed resources, or
            // - We have no serial port manager to cleanup
            if (_isFormClosing || _isSerialPortManagerDisposed || _serialPortManager == null)
            {
                base.OnFormClosing(e);
                return;
            }

            try
            {
                _isFormClosing = true;
                e.Cancel = true;  // Cancel this close attempt

                // Stop periodic commands if running
                await PausePeriodicCommandsAsync();

                // Dispose the serial port manager
                await _serialPortManager.DisposeAsync();
                _isSerialPortManagerDisposed = true;

                // Now close the form without triggering another cleanup
                BeginInvoke(() =>
                {
                    _isFormClosing = false;  // Reset closing flag
                    Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during cleanup: {ex.Message}", "Cleanup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _isFormClosing = false;
                _isSerialPortManagerDisposed = true;  // Consider it disposed even on error
                BeginInvoke(() => Close());
            }
        }

        private async Task<string> ConnectToFireControlPanel()
        {
            string? detectedPort;
            while (string.IsNullOrEmpty(detectedPort = await SerialPortManager.DetectFireControlPanelPortAsync(AddToLog)))
            {
                AddToLog($"Yangın alarm paneli ile iletişim kurulamadı! Panelin PC'ye bağlantısını kontrol edin.");
                labelFireControlPanelConnection.BackColor = Color.Red;
                // Countdown from 5 to 1
                for (int i = 5; i > 0; i--)
                {
                    labelFireControlPanelConnection.Text = $"BAĞLANTI YOK ({i})";
                    await Task.Delay(1000); // Wait 1 second between counts
                }
                labelFireControlPanelConnection.BackColor = Color.Black;
                labelFireControlPanelConnection.Text = "BAĞLANTI KURULUYOR...";
            }
            AddToLog($"Yangın alarm paneli tespit edildi, port {detectedPort}.");
            labelFireControlPanelConnection.BackColor = Color.Green;
            labelFireControlPanelConnection.Text = $"BAĞLANTI {detectedPort}";
            buttonGetZoneNames.Enabled = true;
            buttonUpdateZoneNames.Enabled = true;
            return detectedPort;
        }

        private async void FormUser_Shown(object? sender, EventArgs e)
        {
            //_emulator.Run();
            var detectedPort = await ConnectToFireControlPanel();           

            AddToLog($"Panel ile bağlantı kuruluyor...");
            await _serialPortManager.ConnectAsync(detectedPort);

            AddToLog("Get zone names...");
            try
            {
                var responseBytes = await _serialPortManager.GetZoneNamesAsync();
                ParseAndDisplayZoneNames(responseBytes);
                AddToLog("Zone names obtained successfully.");
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
            }

            await StartPeriodicCommandsAsync();
        }

        private object[]? prevListBoxZoneNamesItems;
        private string[] GetProblemZoneItems(byte responseFirstByte, string start)
        {
            List<int> problemZones = FaultAlarmCommandProcessor.GetZonesWithProblems(responseFirstByte);
            string[] problemZoneItems;
            if (_zoneNameEditor.IsChangedByUser()) // Names changed by user but not yet sent to panel
            {
                problemZoneItems = problemZones.Select(zone => $"{start}: {prevListBoxZoneNamesItems?[zone]}").ToArray();
            }
            else
            {
                problemZoneItems = problemZones.Select(zone => $"{start}: {listBoxZoneNames.Items[zone]}").ToArray();
                prevListBoxZoneNamesItems = listBoxZoneNames.Items.Cast<object>().ToArray(); //copy
            }
            return problemZoneItems;
        }

        private void UpdateUI(byte command, byte responseFirstByte)
        {
            // Instead of clearing everything on 0 response, handle each command separately
            if (command == Constants.IS_THERE_FIRE_ALARM)
            {
                if (responseFirstByte == 0)
                {
                    labelAlarm.BackColor = Color.White;
                    labelAlarm.ForeColor = Color.White;
                    labelAlarm.Text = "";
                    listBoxFireAlarms.Items.Clear();
                }
                else
                {
                    labelAlarm.BackColor = Color.Red;
                    labelAlarm.ForeColor = Color.White;
                    labelAlarm.Text = "ALARM";
                    listBoxFireAlarms.Items.Clear();
                    string[] alarmItems = GetProblemZoneItems(responseFirstByte, "Alarm");
                    listBoxFireAlarms.Items.AddRange(alarmItems);
                }
            }
            else if (command == Constants.IS_THERE_ZONE_LINE_FAULT)
            {
                if (responseFirstByte == 0)
                {
                    // Only clear faults label if no other panel faults exist
                    if (listBoxControlPanelFaults.Items.Count == 0)
                    {
                        labelFault.BackColor = Color.White;
                        labelFault.Text = "";
                    }
                    listBoxZoneFaults.Items.Clear();
                }
                else
                {
                    labelFault.BackColor = Color.Yellow;
                    labelFault.ForeColor = Color.Red;
                    labelFault.Text = "HATA";
                    listBoxZoneFaults.Items.Clear();
                    string[] faultItems = GetProblemZoneItems(responseFirstByte, "Hata");
                    listBoxZoneFaults.Items.AddRange(faultItems);
                }
            }
            else if (command == Constants.IS_THERE_CONTROL_PANEL_FAULT)
            {
                if (responseFirstByte == 0)
                {
                    // Only clear faults label if no other zone faults exist
                    if (listBoxZoneFaults.Items.Count == 0)
                    {
                        labelFault.BackColor = Color.White;
                        labelFault.Text = "";
                    }
                    listBoxControlPanelFaults.Items.Clear();
                }
                else
                {
                    labelFault.BackColor = Color.Yellow;
                    labelFault.ForeColor = Color.Red;
                    labelFault.Text = "HATA";
                    var controlPanelFaults = FaultAlarmCommandProcessor.GetFireControlPanelFaults(responseFirstByte);
                    listBoxControlPanelFaults.Items.Clear();
                    foreach (var fault in controlPanelFaults)
                    {
                        listBoxControlPanelFaults.Items.AddRange(fault);
                    }
                }
            }
        }

        private void AddToLog(string text)
        {
            if (InvokeRequired) // Method called from a different thread - marshal the call to the UI thread
            {
                BeginInvoke(() => AddToLog(text));
                return;
            }
            if (!textBoxLog.IsDisposed) // To prevent exception on window close, which disposes textBoxLog
            {
                textBoxLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + text + Environment.NewLine);

                const int maxLines = 100;
                string[] lines = textBoxLog.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                int lineCount = lines.Length;

                if (lineCount > maxLines)
                {
                    textBoxLog.Text = string.Join(Environment.NewLine,
                        lines.Skip(lineCount - maxLines)) + Environment.NewLine;
                }

                textBoxLog.SelectionStart = textBoxLog.Text.Length; // Positions the caret (text cursor) at the end of the text. This is done before ScrollToCaret() to ensure the TextBox scrolls to show the latest text.
                textBoxLog.ScrollToCaret();
                Logger.Info(text);
            }
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            var formSettings = new FormSettings(
                (pollingPeriod_ms, writeReadDelay_ms, showLog) =>
                {
                    //MessageBox.Show("Ayarlar kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _pollingPeriod_ms = pollingPeriod_ms;
                    _writeReadDelay_ms = writeReadDelay_ms;
                    textBoxLog.Visible = showLog;
                    Logger.Info($"Settings saved, _pollingPeriod_ms: {_pollingPeriod_ms}, _writeReadDelay_ms: {_writeReadDelay_ms}");
                },
                () =>
                {
                    //MessageBox.Show("Settings cancelled!");
                }
            );
            formSettings.ShowDialog();
        }

        private async void buttonGetZoneNames_Click(object sender, EventArgs e)
        {
            AddToLog("Get zone names...");
            buttonGetZoneNames.Enabled = false;
            try
            {
                // Pause periodic commands before getting zone names
                await PausePeriodicCommandsAsync();

                var responseBytes = await _serialPortManager.GetZoneNamesAsync();
                ParseAndDisplayZoneNames(responseBytes);
                AddToLog("Zone names obtained successfully.");
                buttonUpdateZoneNames.Enabled = true;

                // Restart periodic commands after getting zone names
                await StartPeriodicCommandsAsync();
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
                buttonUpdateZoneNames.Enabled = false;

                // Ensure periodic commands are restarted even if there was an error
                await StartPeriodicCommandsAsync();
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

        private async void buttonUpdateZoneNames_Click(object sender, EventArgs e)
        {
            AddToLog("Update zone names");
            buttonUpdateZoneNames.Enabled = false;
            try
            {
                if (listBoxZoneNames.Items.Count == 0)
                {
                    AddToLog("Bölge ismi yok, lütfen önce Bölge İsimlerini Al butonuna basınız.");
                    return;
                }

                // Pause periodic commands before updating zone names
                await PausePeriodicCommandsAsync();

                var response = await _serialPortManager.UpdateZoneNamesAsync(listBoxZoneNames.Items.Cast<string>().ToArray());
                AddToLog($"Bölge isimlerini güncelle emri yollandı. Cevap: {response}");
                _zoneNameEditor.ClearEditHistory(); // Clear edit highlight from edited lines

                // Restart periodic commands after updating zone names
                await StartPeriodicCommandsAsync();
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
            }
            finally
            {
                buttonUpdateZoneNames.Enabled = true;
                // Ensure periodic commands are restarted even if there was an error
                if (_serialPortManager.IsConnected && _periodicCommandsCts == null)
                {
                    await StartPeriodicCommandsAsync();
                }
            }
        }

        private async void buttonResetPanel_Click(object sender, EventArgs e)
        {
            AddToLog("Reset Panel...");
            buttonResetPanel.Enabled = false;
            try
            {
                // Pause periodic commands
                await PausePeriodicCommandsAsync();

                var responseBytes = await _serialPortManager.ResetPanelAsync();
                AddToLog("Reset Panel successful.");

                // Restart periodic commands
                await StartPeriodicCommandsAsync();
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
                // Ensure periodic commands are restarted even if there was an error
                await StartPeriodicCommandsAsync();
            }
            finally
            {
                buttonResetPanel.Enabled = true;
            }
        }

        private async void buttonBuzzerStop_Click(object sender, EventArgs e)
        {
            AddToLog("Buzzer Stop...");
            buttonBuzzerStop.Enabled = false;
            try
            {
                // Pause periodic commands
                await PausePeriodicCommandsAsync();

                var responseBytes = await _serialPortManager.BuzzerStopAsync();
                AddToLog("Buzzer successful.");

                // Restart periodic commands
                await StartPeriodicCommandsAsync();
            }
            catch (Exception ex)
            {
                AddToLog(ex.Message);
                // Ensure periodic commands are restarted even if there was an error
                await StartPeriodicCommandsAsync();
            }
            finally
            {
                buttonBuzzerStop.Enabled = true;
            }
        }
    }
}
