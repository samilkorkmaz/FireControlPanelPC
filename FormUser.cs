
namespace WinFormsSerial
{
    public partial class FormUser : Form
    {
        public FormUser()
        {
            InitializeComponent();
            textBoxLog.Clear();
            AddToLog("Program başladı.");
            //listBoxFireAlarms.Items.Clear();
            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
               listBoxFireAlarms.Items.Add("Alarm " + (i+1));
               listBoxZoneFaults.Items.Add("Zone Fault " + (i+1));
               listBoxZoneNames.Items.Add("Zone Name " + (i+1));
            }
            listBoxControlPanelFaults.Items.AddRange(new string[] { "Batarya yok", "Batarya zayıf", "Şebeke yok", "Şarj zayıf", "Siren1 Arıza", "Siren2 Arıza",
                "Çıkış arıza", "Toprak arıza" });
        }

        private void AddToLog(string message)
        {            
            textBoxLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + message + Environment.NewLine);
        }

        private void buttonSwitchToDev_Click(object sender, EventArgs e)
        {
            var formDev = new FormDeveloper();
            formDev.ShowDialog();
        }
        
    }
}
