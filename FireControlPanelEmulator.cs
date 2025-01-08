using System.IO.Ports;

namespace WinFormsSerial
{
    internal class FireControlPanelEmulator
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _runningTask;
        private Action<string> _logCallback;
        private const string RECEIVE_PORT = "COM5";

        public FireControlPanelEmulator(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public void Run()
        {
            _logCallback("Running emulator...");
            _cancellationTokenSource = new CancellationTokenSource();
            // Start the emulator on a separate task
            _runningTask = RunEmulatorAsync(_cancellationTokenSource.Token);
        }

        public async Task StopAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _logCallback("Emulator stopping...");
                _cancellationTokenSource.Cancel();
                if (_runningTask != null)
                {
                    try
                    {
                        await _runningTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected exception when cancelling
                    }
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task RunEmulatorAsync(CancellationToken cancellationToken)
        {
            using SerialPort serialPort = new SerialPort(RECEIVE_PORT, 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 2000,
                WriteTimeout = 2000
            };

            try
            {
                serialPort.Open();
                _logCallback($"Emulator receive port {RECEIVE_PORT} opened successfully, waiting data to be sent to {RECEIVE_PORT}+1...");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Flush buffers before sending new command
                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();

                        byte[] bufferReceived = new byte[1024];
                        int nBytesReceived = await Task.Factory.FromAsync(
                            serialPort.BaseStream.BeginRead,
                            serialPort.BaseStream.EndRead,
                            bufferReceived, 0, bufferReceived.Length,
                            null);

                        if (nBytesReceived > 0)
                        {
                            byte[] receivedData = new byte[nBytesReceived];
                            Array.Copy(bufferReceived, receivedData, nBytesReceived);
                            _logCallback($"Emulator received data: {string.Join(", ", receivedData.Select(b => b.ToString()))}");

                            byte[] responseBytes;// = [0x01, 0x02, 0x03];
                            var firstByteReceived = receivedData[0];
                            if (firstByteReceived == Constants.GET_ZONE_NAMES_COMMAND)
                            {
                                //_logCallback($"Emulator received GET_ZONE_NAMES_COMMAND");
                                string zoneNames = "";
                                for (int i = 0; i < Constants.NB_OF_ZONES; i++)
                                {
                                    zoneNames += "emulator zone__" + (i + 1);
                                }
                                responseBytes = System.Text.Encoding.ASCII.GetBytes(zoneNames);
                            }
                            else if (firstByteReceived == Constants.SET_ZONE_NAMES_COMMAND)
                            {
                                responseBytes = [245];
                            }
                            else
                            {
                                Random random = new Random();
                                responseBytes = [(random.NextDouble() < 0.3) ? (byte)0 : (byte)random.Next(1, 256)];
                                //responseBytes = [0xAA];
                                //responseBytes = [0x01, 0x02, 0x03];
                            }
                            await Task.Factory.FromAsync(
                                serialPort.BaseStream.BeginWrite,
                                serialPort.BaseStream.EndWrite,
                                responseBytes, 0, responseBytes.Length,
                                null);

                            _logCallback($"Emulator sent acknowledgement: {string.Join(", ", responseBytes.Select(b => b.ToString()))}");
                        }
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logCallback($"ERROR in emulator: {ex.Message}");
            }
            finally
            {
                if (serialPort.IsOpen) serialPort.Close();
                _logCallback($"Emulator stopped and port {RECEIVE_PORT} closed.");
            }
        }
    }
}