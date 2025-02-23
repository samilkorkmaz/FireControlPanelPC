using System.Runtime.InteropServices;

namespace FireControlPanelPC
{
    public class SleepPreventer : IDisposable
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        private bool _preventingSleep = false;
        private readonly Action<string> _logCallback;

        public SleepPreventer(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public void PreventSleep()
        {
            if (!_preventingSleep)
            {
                EXECUTION_STATE result = SetThreadExecutionState(
                    EXECUTION_STATE.ES_CONTINUOUS |
                    EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                    EXECUTION_STATE.ES_DISPLAY_REQUIRED
                );

                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    _logCallback($"Failed to prevent sleep mode. Error code: {error}");
                    throw new InvalidOperationException($"Failed to prevent sleep mode. Error code: {error}");
                }

                _preventingSleep = true;
                _logCallback("Sleep prevention enabled");
            }
        }

        public void AllowSleep()
        {
            if (_preventingSleep)
            {
                EXECUTION_STATE result = SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

                if (result == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    _logCallback($"Failed to restore sleep mode. Error code: {error}");
                    throw new InvalidOperationException($"Failed to restore sleep mode. Error code: {error}");
                }

                _preventingSleep = false;
                _logCallback("Sleep prevention disabled");
            }
        }

        public void Dispose()
        {
            AllowSleep();
        }
    }
}