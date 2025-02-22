using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace FireControlPanelPC
{
    [Flags]
    public enum EXECUTION_STATE : uint
    {
        ES_SYSTEM_REQUIRED = 0x00000001,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_CONTINUOUS = 0x80000000
    }

    public class SerialPortManager : IDisposable
    {
        private SerialPort? _serialPort;
        private readonly FaultAlarmCommandProcessor? _faultAlarmCommandProcessor;
        private readonly Action<string> _logCallback;
        private bool _disposed;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialPortManager(Action<string> logCallback)
        {
            _logCallback = logCallback;
            _faultAlarmCommandProcessor = new FaultAlarmCommandProcessor(logCallback);
        }

        private void PreventSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        }

        private void AllowSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
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

        public async Task<(byte[] response, int bytesRead)> SendCommandWithTimeoutAsync(byte[] command, int expectedResponseLength, int timeoutMs = 1000, int writeReadDelay_ms = 300)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not ready");

            PreventSleep();  // Prevent sleep before starting communication

            try
            {
                using var cts = new CancellationTokenSource(timeoutMs);

                // Flush buffers before sending new command
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                // Create the write task
                var writeTask = Task.Factory.FromAsync(
                    _serialPort.BaseStream.BeginWrite,
                    _serialPort.BaseStream.EndWrite,
                    command, 0, command.Length,
                    null
                );

                // Wait for either the write to complete or timeout
                var completedWriteTask = await Task.WhenAny(
                    writeTask,
                    Task.Delay(timeoutMs, cts.Token)
                );

                if (completedWriteTask != writeTask)
                {
                    // If Task.Delay completed first, we timed out
                    if (writeTask.AsyncState is IAsyncResult asyncResult)
                    {
                        _serialPort.BaseStream.EndWrite(asyncResult);
                    }
                    throw new TimeoutException($"Write operation timed out after {timeoutMs}ms");
                }

                await writeTask; // Ensure write completes and propagate any errors
                _logCallback($"Command sent: {string.Join(", ", command)}");

                // Small delay after successful write
                await Task.Delay(writeReadDelay_ms, cts.Token);

                byte[] responseBytes = new byte[expectedResponseLength];

                // Create the read task
                var readTask = Task.Factory.FromAsync(
                    _serialPort.BaseStream.BeginRead,
                    _serialPort.BaseStream.EndRead,
                    responseBytes,
                    0,
                    expectedResponseLength,
                    null
                );

                // Wait for either the read to complete or timeout
                var completedReadTask = await Task.WhenAny(
                    readTask,
                    Task.Delay(timeoutMs, cts.Token)
                );

                if (completedReadTask != readTask)
                {
                    // If Task.Delay completed first, we timed out
                    if (readTask.AsyncState is IAsyncResult asyncResult)
                    {
                        _serialPort.BaseStream.EndRead(asyncResult);
                    }
                    throw new TimeoutException($"Read operation timed out after {timeoutMs}ms");
                }

                int bytesRead = await readTask;
                _logCallback($"responseBytes: {string.Join(", ", responseBytes)}");
                return (responseBytes, bytesRead);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new IOException($"Serial port {_serialPort.PortName} error: {ex.Message}", ex);
            }
            finally
            {
                AllowSleep();  // Allow sleep after communication is done
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    await Task.Delay(100); // Give port time to close
                }
                catch (Exception ex)
                {
                    _logCallback?.Invoke($"Error during port cleanup: {ex.Message}");
                }
            }

            _serialPort?.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            // For backwards compatibility, wait for async dispose
            DisposeAsync().AsTask().Wait();
        }

        public async Task<(byte, byte[])> ProcessPeriodicCommandsAsync(CancellationTokenSource communicationCts, int millisecondsDelay = 1000)
        {
            if (_faultAlarmCommandProcessor == null) throw new ArgumentNullException("_faultAlarmCommandProcessor == null");

            PreventSleep();  // Prevent sleep during periodic communication
            try
            {
                byte lastCommand = 0;
                byte[] lastResponse = [];

                foreach (byte command in Constants.PERIODIC_COMMANDS_ORDER)
                {
                    if (communicationCts.Token.IsCancellationRequested) break;

                    try
                    {
                        var (responseBytes, bytesRead) = await SendCommandWithTimeoutAsync([command], 1);
                        if (bytesRead > 0)
                        {
                            _faultAlarmCommandProcessor.ProcessResponse(command, responseBytes);
                            lastCommand = command;
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
                    return (0, []); // Return empty array if cancelled
                }

                return (lastCommand, lastResponse); // Return the last valid response
            }
            finally
            {
                AllowSleep();  // Allow sleep when periodic communication stops
            }
        }

        public async Task<byte[]> GetZoneNamesAsync(int timeoutMs = 1000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                var expectedBytesLength = Constants.NB_OF_ZONES * Constants.ZONE_NAME_LENGTH;
                var (responseBytes, readBytesLength) = await SendCommandWithTimeoutAsync([Constants.GET_ZONE_NAMES_COMMAND], expectedBytesLength);

                if (readBytesLength == expectedBytesLength)
                {
                    return responseBytes;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Error on Get Zone Names command {Constants.GET_ZONE_NAMES_COMMAND}: " +
                        $"Expected number of bytes {expectedBytesLength} is different from received {readBytesLength}"
                    );
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Get Zone Names operation timed out after {timeoutMs}ms");
            }
        }

        public async Task<byte> UpdateZoneNamesAsync(string[] zoneNames, int timeoutMs = 1000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                byte[] command = BuildZoneNamesUpdateCommand(zoneNames);
                var (response, _) = await SendCommandWithTimeoutAsync(command, 1);
                var responseFirstByte = response[0];
                const byte expectedResponse = 245;

                if (responseFirstByte == expectedResponse)
                {
                    return responseFirstByte;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Error on Update Zone Names command {Constants.SET_ZONE_NAMES_COMMAND}: " +
                        $"expectedResponse was {expectedResponse}, got {responseFirstByte}"
                    );
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Update Zone Names operation timed out after {timeoutMs}ms");
            }
        }

        private byte[] BuildZoneNamesUpdateCommand(string[] zoneNames)
        {
            const int commandSize = 1 + Constants.NB_OF_ZONES * (1 + Constants.ZONE_NAME_LENGTH) + 1;
            byte[] commandBytes = new byte[commandSize];
            commandBytes[0] = Constants.SET_ZONE_NAMES_COMMAND;

            for (int i = 0; i < Constants.NB_OF_ZONES; i++)
            {
                string zoneName = (zoneNames[i] ?? "").PadRight(Constants.ZONE_NAME_LENGTH);
                byte[] nameBytes = Encoding.ASCII.GetBytes(zoneName);

                int nameStart = 2 + i * (1 + Constants.ZONE_NAME_LENGTH);
                commandBytes[nameStart - 1] = (byte)(200 + i + 1);
                Array.Copy(nameBytes, 0, commandBytes, nameStart, Constants.ZONE_NAME_LENGTH);
            }

            commandBytes[commandSize - 1] = 27; // End marker
            return commandBytes;
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

                    bool portChecked = await Task.Run(() =>
                    {
                        try
                        {
                            testPort.Open();
                            testPort.Write([Constants.IS_THERE_FIRE_ALARM], 0, 1);

                            var response = new byte[1];
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

        public async Task<byte[]> ResetPanelAsync(int timeoutMs = 1000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                var expectedBytesLength = 1;
                var (responseBytes, readBytesLength) = await SendCommandWithTimeoutAsync([Constants.RESET_CONTROL_PANEL], expectedBytesLength);

                if (readBytesLength == expectedBytesLength)
                {
                    return responseBytes;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Error on Reset Panel command {Constants.RESET_CONTROL_PANEL}: " +
                        $"Expected number of bytes {expectedBytesLength} is different from received {readBytesLength}"
                    );
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Reset Panel operation timed out after {timeoutMs}ms");
            }
        }

        public async Task<byte[]> BuzzerStopAsync(int timeoutMs = 1000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                var expectedBytesLength = 1;
                var (responseBytes, readBytesLength) = await SendCommandWithTimeoutAsync([Constants.STOP_BUZZER], expectedBytesLength);

                if (readBytesLength == expectedBytesLength)
                {
                    return responseBytes;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Error on Stop Buzzer command {Constants.STOP_BUZZER}: " +
                        $"Expected number of bytes {expectedBytesLength} is different from received {readBytesLength}"
                    );
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Buzzer Stop operation timed out after {timeoutMs}ms");
            }
        }
    }
}
