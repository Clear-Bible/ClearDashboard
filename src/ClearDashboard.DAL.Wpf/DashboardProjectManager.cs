using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.ViewModels;
using MediatR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace ClearDashboard.DataAccessLayer.Wpf;

public record BackgroundTaskChangedMessage(BackgroundTaskStatus Status);

public record VerseChangedMessage(string Verse);

public record ProjectChangedMessage(ParatextProject Project);

public record TextCollectionChangedMessage(List<TextCollection> TextCollections);

public record ParatextConnectedMessage(bool Connected);

public record UserMessage(User user);

public record LogActivityMessage(string message);

public class DashboardProjectManager : ProjectManager
{
    #nullable disable

    private IEventAggregator EventAggregator { get; set; }

    protected HubConnection HubConnection { get; private set; }
    protected IHubProxy HubProxy { get; private set; }

    private readonly IWindowManager _windowManager;
    private readonly INavigationService _navigationService;

    private bool _licenseCleared = false;

    public DashboardProjectManager(IMediator mediator, IEventAggregator eventAggregator, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectDbContextFactory projectNameDbContextFactory, IWindowManager windowManager, INavigationService navigationService) : base(mediator, paratextProxy, logger, projectNameDbContextFactory)
    {
        EventAggregator = eventAggregator;
        _windowManager = windowManager;
        _navigationService = navigationService;
    }
    public FlowDirection CurrentLanguageFlowDirection { get; set; }

    public override async Task Initialize()
    {
        await base.Initialize();
        await ConfigureSignalRClient();
    }

 

    protected async Task ConfigureSignalRClient()
    {
        HubConnection = new HubConnection("http://localhost:9000/signalr");

        HubProxy = HubConnection.CreateHubProxy("Plugin");


        await HookSignalREvents();
        try
        {
            await HubConnection.Start();

            if (HubConnection.State == ConnectionState.Connected)
            {
                Logger.LogInformation("Connected to SignalR.");
                HubConnection.Closed += HandleSignalRConnectionClosed;
                HubConnection.Error += HandleSignalRConnectionError;
                await PublishSignalRConnected(true);

                CurrentUser = await GetUser();

            }
        }
        catch (HttpRequestException)
        {
            Logger.LogError("Paratext is not running, cannot connect to SignalR.");
            await Task.Delay(10);
            await ConfigureSignalRClient();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred while trying to connect to Paratext.");
            await Task.Delay(10);
            await ConfigureSignalRClient();
        }
    }

    private async void HandleSignalRConnectionError(Exception obj)
    {
        //var retryTimestamp = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

        //while (DateTime.UtcNow < retryTimestamp)
        //{
        //    await ConfigureSignalRClient();
        //    if (HubConnection.State == ConnectionState.Connected)
        //    {
        //        Logger.LogInformation("SignalR connected.");
        //    }
        //}

        //Logger.LogInformation("SignalR Connection is closed.");
    }

    private async void HandleSignalRConnectionClosed()
    {

        var retryTimestamp = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));

        while (DateTime.UtcNow < retryTimestamp)
        {
            await ConfigureSignalRClient();
            if (HubConnection.State == ConnectionState.Connected)
            {
                Logger.LogInformation("SignalR connected.");
                return;
            }
        }

        Logger.LogInformation("SignalR Connection is closed.");
    }


    protected  async Task PublishSignalRConnected(bool connected)
    {
        await EventAggregator.PublishOnUIThreadAsync(new ParatextConnectedMessage(connected));
    }

    protected override async Task PublishParatextUser(User user)
    {
        await EventAggregator.PublishOnUIThreadAsync(new UserMessage(user));
    }

    protected  async Task HookSignalREvents()
    {
        // ReSharper disable AsyncVoidLambda
        HubProxy.On<string>("sendVerse", async (verse) =>
          
        {
            CurrentVerse = verse;
            await EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verse));
        });

        HubProxy.On<ParatextProject>("sendProject", async (project) =>
        {
            project = GetParatextProjectDirectoryPath(project);


            CurrentParatextProject = project;
            await EventAggregator.PublishOnUIThreadAsync(new ProjectChangedMessage(project));
        });

        HubProxy.On<List<TextCollection>>("textCollections", async (textCollection) =>
        {
            await EventAggregator.PublishOnUIThreadAsync(new TextCollectionChangedMessage(textCollection));
        });

        // ReSharper restore AsyncVoidLambda

        await Task.CompletedTask;
    }

    private ParatextProject GetParatextProjectDirectoryPath(ParatextProject project)
    {
        string guid = project.Guid;

        ParatextProxy paratextProxy = new ParatextProxy(Logger as ILogger<ParatextProxy>);
        if (paratextProxy.IsParatextInstalled())
        {
            var path = paratextProxy.ParatextProjectPath;

            // iterate through the directories
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                if (File.Exists(Path.Combine(dir, "settings.xml")))
                {
                    var settings = XDocument.Load(Path.Combine(dir, "settings.xml"));
                    var results = settings.Descendants().First(p => p.Name.LocalName == "Guid");

                    if (results != null && results.Value == guid)
                    {
                        project.DirectoryPath = dir;
                        return project;
                    }
                }
            }
        }

        return project;
    }

    public void CheckLicense <TViewModel>(TViewModel viewModel)
        {
            if (!_licenseCleared)
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(documentsPath, "ClearDashboard_Projects\\license.txt");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var decryptedLicenseKey = LicenseManager.DecryptFromFile(filePath);
                        var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedLicenseKey);
                        if (decryptedLicenseUser.Id != null)
                        {
                            CurrentUser = new User
                            {
                                FirstName = decryptedLicenseUser.FirstName,
                                LastName = decryptedLicenseUser.LastName,
                                Id = Guid.Parse(decryptedLicenseUser.Id)
                            };

                        }

                        _licenseCleared = true;
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show("There was an issue decrypting your license key.");
                        PopupRegistration(viewModel);
                    }
                }
                else
                {   
                    //MessageBox.Show("Your license key file is missing.");
                    PopupRegistration(viewModel);
                }
            }
        }

        private void PopupRegistration <TViewModel>(TViewModel viewModel)
        {
            Logger.LogInformation("Registration called.");

            dynamic settings = new ExpandoObject();
            settings.Width = 850;
            settings.WindowStyle = WindowStyle.None;
            settings.ShowInTaskbar = false;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Absolute;
            settings.HorizontalOffset = SystemParameters.FullPrimaryScreenWidth / 2 - 100;
            settings.VerticalOffset = SystemParameters.FullPrimaryScreenHeight / 2 - 50;
            settings.Title = "License Registration";
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;

            var created = _windowManager.ShowDialogAsync(viewModel, null, settings);
            _licenseCleared = true;
        }

        public static dynamic NewProjectDialogSettings => CreateNewProjectDialogSettings();

        private static dynamic CreateNewProjectDialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.None;
            settings.ShowInTaskbar = false;
            //settings.Title = "Create New Project";  // TODO:  localize
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.NoResize;
            return settings;
    }

        public async Task InvokeDialog<TDialogViewModel,TNavigationViewModel>(dynamic settings , Func<TDialogViewModel, Task<bool>> callback) where TDialogViewModel : new()
        {
            var newProjectPopupViewModel = IoC.Get<TDialogViewModel>();
            var success = await _windowManager.ShowDialogAsync(newProjectPopupViewModel, null, settings);

            if (success)
            {
                var navigate = await callback.Invoke(newProjectPopupViewModel);

                if (navigate)
                {
                    _navigationService.NavigateToViewModel<TNavigationViewModel>();
                }
            }
        }
}