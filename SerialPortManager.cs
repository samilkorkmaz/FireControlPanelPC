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

        public async Task<(byte[] response, int bytesRead)> SendCommandAsync(byte[] command, int expectedResponseLength = 1)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not ready");

            try
            {
                await Task.Factory.FromAsync(
                    _serialPort.BaseStream.BeginWrite,
                    _serialPort.BaseStream.EndWrite,
                    command, 0, command.Length,
                    null);
                _logCallback($"Command sent: {string.Join(", ", command)}");

                await Task.Delay(500); // Non-blocking wait

                byte[] response = new byte[expectedResponseLength];
                int bytesRead = await Task.Factory.FromAsync(
                    _serialPort.BaseStream.BeginRead,
                    _serialPort.BaseStream.EndRead,
                    response, 0, expectedResponseLength,
                    null);

                return (response, bytesRead);
            }
            catch (Exception ex)
            {
                throw new IOException($"Serial port {_serialPort.PortName} error: {ex.Message}", ex);
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

        public async Task<byte[]> ProcessPeriodicCommandsAsync(CancellationTokenSource communicationCts, int millisecondsDelay = 1000)
        {
            if (_faultAlarmCommandProcessor == null) throw new ArgumentNullException("_faultAlarmCommandProcessor == null");

            byte[] lastResponse = [];

            foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
            {
                if (communicationCts.Token.IsCancellationRequested) break;

                try
                {
                    var (responseBytes, bytesRead) = await SendCommandAsync([command]);
                    if (bytesRead > 0)
                    {
                        _faultAlarmCommandProcessor.ProcessResponse(command, responseBytes);
                        lastResponse = responseBytes; // Store the response but don't return yet
                    }
                }
                catch (Exception ex)
                {
                    _logCallback($"Command {command} execution error: {ex.Message}");
                }
            }

            try
            {
                await Task.Delay(millisecondsDelay, communicationCts.Token);
            }
            catch (OperationCanceledException)
            {
                return []; // Return empty array if cancelled
            }

            return lastResponse; // Return the last valid response
        }

        public async Task<byte[]> GetZoneNamesAsync()
        {
            var expectedBytesLength = Constants.NB_OF_ZONES * Constants.ZONE_NAME_LENGTH;
            var (responseBytes, readBytesLength) = await SendCommandAsync([Constants.GET_ZONE_NAMES_COMMAND], expectedBytesLength);

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

        public async Task<byte> UpdateZoneNamesAsync(string[] zoneNames)
        {
            byte[] command = BuildZoneNamesUpdateCommand(zoneNames);
            var (response, _) = await SendCommandAsync(command);
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
        public static async Task<string?> DetectFireControlPanelPortAsync(Action<string> logCallback)
        {
            string[] availablePorts = SerialPort.GetPortNames();
            logCallback($"Yangın alarm paneli ile {availablePorts.Length} porta {Constants.IS_THERE_FIRE_ALARM} komutuyla iletişim deneniyor...");

            foreach (string port in availablePorts)
            {
                logCallback($"{port}...");
                try
                {
                    using SerialPort testPort = new SerialPort(port)
                    {
                        BaudRate = 9600,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        ReadTimeout = 300,
                        WriteTimeout = 300
                    };

                    bool portChecked = await Task.Run(() => {
                        try
                        {
                            testPort.Open();
                            testPort.Write(new byte[] { Constants.IS_THERE_FIRE_ALARM }, 0, 1);

                            byte[] response = new byte[1];
                            int bytesRead = testPort.Read(response, 0, 1);
                            return bytesRead == 1;
                        }
                        catch
                        {
                            return false;
                        }
                        finally
                        {
                            if (testPort.IsOpen) testPort.Close();
                        }
                    });

                    if (portChecked) return port;
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }
    }
}
