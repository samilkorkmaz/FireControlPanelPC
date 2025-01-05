using System.IO.Ports;

namespace WinFormsSerial
{
    public class SerialPortManager : IDisposable
    {
        private SerialPort? _serialPort;
        private CancellationTokenSource? _cts;
        private readonly Action<string> _logCallback;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialPortManager(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public string[] GetAvailablePortNames() => SerialPort.GetPortNames();

        public async Task ConnectAsync(string portName, double timeoutSeconds = 3)
        {
            if (_serialPort?.IsOpen == true)
                throw new InvalidOperationException("Port is already open");

            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _cts = new CancellationTokenSource();
            _cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            await Task.Run(() =>
            {
                try
                {
                    _serialPort.Open();
                }
                catch (Exception) when (_cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException($"Connection attempt timed out after {timeoutSeconds} seconds");
                }
            }, _cts.Token);

            _logCallback("Connected");
        }

        public void Disconnect()
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
                _logCallback("Disconnected");
            }
        }

        public (byte[] response, int bytesRead) SendCommand(byte[] command, int expectedResponseLength = 1)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not ready");

            try
            {
                _serialPort.Write(command, 0, command.Length);
                _logCallback($"Command sent: {BitConverter.ToString(command)}");

                Thread.Sleep(500); // Wait for response

                byte[] response = new byte[expectedResponseLength];
                int bytesRead = _serialPort.Read(response, 0, expectedResponseLength);

                return (response, bytesRead);
            }
            catch (Exception ex)
            {
                //_logCallback($"Communication error: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _cts?.Dispose();
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
            }
            _serialPort?.Dispose();
        }
    }
}
