using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
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

public record ShowTokenizationWindowMessage(string ParatextProjectId, string ProjectName, string TokenizationType,
    Guid CorpusId, Guid TokenizedTextCorpusId, CorpusType CorpusType, bool IsRTL, bool IsNewWindow);

public record ShowParallelTranslationWindowMessage(string TranslationSetId, string AlignmentSetId, string DisplayName, string ParallelCorpusId,
    string? ParallelCorpusDisplayName, bool IsRTL, bool IsNewWindow);
public record BackgroundTaskChangedMessage(BackgroundTaskStatus Status);

public record CloseDockingPane(Guid guid);
public record UiLanguageChangedMessage(string LanguageCode);

public record VerseChangedMessage(string Verse);
public record BCVLoadedMessage();

public record ProjectLoadCompleteMessage(bool Loaded);

public record ActiveDocumentMessage(Guid Guid);

public record ProjectChangedMessage(ParatextProject Project);

public record TextCollectionChangedMessage(List<TextCollection> TextCollections);

public record ParatextConnectedMessage(bool Connected);

public record UserMessage(User User);

public record LogActivityMessage(string Message);


#region ProjectDesignSurfaceMessages
public record NodeSelectedChangedMessage(object Node);
public record ConnectionSelectedChangedMessage(Guid ConnectorId);
public record CorpusAddedMessage(string ParatextId);
public record CorpusDeletedMessage(string ParatextId);
public record CorpusSelectedMessage(string ParatextId);
public record CorpusDeselectedMessage(string ParatextId);
public record ParallelCorpusAddedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
public record ParallelCorpusDeletedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
public record ParallelCorpusSelectedMessage(string SourceParatextId, string TargetParatextId, Guid ConnectorGuid);
public record ParallelCorpusDeselectedMessage(Guid ConnectorGuid);

#endregion //ProjectDesignSurfaceMessages


public class DashboardProjectManager : ProjectManager
{


#nullable disable

    private IEventAggregator EventAggregator { get; set; }

    protected HubConnection HubConnection { get; private set; }
    protected IHubProxy HubProxy { get; private set; }

    private readonly IWindowManager _windowManager;
    private readonly INavigationService _navigationService;

    private bool _licenseCleared = false;

    public DashboardProjectManager(IEventAggregator eventAggregator, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, IWindowManager windowManager, INavigationService navigationService, ILifetimeScope lifetimeScope) : base(paratextProxy, logger, lifetimeScope)
    {
        EventAggregator = eventAggregator;
        _windowManager = windowManager;
        _navigationService = navigationService;
    }
    public FlowDirection CurrentLanguageFlowDirection { get; set; }

    public override async Task Initialize()
    {
        await base.Initialize();
        CurrentUser = GetLicensedUser();
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

                CurrentUser = await UpdateCurrentUserWithParatextUserInformation();

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


    protected async Task PublishSignalRConnected(bool connected)
    {
        await EventAggregator.PublishOnUIThreadAsync(new ParatextConnectedMessage(connected));
    }

    protected override async Task PublishParatextUser(User user)
    {
        await EventAggregator.PublishOnUIThreadAsync(new UserMessage(user));
    }

    protected async Task HookSignalREvents()
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
    
    public static dynamic NewProjectDialogSettings => CreateNewProjectDialogSettings();

    public void CheckLicense<TViewModel>(TViewModel viewModel)
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
                            Id = decryptedLicenseUser.Id
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

    private void PopupRegistration<TViewModel>(TViewModel viewModel)
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
    

    private static dynamic CreateNewProjectDialogSettings()
    {
        dynamic settings = new ExpandoObject();
        settings.WindowStyle = WindowStyle.None;
        settings.ShowInTaskbar = false;
        settings.WindowState = WindowState.Normal;
        settings.ResizeMode = ResizeMode.NoResize;
        return settings;
    }

    public async Task InvokeDialog<TDialogViewModel, TNavigationViewModel>(dynamic settings, Func<TDialogViewModel, Task<bool>> callback, DialogMode dialogMode = DialogMode.Add) where TDialogViewModel : new()
    {
        var dialogViewModel = IoC.Get<TDialogViewModel>();

        if (dialogViewModel is IDialog dialog)
        {
            dialog.DialogMode = dialogMode;
        }

        var success = await _windowManager.ShowDialogAsync(dialogViewModel, null, settings);

        if (success)
        {
            var navigate = await callback.Invoke(dialogViewModel);

            if (navigate)
            {
                _navigationService.NavigateToViewModel<TNavigationViewModel>();
            }
        }
    }

    public async Task InvokeDialog<TDialogViewModel>(dynamic settings, Func<TDialogViewModel, Task> callback, DialogMode dialogMode = DialogMode.Add) where TDialogViewModel : new()
    {
        var dialogViewModel = IoC.Get<TDialogViewModel>();

        if (dialogViewModel is IDialog dialog)
        {
            dialog.DialogMode = dialogMode;
        }

        var success = await _windowManager.ShowDialogAsync(dialogViewModel, null, settings);

        if (success)
        {
            await callback.Invoke(dialogViewModel);
        }
    }
}