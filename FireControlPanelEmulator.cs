using System.IO.Ports;

namespace WinFormsSerial
{
    internal class FireControlPanelEmulator
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _runningTask;
        private Action<string> _logCallback;
        private const string RECEIVE_PORT = "COM4";

        public FireControlPanelEmulator(Action<string> logCallback)
        {
            _logCallback = logCallback;
        }

        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Start the emulator on a separate task
            _runningTask = Task.Run(() => RunEmulator(_cancellationTokenSource.Token));
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

        private void RunEmulator(CancellationToken cancellationToken)
        {
            // Configure the receiver COM port:
            using SerialPort serialPort = new SerialPort(RECEIVE_PORT, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 2000;
            serialPort.WriteTimeout = 2000;

            try
            {
                serialPort.Open();
                _logCallback($"Emulator receive port {RECEIVE_PORT} opened successfully, waiting data to be sent to COM3...");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        byte[] bufferReceived = new byte[1024];
                        // Read incoming data
                        int nBytesReceived = serialPort.Read(bufferReceived, 0, bufferReceived.Length);

                        if (nBytesReceived > 0)
                        {
                            // Create a properly sized array for the received data
                            byte[] receivedData = new byte[nBytesReceived];
                            Array.Copy(bufferReceived, receivedData, nBytesReceived);
                            _logCallback($"Emulator received data: {string.Join(", ", receivedData.Select(b => b.ToString()))}");

                            // Create and send acknowledgement
                            byte[] ackData = new byte[] { 0xAA, 0xBB, 0xCC };
                            serialPort.Write(ackData, 0, ackData.Length);
                            _logCallback($"Emulator sent acknowledgement: {string.Join(", ", ackData.Select(b => b.ToString()))}");
                        }
                    }
                    catch (TimeoutException)
                    {
                        // Ignore timeout and continue listening
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
                if (serialPort.IsOpen)
                    serialPort.Close();
                _logCallback($"Emulator stopped and port {RECEIVE_PORT} closed.");
            }
        }
    }
}