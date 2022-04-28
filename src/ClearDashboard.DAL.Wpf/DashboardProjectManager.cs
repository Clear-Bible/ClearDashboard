using System.Windows;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Wpf;

public class DashboardProjectManager : ProjectManager
{
    public DashboardProjectManager(IMediator mediator, NamedPipesClient namedPipeClient, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectNameDbContextFactory projectNameDbContextFactory) : base(mediator, namedPipeClient, paratextProxy, logger, projectNameDbContextFactory)
    {
    }
    public FlowDirection CurrentLanguageFlowDirection { get; set; }
}