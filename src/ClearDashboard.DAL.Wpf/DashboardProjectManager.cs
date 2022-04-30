using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.ParatextPlugin.Data.Models;
using MediatR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf;

public record VerseChangedMessage(string Verse);

public record ProjectChangedMessage(Project Project);

public class DashboardProjectManager : ProjectManager
{
    private IEventAggregator EventAggregator { get; set; }

    public DashboardProjectManager(IMediator mediator, IEventAggregator eventAggregator, NamedPipesClient namedPipeClient, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory) : base(mediator, namedPipeClient, paratextProxy, logger, projectNameDbContextFactory)
    {
        EventAggregator = eventAggregator;
    }
    public FlowDirection CurrentLanguageFlowDirection { get; set; }

    protected override async Task HookSignalREvents()
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
    }
}