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
using Helpers;

namespace ClearDashboard.Wpf.ViewModels
{
    public class LandingViewModel: ApplicationScreen
    {
        #region   Member Variables
        
        protected IWindowManager _windowManager;

        private bool _licenseCleared = false;
        
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

            if (!_licenseCleared)
            {
                //check if they have license key
                //Check for License.txt
                var DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var FilePath = Path.Combine(DocumentsPath, "ClearDashboard_Projects\\license.txt");
                if (File.Exists(FilePath))
                {
                    //decrypt file at FilePath
                    try
                    {
                        var decryptedLicenseKey = LicenseCryption.DecryptFromFile(FilePath);
                        var decryptedLicenseUser = LicenseCryption.DecryptedJsonToLicenseUser(decryptedLicenseKey);
                        this.OnActivateAsync(cancellationToken: CancellationToken.None);
                        _licenseCleared = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an issue decrypting your license key.");
                        PopupRegistration();
                    }
                }
                else
                {
                    PopupRegistration();
                }
            }
        }

        private void PopupRegistration()
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

            var created = _windowManager.ShowDialogAsync(registrationPopupViewModel, null, settings);
            this.OnActivateAsync(cancellationToken: CancellationToken.None);
            _licenseCleared = true;
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
