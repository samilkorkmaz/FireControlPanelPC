using System.Management;
using System.IO.Ports;

namespace WinFormsSerial
{
    public class SerialPortEnumerator : IDisposable
    {
        private ManagementEventWatcher? watcher;
        public event EventHandler<SerialPortsChangedEventArgs>? PortsChanged;
        private Action<string> _logCallback;

        public class SerialPortsChangedEventArgs : EventArgs
        {
            public string[] Ports { get; private set; }
            public bool HasPorts => Ports != null && Ports.Length > 0;

            public SerialPortsChangedEventArgs(string[] ports)
            {
                Ports = ports ?? new string[0]; // Ensure we never have null
            }
        }

        public SerialPortEnumerator(Action<string> logCallback)
        {
            _logCallback = logCallback;
            InitializeWatcher();
        }

        public string[] GetAvailablePorts()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher
                    ("SELECT * FROM Win32_PnPEntity WHERE (Caption LIKE '%(COM%)')"))
                {
                    var portnames = SerialPort.GetPortNames();

                    // Check if we got any ports
                    if (portnames == null || portnames.Length == 0)
                    {
                        return new string[0];
                    }

                    return portnames;
                }
            }
            catch (Exception ex)
            {
                // Log the error if you have logging
                _logCallback($"Error getting ports: {ex.Message}");
                return new string[0]; // Return empty array instead of null
            }
        }

        private void InitializeWatcher()
        {
            try
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += DeviceChangeEvent;
                watcher.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize port watcher", ex);
            }
        }

        private void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            // Small delay to allow Windows to finish device initialization
            System.Threading.Thread.Sleep(500);
            var ports = GetAvailablePorts();
            PortsChanged?.Invoke(this, new SerialPortsChangedEventArgs(ports));
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}