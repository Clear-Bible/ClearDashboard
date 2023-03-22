using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace PluginManager.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Member Variables   

        private enum ReturnType
        {
            True,
            False,
            Missing
        }


        #endregion //Member Variables



        #region Public Properties


        #endregion //Public Properties


        #region Observable Properties

        private bool _canClose;
        public bool CanClose
        {
            get => _canClose;
            set => Set(ref _canClose, value);
        }

        private string? _version;
        public string? Version
        {
            get => _version;
            set => Set(ref _version, value);
        }

        private string _progressText = "";
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                NotifyOfPropertyChange(() => ProgressText);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public ShellViewModel()
        {

        }

        protected override async void OnViewLoaded(object view)
        {
            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Clear Dashboard Plugin Manager  -  Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            
            var isAquaEnabled = GetAquaRegistryKey();
            if (isAquaEnabled == ReturnType.True)
            {
                EnableAqua();
            }
            else if (isAquaEnabled == ReturnType.False)
            {
                DisableAqua();
            }

            CanClose = true;

            base.OnViewLoaded(view);
        }

        #endregion //Constructor



        #region Methods

        public void Close()
        {
            var startupPath = Environment.CurrentDirectory;
            var filename = Path.Combine(startupPath, "ClearDashboard.Wpf.Application.exe");

            if (File.Exists(filename))
            {
                var psi = new ProcessStartInfo();
                psi.FileName = filename;
                psi.WorkingDirectory = startupPath;
                try
                {
                    var process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                }
                catch (Exception)
                {
                    //If you are here the user clicked decline to grant admin privileges (or he's not administrator)
                }
            }

            TryCloseAsync();
        }

        private ReturnType GetAquaRegistryKey()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\ClearDashboard\AQUA");
            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                if ((string)key.GetValue("IsEnabled") == "true")
                {
                    key.Close();
                    return ReturnType.True;
                }
                else
                {
                    key.Close();
                    return ReturnType.False;
                }

            }
            key!.Close();

            return ReturnType.Missing;
        }

        private void EnableAqua()
        {
            ProgressText = "Starting Migrating of AQUA Plugin";

            // copy files from install directory to the proper spots
            var appStartupPath = Environment.CurrentDirectory;
            ProgressText += $"\nAqua Plugin File Copy from {appStartupPath}";
            var fromPath = Path.Combine(appStartupPath, "Aqua");
            if (Directory.Exists(fromPath))
            {
                // install folder
                var files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    try
                    {
                        ProgressText += $"\nFile: {file}";
                        FileInfo fileInfo = new FileInfo(file);

                        File.Copy(file, Path.Combine(appStartupPath, fileInfo.Name), true);
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Copy Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }
                ProgressText += $"\n";


                // en folder
                fromPath = Path.Combine(appStartupPath, "Aqua", "en");
                files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    try
                    {
                        ProgressText += $"\nFile: {file}";
                        FileInfo fileInfo = new FileInfo(file);

                        if (Directory.Exists(Path.Combine(appStartupPath, "en")) == false)
                        {
                            // create en directory
                            ProgressText += $"\nCreate en Directory";
                            Directory.CreateDirectory(Path.Combine(appStartupPath, "en"));
                            ProgressText += $"\nCreate en Directory Complete";
                        }

                        File.Copy(file, Path.Combine(appStartupPath, "en", fileInfo.Name), true);
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Copy Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }
                ProgressText += $"\n";

                // Services folder
                fromPath = Path.Combine(appStartupPath, "Aqua", "Services");
                files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    try
                    {
                        ProgressText += $"\nFile: {file}";
                        FileInfo fileInfo = new FileInfo(file);

                        if (Directory.Exists(Path.Combine(appStartupPath, "Services")) == false)
                        {
                            // create Services directory
                            ProgressText += $"\nCreate Services Directory";
                            Directory.CreateDirectory(Path.Combine(appStartupPath, "Services"));
                            ProgressText += $"\nCreate Services Directory Complete";
                        }

                        File.Copy(file, Path.Combine(appStartupPath, "Services", fileInfo.Name), true);
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Copy Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }
            }


        }

        private void DisableAqua()
        {
            ProgressText = "Starting Removal of AQUA Plugin";

            // copy files from install directory to the proper spots
            var appStartupPath = Environment.CurrentDirectory;
            ProgressText += $"\nAqua Plugin File Copy from {appStartupPath}";
            var fromPath = Path.Combine(appStartupPath, "Aqua");
            if (Directory.Exists(fromPath))
            {
                // install folder
                var files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    try
                    {
                        File.Delete(Path.Combine(appStartupPath, fileInfo.Name));
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Delete Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }

                // en folder
                fromPath = Path.Combine(appStartupPath, "Aqua", "en");
                files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    try
                    {
                        File.Delete(Path.Combine(appStartupPath, "en", fileInfo.Name));
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Delete Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }

                // Services folder
                fromPath = Path.Combine(appStartupPath, "Aqua", "Services");
                files = Directory.EnumerateFiles(fromPath);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    try
                    {
                        File.Delete(Path.Combine(appStartupPath, "Services", fileInfo.Name));
                    }
                    catch (Exception e)
                    {
                        ProgressText += "\nAqua Plugin File Delete Error";
                        ProgressText += $"\n  {e.Message}";
                    }
                }
            }
        }

        #endregion // Methods


    }
}
