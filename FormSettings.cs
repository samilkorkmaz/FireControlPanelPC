namespace FireControlPanelPC
{
    public partial class FormSettings : Form
    {
        private readonly Action<int, int> _onOkCallback;
        private readonly Action _onCancelCallback;

        public FormSettings(Action<int, int> onOkCallback, Action onCancelCallback)
        {
            InitializeComponent();
            _onOkCallback = onOkCallback;
            _onCancelCallback = onCancelCallback;
        }

        public int getPollingPeriod_ms()
        {
            return (int)numericUpDownPollingPeriod_ms.Value;
        }

        public int getSerialWriteReadlDelay_ms()
        {
            return (int)numericUpDownWriteReadDelay_ms.Value;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _onOkCallback(getPollingPeriod_ms(), getSerialWriteReadlDelay_ms());
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _onCancelCallback();
            Close();
        }
    }
}