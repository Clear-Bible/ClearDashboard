using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Popups;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Logging;
using MessageBox = System.Windows.Forms.MessageBox;
using System;


using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Popups;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables
        
        protected IWindowManager _windowManager;
        
        #endregion

        #region Observable Objects

       

        public ObservableCollection<DashboardProject> DashboardProjects { get; set; } =
            new ObservableCollection<DashboardProject>();

        #endregion

        #region Constructor

        /// <summary>
        /// Required for design-time support.
        /// </summary>
        public LandingViewModel()
        {

        }

        public LandingViewModel(IWindowManager windowManager, DashboardProjectManager projectManager, INavigationService navigationService, IEventAggregator eventAggregator, ILogger<LandingViewModel> logger)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            Logger.LogInformation("LandingViewModel constructor called.");
            _windowManager = windowManager;

            //check if they have license key
            //Check for License.Key
            var DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var FilePath = Path.Combine(DocumentsPath, "ClearDashboard_Projects\\license.key");
            if (!File.Exists(FilePath))
            {
                //decrypt file at FilePath
                //var contents = DecryptFromFile(FilePath);
            }
            else
            {
                //popup license key form
                Logger.LogInformation("Registration called.");

                dynamic settings = new ExpandoObject();
                settings.WindowStyle = WindowStyle.ThreeDBorderWindow;
                settings.ShowInTaskbar = false;
                settings.Title = "Register License";
                settings.WindowState = WindowState.Normal;
                settings.ResizeMode = ResizeMode.NoResize;

                var registrationPopupViewModel = IoC.Get<RegistrationDialogViewModel>();
                var created =  _windowManager.ShowDialogAsync(registrationPopupViewModel, null, settings);

                //if (created.HasValue && created.Value)
                //if (created)
                //{
                //    var projectName = newProjectPopupViewModel.ProjectName;

                //    await ProjectManager.CreateNewProject(projectName);
                //    //NavigationService.NavigateToViewModel<NewProjectWorkflowShellViewModel>();
                //}
            }
        }

        private async Task<string> DecryptFromFile(string path)
        {
            try
            {
                using (FileStream fileStream = new(path, FileMode.Open))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        byte[] key =
                        {
                            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                        };

                        using (CryptoStream cryptoStream = new(
                                   fileStream,
                                   aes.CreateDecryptor(key, iv),
                                   CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new(cryptoStream))
                            {
                                string decryptedMessage = await decryptReader.ReadToEndAsync();
                                //_output.WriteLine($"The decrypted original message: {decryptedMessage}");
                                return decryptedMessage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_output.WriteLine($"The decryption failed. {ex}");
                return "";
            }
        }

        private async Task EncryptToFile()
        {
            try
            {

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    LicenseKey = Guid.NewGuid().ToString(),
                    FirstName = "Bob",
                    LastName = "Smith"
                };

                using (FileStream fileStream = new("TestData.txt", FileMode.OpenOrCreate))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] key =
                        {
                            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                        };
                        aes.Key = key;

                        byte[] iv = aes.IV;
                        fileStream.Write(iv, 0, iv.Length);

                        using (CryptoStream cryptoStream = new(
                                   fileStream,
                                   aes.CreateEncryptor(),
                                   CryptoStreamMode.Write))
                        {
                            using (StreamWriter encryptWriter = new(cryptoStream))
                            {
                                //encryptWriter.WriteLine($"LicenseKey: {Guid.NewGuid()}");
                                //encryptWriter.WriteLine($"UserId: {Guid.NewGuid()}");
                                //encryptWriter.WriteLine($"FirstName: Bob");
                                //encryptWriter.WriteLine($"LastName: Smith");

                                encryptWriter.WriteLine(JsonSerializer.Serialize<User>(user));
                            }
                        }
                    }
                }

                //_output.WriteLine("The file was encrypted.");
            }
            catch (Exception ex)
            {
                //_output.WriteLine($"The encryption failed. {ex}");
            }
        }

        protected override void OnViewAttached(object view, object context)
        { base.OnViewAttached(view, context);
        }

        protected  override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
           var results = await ExecuteRequest(new GetDashboardProjectQuery(), CancellationToken.None);
           if (results.Success)
           {
               DashboardProjects = results.Data;
           }
        }

        #endregion

        #region Methods

        public void CreateNewProject()
        {
            Logger.LogInformation("CreateNewProject called.");
            NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        }

        public async void NewProject()
        {
            Logger.LogInformation("NewProject called.");
            
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.ThreeDBorderWindow;
            settings.ShowInTaskbar = false;
            settings.Title = "Create New Project";
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;

            var newProjectPopupViewModel = IoC.Get<NewProjectDialogViewModel>();
            var created = await _windowManager.ShowDialogAsync(newProjectPopupViewModel, null, settings);

            //if (created.HasValue && created.Value)
            if (created)
            {
                var projectName = newProjectPopupViewModel.ProjectName;

                await ProjectManager.CreateNewProject(projectName);
                //NavigationService.NavigateToViewModel<NewProjectWorkflowShellViewModel>();
            }

        }

        public void AlignmentSample()
        {
            Logger.LogInformation("AlignmentSample called.");
            //NavigationService.NavigateToViewModel<CreateNewProjectWorkflowShellViewModel>();
        }

        public void Workspace(DashboardProject project)
        {
            if (project is null)
            {
                return;
            }

            // TODO HACK TO READ IN PROJECT AS OBJECT
            string sTempFile = @"c:\temp\project.json";
            if (File.Exists(sTempFile) == false)
            {
                MessageBox.Show($"MISSING TEMP PROJECT FILE : {sTempFile}");
            }

            var jsonString =File.ReadAllText(@"c:\temp\project.json");
            project = JsonSerializer.Deserialize<DashboardProject>(jsonString);


            Logger.LogInformation("Workspace called."); 
            ProjectManager.CurrentDashboardProject = project;
            NavigationService.NavigateToViewModel<WorkSpaceViewModel>();
        }

        public void Settings()
        {
            Logger.LogInformation("Settings called.");
            NavigationService.NavigateToViewModel<SettingsViewModel>();

        }

        #endregion // Methods
    }
}
