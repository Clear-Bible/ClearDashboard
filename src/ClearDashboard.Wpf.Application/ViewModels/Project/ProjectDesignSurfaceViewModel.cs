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
using ClearDashboard.Wpf.Application.Exceptions;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.Views.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Controls;
using MahApps.Metro.IconPacks;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using TopLevelProjectIds = ClearDashboard.DAL.Alignment.TopLevelProjectIds;
using System.Windows.Input;


// ReSharper disable once CheckNamespace
namespace ClearDashboard.Wpf.Application.ViewModels.Project
{

    public class ProjectDesignSurfaceViewModel : DashboardApplicationScreen
    {
        #region Member Variables

        //public record CorporaLoadedMessage(IEnumerable<DAL.Alignment.Corpora.Corpus> Copora);
        public record TokenizedTextCorpusLoadedMessage(TokenizedTextCorpus TokenizedTextCorpus, string TokenizationName, ParatextProjectMetadata? ProjectMetadata);

        private readonly IWindowManager? _windowManager;
        private readonly LongRunningTaskManager? _longRunningTaskManager;
        #endregion //Member Variables


        #region Observable Properties


        private BindableCollection<Corpus>? Corpora { get; set; }

        ///// <summary>
        ///// This is the design surface that is displayed in the window.
        ///// It is the main part of the view-model.
        ///// </summary>
        /////
        //private ProjectDesignSurface? ProjectDesignSurface { get; set; }

        /// <summary>
        /// This is the design surface that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        private DesignSurfaceViewModel? _designSurface;

        public DesignSurfaceViewModel? DesignSurface
        {
            get => _designSurface;
            private set => Set(ref _designSurface, value);
        }




        private object? _selectedDesignSurfaceComponent;
        public object? SelectedDesignSurfaceComponent
        {
            get
            {
                if (_selectedDesignSurfaceComponent is CorpusNodeViewModel node)
                {
                    foreach (var corpusNode in DesignSurface.CorpusNodes)
                    {
                        if (corpusNode.ParatextProjectId == node.ParatextProjectId)
                        {
                            return corpusNode;
                        }
                    }
                }
                else if (_selectedDesignSurfaceComponent is ParallelCorpusConnectionViewModel conn)
                {
                    foreach (var connection in DesignSurface.ParallelCorpusConnections)
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

        private bool _addManuscriptHebrewEnabled = true;
        public bool AddManuscriptHebrewEnabled
        {
            get => _addManuscriptHebrewEnabled;
            set => Set(ref _addManuscriptHebrewEnabled, value);
        }

        private bool _addManuscriptGreekEnabled = true;
        public bool AddManuscriptGreekEnabled
        {
            get => _addManuscriptGreekEnabled;
            set => Set(ref _addManuscriptGreekEnabled, value);
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

            Corpora = new BindableCollection<Corpus>();
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            _busyState.CollectionChanged += BusyStateOnCollectionChanged;
            return base.OnActivateAsync(cancellationToken);
        }

        private void BusyStateOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => IsBusy);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _busyState.CollectionChanged -= BusyStateOnCollectionChanged;
            await SaveDesignSurfaceData();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            //
            // Create a design surface, the root of the view-model.
            //
            DesignSurface = LifetimeScope!.Resolve<DesignSurfaceViewModel>();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (DesignSurface.ProjectDesignSurface == null)
            {
                if (view is ProjectDesignSurfaceView projectDesignSurfaceView)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    DesignSurface.ProjectDesignSurface = (ProjectDesignSurface)projectDesignSurfaceView.FindName("ProjectDesignSurface");

                }
            }
            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Caliburn.Micro overrides

        //protected override async void OnViewLoaded(object view)
        //{
        //    Console.WriteLine();
        //    base.OnViewLoaded(view);
        //}

        //protected override async void OnViewReady(object view)
        //{
        //    Console.WriteLine();
        //    base.OnViewReady(view);
        //}
        #endregion //Constructor

        #region Methods

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
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                surface.TokenizedCorpora.Add(new SerializedTokenizedCorpus
                {
                    ParatextProjectId = corpusNode.ParatextProjectId,
                    CorpusType = corpusNode.CorpusType,
                    Name = corpusNode.Name,
                    X = corpusNode.X,
                    Y = corpusNode.Y,
                    Tokenizations = corpusNode.Tokenizations,
                    CorpusId = corpusNode.CorpusId,
                    IsRTL = corpusNode.IsRtl,
                    TranslationFontFamily = corpusNode.TranslationFontFamily,
                });
            }

            // save all the connections
            foreach (var connection in DesignSurface.ParallelCorpusConnections)
            {
                var serializedTranslationSet = connection.TranslationSetInfo.Select(translationSet => new TranslationSetInfo
                {
                    DisplayName = translationSet.DisplayName ?? string.Empty,
                    TranslationSetId = translationSet.TranslationSetId,
                    ParallelCorpusDisplayName = translationSet.ParallelCorpusDisplayName ?? string.Empty,
                    ParallelCorpusId = translationSet.ParallelCorpusId,
                    AlignmentSetDisplayName = translationSet.AlignmentSetDisplayName ?? string.Empty,
                    AlignmentSetId = translationSet.AlignmentSetId,
                    IsRTL = translationSet.IsRTL,
                    SourceFontFamily = translationSet.SourceFontFamily,
                    TargetFontFamily = translationSet.TargetFontFamily,
                })
                    .ToList();

                var serializedAlignmentSet = connection.AlignmentSetInfo.Select(alignmentSetInfo => new AlignmentSetInfo
                {
                    DisplayName = alignmentSetInfo.DisplayName ?? string.Empty,
                    AlignmentSetId = alignmentSetInfo.AlignmentSetId,
                    ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusDisplayName ?? string.Empty,
                    ParallelCorpusId = alignmentSetInfo.ParallelCorpusId,
                    IsRtl = alignmentSetInfo.IsRtl,
                    IsTargetRtl = alignmentSetInfo.IsTargetRtl,
                    SourceFontFamily = alignmentSetInfo.SourceFontFamily,
                    TargetFontFamily = alignmentSetInfo.TargetFontFamily,
                })
                    .ToList();

                surface.ParallelCorpora.Add(new SerializedParallelCorpus
                {
                    SourceConnectorId = connection.SourceConnector.ParatextId,
                    TargetConnectorId = connection.DestinationConnector.ParatextId,
                    TranslationSetInfo = serializedTranslationSet,
                    AlignmentSetInfo = serializedAlignmentSet,
                    ParallelCorpusDisplayName = connection.ParallelCorpusDisplayName,
                    ParallelCorpusId = connection.ParallelCorpusId!.Id.ToString(),
                    SourceFontFamily = connection.SourceFontFamily,
                    TargetFontFamily = connection.TargetFontFamily,
                });
            }

