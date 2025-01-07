using System.IO.Ports;
using System.Text;

namespace WinFormsSerial
{
    public class SerialPortManager : IDisposable
    {
        private SerialPort? _serialPort;
        private readonly FaultAlarmCommandProcessor? _faultAlarmCommandProcessor;
        private readonly Action<string> _logCallback;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialPortManager(Action<string> logCallback)
        {
            _logCallback = logCallback;
            _faultAlarmCommandProcessor = new FaultAlarmCommandProcessor(logCallback);
        }

        public string getPortName()
        {
            return _serialPort != null ? _serialPort.PortName : "No port!";
        }

        public string[] GetAvailablePortNames() => SerialPort.GetPortNames();

        public async Task ConnectAsync(string portName, double timeoutSeconds = 3)
        {
            _logCallback($"Connecting to {portName}...");
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

            var _cts = new CancellationTokenSource();
            _cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            await Task.Run(() =>
            {
                try
                {
                    _serialPort.Open();
                }
                catch (Exception) when (_cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException($"ERROR: Connection attempt to {_serialPort.PortName} timed out after {timeoutSeconds} seconds.");
                }
            }, _cts.Token);

            _logCallback($"Connected to {_serialPort.PortName}");
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
                _logCallback($"Command sent: {string.Join(", ", command)}");
            }
            catch (Exception ex)
            {
                //_logCallback($"Write error: {ex.Message}");
                throw new IOException($"Failed to write to serial port {_serialPort.PortName}: {ex.Message}", ex);
            }

            Thread.Sleep(500); // Wait for response

            try
            {
                byte[] response = new byte[expectedResponseLength];
                int bytesRead = _serialPort.Read(response, 0, expectedResponseLength);
                return (response, bytesRead);
            }
            catch (Exception ex)
            {
                //_logCallback($"Read error: {ex.Message}");
                throw new IOException($"Failed to read from serial port {_serialPort.PortName}: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
            }
            _serialPort?.Dispose();
        }

        public async Task ProcessPeriodicCommandsAsync(CancellationTokenSource communicationCts)
        {
            if (_faultAlarmCommandProcessor == null) throw new  ArgumentNullException("_faultAlarmCommandProcessor == null");

            foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
            {
                if (communicationCts.Token.IsCancellationRequested) break;

                try
                {
                    var (response, bytesRead) = SendCommand([command]);
                    if (bytesRead > 0)
                    {
                        _faultAlarmCommandProcessor.ProcessResponse(command, response);
                    }
                }
                catch (Exception ex)
                {
                    _logCallback($"Command {command} execution error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(1000, communicationCts.Token); // 1Hz frequency
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        public byte[] GetZoneNames()
        {
            var expectedBytesLength = Constants.NB_OF_ZONES * Constants.ZONE_NAME_LENGTH;
            var (responseBytes, readBytesLength) = SendCommand([Constants.GET_ZONE_NAMES_COMMAND], expectedBytesLength);

            if (readBytesLength == expectedBytesLength)
            {
                return responseBytes;
            }
            else
            {
                throw new InvalidOperationException($"Error when getting zone names command {Constants.GET_ZONE_NAMES_COMMAND}: " +
                    $"Expected number of bytes {expectedBytesLength} is different from received {readBytesLength}");
            }
        }

        public byte UpdateZoneNames(string[] zoneNames)
        {
            byte[] command = BuildZoneNamesUpdateCommand(zoneNames);
            var (response, _) = SendCommand(command);
            var responseFirstByte = response[0];
            const byte expectedResponse = 245;
            if (responseFirstByte == expectedResponse)
            {
                return responseFirstByte;
            }
            else
            {
                throw new InvalidOperationException($"Error for updating zone names command {Constants.SET_ZONE_NAMES_COMMAND}: " +
                    $"expectedResponse was {expectedResponse}, got {responseFirstByte}");
            }
        }

        private byte[] BuildZoneNamesUpdateCommand(string[] zoneNames)
        {
            const int commandSize = 1 + Constants.NB_OF_ZONES * (1 + Constants.ZONE_NAME_LENGTH) + 1;
            byte[] command = new byte[commandSize];
            command[0] = Constants.SET_ZONE_NAMES_COMMAND;

            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                string zoneName = (zoneNames[i]?? "").PadRight(Constants.ZONE_NAME_LENGTH);
                byte[] nameBytes = Encoding.ASCII.GetBytes(zoneName);

                int nameStart = 2 + i * (1 + Constants.ZONE_NAME_LENGTH);
                command[nameStart - 1] = (byte)(200 + i + 1);
                Array.Copy(nameBytes, 0, command, nameStart, Constants.ZONE_NAME_LENGTH);
            }

            command[commandSize - 1] = 27; // End marker
            return command;
        }

        public string? DetectFirePanelPort()
        {
            string[] availablePorts = SerialPort.GetPortNames();
            _logCallback($"Checking if fire control panel is connected by sending command {Constants.IS_THERE_FIRE_ALARM} to {availablePorts.Length} availabale ports...");
            foreach (string port in availablePorts)
            {
                _logCallback($"Checking {port}...");
                try
                {
                    using (SerialPort testPort = new SerialPort(port))
                    {
                        testPort.BaudRate = 9600;    // Match your device settings
                        testPort.DataBits = 8;
                        testPort.Parity = Parity.None;
                        testPort.StopBits = StopBits.One;
                        testPort.ReadTimeout = 300;   // Short timeout for quick scanning
                        testPort.WriteTimeout = 300;

                        testPort.Open();

                        // Clear any existing data
                        testPort.DiscardInBuffer();
                        testPort.DiscardOutBuffer();

                        // Send the "are you there?" command
                        testPort.Write(new byte[] { Constants.IS_THERE_FIRE_ALARM }, 0, 1);

                        // Read response
                        byte[] response = new byte[1];
                        int bytesRead = testPort.Read(response, 0, 1);
                        testPort.Close();

                        if (bytesRead == 1)
                        {
                            return port;
                        }
                    }
                }
                catch (Exception)
                {
                    // Port is either in use or not the correct one
                    continue;
                }
            }
            return null;
        }
    }
}
