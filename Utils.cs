using System.Reflection;
using System.Runtime.InteropServices;

namespace FireControlPanelPC
{
    internal class Utils
    {
        public static void DisableAllGUIControls(Control.ControlCollection controls, bool disable = true)
        {
            foreach (Control control in controls)
            {
                // Disable/enable the current control
                if (control is not Form)  // Skip the form itself
                {
                    control.Enabled = !disable;
                }

                // Recursively process any child controls
                if (control.HasChildren)
                {
                    DisableAllGUIControls(control.Controls, disable);
                }
            }
        }

        public static (Version osVersion, string windowsVersion) CheckWindowsVersion()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("This application requires Windows.");

            var osVersion = Environment.OSVersion.Version;

            // Check for minimum Windows version (Windows 8 = 6.2)
            if (osVersion.Major < 6 || (osVersion.Major == 6 && osVersion.Minor < 2))
            {
                throw new PlatformNotSupportedException(
                    $"This application requires Windows 8 or later. Detected Windows version: {osVersion.Major}.{osVersion.Minor}"
                );
            }

            string windowsVersion;

            if (osVersion.Major == 10 && osVersion.Build >= 22000)
            {
                windowsVersion = "Windows 11";
            }
            else if (osVersion.Major == 10)
            {
                windowsVersion = "Windows 10";
            }
            else if (osVersion.Major == 6)
            {
                switch (osVersion.Minor)
                {
                    case 3:
                        windowsVersion = "Windows 8.1";
                        break;
                    case 2:
                        windowsVersion = "Windows 8";
                        break;
                    case 1:
                        windowsVersion = "Windows 7";
                        break;
                    case 0:
                        windowsVersion = "Windows Vista";
                        break;
                    default:
                        windowsVersion = "Unknown Windows Version";
                        throw new PlatformNotSupportedException(windowsVersion);
                }
            }
            else
            {
                windowsVersion = $"Legacy Windows (Version {osVersion})";
            }

            return (osVersion, windowsVersion);
        }

        public static string? GetBuildDate()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                    .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
                    .FirstOrDefault(attr => attr.Key == "BuildDate")?.Value;
        }

    }
}
