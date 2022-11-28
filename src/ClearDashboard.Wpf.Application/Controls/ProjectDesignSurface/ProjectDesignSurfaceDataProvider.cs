using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using MediatR;
using Microsoft.Extensions.Logging;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;

public interface IDesignSurfaceDataProvider<out TViewModel, TData>
{
    TViewModel? DesignSurfaceViewModel { get; }
    Task<TData?> GetAsync();
    Task SaveAsync(TData data, CancellationToken cancellationToken);
}

public class ProjectDesignSurfaceDataProvider : IDesignSurfaceDataProvider<DesignSurfaceViewModel,ProjectDesignSurfaceSerializationModel>
{
    protected ILogger<ProjectDesignSurfaceDataProvider>? Logger { get; }

    protected DashboardProjectManager? ProjectManager { get; }

    //private readonly ILifetimeScope _lifecycleScope;
    //private readonly IEventAggregator? _eventAggregator;
    protected IMediator Mediator { get; }

    public ProjectDesignSurfaceDataProvider(ILogger<ProjectDesignSurfaceDataProvider>? logger, DashboardProjectManager? projectManager, IMediator mediator)
    {
        Logger = logger;
        ProjectManager = projectManager;
        Mediator = mediator;
    }

    public DesignSurfaceViewModel? DesignSurfaceViewModel { get; set; }

    public async Task<ProjectDesignSurfaceSerializationModel?> GetAsync()
    {
        var projectDesignSurfaceSerializationModel = LoadDesignSurfaceData();
        await Task.CompletedTask;
        return projectDesignSurfaceSerializationModel;
    }

    public async Task SaveAsync(ProjectDesignSurfaceSerializationModel data, CancellationToken cancellationToken)
    //public async Task SaveAsync()
    {
        await SaveDesignSurfaceData();
    }


    public async Task SaveDesignSurfaceData()
    {
        _ = await Task.Factory.StartNew(async () =>
        {
            var json = SerializeDesignSurface();

            ProjectManager!.CurrentProject.DesignSurfaceLayout = json;

            Logger!.LogInformation($"DesignSurfaceLayout : {ProjectManager.CurrentProject.DesignSurfaceLayout}");

            try
            {
                await ProjectManager.UpdateProject(ProjectManager.CurrentProject).ConfigureAwait(false);
                await Task.Delay(250);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex,
                    $"An unexpected error occurred while saving the project layout to the '{ProjectManager.CurrentProject.ProjectName} database.");
            }
        });

    }

    public string SerializeDesignSurface()
    {
        var surface = new ProjectDesignSurfaceSerializationModel();

        // save all the nodes
        foreach (var corpusNode in DesignSurfaceViewModel!.CorpusNodes)
        {
            surface.CorpusNodeLocations.Add(new CorpusNodeLocation
            {
                X = corpusNode.X,
                Y = corpusNode.Y,
                CorpusId = corpusNode.CorpusId,
            });
        }

        JsonSerializerOptions options = new()
        {
            IncludeFields = true,
            WriteIndented = false,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        return JsonSerializer.Serialize(surface, options);

    }

    public async Task<ProjectDesignSurfaceSerializationModel?> LoadDesignSurface()
    {

        DesignSurfaceViewModel!.AddManuscriptGreekEnabled = true;
        DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = true;
        DesignSurfaceViewModel.ProjectDesignSurfaceViewModel.LoadingDesignSurface = true;
        DesignSurfaceViewModel.ProjectDesignSurfaceViewModel.DesignSurfaceLoaded = false;
        Stopwatch sw = new();
        sw.Start();

        ProjectDesignSurfaceSerializationModel? designSurfaceData = null;
        try
        {
            designSurfaceData = LoadDesignSurfaceData();
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            // restore the nodes
            if (designSurfaceData != null)
            {
                foreach (var corpusId in topLevelProjectIds.CorpusIds)
                {
                    if (corpusId.CorpusType == CorpusType.ManuscriptHebrew.ToString())
                    {
                        DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = false;
                    }
                    else if (corpusId.CorpusType == CorpusType.ManuscriptGreek.ToString())
                    {
                        DesignSurfaceViewModel!.AddManuscriptGreekEnabled = false;
                    }

                    var corpus = new Corpus(corpusId);
                    var corpusNodeLocation = designSurfaceData.CorpusNodeLocations.FirstOrDefault(cn => cn.CorpusId == corpusId.Id);
                    var point = corpusNodeLocation != null ? new Point(corpusNodeLocation.X, corpusNodeLocation.Y) : new Point();
                    var node = DesignSurfaceViewModel!.CreateCorpusNode(corpus, point);
                    var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusId.Id);

                    DesignSurfaceViewModel!.CreateCorpusNodeMenu(node, tokenizedCorpora);
                }

                foreach (var parallelCorpusId in topLevelProjectIds.ParallelCorpusIds)
                {

                    var sourceNode = DesignSurfaceViewModel!.CorpusNodes.FirstOrDefault(p =>
                        p.ParatextProjectId == parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.ParatextGuid);
                    var targetNode = DesignSurfaceViewModel!.CorpusNodes.FirstOrDefault(p =>
                        p.ParatextProjectId == parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.ParatextGuid);


                    if (sourceNode is not null && targetNode is not null)
                    {
                        var connection = new ParallelCorpusConnectionViewModel
                        {
                            SourceConnector = sourceNode.OutputConnectors[0],
                            DestinationConnector = targetNode.InputConnectors[0],
                            ParallelCorpusDisplayName = parallelCorpusId.DisplayName,
                            ParallelCorpusId = parallelCorpusId,
                            SourceFontFamily = parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.FontFamily,
                            TargetFontFamily = parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.FontFamily,
                        };
                        DesignSurfaceViewModel.ParallelCorpusConnections.Add(connection);
                        // add in the context menu

                        // UnDO
                        //DesignSurfaceViewModel!.CreateConnectionMenu(connection, topLevelProjectIds, this);
                    }
                }

                
            }


        }
        finally
        {
            DesignSurfaceViewModel.ProjectDesignSurfaceViewModel.LoadingDesignSurface = false;
            DesignSurfaceViewModel.ProjectDesignSurfaceViewModel.DesignSurfaceLoaded = true;
            sw.Stop();

            Debug.WriteLine($"LoadCanvas took {sw.ElapsedMilliseconds} ms ({sw.Elapsed.Seconds} seconds)");
        }
        return designSurfaceData;
    }

    public ProjectDesignSurfaceSerializationModel? LoadDesignSurfaceData()
    {
        if (ProjectManager!.CurrentProject is null)
        {
            return null;
        }
        if (ProjectManager?.CurrentProject.DesignSurfaceLayout == "")
        {
            return null;
        }

        var json = ProjectManager?.CurrentProject.DesignSurfaceLayout;

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            IncludeFields = true,
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        return JsonSerializer.Deserialize<ProjectDesignSurfaceSerializationModel>(json, options);
    }

}