using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Converters;
using ClearDashboard.Wpf.Application.Enums;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Popups;
using ClearDashboard.Wpf.Application.ViewModels.PopUps;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;


// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{

    public class ProjectDesignSurfaceViewModel : DashboardConductorOneActive<Screen>, IProjectDesignSurfaceViewModel, IHandle<UiLanguageChangedMessage>, IDisposable, IHandle<RedrawParallelCorpusMenus>
    {
        #region Member Variables

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        //public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly IWindowManager? _windowManager;
        public readonly BackgroundTasksViewModel BackgroundTasksViewModel;
        private readonly LongRunningTaskManager? _longRunningTaskManager;
        private readonly ILocalizationService _localizationService;
        private readonly SystemPowerModes _systemPowerModes;

        private const string TaskName = "Alignment Deletion";

        #endregion //Member Variables

        #region Observable Properties

        public IEnhancedViewManager EnhancedViewManager { get; }

        public bool LoadingDesignSurface { get; set; }

        public bool DesignSurfaceLoaded { get; set; }

        public MainViewModel MainViewModel => (MainViewModel)Parent;

        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private DesignSurfaceViewModel? _designSurfaceViewModel;
        public DesignSurfaceViewModel? DesignSurfaceViewModel
        {
            get => _designSurfaceViewModel;
            private set => Set(ref _designSurfaceViewModel, value);
        }

        private object? _selectedDesignSurfaceComponent;
        public object? SelectedDesignSurfaceComponent
        {
            get
            {
                if (_selectedDesignSurfaceComponent is CorpusNodeViewModel node)
                {
                    foreach (var corpusNode in DesignSurfaceViewModel!.CorpusNodes)
                    {
                        if (corpusNode.ParatextProjectId == node.ParatextProjectId)
                        {
                            return corpusNode;
                        }
                    }
                }
                else if (_selectedDesignSurfaceComponent is ParallelCorpusConnectionViewModel conn)
                {
                    foreach (var connection in DesignSurfaceViewModel!.ParallelCorpusConnections)
                    {
                        if (connection.Id == conn.Id)
                        {
                            return connection;
                        }
                    }
                }
                else
                {
                    return _selectedDesignSurfaceComponent;
                }

#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
            }
            set => Set(ref _selectedDesignSurfaceComponent, value);
        }

        private string? _projectName;
        public string? ProjectName
        {
            get => _projectName;
            set => Set(ref _projectName, value);
        }

        #endregion //Observable Properties

        #region Constructor

        // Required for design-time binding
#pragma warning disable CS8618
        public ProjectDesignSurfaceViewModel()
#pragma warning restore CS8618
        {
            //no-op
        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService,
            IWindowManager windowManager,
            ILogger<ProjectDesignSurfaceViewModel> logger,
            DashboardProjectManager? projectManager,
            BackgroundTasksViewModel backgroundTasksViewModel,
            IEventAggregator? eventAggregator,
            SystemPowerModes systemPowerModes,
            IMediator mediator,
            ILifetimeScope lifetimeScope,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService,
            IEnhancedViewManager enhancedViewManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            EnhancedViewManager = enhancedViewManager;
            _systemPowerModes = systemPowerModes;
            _windowManager = windowManager;
            BackgroundTasksViewModel = backgroundTasksViewModel;
            _longRunningTaskManager = longRunningTaskManager;
            _localizationService = localizationService;

            EventAggregator.SubscribeOnUIThread(this);

        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            // NB:  This gets called after all of the AvalonDock tabs have been rendered so
            //      I have added the Initialize method below which gets called in 
            //      MainViewModel.OnActivate.ActivateDockedWindowViewModels
            await base.OnInitializeAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            EventAggregator!.Unsubscribe(this);
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // NB:  This gets called after all of the AvalonDock tabs have been rendered so
            //      I have added the Initialize method below which gets called in 
            //      MainViewModel.OnActivate.ActivateDockedWindowViewModels
            await base.OnActivateAsync(cancellationToken);
        }

        /// <summary>
        /// Activates the DesignSurfaceViewModel and draws the design surface based on data from Project table in the specific
        /// project database
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <remarks>NB:  This method is called explicitly from MainViewModel.OnActivate.ActivateDockedWindowViewModels</remarks>
        /// <returns></returns>
        public async Task Initialize(CancellationToken cancellationToken)
        {
            Items.Clear();
            EventAggregator.SubscribeOnUIThread(this);
            try
            {
                DesignSurfaceViewModel = await ActivateItemAsync<DesignSurfaceViewModel>(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex.Message, ex);
            }

            ProjectName = ProjectManager!.CurrentProject.ProjectName!;

            await DrawDesignSurface();

            _busyState.CollectionChanged += BusyStateOnCollectionChanged;
        }

        private void BusyStateOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsBusy);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
            EventAggregator!.Unsubscribe(this);
            await SaveDesignSurfaceData();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        private new async Task<TViewModel?> ActivateItemAsync<TViewModel>(CancellationToken cancellationToken = default)
            where TViewModel : Screen
        {

            // NOTE:  This is the hack to get OnViewAttached and OnViewReady methods to be called on conducted ViewModels.  Also note
            //   OnViewLoaded is not called.

            var viewModel = LifetimeScope!.Resolve<TViewModel>();

            viewModel.Parent = this;
            viewModel.ConductWith(this);

            // Binding ProjectDesignView to the the view model.  Note this is different
            // from other conductors where we bind to view/viewmodel pair.
            var view = ViewLocator.LocateForModel(this, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);

            return viewModel;
        }

        #endregion //Constructor


        #region Methods


        public async Task SaveDesignSurfaceData()
        {
            _ = await Task.Factory.StartNew(async () =>
            //_ = await Task.Run(async () =>

       {
           var json = SerializeDesignSurface();

           ProjectManager!.CurrentProject.DesignSurfaceLayout = json;

           Logger!.LogInformation($"DesignSurfaceLayout : {ProjectManager.CurrentProject.DesignSurfaceLayout}");

           try
           {
               await ProjectManager.UpdateProject(ProjectManager.CurrentProject);
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
                    CorpusName = corpusNode.Name
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

        public async Task DrawDesignSurface()
        {
            var resourceList = DesignSurfaceViewModel!.GetParatextResourceNames();

            Logger!.LogInformation($"Drawing design surface for project '{ProjectName}'.");
            DesignSurfaceViewModel!.AddManuscriptGreekEnabled = true;
            DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = true;
            LoadingDesignSurface = true;
            DesignSurfaceLoaded = false;

            var sw = Stopwatch.StartNew();

            try
            {
                //_ = await Task.Factory.StartNew(async () =>
                //{
                var designSurfaceData = LoadDesignSurfaceData();
                var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

                // restore the nodes
                if (designSurfaceData != null)
                {
                    bool currentParatextProjectPresent = false;
                    bool standardCorporaPresent = false;

                    foreach (var corpusId in topLevelProjectIds.CorpusIds.OrderBy(c => c.Created))
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
                        var corpusNodeLocation =
                            designSurfaceData.CorpusNodeLocations.FirstOrDefault(cn => cn.CorpusId == corpusId.Id);
                        var point = corpusNodeLocation != null
                            ? new Point(corpusNodeLocation.X, corpusNodeLocation.Y)
                            : new Point();

                        // check to see if this is a resource and not a Standard
                        if (corpus.CorpusId.CorpusType == CorpusType.Standard.ToString())
                        {
                            var resource = resourceList.FirstOrDefault(x => x == corpus.CorpusId.Name);
                            if (resource != null)
                            {
                                corpus.CorpusId.CorpusType = CorpusType.Resource.ToString();
                            }
                            else
                            {
                                standardCorporaPresent = true;
                                if (corpus.CorpusId.ParatextGuid == ProjectManager.CurrentParatextProject.Guid)
                                {
                                    currentParatextProjectPresent = true;
                                }
                            }
                        }
                        
                        var node = DesignSurfaceViewModel!.CreateCorpusNode(corpus, point);
                        var tokenizedCorpora =
                            topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusId.Id);

                        await DesignSurfaceViewModel!.CreateCorpusNodeMenu(node, tokenizedCorpora);
                    }

                    if (standardCorporaPresent  && !currentParatextProjectPresent)
                    {
                        var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

                        if (confirmationViewPopupViewModel == null)
                        {
                            throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
                        }

                        confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.SwitchParatextProjectMessage;

                        var result = await _windowManager!.ShowDialogAsync(confirmationViewPopupViewModel, null,
                            SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));
                    }


                    DesignSurfaceViewModel.ProjectDesignSurface!.InvalidateArrange();
                    //DesignSurfaceViewModel.ProjectDesignSurface!.UpdateLayout();


                    foreach (var parallelCorpusId in topLevelProjectIds.ParallelCorpusIds)
                    {

                        var sourceNode = DesignSurfaceViewModel!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId ==
                            parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.ParatextGuid);
                        var targetNode = DesignSurfaceViewModel!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId ==
                            parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.ParatextGuid);


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
                            DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(connection, topLevelProjectIds);
                        }
                    }
                }

                //});
            }
            catch (Exception e)
            {
                Logger!.LogError(e.Message, e);
            }
            finally
            {
                LoadingDesignSurface = false;
                DesignSurfaceLoaded = true;
                sw.Stop();

                Debug.WriteLine($"LoadCanvas took {sw.ElapsedMilliseconds} ms ({sw.Elapsed.Seconds} seconds)");
            }

        }


        private ProjectDesignSurfaceSerializationModel? LoadDesignSurfaceData()
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


        private readonly ObservableDictionary<string, bool> _busyState = new();

        public new bool IsBusy => _busyState.Count > 0;


        // ReSharper disable once UnusedMember.Global
        public async void AddManuscriptHebrewCorpus()
        {
            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }


            Logger!.LogInformation("AddManuscriptHebrewCorpus called.");

            var taskName = MaculaCorporaNames.HebrewCorpusName;

            DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = false;

            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken,
                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.H)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var books = BookInfo.GenerateScriptureBookList()
                .Where(bi => sourceCorpus.Texts
                    .Select(t => t.Id)
                    .Contains(bi.Code))
                .ToList();

            var metadata = new ParatextProjectMetadata
            {
                Id = ManuscriptIds.HebrewManuscriptId,
                CorpusType = CorpusType.ManuscriptHebrew,
                Name = MaculaCorporaNames.HebrewCorpusName,
                AvailableBooks = books,
            };


            _ = Task.Run(async () =>
            // _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);
                CorpusNodeViewModel corpusNode = new();
                var soundType = SoundType.Success;

                try
                {

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken,
                        backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    var corpus = await Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: true,
                        Name: MaculaCorporaNames.HebrewCorpusName,
                        Language: "he",
                        CorpusType: CorpusType.ManuscriptHebrew.ToString(),
                        ParatextId: ManuscriptIds.HebrewManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = FontNames.HebrewFontFamily;
                    corpusNode = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());



                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                    cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    // ReSharper disable once UnusedVariable
                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator!, corpus.CorpusId,
                        MaculaCorporaNames.HebrewCorpusName,
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        ScrVers.Original,
                        cancellationToken,
                        true);

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    await DesignSurfaceViewModel!.UpdateNodeTokenization(corpusNode);


                }
                catch (OperationCanceledException)
                {
                    Logger!.LogInformation($"AddManuscriptHebrewCorpus - operation canceled.");
                }
                catch (MediatorErrorEngineException ex)
                {
                    if (ex.Message.Contains("The operation was canceled."))
                    {
                        Logger!.LogInformation($"AddManuscriptHebrewCorpus - operation canceled.");
                    }
                    else
                    {
                        Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                    }

                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        soundType = SoundType.Error;
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                           exception: ex, cancellationToken: cancellationToken,
                           backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DeleteCorpusNode(corpusNode, true);
                        // What other work needs to be done?  how do we know which steps have been executed?
                        DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(soundType);
                    }

                    // check to see if there are still High Performance Tasks still out there
                    var numTasks = BackgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                    if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                    {
                        // shut down high performance mode
                        await _systemPowerModes.TurnOffHighPerformanceMode();
                    }

                }
            }, cancellationToken);


        }

        public async void AddManuscriptGreekCorpus()
        {
            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }


            Logger!.LogInformation("AddGreekCorpus called.");

            DesignSurfaceViewModel!.AddManuscriptGreekEnabled = false;

            var taskName = MaculaCorporaNames.GreekCorpusName;
            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;


            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken,
                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);


            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.G)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var books = BookInfo.GenerateScriptureBookList()
                .Where(bi => sourceCorpus.Texts
                    .Select(t => t.Id)
                    .Contains(bi.Code))
                .ToList();

            var metadata = new ParatextProjectMetadata
            {
                Id = ManuscriptIds.GreekManuscriptId,
                CorpusType = CorpusType.ManuscriptGreek,
                Name = MaculaCorporaNames.GreekCorpusName,
                AvailableBooks = books,
            };

            _ = Task.Run(async () =>
            //_ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);
                var soundType = SoundType.Success;
                CorpusNodeViewModel corpusNode = new();

                try
                {
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken,
                        backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    var corpus = await Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: false,
                        Name: MaculaCorporaNames.GreekCorpusName,
                        Language: "el",
                        CorpusType: CorpusType.ManuscriptGreek.ToString(),
                        ParatextId: ManuscriptIds.GreekManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = FontNames.GreekFontFamily;
                    corpusNode = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                        cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    // ReSharper disable once UnusedVariable
                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator!, corpus.CorpusId,
                        MaculaCorporaNames.GreekCorpusName,
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        ScrVers.Original,
                        cancellationToken,
                        true);

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                    await DesignSurfaceViewModel!.UpdateNodeTokenization(corpusNode);

                }
                catch (OperationCanceledException)
                {
                    Logger!.LogInformation($"AddManuscriptGreekCorpus - operation canceled.");
                }
                catch (MediatorErrorEngineException ex)
                {
                    if (ex.Message.Contains("The operation was canceled."))
                    {
                        Logger!.LogInformation($"AddManuscriptGreekCorpus - operation canceled.");
                    }
                    else
                    {
                        Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                    }

                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {metadata.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        soundType = SoundType.Error;
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                            exception: ex, cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DeleteCorpusNode(corpusNode, true);
                        DesignSurfaceViewModel!.AddManuscriptGreekEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(soundType);
                    }

                    // check to see if there are still High Performance Tasks still out there
                    var numTasks = BackgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                    if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                    {
                        // shut down high performance mode
                        await _systemPowerModes.TurnOffHighPerformanceMode();
                    }
                }
            }, cancellationToken);
        }

        public async Task AddUsfmCorpus()
        {
            // TODO:  need to complete implementation
            await Task.CompletedTask;
        }

        public async Task AddParatextCorpus()
        {
            await AddParatextCorpus("");
        }

        public async Task AddParatextCorpus(string selectedParatextProjectId, ParallelProjectType parallelProjectType = ParallelProjectType.WholeProcess)
        {
            Logger!.LogInformation("AddParatextCorpus called.");

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Add),
                new NamedParameter("initialParatextProjectId", selectedParatextProjectId)
            };

            var dialogViewModel = LifetimeScope?.Resolve<ParatextCorpusDialogViewModel>(parameters);

            try
            {
                var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    // check to see if we want to run this in High Performance mode
                    if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
                    {
                        await _systemPowerModes.TurnOnHighPerformanceMode();
                    }

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    var selectedProject = dialogViewModel!.SelectedProject;
                    var bookIds = dialogViewModel.BookIds;
                    var taskName = $"{selectedProject!.Name}";
                    _busyState.Add(taskName, true);

                    var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
                    var cancellationToken = task.CancellationTokenSource!.Token;

                    _ = Task.Run(async () =>
                    //_ = await Task.Factory.StartNew(async () =>
                    {
                        CorpusNodeViewModel node = new()
                        {
                            TranslationFontFamily = selectedProject.FontFamily
                        };

                        if (DesignSurfaceViewModel!.CorpusNodes.Any(cn => cn.ParatextProjectId == selectedProject.Id))
                        {
                            node = DesignSurfaceViewModel!.CorpusNodes.Single(cn => cn.ParatextProjectId == selectedProject.Id);
                        }

                        var soundType = SoundType.Success;

                        try
                        {
                            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                            var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.ParatextGuid == selectedProject.Id);
                            var corpus = corpusId != null ? await Corpus.Get(Mediator!, corpusId) : null;

                            // first time for this corpus
                            if (corpus is null)
                            {
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                                    description: $"Creating corpus '{selectedProject.Name}'...", cancellationToken: cancellationToken,
                                    backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
#pragma warning disable CS8604
                                corpus = await Corpus.Create(
                                    mediator: Mediator,
                                    IsRtl: selectedProject.IsRtl,
                                    FontFamily: selectedProject.FontFamily,
                                    Name: selectedProject.Name,
                                    Language: selectedProject.LanguageId,
                                    CorpusType: selectedProject.CorpusTypeDisplay,
                                    ParatextId: selectedProject.Id,
                                    token: cancellationToken);

                                corpus.CorpusId.FontFamily = selectedProject.FontFamily;

                                node = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());
#pragma warning restore CS8604
                            }



                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                                description: $"Tokenizing and transforming '{selectedProject.Name}' corpus...", cancellationToken: cancellationToken,
                                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                            var textCorpus = dialogViewModel.SelectedTokenizer switch
                            {
                                Tokenizers.LatinWordTokenizer =>
                                    (await ParatextProjectTextCorpus.Get(Mediator!, selectedProject.Id!, bookIds, cancellationToken))
                                    .Tokenize<LatinWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>()
                                    .Transform<SetTrainingBySurfaceLowercase>(),
                                Tokenizers.WhitespaceTokenizer =>
                                    (await ParatextProjectTextCorpus.Get(Mediator!, selectedProject.Id!, bookIds, cancellationToken))
                                    .Tokenize<WhitespaceTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>()
                                    .Transform<SetTrainingBySurfaceLowercase>(),
                                Tokenizers.ZwspWordTokenizer => (await ParatextProjectTextCorpus.Get(Mediator!, selectedProject.Id!, bookIds, cancellationToken))
                                    .Tokenize<ZwspWordTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>()
                                    .Transform<SetTrainingBySurfaceLowercase>(),
                                _ => (await ParatextProjectTextCorpus.Get(Mediator!, selectedProject.Id!, null, cancellationToken))
                                    .Tokenize<WhitespaceTokenizer>()
                                    .Transform<IntoTokensTextRowProcessor>()
                                    .Transform<SetTrainingBySurfaceLowercase>()
                            };

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                                description: $"Creating tokenized text corpus for '{selectedProject.Name}' corpus...",
                                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

#pragma warning disable CS8604
                            // ReSharper disable once UnusedVariable
                            var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                                selectedProject.Name, dialogViewModel.SelectedTokenizer.ToString(), selectedProject.ScrVers, cancellationToken);
#pragma warning restore CS8604


                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                                description: $"Creating tokenized text corpus for '{selectedProject.Name}' corpus...Completed",
                                cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                            await DesignSurfaceViewModel!.UpdateNodeTokenization(node);

                            _longRunningTaskManager.TaskComplete(taskName);
                        }
                        catch (OperationCanceledException)
                        {
                            Logger!.LogInformation("AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                        }
                        catch (MediatorErrorEngineException ex)
                        {
                            if (ex.Message.Contains("The operation was canceled"))
                            {
                                Logger!.LogInformation($"AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                            }
                            else
                            {
                                Logger!.LogError(ex, "an unexpected Engine exception was thrown.");
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    soundType = SoundType.Error;
                                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                        exception: ex, cancellationToken: cancellationToken,
                                        backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            Logger!.LogError(ex, $"An unexpected error occurred while creating the corpus for {selectedProject.Name} ");
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                soundType = SoundType.Error;
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                    exception: ex, cancellationToken: cancellationToken,
                                    backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                            }
                        }
                        finally
                        {
                            _longRunningTaskManager.TaskComplete(taskName);
                            _busyState.Remove(taskName);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                DeleteCorpusNode(node, true);
                            }
                            else
                            {
                                sw.Stop();

                                PlaySound.PlaySoundFromResource(soundType);
                            }

                            // check to see if there are still High Performance Tasks still out there
                            var numTasks = BackgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                            if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                            {
                                // shut down high performance mode
                                await _systemPowerModes.TurnOffHighPerformanceMode();
                            }

                        }
                    }, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                await SaveDesignSurfaceData();
            }
        }

        private async Task AddNewInterlinear(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connection = DesignSurfaceViewModel!.ParallelCorpusConnections.FirstOrDefault(x => x.Id == connectionMenuItem.ConnectionId);

            ParallelCorpusId parallelCorpusId;
            try
            {
                parallelCorpusId = new ParallelCorpusId(connection!.ParallelCorpusId!.Id,
                    new TokenizedTextCorpusId(connection.ParallelCorpusId!.SourceTokenizedCorpusId!.CorpusId!.Id),
                    new TokenizedTextCorpusId(connection.ParallelCorpusId!.TargetTokenizedCorpusId!.CorpusId!.Id), DisplayName,
                    new Dictionary<string, object>(), new DateTimeOffset(), connection.ParallelCorpusId!.UserId!);

                var parameters = new List<Autofac.Core.Parameter>
                {
                    new NamedParameter("parallelCorpusId", parallelCorpusId),
                };

                var dialogViewModel = LifetimeScope!.Resolve<InterlinearDialogViewModel>(parameters);

                var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null, DialogSettings.AddNewInterlinearDialogSettings);

                if (result)
                {
                    try
                    {
                        var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                        DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(connection, topLevelProjectIds);
                        await SaveDesignSurfaceData();

                    }
                    catch (Exception ex)
                    {
                        Logger!.LogError(ex, $"An unexpected error occurred while adding the interlinear for {connectionMenuItem.ParallelCorpusId!}");
                    }

                }

            }
            catch (Exception e)
            {
                Logger!.LogError(e, $"An unexpected error occurred while adding the interlinear for {connectionMenuItem.ParallelCorpusId!}");
            }
        }

        private async Task AddNewAlignment(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connection = DesignSurfaceViewModel!.ParallelCorpusConnections.FirstOrDefault(c => c.Id == connectionMenuItem.ConnectionId);
            await AddParallelCorpus(connection!, ParallelProjectType.AlignmentOnly);
        }

        private async Task ResetVerseVersification(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var localizedString = _localizationService!["Migrate_Header"];

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<MigrateDatabaseViewModel>();
            viewModel.Project = null;
            viewModel.ProjectPickerViewModel = null;
            viewModel.ParallelId = connectionMenuItem.ConnectionId;

            IWindowManager manager = new WindowManager();
            manager.ShowDialogAsync(viewModel, null, settings);
        }

        public async Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection,
            ParallelProjectType parallelProjectType = ParallelProjectType.WholeProcess)
        {
            var sourceCorpusNode = DesignSurfaceViewModel!.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.SourceConnector!.ParentNode!.Id);
            if (sourceCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the source Corpus node for the Corpus with Id '{newParallelCorpusConnection.SourceConnector!.ParentNode!.CorpusId}'.");
            }
            var targetCorpusNode = DesignSurfaceViewModel.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.DestinationConnector!.ParentNode!.Id);
            if (targetCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the target Corpus node for the Corpus with Id '{newParallelCorpusConnection.DestinationConnector!.ParentNode!.CorpusId}'.");
            }

            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds;
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Add),
                new NamedParameter("connectionViewModel", newParallelCorpusConnection),
                new NamedParameter("sourceCorpusNodeViewModel", sourceCorpusNode),
                new NamedParameter("targetCorpusNodeViewModel", targetCorpusNode),
                new NamedParameter("tokenizedCorpora", tokenizedCorpora),
                new NamedParameter("parallelProjectType", parallelProjectType)
            };

            var dialogViewModel = LifetimeScope?.Resolve<ParallelCorpusDialogViewModel>(parameters);

            try
            {
                var success =
                    await _windowManager!.ShowDialogAsync(dialogViewModel, null,
                        DialogSettings.NewProjectDialogSettings);

                PlaySound.PlaySoundFromResource();

                if (success)
                {
                    // get TranslationSet , etc from the dialogViewModel
                    //var translationSet = dialogViewModel!.TranslationSet;


                    if (parallelProjectType == ParallelProjectType.WholeProcess)
                    {
                        newParallelCorpusConnection.ParallelCorpusId =
                            dialogViewModel!.ParallelTokenizedCorpus.ParallelCorpusId;
                        newParallelCorpusConnection.ParallelCorpusDisplayName =
                            dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId.DisplayName;
                    }

                    topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(newParallelCorpusConnection,
                        topLevelProjectIds);

                }
                else
                {
                    if (dialogViewModel!.ProjectType == ParallelProjectType.WholeProcess)
                    {
                        DesignSurfaceViewModel!.DeleteParallelCorpusConnection(newParallelCorpusConnection, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await SaveDesignSurfaceData();
            }
        }

        public async Task UpdateParatextCorpus(string selectedParatextProjectId, string? selectedTokenizer)
        {
            Logger!.LogInformation("UpdateParatextCorpus called.");

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Edit),
                new NamedParameter("paratextProjectId", selectedParatextProjectId)
            };

            if (!Enum.TryParse(selectedTokenizer, out Tokenizers tokenizer))
            {
                Logger!.LogError($"UpdateParatextCorups received an invalid selectedTokenizer value '{selectedTokenizer}'");
                throw new ArgumentException($@"Unable to parse value as Enum '{selectedTokenizer}'", nameof(selectedTokenizer));
            }

            var dialogViewModel = LifetimeScope?.Resolve<UpdateParatextCorpusDialogViewModel>(parameters);

            try
            {
                var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    var selectedProject = dialogViewModel!.SelectedProject;
                    var bookIds = dialogViewModel.BookIds;

                    var taskName = $"{selectedProject!.Name}";
                    _busyState.Add(taskName, true);

                    var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
                    var cancellationToken = task.CancellationTokenSource!.Token;

                    _ = Task.Run(async () =>
                    {
                        var soundType = SoundType.Success;
                        try
                        {
                            var node = DesignSurfaceViewModel!.CorpusNodes
                                .Single(cn => cn.ParatextProjectId == selectedProject.Id);

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Tokenizing and transforming '{selectedProject.Name}' corpus...", cancellationToken: cancellationToken);

                            var textCorpus = await GetTokenizedTransformedParatextProjectTextCorpus(
                                selectedProject.Id!,
                                bookIds,
                                tokenizer,
                                cancellationToken
                            );

                            var tokenizedTextCorpusId = (await TokenizedTextCorpus.GetAllTokenizedCorpusIds(
                                    Mediator!,
                                    new CorpusId(node.CorpusId)))
                                .FirstOrDefault(tc => tc.TokenizationFunction == tokenizer.ToString());

                            if (tokenizedTextCorpusId is null)
                            {
                                throw new ArgumentException($"No TokenizedTextCorpusId found for corpus '{node.CorpusId}' and tokenization '{tokenizer.ToString()}'");
                            }

                            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, tokenizedTextCorpusId);
                            await tokenizedTextCorpus.UpdateOrAddVerses(Mediator!, textCorpus, cancellationToken);

                            //await EventAggregator.PublishOnUIThreadAsync(new ReloadDataMessage(ReloadType.Force), cancellationToken);

                            await EventAggregator.PublishOnUIThreadAsync(new TokenizedCorpusUpdatedMessage(tokenizedTextCorpusId), cancellationToken);

                            _longRunningTaskManager.TaskComplete(taskName);

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                                description: $"Updating verses in tokenized text corpus for '{selectedProject.Name}' corpus...Completed", cancellationToken: cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            Logger!.LogInformation("UpdateParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                        }
                        catch (MediatorErrorEngineException ex)
                        {
                            if (ex.Message.Contains("The operation was canceled"))
                            {
                                Logger!.LogInformation($"UpdateParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                            }
                            else
                            {
                                Logger!.LogError(ex, "an unexpected Engine exception was thrown.");
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {selectedProject.Name} ");
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                soundType = SoundType.Error;
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                   exception: ex, cancellationToken: cancellationToken);
                            }
                        }
                        finally
                        {
                            _longRunningTaskManager.TaskComplete(taskName);
                            _busyState.Remove(taskName);

                            PlaySound.PlaySoundFromResource(soundType);
                        }
                    }, cancellationToken);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                await SaveDesignSurfaceData();
            }

        }

        private static ITokenizer<string, int, string> InstantiateTokenizer(Tokenizers tokenizerEnum)
        {
            var assemblyTokenizerType = typeof(LatinWordTokenizer);
            var assembly = assemblyTokenizerType.Assembly;
            var tokenizerType = assembly.GetType($"{assemblyTokenizerType.Namespace}.{tokenizerEnum}");

            if (tokenizerType is null)
            {
                throw new ArgumentException($"Tokenizer '{tokenizerEnum}' not a valid class in the '{assemblyTokenizerType.Namespace}' namespace");
            }

            var tokenizer = (ITokenizer<string, int, string>)Activator.CreateInstance(tokenizerType)!;
            return tokenizer;
        }

        private async Task<ITextCorpus> GetTokenizedTransformedParatextProjectTextCorpus(
            string paratextProjectId,
            IEnumerable<string>? bookIds,
            Tokenizers tokenizerEnum,
            CancellationToken cancellationToken)
        {
            var paratextProjectTextCorpus = await ParatextProjectTextCorpus.Get(
                Mediator!,
                paratextProjectId,
                bookIds,
                cancellationToken);

            var tokenizer = InstantiateTokenizer(tokenizerEnum);

            var textCorpus = paratextProjectTextCorpus
               .Tokenize(tokenizer)
               .Transform<IntoTokensTextRowProcessor>()
               .Transform<SetTrainingBySurfaceLowercase>();

            return textCorpus;
        }

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status,
            CancellationToken cancellationToken, string? description = null, Exception? exception = null,
            BackgroundTaskMode backgroundTaskMode = BackgroundTaskMode.Normal)
        {
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status,
                BackgroundTaskType = backgroundTaskMode,
                BackgroundTaskSource = typeof(ProjectDesignSurfaceViewModel)
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }


        public async Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connectionViewModel = connectionMenuItem.ConnectionViewModel;
            switch (connectionMenuItem.Id)
            {

                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentsBatchReviewViewToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentsBatchReviewViewToNewEnhancedView:
                    //await AddAlignmentsBatchReview(connectionMenuItem);
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EnhancedViewManager.AddMetadatumEnhancedView(new AlignmentEnhancedViewItemMetadatum
                        {
                            AlignmentSetId = connectionMenuItem.AlignmentSetId,
                            DisplayName = connectionMenuItem.DisplayName,
                            ParallelCorpusId = connectionMenuItem.ParallelCorpusId ??
                                               throw new InvalidDataEngineException(name: "ParallelCorpusId",
                                                   value: "null"),
                            ParallelCorpusDisplayName = $"{connectionMenuItem.ParallelCorpusDisplayName} [{connectionMenuItem.SmtModel}]",
                            //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsRtl = connectionMenuItem.IsRtl,
                            //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsTargetRtl = connectionMenuItem.IsTargetRtl,
                            IsNewWindow = connectionMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds
                                .AddAlignmentsBatchReviewViewToNewEnhancedView,
                            SourceParatextId = connectionMenuItem.SourceParatextId,
                            TargetParatextId = connectionMenuItem.TargetParatextId,
                            EditMode = EditMode.EditorViewOnly
                        }, CancellationToken.None);
                    }
                    Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.AlignmentViewAddedCount, 1);
                    break;

                    break;

                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddTranslationSet:
                    // find the right connection to send
                    var connection = DesignSurfaceViewModel!.ParallelCorpusConnections.FirstOrDefault(c => c.Id == connectionMenuItem.ConnectionId);

                    if (connection != null)
                    {
                        // kick off the add new tokenization dialog
                        await AddParallelCorpus(connection);
                    }
                    else
                    {
                        Logger!.LogError("Could not find connection with id {0}", connectionMenuItem.ConnectionId);
                    }
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.Separator:
                    // no-op
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.ShowParallelCorpusProperties:
                    // node properties
                    SelectedDesignSurfaceComponent = connectionViewModel;
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.CreateNewAlignmentSet:
                    await AddNewAlignment(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.CreateNewInterlinear:
                    await AddNewInterlinear(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.ResetVerseVersifications:
                    await ResetVerseVersification(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentSetToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentSetToNewEnhancedView:
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EnhancedViewManager.AddMetadatumEnhancedView(new AlignmentEnhancedViewItemMetadatum
                        {
                            AlignmentSetId = connectionMenuItem.AlignmentSetId,
                            DisplayName = connectionMenuItem.DisplayName,
                            ParallelCorpusId = connectionMenuItem.ParallelCorpusId ??
                                               throw new InvalidDataEngineException(name: "ParallelCorpusId",
                                                   value: "null"),
                            ParallelCorpusDisplayName = $"{connectionMenuItem.ParallelCorpusDisplayName} [{connectionMenuItem.SmtModel}]",
                            //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsRtl = connectionMenuItem.IsRtl,
                            //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsTargetRtl = connectionMenuItem.IsTargetRtl,
                            IsNewWindow = connectionMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds
                                .AddAlignmentSetToNewEnhancedView,
                            SourceParatextId = connectionMenuItem.SourceParatextId,
                            TargetParatextId = connectionMenuItem.TargetParatextId
                        }, CancellationToken.None);
                    }
                    Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.AlignmentViewAddedCount, 1);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.DeleteAlignmentSet:
                    await DeleteAlignmentSet(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.DeleteTranslationSet:
                    await DeleteTranslationSet(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddInterlinearToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddInterlinearToNewEnhancedView:
                    if (connectionMenuItem.IsEnabled)
                    {

                        await EnhancedViewManager.AddMetadatumEnhancedView(new InterlinearEnhancedViewItemMetadatum
                        {

                            TranslationSetId = connectionMenuItem.TranslationSetId,
                            DisplayName = connectionMenuItem.DisplayName,
                            ParallelCorpusId = connectionMenuItem.ParallelCorpusId ??
                                                   throw new InvalidDataEngineException(name: "ParallelCorpusId",
                                                       value: "null"),
                            ParallelCorpusDisplayName = connectionMenuItem.ParallelCorpusDisplayName,
                            //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsRtl = connectionMenuItem.IsRtl,
                            //FIXME:surface serialization null,
                            IsNewWindow = connectionMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds
                                    .AddInterlinearToNewEnhancedView,
                            IsTargetRtl = connectionMenuItem.IsTargetRtl,
                            SourceParatextId = connectionMenuItem.SourceParatextId,
                            TargetParatextId = connectionMenuItem.TargetParatextId
                        }, CancellationToken.None
                        );
                        Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.InterlinearViewAddedCount, 1);
                    }
                    break;
                //default:
                //    //no-op
                //    break;
            }
        }

        private async Task DeleteTranslationSet(ParallelCorpusConnectionMenuItemViewModel parallelCorpusConnectionMenuItemViewModel)
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            var translationSetId = topLevelProjectIds.TranslationSetIds.FirstOrDefault(x => x.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.TranslationSetId);

            if (translationSetId != null)
            {
                DesignSurfaceViewModel!.DeleteTranslationFromMenus(translationSetId);

#pragma warning disable CS4014
                Task.Factory.StartNew(async () =>
                {
                    await DAL.Alignment.Translation.TranslationSet.Delete(Mediator!, translationSetId);

                    topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

                    foreach (var parallel in DesignSurfaceViewModel.ParallelCorpusConnections)
                    {
                        DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(parallel, topLevelProjectIds);
                    }
                });
#pragma warning restore CS4014
            }
        }

        private async Task DeleteAlignmentSet(
            ParallelCorpusConnectionMenuItemViewModel parallelCorpusConnectionMenuItemViewModel)
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            var alignmentSetId = topLevelProjectIds.AlignmentSetIds.FirstOrDefault(x =>

                x.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.AlignmentSetId
            );

            if (alignmentSetId != null)
            {
                // see if this is the last one or not
                var alignmentSetIdCount = topLevelProjectIds.AlignmentSetIds.Where(x =>
                    x.ParallelCorpusId!.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.ParallelCorpusId).ToList();

                var parallelCorpusConnectionViewModel = DesignSurfaceViewModel!.ParallelCorpusConnections.FirstOrDefault(x =>
                    x.ParallelCorpusId!.Id.ToString() == parallelCorpusConnectionMenuItemViewModel.ParallelCorpusId);

                if (alignmentSetIdCount.Count == 1)
                {

                    if (parallelCorpusConnectionViewModel != null)
                    {
                        // kill off the whole parallel line
                        DeleteParallelCorpusConnection(parallelCorpusConnectionViewModel);
                        return;
                    }
                }

                // remove any related translation sets
                var translationSetId =
                    topLevelProjectIds.TranslationSetIds.FirstOrDefault(x => x.AlignmentSetGuid == alignmentSetId.Id);
                if (translationSetId is not null)
                {
                    DesignSurfaceViewModel!.DeleteTranslationFromMenus(translationSetId);
                    //note that the translation set gets deleted automatically in the database
                    //by the AlignmentSet.Delete() method below since it is linked.
                }

                DesignSurfaceViewModel!.DeleteAlignmentFromMenus(alignmentSetId);

#pragma warning disable CS4014
                Task.Factory.StartNew(async () =>
                {
                    await AlignmentSet.Delete(Mediator!, alignmentSetId);

                    topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(parallelCorpusConnectionViewModel, topLevelProjectIds);
                });
#pragma warning restore CS4014
            }


        }


        public async Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem)
        {
            var corpusNodeViewModel = corpusNodeMenuItem.CorpusNodeViewModel;
            if (corpusNodeViewModel == null)
            {
                Logger!.LogInformation($"The CorpusNodeViewModel on the CorpusNodeMenuItem: '{corpusNodeMenuItem.Id}' is null., Returning...");
                return;
            }

            switch (corpusNodeMenuItem.Id)
            {
             
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddParatextCorpus:
                    // kick off the add new tokenization dialog
                    await AddParatextCorpus(corpusNodeViewModel.ParatextProjectId);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.Separator:
                    // no-op
                    break;

                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddTokenizedCorpusToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddTokenizedCorpusToNewEnhancedView:

                    var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    var tokenizedCorpus =
                        topLevelProjectIds.TokenizedTextCorpusIds.FirstOrDefault(tc =>
                            tc.CorpusId!.Id == corpusNodeViewModel.CorpusId && tc.TokenizationFunction == corpusNodeMenuItem.Tokenizer);

                    if (tokenizedCorpus == null)
                    {
                        Logger!.LogDebug($"Could not locate a TokenizedCorpus with a TokenizationFunction: '{corpusNodeMenuItem.Tokenizer}'.");
                        return;
                    }

                    await EnhancedViewManager.AddMetadatumEnhancedView(new TokenizedCorpusEnhancedViewItemMetadatum
                    {
                        ParatextProjectId = corpusNodeViewModel.ParatextProjectId,
                        ProjectName = corpusNodeViewModel.Name,
                        TokenizationType = corpusNodeMenuItem.Tokenizer!,
                        CorpusId = tokenizedCorpus.CorpusId!.Id,
                        TokenizedTextCorpusId = tokenizedCorpus.Id,
                        CorpusType = corpusNodeViewModel.CorpusType,
                        //FIXME:new EngineStringDetokenizer(new LatinWordDetokenizer()),
                        IsRtl = corpusNodeViewModel.IsRtl,
                        IsNewWindow = corpusNodeMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds.AddTokenizedCorpusToNewEnhancedView,
                        DisplayName = corpusNodeViewModel.Name + " (" + corpusNodeMenuItem.Tokenizer! + ")"
                    }, CancellationToken.None);
                    Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.VerseViewAddedCount, 1);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.ShowCorpusNodeProperties:
                    // node properties
                    SelectedDesignSurfaceComponent = corpusNodeViewModel;
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.ShowTokenizationProperties:
                    //TODO:  Fix this

                    // get the selected tokenizer
                    //                    var nodeTokenization =
                    //                        corpusNodeViewModel.Tokenizations.FirstOrDefault(b =>
                    //                            b.TokenizationName == corpusNodeMenuItem.Tokenizer);
                    //#pragma warning disable CS8601
                    //SelectedDesignSurfaceComponent = nodeTokenization;
                    //#pragma warning restore CS8601
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.UpdateParatextCorpus:
                    await UpdateParatextCorpus(corpusNodeViewModel.ParatextProjectId, corpusNodeMenuItem.Tokenizer);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.ShowLexiconDialog:
                    await ShowLexiconDialog(corpusNodeViewModel.CorpusId);
                    break;
            }
        }

        private async Task ShowLexiconDialog(Guid corpusId)
        {
            var localizedString = _localizationService!.Get("LexiconImport_Title");

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.MinWidth = 500;
            settings.MinHeight = 500;
            settings.Title = $"{localizedString}";

            var viewModel = IoC.Get<LexiconImportsViewModel>();
            viewModel.SelectedProjectId = corpusId;

            IWindowManager manager = new WindowManager();
            await manager.ShowDialogAsync(viewModel, null, settings);
        }


        public void ShowCorpusProperties(CorpusNodeViewModel corpus)
        {
            SelectedDesignSurfaceComponent = corpus;
        }

        public void ShowConnectionProperties(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            SelectedDesignSurfaceComponent = parallelCorpusConnection;
        }

        public async void DeleteParallelCorpusConnection(ParallelCorpusConnectionViewModel connection)
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);


            var alignmentSetIds = topLevelProjectIds.AlignmentSetIds.Where(x => x.ParallelCorpusId == connection.ParallelCorpusId).ToList();

            var translationSetIds = topLevelProjectIds.TranslationSetIds.Where(x => x.ParallelCorpusId == connection.ParallelCorpusId).ToList();


            if (alignmentSetIds.Count > 1 || translationSetIds.Count > 1)
            {
                // show the warning dialog if there are multiple alignment sets that are going to be deleted
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                settings.ResizeMode = ResizeMode.CanResize;
                settings.MinWidth = 800;
                settings.MinHeight = 600;
                settings.Title = "Delete Alignments/Interlinears";

                var viewModel = IoC.Get<DeleteParallelizationLineViewModel>();
                viewModel.AlignmentSetIds = alignmentSetIds;
                viewModel.TranslationSetIds = translationSetIds;
                viewModel.DesignSurfaceViewModel = DesignSurfaceViewModel!;

                IWindowManager manager = new WindowManager();
                var ret = manager.ShowDialogAsync(viewModel, null, settings);

                if ((bool)ret.GetType().GetProperty("Result").GetValue(ret, null) == false)
                {
                    // cancelled by user
                    return;
                }
            }


            // Removes the connector between corpus nodes:
            DesignSurfaceViewModel!.DeleteParallelCorpusConnection(connection);

            await Task.Factory.StartNew(async () =>
            {
                // ****************************************************************************
                // MICHAEL: not sure what null checking (if any) needs to happen with 
                // connection.ParallelCorpusId.  Also, this method will accept a third
                // CancellationToken argument if you have one available here.
                //
                // If ParallelCorpusId is invalid/doesn't exist, this will throw an
                // exception - do you want to catch it here or let it bubble out?
                // ****************************************************************************
                if (connection.ParallelCorpusId is not null)
                {
                    var task = _longRunningTaskManager.Create(TaskName, LongRunningTaskStatus.Running);
                    var cancellationToken = task.CancellationTokenSource!.Token;
                    // send to the task started event aggregator for everyone else to hear about a background task starting
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = TaskName,
                        Description = "Deleting Alignment Data...",
                        StartTime = DateTime.Now,
                        TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
                    }), cancellationToken);

                    await DAL.Alignment.Corpora.ParallelCorpus.Delete(Mediator!, connection.ParallelCorpusId);

                    _longRunningTaskManager.TaskComplete(TaskName);
                    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                    {
                        Name = TaskName,
                        Description = "Deleting Alignment Data...",
                        StartTime = DateTime.Now,
                        TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
                    }), cancellationToken);
                }
            });
        }

        public async void DeleteCorpusNode(CorpusNodeViewModel node, bool wasTokenizing)
        {
            // check to see if is in the middle of working or not by tokenizing
            var isCorpusProcessing = BackgroundTasksViewModel.CheckBackgroundProcessForTokenizationInProgressIgnoreCompletedOrFailedOrCancelled(node.Name);
            if (isCorpusProcessing)
            {
                await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
                {
                    Name = node.Name,
                    Description = "Tokenization Cancelled",
                    StartTime = DateTime.Now,
                    TaskLongRunningProcessStatus = LongRunningTaskStatus.CancellationRequested
                }));
                return;
            }

            if (!wasTokenizing)
            {
                var confirmationViewPopupViewModel = LifetimeScope!.Resolve<ConfirmationPopupViewModel>();

                if (confirmationViewPopupViewModel == null)
                {
                    throw new ArgumentNullException(nameof(confirmationViewPopupViewModel), "ConfirmationPopupViewModel needs to be registered with the DI container.");
                }

                confirmationViewPopupViewModel.SimpleMessagePopupMode = SimpleMessagePopupMode.DeleteCorpusNodeConfirmation;

                bool result = false;
                OnUIThread(async () =>
                {
                    result = await _windowManager!.ShowDialogAsync(confirmationViewPopupViewModel, null,
                        SimpleMessagePopupViewModel.CreateDialogSettings(confirmationViewPopupViewModel.Title));
                });

                if (!result)
                {
                    return;
                }
            }

            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = node.Name,
                Description = "Deleting parallel corpus connections and the node itself...",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Running
            }));

            // Deletes the ParallelCorpora and removes the connector between nodes. 
            foreach (var connection in node.AttachedConnections)
            {
                //connection.ParallelCorpusId
                DeleteParallelCorpusConnection(connection);
            }


            // Removes the CorpusNode form the project design surface:
            DesignSurfaceViewModel!.DeleteCorpusNode(node);



            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.Id == node.CorpusId);

            await Task.Factory.StartNew(async () =>
            {
                // ****************************************************************************
                // MICHAEL: not sure what needs to happen if 'corpusId' is null.  Also,
                // this method will accept a third CancellationToken argument if you have
                // one available here
                //
                // If corpusId is invalid/doesn't exist, this will throw an exception - do you 
                // want to catch it here or let it bubble out?
                // ****************************************************************************
                if (corpusId is not null)
                {
                    await Corpus.Delete(Mediator!, corpusId);
                }
                else
                {
                    var id = new CorpusId(node.CorpusId);
                    await Corpus.Delete(Mediator!, id);
                }
            });

            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
            {
                Name = node.Name,
                Description = "Delete Complete",
                StartTime = DateTime.Now,
                TaskLongRunningProcessStatus = LongRunningTaskStatus.Completed
            }));
        }

        #region IHandle

        public async Task HandleAsync(UiLanguageChangedMessage message, CancellationToken cancellationToken)
        {
            if (LoadingDesignSurface && !DesignSurfaceLoaded)
            {
                return;
            }
            //var language = message.LanguageCode;
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            // re-render the context menus
            foreach (var corpusNode in DesignSurfaceViewModel!.CorpusNodes)
            {
                var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusNode.CorpusId);
                await DesignSurfaceViewModel!.CreateCorpusNodeMenu(corpusNode, tokenizedCorpora);

                foreach (var parallelCorpus in corpusNode.AttachedConnections)
                {
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(parallelCorpus, topLevelProjectIds);
                }

            }


        }


        public async Task HandleAsync(RedrawParallelCorpusMenus message, CancellationToken cancellationToken)
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            foreach (var parallel in DesignSurfaceViewModel.ParallelCorpusConnections)
            {
                DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(parallel, topLevelProjectIds);
            }
        }

        #endregion

        #endregion // Methods


    }
}