            // save out the corpora
            foreach (var corpus in this.Corpora)
            {
                surface.Corpora.Add(new SerializedCorpus
                {
                    CorpusId = corpus.CorpusId.Id.ToString(),
                    CorpusType = corpus.CorpusId.CorpusType,
                    Created = corpus.CorpusId.Created,
                    DisplayName = corpus.CorpusId.DisplayName,
                    IsRtl = corpus.CorpusId.IsRtl,
                    TranslationFontFamily = corpus.CorpusId.FontFamily ?? Corpus.DefaultFontFamily,
                    Language = corpus.CorpusId.Language,
                    Name = corpus.CorpusId.Name,
                    ParatextGuid = corpus.CorpusId.ParatextGuid,
                    UserId = corpus.CorpusId.UserId?.Id.ToString()
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

        public async Task LoadDesignSurface()
        {
            AddManuscriptGreekEnabled = true;
            AddManuscriptHebrewEnabled = true;

            Stopwatch sw = new();
            sw.Start();

            var designSurfaceData = LoadDesignSurfaceData();
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            // restore the nodes
            if (designSurfaceData != null)
            {
                foreach (var corpusId in topLevelProjectIds.CorpusIds)
                {
                    if (corpusId.CorpusType == CorpusType.ManuscriptHebrew.ToString())
                    {
                        AddManuscriptHebrewEnabled = false;
                    }
                    else if (corpusId.CorpusType == CorpusType.ManuscriptGreek.ToString())
                    {
                        AddManuscriptGreekEnabled = false;
                    }

                    var corpus = new Corpus(corpusId);
                    Corpora!.Add(corpus);

                    var corpusNode = designSurfaceData.TokenizedCorpora.FirstOrDefault(cn => cn.CorpusId == corpusId.Id);

                    var point = corpusNode != null ? new Point(corpusNode.X, corpusNode.Y) : new Point();

                    var node = CreateCorpusNode(corpus, point);

                    var tokenizedCorpora = topLevelProjectIds.TokenizedTextCorpusIds.Where(ttc => ttc.CorpusId == corpusId);

                    foreach(var tokenizedCorpus in tokenizedCorpora)
                    {
                        if (!string.IsNullOrEmpty(tokenizedCorpus.TokenizationFunction))
                        {
                            var tokenizer = (Tokenizers)Enum.Parse(typeof(Tokenizers),
                                tokenizedCorpus.TokenizationFunction);
                            //ToDO:  revisit
                            node.Tokenizations.Add(new SerializedTokenization
                            {
                                CorpusId = corpus.CorpusId.Id.ToString(),
                                TokenizationFriendlyName = EnumHelper.GetDescription(tokenizer),
                                IsSelected = false,
                                TokenizationName = tokenizer.ToString(),
                                TokenizedTextCorpusId = tokenizedCorpus.Id.ToString()
                            });
                        }
                    }

                    // add in the menu
                    CreateCorpusNodeMenu(node);
                }

                foreach (var parallelCorpusId in topLevelProjectIds.ParallelCorpusIds)
                {
                    var deserializedConnection = designSurfaceData.ParallelCorpora.FirstOrDefault(pc => pc.ParallelCorpusId == parallelCorpusId.Id.ToString());
                    if (deserializedConnection != null)
                    {
                        var sourceNode = DesignSurface!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId == parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.ParatextGuid);
                        var targetNode = DesignSurface!.CorpusNodes.FirstOrDefault(p =>
                            p.ParatextProjectId == parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.ParatextGuid);


                        if (sourceNode is not null && targetNode is not null)
                        {
                            var connection = new ParallelCorpusConnectionViewModel
                            {
                                SourceConnector = sourceNode.OutputConnectors[0],
                                DestinationConnector = targetNode.InputConnectors[0],
                                TranslationSetInfo = deserializedConnection.TranslationSetInfo,
                                AlignmentSetInfo = deserializedConnection.AlignmentSetInfo,
                                ParallelCorpusDisplayName = parallelCorpusId.DisplayName,
                                ParallelCorpusId = parallelCorpusId,
                                SourceFontFamily = parallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.FontFamily,
                                TargetFontFamily = parallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.FontFamily,
                            };
                            DesignSurface.ParallelCorpusConnections.Add(connection);
                            // add in the context menu
                            CreateConnectionMenu(connection);
                        }
                    }
                }
            }

            sw.Stop();

            Debug.WriteLine($"LoadCanvas took {sw.ElapsedMilliseconds} ms ({sw.Elapsed.Seconds} seconds)");
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

        private bool _isBusy;
        public override bool IsBusy
        {
            get => _busyState.Count > 0;
            set => Set(ref _isBusy, _busyState.Count > 0);
        }


        // ReSharper disable once UnusedMember.Global
        public async void AddManuscriptHebrewCorpus()
        {
            Logger!.LogInformation("AddManuscriptHebrewCorpus called.");

            var taskName = "HebrewCorpus";

            AddManuscriptHebrewEnabled = false;

            var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;

            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);

            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.H)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var bookInfo = new BookInfo();
            var books = bookInfo.GenerateScriptureBookList()
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

            _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);
                CorpusNodeViewModel corpusNode = new();

                try
                {

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await Corpus.Create(
                        mediator: Mediator,
                        IsRtl: true,
                        Name: "Macula Hebrew",
                        Language: "Hebrew",
                        CorpusType: CorpusType.ManuscriptHebrew.ToString(),
                        ParatextId: ManuscriptIds.HebrewManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = ManuscriptIds.HebrewFontFamily;

                    OnUIThread(() =>
                    {
                        Corpora.Add(corpus);
                        corpusNode = CreateCorpusNode(corpus, new Point());
                    });


                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...",
                        cancellationToken: cancellationToken);

                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator, corpus.CorpusId,
                        "Macula Hebrew",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    OnUIThread(() =>
                    {

                        //corpusNode = CreateCorpusNode(corpus, new Point());
                        //TODO:  revisit
                        corpusNode.Tokenizations.Add(new SerializedTokenization
                        {
                            CorpusId = corpus.CorpusId.Id.ToString(),
                            TokenizationFriendlyName = EnumHelper.GetDescription(Tokenizers.WhitespaceTokenizer),
                            IsSelected = false,
                            TokenizationName = Tokenizers.WhitespaceTokenizer.ToString(),
                            TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString(),
                        });
                    });

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed",
                        cancellationToken: cancellationToken);


                    Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                    OnUIThread(async () =>
                    {
                        await UpdateNodeTokenization(corpusNode, corpus, tokenizedTextCorpus,
                            Tokenizers.WhitespaceTokenizer);
                    });

                }
                catch (OperationCanceledException ex)
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
                        DeleteCorpusNode(corpusNode);
                        // What other work needs to be done?  how do we know which steps have been executed?
                        AddManuscriptHebrewEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(null, null);
                    }

                }
            }, cancellationToken);


        }


        public async void AddManuscriptGreekCorpus()
        {
            Logger!.LogInformation("AddGreekCorpus called.");


            AddManuscriptGreekEnabled = false;

            var taskName = "GreekCorpus";
            var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = task.CancellationTokenSource!.Token;


            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                description: $"Transforming syntax trees...", cancellationToken: cancellationToken);


            var syntaxTree = new SyntaxTrees();
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.G)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();

            var bookInfo = new BookInfo();
            var books = bookInfo.GenerateScriptureBookList()
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

            _ = await Task.Factory.StartNew(async () =>
            {
                _busyState.Add(taskName, true);

                CorpusNodeViewModel node = new();

                try
                {
                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var corpus = await DAL.Alignment.Corpora.Corpus.Create(
                        mediator: Mediator!,
                        IsRtl: false,
                        Name: "Macula Greek",
                        Language: "Greek",
                        CorpusType: CorpusType.ManuscriptGreek.ToString(),
                        ParatextId: ManuscriptIds.GreekManuscriptId,
                        token: cancellationToken);

                    corpus.CorpusId.FontFamily = ManuscriptIds.GreekFontFamily;

                    OnUIThread(() =>
                    {
                        
                        Corpora.Add(corpus);
                        node = CreateCorpusNode(corpus, new Point());
                    });

                   

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                    var tokenizedTextCorpus = await sourceCorpus.Create(Mediator, corpus.CorpusId,
                        "Macula Greek",
                        Tokenizers.WhitespaceTokenizer.ToString(),
                        cancellationToken);

                    OnUIThread(() =>
                    {
                       

                        //TODO:  revisit
                        node.Tokenizations.Add(new SerializedTokenization
                        {
                            CorpusId = corpus.CorpusId.Id.ToString(),
                            TokenizationFriendlyName = EnumHelper.GetDescription(Tokenizers.WhitespaceTokenizer),
                            IsSelected = false,
                            TokenizationName = Tokenizers.WhitespaceTokenizer.ToString(),
                            TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString(),
                        });
                    });

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);

                    Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                    OnUIThread(async () =>
                    {
                        await UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, Tokenizers.WhitespaceTokenizer);
                    });

                }
                catch (OperationCanceledException ex)
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
                        DeleteCorpusNode(node);
                        AddManuscriptGreekEnabled = true;
                    }
                    else
                    {
                        PlaySound.PlaySoundFromResource(null, null);
                    }
                }
            }, cancellationToken);


        }

        public async void AddParatextCorpus()
        {
            await AddParatextCorpus("");
        }


        // ReSharper restore UnusedMember.Global
        // ReSharper disable once UnusedMember.Global
        public async Task AddParatextCorpus(string selectedParatextProjectId)
        {
            Logger!.LogInformation("AddParatextCorpus called.");

            var dialogViewModel = LifetimeScope!.Resolve<AddParatextCorpusDialogViewModel>();
            var result = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var metadata = dialogViewModel.SelectedProject;
                var taskName = $"{metadata.Name}";
                _busyState.Add(taskName, true);

                var task = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
                var cancellationToken = task.CancellationTokenSource!.Token;
                _ = await Task.Factory.StartNew(async () =>
                {
                    CorpusNodeViewModel node = new();
                    node.TranslationFontFamily = metadata.FontFamily;

                    try
                    {
                        Corpus? corpus = null;

                        // is this corpus already made for a different tokenization
                        foreach (var corpusNode in Corpora)
                        {
                            if (corpusNode.CorpusId.ParatextGuid == metadata.Id)
                            {
                                corpus = corpusNode;

                                // find the node on the design surface
                                foreach (var designSurfaceCorpusNode in DesignSurface.CorpusNodes)
                                {
                                    if (designSurfaceCorpusNode.ParatextProjectId == metadata.Id)
                                    {
                                        node = designSurfaceCorpusNode;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        // first time for this corpus
                        if (corpus is null)
                        {
                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Creating corpus '{metadata.Name}'...", cancellationToken: cancellationToken);
#pragma warning disable CS8604
                            corpus = await Corpus.Create(
                                 mediator: Mediator,
                                 IsRtl: metadata.IsRtl,

                                 Name: metadata.Name,

                                 Language: metadata.LanguageName,
                                 CorpusType: metadata.CorpusTypeDisplay,
                                 ParatextId: metadata.Id,
                                 token: cancellationToken);

                            corpus.CorpusId.FontFamily = metadata.FontFamily;
#pragma warning restore CS8604
                        }
                        OnUIThread(() =>
                        {
                            Corpora.Add(corpus);
                            node = CreateCorpusNode(corpus, new Point());

                        });

                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                           description: $"Tokenizing and transforming '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

                        var textCorpus = dialogViewModel.SelectedTokenizer switch
                        {
                            Tokenizers.LatinWordTokenizer =>
                               (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<LatinWordTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            Tokenizers.WhitespaceTokenizer =>
                               (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<WhitespaceTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            Tokenizers.ZwspWordTokenizer => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<ZwspWordTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>(),
                            _ => (await ParatextProjectTextCorpus.Get(Mediator!, metadata.Id!, cancellationToken))
                               .Tokenize<WhitespaceTokenizer>()
                               .Transform<IntoTokensTextRowProcessor>()
                               .Transform<SetTrainingBySurfaceLowercase>()
                        };


                     

                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                           description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...", cancellationToken: cancellationToken);

#pragma warning disable CS8604
                        var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                           metadata.Name, dialogViewModel.SelectedTokenizer.ToString(), cancellationToken);
#pragma warning restore CS8604

                        

                        OnUIThread(() =>
                        {
                            
                            //TODO:  revisit
                            node.Tokenizations.Add(new SerializedTokenization
                            {
                                CorpusId = corpus.CorpusId.Id.ToString(),
                                TokenizationFriendlyName = EnumHelper.GetDescription(dialogViewModel.SelectedTokenizer),
                                IsSelected = false,
                                TokenizationName = dialogViewModel.SelectedTokenizer.ToString(),
                                TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString()
                            });

                        });
                       
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                           description: $"Creating tokenized text corpus for '{metadata.Name}' corpus...Completed", cancellationToken: cancellationToken);

                        Logger!.LogInformation("Sending TokenizedTextCorpusLoadedMessage via EventAggregator.");

                        OnUIThread(async () =>
                        {
                            await UpdateNodeTokenization(node, corpus, tokenizedTextCorpus, dialogViewModel.SelectedTokenizer);
                        });

                        _longRunningTaskManager.TaskComplete(taskName);
                    }
                    catch (OperationCanceledException ex)
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
                            DeleteCorpusNode(node);
                        }
                        else
                        {
                            PlaySound.PlaySoundFromResource(null, null);
                        }

                    }
                }, cancellationToken);
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


        /// <summary>
        /// adds on the tokenizedtextid to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="corpus"></param>
        /// <param name="tokenizedTextCorpus"></param>
        /// <param name="viewModelSelectedTokenizers"></param>
        private async Task UpdateNodeTokenization(CorpusNodeViewModel node, DAL.Alignment.Corpora.Corpus corpus,
            TokenizedTextCorpus tokenizedTextCorpus, Tokenizers viewModelSelectedTokenizers)
        {
            var corpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == node.Id);
            if (corpusNode is not null)
            {
                var nodeTokenization = corpusNode.Tokenizations.FirstOrDefault(b =>
                    b.TokenizationName == viewModelSelectedTokenizers.ToString());

                if (nodeTokenization is not null)
                {
                    nodeTokenization.IsSelected = false;
                    nodeTokenization.IsPopulated = true;
                    nodeTokenization.TokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id.ToString();
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);
                }
                else
                {
                    corpusNode.Tokenizations.Add(new SerializedTokenization
                    {
                        CorpusId = corpus.CorpusId.Id.ToString(),
                        TokenizationFriendlyName = EnumHelper.GetDescription(viewModelSelectedTokenizers),
                        IsSelected = false,
                        IsPopulated = true,
                        TokenizationName = viewModelSelectedTokenizers.ToString(),
                    });

                    // TODO the UI chip is not being updated with the new count...why?

                    //NotifyOfPropertyChange(() => corpusNode);
                    NotifyOfPropertyChange(() => DesignSurface.CorpusNodes);

                    // force a redraw
                    DesignSurface.ProjectDesignSurface?.InvalidateVisual();
                }

                CreateCorpusNodeMenu(corpusNode);
                await SaveDesignSurfaceData();
            }
        }


        /// <summary>
        /// creates the databound menu for the node
        /// </summary>
        /// <param name="corpusNode"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode)
        {
            // initiate the menu system
            corpusNode.MenuItems.Clear();

            BindableCollection<CorpusNodeMenuItemViewModel> nodeMenuItems = new();

            // restrict the ability of Manuscript to add new tokenizers
            if (corpusNode.CorpusType != CorpusType.ManuscriptHebrew || corpusNode.CorpusType != CorpusType.ManuscriptGreek)
            {
                // Add new tokenization
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                {
                    Header = LocalizationStrings.Get("Pds_AddNewTokenizationMenu", Logger),
                    Id = "AddTokenizationId",
                    IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                    ProjectDesignSurfaceViewModel = this,
                    CorpusNodeViewModel = corpusNode,
                });
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
            }

            foreach (var nodeTokenization in corpusNode.Tokenizations)
            {
                nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
                {
                    Header = nodeTokenization.TokenizationFriendlyName,
                    Id = nodeTokenization.TokenizedTextCorpusId,
                    IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                    MenuItems = new BindableCollection<CorpusNodeMenuItemViewModel>
                    {
                        new CorpusNodeMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddToEnhancedViewMenu", Logger),
                            Id = "AddToEnhancedViewId",
                            ProjectDesignSurfaceViewModel = this,
                            IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                            CorpusNodeViewModel = corpusNode,
                            Tokenizer = nodeTokenization.TokenizationName,
                        },
                        new CorpusNodeMenuItemViewModel
                        {
                            // Show Verses in New Windows
                            Header = LocalizationStrings.Get("Pds_ShowVersesMenu", Logger),
                            Id = "ShowVerseId", ProjectDesignSurfaceViewModel = this,
                            IconKind = PackIconPicolIconsKind.DocumentText.ToString(),
                            CorpusNodeViewModel = corpusNode,
                            Tokenizer = nodeTokenization.TokenizationName,
                        },
                        //new CorpusNodeMenuItemViewModel
                        //{
                        //    // Properties
                        //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
                        //    Id = "TokenizerPropertiesId",
                        //    ProjectDesignSurfaceViewModel = this,
                        //    IconKind = "Settings",
                        //    CorpusNodeViewModel = corpusNode,
                        //    Tokenizer = nodeTokenization.TokenizationName,
                        //}
                    }
                });
            }

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                Header = "",
                Id = "SeparatorId",
                ProjectDesignSurfaceViewModel = this,
                IsSeparator = true
            });

            nodeMenuItems.Add(new CorpusNodeMenuItemViewModel
            {
                // Properties
                Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
                Id = "PropertiesId",
                IconKind = PackIconPicolIconsKind.Settings.ToString(),
                CorpusNodeViewModel = corpusNode,
                ProjectDesignSurfaceViewModel = this
            });

            corpusNode.MenuItems = nodeMenuItems;
        }


        /// <summary>
        /// creates the data bound menu for the node
        /// </summary>
        /// <param name="parallelCorpusConnection"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            // initiate the menu system
            parallelCorpusConnection.MenuItems.Clear();

            BindableCollection<ParallelCorpusConnectionMenuItemViewModel> connectionMenuItems = new()
            {
                // Add new alignment set
                new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = LocalizationStrings.Get("Pds_CreateNewAlignmentSetMenu", Logger!),
                    Id = "CreateAlignmentSetId",
                    IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                    ProjectDesignSurfaceViewModel = this,
                    ConnectionId = parallelCorpusConnection.Id,
                    ParallelCorpusId = parallelCorpusConnection.ParallelCorpusId?.Id.ToString(),
                    ParallelCorpusDisplayName = parallelCorpusConnection.ParallelCorpusDisplayName,
                    IsRtl = parallelCorpusConnection.IsRtl,
                    SourceParatextId = parallelCorpusConnection.SourceConnector?.ParatextId,
                    TargetParatextId = parallelCorpusConnection.DestinationConnector?.ParatextId,
                },
                new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = "",
                    Id = "SeparatorId",
                    ProjectDesignSurfaceViewModel = this,
                    IsSeparator = true
                }
            };


            // ALIGNMENT SETS
            foreach (var alignmentSetInfo in parallelCorpusConnection.AlignmentSetInfo)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = alignmentSetInfo.DisplayName,
                    Id = alignmentSetInfo.AlignmentSetId,
                    IconKind = PackIconPicolIconsKind.Sitemap.ToString(),
                    IsEnabled = true,
                    MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                    {
                        new ParallelCorpusConnectionMenuItemViewModel
                        {
                            // Add Verses to focused enhanced view
                            Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
                            Id = "AddAlignmentToEnhancedViewId",
                            ProjectDesignSurfaceViewModel = this,
                            IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                            AlignmentSetId = alignmentSetInfo.AlignmentSetId,
                            DisplayName = alignmentSetInfo.DisplayName,
                            ParallelCorpusId = alignmentSetInfo.ParallelCorpusId,
                            ParallelCorpusDisplayName = alignmentSetInfo.ParallelCorpusDisplayName,
                            IsEnabled = true,
                            IsRtl = alignmentSetInfo.IsRtl,
                            IsTargetRTL = alignmentSetInfo.IsTargetRtl,
                            SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                            TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                        },
                    }
                });
            }

            // TRANSLATION SET
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

            // Add new tokenization
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = LocalizationStrings.Get("Pds_CreateNewInterlinear", Logger),
                Id = "CreateNewInterlinearId",
                IconKind = PackIconPicolIconsKind.BookTextAdd.ToString(),
                ProjectDesignSurfaceViewModel = this,
                ConnectionId = parallelCorpusConnection.Id,
                Enabled = (parallelCorpusConnection.AlignmentSetInfo.Count > 0)
            });
            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });


            foreach (var info in parallelCorpusConnection.TranslationSetInfo)
            {
                connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
                {
                    Header = info.DisplayName,
                    Id = info.TranslationSetId,
                    IconKind = PackIconPicolIconsKind.Relevance.ToString(),
                    MenuItems = new BindableCollection<ParallelCorpusConnectionMenuItemViewModel>
                        {
                            new ParallelCorpusConnectionMenuItemViewModel
                            {
                                // Add Verses to focused enhanced view
                                Header = LocalizationStrings.Get("Pds_AddConnectionToEnhancedViewMenu", Logger),
                                Id = "AddTranslationToEnhancedViewId", ProjectDesignSurfaceViewModel = this,
                                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString(),
                                TranslationSetId = info.TranslationSetId,
                                DisplayName = info.DisplayName,
                                ParallelCorpusId = info.ParallelCorpusId,
                                ParallelCorpusDisplayName = info.ParallelCorpusDisplayName,
                                IsRtl = info.IsRTL,
                                SourceParatextId = parallelCorpusConnection.SourceConnector.ParatextId,
                                TargetParatextId = parallelCorpusConnection.DestinationConnector.ParatextId,
                            }
                        }
                });
            }


            connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            { Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = this, IsSeparator = true });

            //connectionMenuItems.Add(new ParallelCorpusConnectionMenuItemViewModel
            //{
            //    // Properties
            //    Header = LocalizationStrings.Get("Pds_PropertiesMenu", Logger),
            //    Id = "PropertiesId",
            //    IconKind = "Settings",
            //    ConnectionViewModel = connection,
            //    ProjectDesignSurfaceViewModel = this
            //});

            parallelCorpusConnection.MenuItems = connectionMenuItems;
        }

        public async Task ExecuteConnectionMenuCommand(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var connectionViewModel = connectionMenuItem.ConnectionViewModel;
            switch (connectionMenuItem.Id)
            {
                case "AddTranslationSetId":
                    // find the right connection to send
                    var connection = DesignSurface.ParallelCorpusConnections.First(c => c.Id == connectionMenuItem.ConnectionId);

                    if (connection is not null)
                    {
                        // kick off the add new tokenization dialog
                        await AddParallelCorpus(connection);
                    }
                    else
                    {
                        Logger!.LogError("Could not find connection with id {0}", connectionMenuItem.ConnectionId);
                    }
                    break;
                case "SeparatorId":
                    // no-op
                    break;
                case "PropertiesId":
                    // node properties
                    SelectedDesignSurfaceComponent = connectionViewModel;
                    break;
                case "CreateNewInterlinearId":
                    await AddNewInterlinear(connectionMenuItem);
                    break;
                case "AddAlignmentToEnhancedViewId":
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new ShowParallelTranslationWindowMessage(
                                null,
                                connectionMenuItem.AlignmentSetId,
                                connectionMenuItem.DisplayName,
                                connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsRtl,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsTargetRTL,
                                IsNewWindow: false,
                                connectionMenuItem.SourceParatextId,
                                connectionMenuItem.TargetParatextId));
                    }
                    break;
                case "AddTranslationToEnhancedViewId":
                    if (connectionMenuItem.IsEnabled)
                    {
                        await EventAggregator.PublishOnUIThreadAsync(
                            new ShowParallelTranslationWindowMessage(
                                connectionMenuItem.TranslationSetId,
                                null,
                                connectionMenuItem.DisplayName,
                                connectionMenuItem.ParallelCorpusId ?? throw new InvalidDataEngineException(name: "ParallelCorpusId", value: "null"),
                                connectionMenuItem.ParallelCorpusDisplayName,
                                //FIXME:surface serialization new EngineStringDetokenizer(new LatinWordDetokenizer()),
                                connectionMenuItem.IsRtl,
                                //FIXME:surface serialization null,
                                null,
                                IsNewWindow: false,
                                connectionMenuItem.SourceParatextId,
                                connectionMenuItem.TargetParatextId));
                    }
                    break;
                default:

                    break;
            }
        }



        public async Task ExecuteCorpusNodeMenuCommand(CorpusNodeMenuItemViewModel corpusNodeMenuItem)
        {
            var corpusNodeViewModel = corpusNodeMenuItem.CorpusNodeViewModel;
            switch (corpusNodeMenuItem.Id)
            {
                case "AddTokenizationId":
                    // kick off the add new tokenization dialog
                    await AddParatextCorpus(corpusNodeViewModel.ParatextProjectId);
                    break;
                case "SeparatorId":
                    // no-op
                    break;

                case "AddToEnhancedViewId":
                case "ShowVerseId":
                    // ShowTokenizationWindowMessage(string ParatextProjectId, string projectName, string TokenizationType, Guid corpusId, Guid tokenizedTextCorpusId);
                    var tokenization = corpusNodeViewModel.Tokenizations.FirstOrDefault(b => b.TokenizationName == corpusNodeMenuItem.Tokenizer);
                    if (tokenization == null)
                    {
                        return;
                    }

                    var showInNewWindow = corpusNodeMenuItem.Id == "ShowVerseId";
                    var corpusId = Guid.Parse(tokenization.CorpusId);
                    var tokenizedTextCorpusId = Guid.Parse(tokenization.TokenizedTextCorpusId);
                    await EventAggregator.PublishOnUIThreadAsync(
                        new ShowTokenizationWindowMessage(
                            corpusNodeViewModel.ParatextProjectId,
                            ProjectName: corpusNodeViewModel.Name,
                            TokenizationType: corpusNodeMenuItem.Tokenizer,
                            CorpusId: corpusId,
                            TokenizedTextCorpusId: tokenizedTextCorpusId,
                            corpusNodeViewModel.CorpusType,
                            //FIXME:new EngineStringDetokenizer(new LatinWordDetokenizer()),
                            corpusNodeViewModel.IsRtl,
                            IsNewWindow: showInNewWindow));
                    break;
                case "PropertiesId":
                    // node properties
                    SelectedDesignSurfaceComponent = corpusNodeViewModel;
                    break;
                case "TokenizerPropertiesId":
                    // get the selected tokenizer
                    var nodeTokenization =
                        corpusNodeViewModel.Tokenizations.FirstOrDefault(b =>
                            b.TokenizationName == corpusNodeMenuItem.Tokenizer);
#pragma warning disable CS8601
                    SelectedDesignSurfaceComponent = nodeTokenization;
#pragma warning restore CS8601
                    break;
            }
        }


        /// <summary>
        /// gets the position below the last node on the surface
        /// </summary>
        /// <returns></returns>
        private Point GetFreeSpot()
        {
            double x = 25;
            double y = 25;
            double yOffset = 0;

            foreach (var corpusNode in DesignSurface!.CorpusNodes)
            {
                var positionX = corpusNode.X;
                var positionY = corpusNode.Y + corpusNode.Size.Height;
                yOffset = corpusNode.Size.Height;

                if (positionX > x && !double.IsNegativeInfinity(positionX) && !double.IsPositiveInfinity(positionX) && !double.IsNaN(positionX))
                {
                    x = positionX;
                }
                if (positionY > y && !double.IsNegativeInfinity(positionY) && !double.IsPositiveInfinity(positionY) && !double.IsNaN(positionY))
                {
                    y = positionY;
                }
            }

            if (double.IsNegativeInfinity(y) || double.IsPositiveInfinity(y) || double.IsNaN(y))
            {
                y = 150;
            }

            if (double.IsNegativeInfinity(x) || double.IsPositiveInfinity(x) || double.IsNaN(x))
            {
                x = 150;
            }

            return new Point(x, y + (yOffset * 0.5));
        }

        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new connection.
        /// </summary>
        public ParallelCorpusConnectionViewModel ConnectionDragStarted(ParallelCorpusConnectorViewModel draggedOutParallelCorpusConnector, Point curDragPoint)
        {
            //
            // Create a new connection to add to the view-model.
            //
            var connection = new ParallelCorpusConnectionViewModel();

            if (draggedOutParallelCorpusConnector.Type == ConnectorType.Output)
            {
                //
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                //
                connection.SourceConnector = draggedOutParallelCorpusConnector;
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                //
                // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                //
                connection.DestinationConnector = draggedOutParallelCorpusConnector;
                connection.SourceConnectorHotspot = curDragPoint;
            }

            //
            // Add the new connection to the view-model.
            //
            DesignSurface.ParallelCorpusConnections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Event raised, to query for feedback, while the user is dragging a connection.
        /// </summary>
        public void OnProjectDesignSurfaceQueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        {
            var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var draggedOverConnector = (ParallelCorpusConnectorViewModel)e.DraggedOverConnector;

            QueryConnectionFeedback(draggedOutConnector, draggedOverConnector, out var feedbackIndicator, out var connectionOk);

            //
            // Return the feedback object to ProjectDesignSurfaceView.
            // The object combined with the data-template for it will be used to create a 'feedback icon' to
            // display (in an adorner) to the user.
            //
            e.FeedbackIndicator = feedbackIndicator;

            //
            // Let ProjectDesignSurfaceView know if the connection is ok or not ok.
            //
            e.ConnectionOk = connectionOk;
        }

        /// <summary>
        /// Event raised when the user has started to drag out a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            if (IsBusy)
            {
                return;
            }

            var draggedOutConnector = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(DesignSurface.ProjectDesignSurface);

            //
            // Delegate the real work to the view model.
            //
            var connection = ConnectionDragStarted(draggedOutConnector, curDragPoint);

            //
            // Must return the view-model object that represents the connection via the event args.
            // This is so that ProjectDesignSurfaceView can keep track of the object while it is being dragged.
            //
            e.Connection = connection;
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            var curDragPoint = Mouse.GetPosition(DesignSurface.ProjectDesignSurface);
            var connection = (ParallelCorpusConnectionViewModel)e.Connection;
            ConnectionDragging(curDragPoint, connection);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a connection.
        /// </summary>
        public void OnParallelCorpusConnectionDragCompleted(object? sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (ParallelCorpusConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ParallelCorpusConnectionViewModel)e.Connection;
            ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        }

        public async void OnCorpusNodeDragCompleted(object? sender, NodeDragCompletedEventArgs? e)
        {
            Logger!.LogInformation("NodeDragCompleted");

            //if (!IsBusy)
            //{
            await SaveDesignSurfaceData();
            //}
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Called to query the application for feedback while the user is dragging the connection.
        /// </summary>
        public void QueryConnectionFeedback(ParallelCorpusConnectorViewModel draggedOutParallelCorpusConnector, ParallelCorpusConnectorViewModel draggedOverParallelCorpusConnector, out object feedbackIndicator, out bool connectionOk)
        {
            if (draggedOutParallelCorpusConnector == draggedOverParallelCorpusConnector)
            {
                //
                // Can't connect to self!
                // Provide feedback to indicate that this connection is not valid!
                //
                feedbackIndicator = new ConnectionBadIndicator();
                connectionOk = false;
            }
            else
            {
                var sourceConnector = draggedOutParallelCorpusConnector;
                var destConnector = draggedOverParallelCorpusConnector;

                //
                // Only allow connections from output connector to input connector (ie each
                // connector must have a different type).
                // Also only allocation from one node to another, never one node back to the same node.
                //
                connectionOk = sourceConnector.ParentNode != destConnector.ParentNode &&
                               sourceConnector.Type != destConnector.Type;

                if (connectionOk)
                {
                    // 
                    // Yay, this is a valid connection!
                    // Provide feedback to indicate that this connection is ok!
                    //
                    feedbackIndicator = new ConnectionOkIndicator();
                }
                else
                {
                    //
                    // Connectors with the same connector type (eg input & input, or output & output)
                    // can't be connected.
                    // Only connectors with separate connector type (eg input & output).
                    // Provide feedback to indicate that this connection is not valid!
                    //
                    feedbackIndicator = new ConnectionBadIndicator();
                }
            }
        }

        /// <summary>
        /// Called as the user continues to drag the connection.
        /// </summary>
        public void ConnectionDragging(Point curDragPoint, ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {

            Logger!.LogDebug($"Current drag point: {curDragPoint.X}, {curDragPoint.Y}");
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (parallelCorpusConnection is not null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (parallelCorpusConnection.DestinationConnector == null)
                {
                    parallelCorpusConnection.DestConnectorHotspot = curDragPoint;
                }
                else
                {
                    parallelCorpusConnection.SourceConnectorHotspot = curDragPoint;
                }
            }
        }

        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// </summary>
        public async void ConnectionDragCompleted(ParallelCorpusConnectionViewModel newParallelCorpusConnection, ParallelCorpusConnectorViewModel parallelCorpusConnectorDraggedOut, ParallelCorpusConnectorViewModel parallelCorpusConnectorDraggedOver)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (parallelCorpusConnectorDraggedOver == null)
            {
                //
                // The connection was unsuccessful.
                // Maybe the user dragged it out and dropped it in empty space.
                //
                this.DesignSurface.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                return;
            }

            //
            // Only allow connections from output connector to input connector (ie each
            // connector must have a different type).
            // Also only allocation from one node to another, never one node back to the same node.
            //
            var connectionOk = parallelCorpusConnectorDraggedOut.ParentNode != parallelCorpusConnectorDraggedOver.ParentNode &&
                               parallelCorpusConnectorDraggedOut.Type != parallelCorpusConnectorDraggedOver.Type;

            if (!connectionOk)
            {
                //
                // Connections between connectors that have the same type,
                // eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                //
                DesignSurface.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                return;
            }

            //
            // The user has dragged the connection on top of another valid connector.
            //

            //
            // Remove any existing connection between the same two connectors.
            //
            var existingConnection = FindConnection(parallelCorpusConnectorDraggedOut, parallelCorpusConnectorDraggedOver);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (existingConnection != null)
            {
                DesignSurface.ParallelCorpusConnections.Remove(existingConnection);
            }

            //
            // Finalize the connection by attaching it to the connector
            // that the user dragged the mouse over.
            //
            bool added;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (newParallelCorpusConnection.DestinationConnector is null)
            {
                newParallelCorpusConnection.DestinationConnector = parallelCorpusConnectorDraggedOver;
                added = true;
            }
            else
            {
                newParallelCorpusConnection.SourceConnector = parallelCorpusConnectorDraggedOver;
                added = true;
            }

            if (added)
            {
                // check to see if we somehow didn't get a source/target id properly.  If so remove the line
                if (newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId == "" || newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                if (newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId == "" || newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId is null)
                {
                    DesignSurface.ParallelCorpusConnections.Remove(newParallelCorpusConnection);
                    return;
                }

                await EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusAddedMessage(
                    SourceParatextId: newParallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                    TargetParatextId: newParallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                    ConnectorGuid: newParallelCorpusConnection.Id));

                var mainViewModel = IoC.Get<MainViewModel>();
                newParallelCorpusConnection.SourceFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newParallelCorpusConnection.SourceConnector.ParentNode
                    .ParatextProjectId);

                newParallelCorpusConnection.TargetFontFamily = mainViewModel.GetFontFamilyFromParatextProjectId(newParallelCorpusConnection.DestinationConnector.ParentNode
                    .ParatextProjectId);

                await AddParallelCorpus(newParallelCorpusConnection);
            }

            await SaveDesignSurfaceData();
        }


        private async Task AddNewInterlinear(ParallelCorpusConnectionMenuItemViewModel connectionMenuItem)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("parallelCorpusId", connectionMenuItem.ParallelCorpusId)
            };

            var dialogViewModel = LifetimeScope!.Resolve<InterlinearDialogViewModel>(parameters);
            var result = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

            if (result)
            {
                var translationSet = await TranslationSet.Create(null, dialogViewModel.SelectedAlignmentSet,
                        dialogViewModel.TranslationSetDisplayName, new Dictionary<string, object>(),
                        dialogViewModel.SelectedAlignmentSet.ParallelCorpusId, Mediator);

                if (translationSet != null)
                {
                    connectionMenuItem.ConnectionViewModel.TranslationSetInfo.Add(new TranslationSetInfo
                    {
                        DisplayName = translationSet.TranslationSetId.DisplayName,
                        TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                        ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                        ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                        AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                        AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName
                    });

                    CreateConnectionMenu(connectionMenuItem.ConnectionViewModel);
                    await SaveDesignSurfaceData();
                }
            }
        }

        public async Task AddParallelCorpus(ParallelCorpusConnectionViewModel newParallelCorpusConnection)
        {
            var sourceCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.SourceConnector.ParentNode.Id);
            if (sourceCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the source Corpus node for the Corpus with Id '{newParallelCorpusConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetCorpusNode = DesignSurface.CorpusNodes.FirstOrDefault(b => b.Id == newParallelCorpusConnection.DestinationConnector.ParentNode.Id);
            if (targetCorpusNode == null)
            {
                throw new MissingCorpusNodeException(
                    $"Cannot find the target Corpus node for the Corpus with Id '{newParallelCorpusConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var sourceNodeTokenization = sourceCorpusNode.Tokenizations.FirstOrDefault();
            if (sourceNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the source TokenizedTextCorpusId associated to Corpus with Id '{newParallelCorpusConnection.SourceConnector.ParentNode.CorpusId}'.");
            }
            var targetNodeTokenization = targetCorpusNode.Tokenizations.FirstOrDefault();
            if (targetNodeTokenization == null)
            {
                throw new MissingTokenizedTextCorpusIdException(
                    $"Cannot find the target TokenizedTextCorpusId associated to Corpus with Id '{newParallelCorpusConnection.DestinationConnector.ParentNode.CorpusId}'.");
            }

            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Add),
                new NamedParameter("connectionViewModel", newParallelCorpusConnection),
                new NamedParameter("sourceCorpusNodeViewModel", sourceCorpusNode),
                new NamedParameter("targetCorpusNodeViewModel", targetCorpusNode)
            };

            var dialogViewModel = LifetimeScope?.Resolve<ParallelCorpusDialogViewModel>(parameters);

            try
            {
                var success = await _windowManager.ShowDialogAsync(dialogViewModel, null, DashboardProjectManager.NewProjectDialogSettings);

                PlaySound.PlaySoundFromResource(null, null);

                if (success)
                {
                    // get TranslationSet , etc from the dialogViewModel
                    var translationSet = dialogViewModel!.TranslationSet;

                    if (translationSet != null)
                    {
                        newParallelCorpusConnection.TranslationSetInfo.Add(new TranslationSetInfo
                        {
                            DisplayName = translationSet.TranslationSetId.DisplayName,
                            TranslationSetId = translationSet.TranslationSetId.Id.ToString(),
                            ParallelCorpusDisplayName = translationSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = translationSet.ParallelCorpusId.Id.ToString(),
                            AlignmentSetId = translationSet.AlignmentSetId.Id.ToString(),
                            AlignmentSetDisplayName = translationSet.AlignmentSetId.DisplayName,
                            SourceFontFamily = newParallelCorpusConnection.SourceFontFamily,
                            TargetFontFamily = newParallelCorpusConnection.TargetFontFamily,
                        });
                    }

                    var alignmentSet = dialogViewModel.AlignmentSet;
                    if (alignmentSet != null)
                    {
                        newParallelCorpusConnection.AlignmentSetInfo.Add(new AlignmentSetInfo
                        {
                            DisplayName = alignmentSet.AlignmentSetId.DisplayName,
                            AlignmentSetId = alignmentSet.AlignmentSetId.Id.ToString(),
                            ParallelCorpusDisplayName = alignmentSet.ParallelCorpusId.DisplayName,
                            ParallelCorpusId = alignmentSet.ParallelCorpusId.Id.ToString(),
                            IsRtl = newParallelCorpusConnection.SourceConnector.ParentNode.IsRtl,
                            IsTargetRtl = newParallelCorpusConnection.DestinationConnector.ParentNode.IsRtl
                        });
                    }

                    newParallelCorpusConnection.ParallelCorpusId = dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId;
                    newParallelCorpusConnection.ParallelCorpusDisplayName =
                        dialogViewModel.ParallelTokenizedCorpus.ParallelCorpusId.DisplayName;
                    CreateConnectionMenu(newParallelCorpusConnection);

                }
                else
                {
                    DeleteConnection(newParallelCorpusConnection);
                }
            }
            finally
            {
                await SaveDesignSurfaceData();
            }


        }

        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        private ParallelCorpusConnectionViewModel FindConnection(ParallelCorpusConnectorViewModel connector1, ParallelCorpusConnectorViewModel connector2)
        {
            Trace.Assert(connector1.Type != connector2.Type);

            //
            // Figure out which one is the source connector and which one is the
            // destination connector based on their connector types.
            //
            var sourceConnector = connector1.Type == ConnectorType.Output ? connector1 : connector2;
            var destConnector = connector1.Type == ConnectorType.Output ? connector2 : connector1;

            //
            // Now we can just iterate attached connections of the source
            // and see if it each one is attached to the destination connector.
            //

            foreach (var connection in sourceConnector.AttachedConnections)
            {
                if (connection.DestinationConnector == destConnector)
                {
                    //
                    // Found a connection that is outgoing from the source connector
                    // and incoming to the destination connector.
                    //
                    return connection;
                }
            }

#pragma warning disable CS8603
            return null;
#pragma warning restore CS8603
        }

        /// <summary>
        /// Delete the currently selected nodes from the view-model.
        /// </summary>
        public void DeleteSelectedNodes()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = this.DesignSurface.CorpusNodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    DeleteCorpusNode(node);
                }
            }
        }

        /// <summary>
        /// Delete the node from the view-model.
        /// Also deletes any connections to or from the node.
        /// </summary>
        public void DeleteCorpusNode(CorpusNodeViewModel node)
        {
            OnUIThread(() =>
            {
                //
                // Remove all connections attached to the node.
                //
                DesignSurface!.ParallelCorpusConnections.RemoveRange(node.AttachedConnections);

                //
                // Remove the node from the design surface.
                //
                DesignSurface.CorpusNodes.Remove(node);

                EventAggregator.PublishOnUIThreadAsync(new CorpusDeletedMessage(node.ParatextProjectId));
            });

        }

        /// <summary>
        /// Create a node and add it to the view-model.
        /// </summary>
        private CorpusNodeViewModel CreateCorpusNode(Corpus corpus, Point nodeLocation)
        {
            if (nodeLocation.X == 0 && nodeLocation.Y == 0)
            {
                // figure out some offset based on the number of nodes already on the design surface
                // so we don't overlap
                nodeLocation = GetFreeSpot();
            }

            var node = new CorpusNodeViewModel(corpus.CorpusId.Name ?? string.Empty, EventAggregator, ProjectManager)
            {
                X = (double.IsNegativeInfinity(nodeLocation.X) || double.IsPositiveInfinity(nodeLocation.X) || double.IsNaN(nodeLocation.X)) ? 150 : nodeLocation.X,
                Y = (double.IsNegativeInfinity(nodeLocation.Y) || double.IsPositiveInfinity(nodeLocation.Y) || double.IsNaN(nodeLocation.Y)) ? 150 : nodeLocation.Y,
                CorpusType = (CorpusType)Enum.Parse(typeof(CorpusType), corpus.CorpusId.CorpusType),
                ParatextProjectId = corpus.CorpusId.ParatextGuid ?? string.Empty,
                CorpusId = corpus.CorpusId.Id,
                IsRtl = corpus.CorpusId.IsRtl,
                TranslationFontFamily = corpus.CorpusId.FontFamily ?? Corpus.DefaultFontFamily,
            };

            node.InputConnectors.Add(new ParallelCorpusConnectorViewModel("Target", EventAggregator, node.ParatextProjectId)
            {
                Type = ConnectorType.Input
            });

            node.OutputConnectors.Add(new ParallelCorpusConnectorViewModel("Source", EventAggregator, node.ParatextProjectId)
            {
                Type = ConnectorType.Output
            });


            //
            // Add the node to the view-model.
            //
            OnUIThread(() =>
            {
                DesignSurface!.CorpusNodes.Add(node);
            });

            EventAggregator.PublishOnUIThreadAsync(new CorpusAddedMessage(node.ParatextProjectId));

            return node;
        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            EventAggregator.PublishOnUIThreadAsync(new ParallelCorpusDeletedMessage(
                SourceParatextId: parallelCorpusConnection.SourceConnector.ParentNode.ParatextProjectId,
                TargetParatextId: parallelCorpusConnection.DestinationConnector.ParentNode.ParatextProjectId,
                ConnectorGuid: parallelCorpusConnection.Id));

            DesignSurface.ParallelCorpusConnections.Remove(parallelCorpusConnection);
        }




        public void ShowCorpusProperties(object corpus)
        {
            SelectedDesignSurfaceComponent = corpus;
        }

        public void ShowConnectionProperties(ParallelCorpusConnectionViewModel parallelCorpusConnection)
        {
            SelectedDesignSurfaceComponent = parallelCorpusConnection;
        }

        public void UiLanguageChangedMessage(UiLanguageChangedMessage message)
        {
            //var language = message.LanguageCode;

            // rerender the context menus
            foreach (var corpusNode in DesignSurface.CorpusNodes)
            {
                CreateCorpusNodeMenu(corpusNode);
            }
        }

        #endregion // Methods


    }
}
