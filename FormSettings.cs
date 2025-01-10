namespace FireControlPanelPC
{
    public partial class FormSettings : Form
    {
        private readonly Action<int, int, bool> _onOkCallback;
        private readonly Action _onCancelCallback;

        public FormSettings(int pollingPeriod_ms, int writeReadDelay_ms, bool showLog,
            Action<int, int, bool> onOkCallback, Action onCancelCallback)
        {
            InitializeComponent();
            numericUpDownPollingPeriod_ms.Value = pollingPeriod_ms;
            numericUpDownWriteReadDelay_ms.Value = writeReadDelay_ms;
            checkBoxShowLog.Checked = showLog;
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
            _onOkCallback(getPollingPeriod_ms(), getSerialWriteReadlDelay_ms(), checkBoxShowLog.Checked);
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _onCancelCallback();
            Close();
        }
    }
}