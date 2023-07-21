using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace ClearDashboard.Wpf.Application;

public class DashboardProjectManager : ProjectManager
{


#nullable disable

    private IEventAggregator EventAggregator { get; set; }

    protected HubConnection HubConnection { get; private set; }
    protected IHubProxy HubProxy { get; private set; }

    private readonly IWindowManager _windowManager;
    private readonly INavigationService _navigationService;

    private bool _licenseCleared = false;
    private bool UpdatingCurrentVerse { get; set; }
    
    public List<ParatextProjectMetadata> ProjectMetadata = new();

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
        SetCurrentUserFromLicense();
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
                HubConnection.Reconnecting += HandleSignalRConnectionReconnecting;
                HubConnection.Error += HandleSignalRConnectionError;
                await PublishSignalRConnected(true);

                CurrentUser = await UpdateCurrentUserWithParatextUserInformation();

            }
        }
        catch (HttpRequestException)
        {
            Logger.LogError("Paratext is not running, cannot connect to SignalR.");
            await PublishSignalRConnected(false);
            await ConfigureSignalRClient();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred while trying to connect to Paratext.");
            await PublishSignalRConnected(false);
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
        await Task.CompletedTask;
    }

    private async void HandleSignalRConnectionReconnecting()
    {
        HubConnection.Dispose();
       
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
        //this.ParatextUserName = user.ParatextUserName;
    }

    protected async Task HookSignalREvents()
    {
        List<string> requestedVerses = new();
        // ReSharper disable AsyncVoidLambda
        HubProxy.On<string>("sendVerse", async (verse) =>
        {
            requestedVerses.Add(verse);
            if (!UpdatingCurrentVerse)
            {
                UpdatingCurrentVerse = true;
              
                CurrentVerse = verse.PadLeft(9, '0');
                await EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verse));
                
                UpdatingCurrentVerse = false;

                if (requestedVerses.Last().PadLeft(9, '0') != CurrentVerse.PadLeft(9, '0'))
                {
                    CurrentVerse = requestedVerses.Last();
                    await EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(CurrentVerse));
                }
            }
        });

        HubProxy.On<ParatextProject>("sendProject", async (project) =>
        {
            project = GetParatextProjectDirectoryPath(project);


            CurrentParatextProject = project;
            EventAggregator.PublishOnUIThreadAsync(new ProjectChangedMessage(project));//if the message isn't making it try commenting this line out
        });


        HubProxy.On<ParatextProject>("sendCurrentProject", async (project) =>
        {
            project = GetParatextProjectDirectoryPath(project);


            CurrentParatextProject = project;
            await EventAggregator.PublishOnUIThreadAsync(new ProjectChangedMessage(project));
        });


        HubProxy.On<List<TextCollection>>("textCollections", async (textCollection) =>
        {
            await EventAggregator.PublishOnUIThreadAsync(new TextCollectionChangedMessage(textCollection));
        });

        HubProxy.On<PluginClosing>("sendPluginClosing", async (pluginClosing) =>
        {
            if (pluginClosing.PluginConnectionChangeType == PluginConnectionChangeType.Closing)
            {
                await EventAggregator.PublishOnUIThreadAsync(new ParatextConnectedMessage(false));
            }
            
            Logger.LogInformation($"sendPluginClosing received: {pluginClosing.PluginConnectionChangeType}");
        });


        // ReSharper restore AsyncVoidLambda

        await Task.CompletedTask;
    }

    private ParatextProject GetParatextProjectDirectoryPath(ParatextProject project)
    {
        string guid = project.Guid;

        var logger = LifetimeScope.Resolve<ILogger<ParatextProxy>>();
        ParatextProxy paratextProxy = new ParatextProxy(logger);
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