using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;


// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{

    public class ProjectDesignSurfaceViewModel : DashboardConductorOneActive<Screen>, IHandle<UiLanguageChangedMessage>, IDisposable
    {
        #region Member Variables

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        //public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly IWindowManager? _windowManager;
        private readonly LongRunningTaskManager? _longRunningTaskManager;
        #endregion //Member Variables

        #region Observable Properties

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
        public ProjectDesignSurfaceViewModel()
        {
            //no-op
        }

        public ProjectDesignSurfaceViewModel(INavigationService navigationService, IWindowManager windowManager,
            ILogger<ProjectDesignSurfaceViewModel> logger, DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, LongRunningTaskManager longRunningTaskManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _windowManager = windowManager;
            _longRunningTaskManager = longRunningTaskManager;

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
            DesignSurfaceViewModel = await ActivateItemAsync<DesignSurfaceViewModel>(cancellationToken);
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

        public async Task DrawDesignSurface()
        {
            Logger!.LogInformation($"Drawing design surface for project '{ProjectName}.");
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
                        var corpusNodeLocation =
                            designSurfaceData.CorpusNodeLocations.FirstOrDefault(cn => cn.CorpusId == corpusId.Id);
                        var point = corpusNodeLocation != null
                            ? new Point(corpusNodeLocation.X, corpusNodeLocation.Y)
                            : new Point();
                        var node = DesignSurfaceViewModel!.CreateCorpusNode(corpus, point);
                        var tokenizedCorpora =
                            topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId!.Id == corpusId.Id);
                        await DesignSurfaceViewModel!.CreateCorpusNodeMenu(node, tokenizedCorpora);
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

            ProjectName = ProjectManager.CurrentProject.ProjectName!;

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
            Logger!.LogInformation("AddManuscriptHebrewCorpus called.");

            var taskName = "HebrewCorpus";

            DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = false;

            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);

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
                Name = "Macula Hebrew",
                AvailableBooks = books,
            };

            _ = Task.Run(async () =>
            // _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);
                CorpusNodeViewModel corpusNode = new();

                try
                {

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: true,
                        Name: "Macula Hebrew",
                        Language: "Hebrew",
                        CorpusType: CorpusType.ManuscriptHebrew.ToString(),
                        ParatextId: ManuscriptIds.HebrewManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = FontNames.HebrewFontFamily;
                    corpusNode = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());



                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                    description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                    cancellationToken: cancellationToken);

                    // ReSharper disable once UnusedVariable
                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator!, corpus.CorpusId,
                        "Macula Hebrew",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        cancellationToken: cancellationToken);

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
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                           exception: ex, cancellationToken: cancellationToken);

                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DesignSurfaceViewModel!.DeleteCorpusNode(corpusNode);
                        // What other work needs to be done?  how do we know which steps have been executed?
                        DesignSurfaceViewModel!.AddManuscriptHebrewEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource();
                    }

                }
            }, cancellationToken);


        }

        public async void AddManuscriptGreekCorpus()
        {
            Logger!.LogInformation("AddGreekCorpus called.");

            DesignSurfaceViewModel!.AddManuscriptGreekEnabled = false;

            var taskName = "GreekCorpus";
            var task = _longRunningTaskManager!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;


            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);


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
                Name = "Macula Greek",
                AvailableBooks = books,
            };

            _ = Task.Run(async () =>
            //_ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);

                CorpusNodeViewModel corpusNode = new();

                try
                {
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: false,
                        Name: "Macula Greek",
                        Language: "Greek",
                        CorpusType: CorpusType.ManuscriptGreek.ToString(),
                        ParatextId: ManuscriptIds.GreekManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = FontNames.GreekFontFamily;
                    corpusNode = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    // ReSharper disable once UnusedVariable
                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator!, corpus.CorpusId,
                        "Macula Greek",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);
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
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                            exception: ex, cancellationToken: cancellationToken);
                    }
                }
                finally
                {
                    _longRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DesignSurfaceViewModel!.DeleteCorpusNode(corpusNode);
                        DesignSurfaceViewModel!.AddManuscriptGreekEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource();
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
                throw new ArgumentException($"Unable to parse value as Enum '{selectedTokenizer}'", nameof(selectedTokenizer));
            }

            var dialogViewModel = LifetimeScope?.Resolve<UpdateParatextCorpusDialogViewModel>(parameters);

            try
            {
                var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null,
                    DashboardProjectManager.AddParatextCorpusDialogSettings);

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

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                               description: $"Updating verses in tokenized text corpus for '{selectedProject.Name}' corpus...Completed", cancellationToken: cancellationToken);

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
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                   exception: ex, cancellationToken: cancellationToken);
                            }
                        }
                        finally
                        {
                            _longRunningTaskManager.TaskComplete(taskName);
                            _busyState.Remove(taskName);
                                
                            PlaySound.PlaySoundFromResource();
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
            var assembly = assemblyTokenizerType!.Assembly;
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

        public async Task AddParatextCorpus(string selectedParatextProjectId)
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
                    DashboardProjectManager.AddParatextCorpusDialogSettings);

                if (result)
                {
                    var selectedProject = dialogViewModel.SelectedProject;
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

                        try
                        {
                            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                            var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.ParatextGuid == selectedProject.Id);
                            var corpus = corpusId != null ? await Corpus.Get(Mediator!, corpusId) : null;

                            // first time for this corpus
                            if (corpus is null)
                            {
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                                   description: $"Creating corpus '{selectedProject.Name}'...", cancellationToken: cancellationToken);
#pragma warning disable CS8604
                                corpus = await Corpus.Create(
                                      mediator: Mediator,
                                      IsRtl: selectedProject.IsRtl,
                                      FontFamily: selectedProject.FontFamily,
                                      Name: selectedProject.Name,
                                      Language: selectedProject.LanguageName,
                                      CorpusType: selectedProject.CorpusTypeDisplay,
                                      ParatextId: selectedProject.Id,
                                      token: cancellationToken);

                                corpus.CorpusId.FontFamily = selectedProject.FontFamily;

                                node = DesignSurfaceViewModel!.CreateCorpusNode(corpus, new Point());
#pragma warning restore CS8604
                            }



                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Tokenizing and transforming '{selectedProject.Name}' corpus...", cancellationToken: cancellationToken);

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
                               description: $"Creating tokenized text corpus for '{selectedProject.Name}' corpus...", cancellationToken: cancellationToken);

