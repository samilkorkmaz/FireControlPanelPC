using System.Management;
using System.IO.Ports;

namespace WinFormsSerial
{
    public class SerialPortEnumerator : IDisposable
    {
        private ManagementEventWatcher? watcher;
        public event EventHandler<SerialPortsChangedEventArgs>? PortsChanged;

        public class SerialPortsChangedEventArgs : EventArgs
        {
            public string[] Ports { get; private set; }
            public SerialPortsChangedEventArgs(string[] ports)
            {
                Ports = ports;
            }
        }

        public SerialPortEnumerator()
        {
            InitializeWatcher();
        }

        public string[] GetAvailablePorts()
        {
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM Win32_PnPEntity WHERE (Caption LIKE '%(COM%)')"))
            {
                var portnames = SerialPort.GetPortNames();
                var ports = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(p => p["Caption"].ToString())
                    .ToArray();

                return portnames;
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