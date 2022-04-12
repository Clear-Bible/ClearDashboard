using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Helpers
{
    public static class LaunchWebPage
    {
        public static bool TryOpenUrl(string p_url)
        {
            // try use default browser [registry: HKEY_CURRENT_USER\Software\Classes\http\shell\open\command]
            try
            {
                string keyValue = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\http\shell\open\command", "", null) as string;
                if (string.IsNullOrEmpty(keyValue) == false)
                {
                    string browserPath = keyValue.Replace("%1", p_url);
                    System.Diagnostics.Process.Start(browserPath);
                    return true;
                }
            }
            catch { }

            // try open browser as default command
            try
            {
                System.Diagnostics.Process.Start(p_url); //browserPath, argUrl);
                return true;
            }
            catch { }

            // try open through 'explorer.exe'
            try
            {
                string browserPath = GetWindowsPath("explorer.exe");
                string argUrl = "\"" + p_url + "\"";

                System.Diagnostics.Process.Start(browserPath, argUrl);
                return true;
            }
            catch { }

            // return false, all failed
            return false;
        }

        internal static string GetWindowsPath(string p_fileName)
        {
            string path = null;
            string sysdir;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (i == 0)
                    {
                        path = Environment.GetEnvironmentVariable("SystemRoot");
                    }
                    else if (i == 1)
                    {
                        path = Environment.GetEnvironmentVariable("windir");
                    }
                    else if (i == 2)
                    {
                        sysdir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                        path = System.IO.Directory.GetParent(sysdir).FullName;
                    }

                    if (path != null)
                    {
                        path = System.IO.Path.Combine(path, p_fileName);
                        if (System.IO.File.Exists(path) == true)
                        {
                            return path;
                        }
                    }
                }
                catch { }
            }

            // not found
            return null;
        }
    }
}
