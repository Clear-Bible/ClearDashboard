using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using MediatR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf;

public record VerseChangedMessage(string Verse);

public record ProjectChangedMessage(Project Project);

public record ParatextConnectedMessage(bool Connected);

public record ParatextUserMessage(string ParatextUserName);

public record LogActivityMessage(string message);

public class DashboardProjectManager : ProjectManager
{
    private IEventAggregator EventAggregator { get; set; }

    protected HubConnection? HubConnection { get; private set; }
    protected IHubProxy? HubProxy { get; private set; }

    public DashboardProjectManager(IMediator mediator, IEventAggregator eventAggregator, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory) : base(mediator, paratextProxy, logger, projectNameDbContextFactory)
    {
        EventAggregator = eventAggregator;
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

            }
        }
        catch (HttpRequestException ex)
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

    protected override async Task PublishParatextUser(string paratextUserName)
    {
        await EventAggregator.PublishOnUIThreadAsync(new ParatextUserMessage(paratextUserName));
    }

    protected  async Task HookSignalREvents()
    {
        // ReSharper disable AsyncVoidLambda
        HubProxy.On<string>("sendVerse", async (verse) =>
          
        {
            CurrentVerse = verse;
            await EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verse));
        });

        HubProxy.On<Project>("sendProject", async (project) =>
        {
            ParatextProject = project;
            await EventAggregator.PublishOnUIThreadAsync(new ProjectChangedMessage(project));
        });

        // ReSharper restore AsyncVoidLambda

        await Task.CompletedTask;
    }
}