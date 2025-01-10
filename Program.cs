using NLog;

namespace FireControlPanelPC
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 

        private static Mutex? _mutex;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main()
        {
            string? appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            bool createdNew;

            Logger.Info(appName + " started...");

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // App is already running! Bring it to front
                string message = "Zaten çalışan bir program var, ikinci bir program çalıştırılamaz.";
                Logger.Warn(message);
                MessageBox.Show(message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new FormUser());

            // Clean up
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
    }
}