#pragma warning disable CS8604
                            // ReSharper disable once UnusedVariable
                            var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                               selectedProject.Name, dialogViewModel.SelectedTokenizer.ToString(), cancellationToken);
#pragma warning restore CS8604


                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                               description: $"Creating tokenized text corpus for '{selectedProject.Name}' corpus...Completed", cancellationToken: cancellationToken);

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
                                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                        exception: ex, cancellationToken: cancellationToken);
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {selectedProject.Name} ");
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                   exception: ex, cancellationToken: cancellationToken);
                            }
                        }
                        finally
                        {
                            _longRunningTaskManager.TaskComplete(taskName);
                            _busyState.Remove(taskName);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                DesignSurfaceViewModel!.DeleteCorpusNode(node);
                            }
                            else
                            {
                                PlaySound.PlaySoundFromResource();
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

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }





        public async Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connectionViewModel = connectionMenuItem.ConnectionViewModel;
            switch (connectionMenuItem.Id)
            {
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
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.CreateNewInterlinear:
                    await AddNewInterlinear(connectionMenuItem);
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentSetToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentSetToNewEnhancedView:
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new AddAlignmentSetToEnhancedViewMessage(new AlignmentEnhancedViewItemMetadatum
                            {
                                AlignmentSetId = connectionMenuItem.AlignmentSetId,
                                DisplayName = connectionMenuItem.DisplayName,
                                ParallelCorpusId = connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                ParallelCorpusDisplayName = connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                IsRtl = connectionMenuItem.IsRtl,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                IsTargetRtl = connectionMenuItem.IsTargetRTL,
                                IsNewWindow = connectionMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds.AddAlignmentSetToNewEnhancedView,
                                SourceParatextId = connectionMenuItem.SourceParatextId,
                                TargetParatextId = connectionMenuItem.TargetParatextId
                            }
                        ));
                    }
                    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddInterlinearToCurrentEnhancedView:
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AddInterlinearToNewEnhancedView:
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new AddInterlinearToEnhancedViewMessage(new InterlinearEnhancedViewItemMetadatum
                            {

                                TranslationSetId = connectionMenuItem.TranslationSetId,
                                DisplayName = connectionMenuItem.DisplayName,
                                ParallelCorpusId = connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                ParallelCorpusDisplayName = connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                IsRtl = connectionMenuItem.IsRtl,
                                //FIXME:surface serialization null,
                                IsNewWindow = connectionMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds.AddInterlinearToNewEnhancedView,
                                IsTargetRtl = connectionMenuItem.IsTargetRTL,
                                SourceParatextId = connectionMenuItem.SourceParatextId,
                                TargetParatextId = connectionMenuItem.TargetParatextId
                            }
                        ));
                    }
                    break;
                default:
                    //no-op
                    break;
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
                            tc.CorpusId.Id == corpusNodeViewModel.CorpusId && tc.TokenizationFunction == corpusNodeMenuItem.Tokenizer);

                    if (tokenizedCorpus == null)
                    {
                        Logger!.LogDebug($"Could not locate a TokenizedCorpus with a TokenizationFunction: '{corpusNodeMenuItem.Tokenizer}'.");
                        return;
                    }

                    await EventAggregator.PublishOnUIThreadAsync(
                        new AddTokenizedCorpusToEnhancedViewMessage(new TokenizedCorpusEnhancedViewItemMetadatum
                        {
                            ParatextProjectId = corpusNodeViewModel.ParatextProjectId,
                            ProjectName = corpusNodeViewModel.Name,
                            TokenizationType = corpusNodeMenuItem.Tokenizer!,
                            CorpusId = tokenizedCorpus.CorpusId!.Id,
                            TokenizedTextCorpusId = tokenizedCorpus.Id,
                            CorpusType = corpusNodeViewModel.CorpusType,
                            //FIXME:new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            IsRtl = corpusNodeViewModel.IsRtl,
                            IsNewWindow = corpusNodeMenuItem.Id == DesignSurfaceViewModel.DesignSurfaceMenuIds.AddTokenizedCorpusToNewEnhancedView
                        }));
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
            }
        }

        private async Task AddNewInterlinear(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("parallelCorpusId", connectionMenuItem.ParallelCorpusId!)
            };

            var dialogViewModel = LifetimeScope!.Resolve<InterlinearDialogViewModel>(parameters);
            var result = await _windowManager!.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                try
                {
                    var translationSet = await TranslationSet.Create(null, dialogViewModel.SelectedAlignmentSet!,
                        dialogViewModel.TranslationSetDisplayName, new Dictionary<string, object>(),
                        dialogViewModel.SelectedAlignmentSet!.ParallelCorpusId!, Mediator!);

                    var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(connectionMenuItem.ConnectionViewModel, topLevelProjectIds);
                    await SaveDesignSurfaceData();

                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while adding the interlinear for {connectionMenuItem.ParallelCorpusId!}");
                }

            }
        }

        public async Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection)
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
                new NamedParameter("tokenizedCorpora", tokenizedCorpora)
            };

            var dialogViewModel = LifetimeScope?.Resolve<ParallelCorpusDialogViewModel>(parameters);

            try
            {
                var success = await _windowManager!.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

                PlaySound.PlaySoundFromResource();

                if (success)
                {
                    // get TranslationSet , etc from the dialogViewModel
                    var translationSet = dialogViewModel!.TranslationSet;


                    newParallelCorpusConnection.ParallelCorpusId = dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId;
                    newParallelCorpusConnection.ParallelCorpusDisplayName =
                        dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId.DisplayName;

                    topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(newParallelCorpusConnection, topLevelProjectIds);

                }
                else
                {
                    DesignSurfaceViewModel!.DeleteParallelCorpusConnection(newParallelCorpusConnection);
                }
            }
            finally
            {
                await SaveDesignSurfaceData();
            }
        }

        public async Task ExecuteAquaCorpusAnalysisMenuCommand(AquaCorpusAnalysisMenuItemViewModel aquaCorpusAnalysisMenuItemViewModel)
        {
            var corpusNodeViewModel = aquaCorpusAnalysisMenuItemViewModel.CorpusNodeViewModel;
            if (corpusNodeViewModel == null)
            {
                Logger!.LogInformation($"The CorpusNodeViewModel on the CorpusNodeMenuItem: '{aquaCorpusAnalysisMenuItemViewModel.Id}' is null., Returning...");
                return;
            }

            switch (aquaCorpusAnalysisMenuItemViewModel.Id)
            {
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AquaRequestCorpusAnalysis:  //fixme
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AquaAddLatestCorpusAnalysisToCurrentEnhancedView:
                    await EventAggregator.PublishOnUIThreadAsync(new AddAquaCorpusAnalysisToEnhancedViewMessage(new AquaCorpusAnalysisEnhancedViewItemMetadatum()
                    {
                        IsNewWindow = false
                    })); ;
                    break;
                //case DesignSurfaceViewModel.DesignSurfaceMenuIds.AquaRequestCorpusAnalysis:
                //    await AquaRequestCorpusAnalysis(corpusNodeViewModel.ParatextProjectId);
                //    break;
                case DesignSurfaceViewModel.DesignSurfaceMenuIds.AquaGetCorpusAnalysis:
                    await AquaGetCorpusAnalysis(corpusNodeViewModel.ParatextProjectId);
                    break;
            }
        }

        private async Task AquaRequestCorpusAnalysis(string paratextProjectId)
        {

        }

        private async Task AquaGetCorpusAnalysis(string paratextProjectId)
        {

        }


        public void ShowCorpusProperties(CorpusNodeViewModel corpus)
        {
            SelectedDesignSurfaceComponent = corpus;
        }

        public void ShowConnectionProperties(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            SelectedDesignSurfaceComponent = parallelCorpusConnection;
        }

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
                DesignSurfaceViewModel!.CreateCorpusNodeMenu(corpusNode, tokenizedCorpora);

                foreach (var parallelCorpus in corpusNode.AttachedConnections)
                {
                    DesignSurfaceViewModel!.CreateParallelCorpusConnectionMenu(parallelCorpus, topLevelProjectIds);
                }

            }


        }

        #endregion // Methods


        public async void DeleteParallelCorpusConnection(ParallelCorpusConnectionViewModel connection)
        {

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
                    await DAL.Alignment.Corpora.ParallelCorpus.Delete(Mediator!, connection.ParallelCorpusId);
                }
            });
           

            // Removes the connector between corpus nodes:
            DesignSurfaceViewModel!.DeleteParallelCorpusConnection(connection);
        }

        public async void DeleteCorpusNode(CorpusNodeViewModel node)
        {
           
                // Deletes the ParallelCorpora and removes the connector between nodes. 
                foreach (var connection in node.AttachedConnections)
                {
                    //connection.ParallelCorpusId
                    DeleteParallelCorpusConnection(connection);
                }

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
                });
            

                // Removes the CorpusNode form the project design surface:
                DesignSurfaceViewModel!.DeleteCorpusNode(node);
         
        
        }
    }
}
