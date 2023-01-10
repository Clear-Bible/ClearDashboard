using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Properties;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using Uri = System.Uri;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class EnhancedViewModel : DashboardConductorAllActive<EnhancedViewItemViewModel>, IPaneViewModel,
        IHandle<VerseSelectedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<BCVLoadedMessage>,
        IHandle<ReloadDataMessage>,
        IHandle<TokenizedCorpusUpdatedMessage>
    {
        #region Commands

        public ICommand MoveCorpusDownRowCommand { get; set; }
        public ICommand MoveCorpusUpRowCommand { get; set; }
        public ICommand DeleteCorpusRowCommand { get; set; }

        #endregion

        #region Member Variables
        private readonly ILogger<EnhancedViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;

        public NoteManager NoteManager { get; }
        private VerseManager VerseManager { get; }
        public SelectionManager SelectionManager { get; }

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";
        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels => Items.Where(item => item.GetType() == typeof(VerseAwareEnhancedViewItemViewModel)).Cast<VerseAwareEnhancedViewItemViewModel>();

        #endregion //Member Variables

        #region Public Properties

        public EnhancedViewLayout? EnhancedViewLayout
        {
            get => _enhancedViewLayout;
            set => Set(ref _enhancedViewLayout, value);
        }




        public MainViewModel MainViewModel => (MainViewModel)Parent;

        private VerseDisplayViewModel _selectedVerseDisplayViewModel;
        public VerseDisplayViewModel SelectedVerseDisplayViewModel
        {
            get => _selectedVerseDisplayViewModel;
            set => Set(ref _selectedVerseDisplayViewModel, value);
        }

        #region BCV
        private bool _paratextSync = true;

        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                if (value)
                {
                    // update Paratext with the verseId
                    //_ = Task.Run(() =>
                    //    ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));

                    // update this window with the Paratext verse
                    CurrentBcv.SetVerseFromId(_projectManager!.CurrentVerse);
                }

                var set = Set(ref _paratextSync, value);
                if (set)
                {
                    EnhancedViewLayout!.ParatextSync = _paratextSync;
                }
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set => Set(ref _bcvDictionary, value);
        }

        private BookChapterVerseViewModel _currentBcv = new();
        public BookChapterVerseViewModel CurrentBcv
        {
            get => _currentBcv;
            set
            {
                var set = Set(ref _currentBcv, value);
                if (set)
                {
                    EnhancedViewLayout!.BBBCCCVVV = _currentBcv.BBBCCCVVV;
                }
            }
        }

        private int _verseOffsetRange;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                var set = Set(ref _verseOffsetRange, value);
                if (set)
                {
                    EnhancedViewLayout!.VerseOffset = value;
#pragma warning disable CS4014
                    VerseChangeRerender();
#pragma warning restore CS4014
                }
            }
        }

        private BookInfo? _currentBook;
        public BookInfo? CurrentBook
        {
            get => _currentBook;
            set
            {
                Set(ref _currentBook, value);
                NotifyOfPropertyChange(() => CurrentBookDisplay);
            }
        }

        private string _verseChange = string.Empty;
        public string VerseChange
        {
            get => _verseChange;
            set
            {
                if (_verseChange == "000000000")
                {
                    return;
                }

                if (_verseChange == "")
                {
                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
                else if (_verseChange != value)
                {
                  
                    ProjectManager!.CurrentVerse = value;
                    // push to Paratext
                    if (ParatextSync && !DashboardProjectManager.IncomingChangesStarted)
                    {
                        Task.Run(() =>
                            ExecuteRequest(new SetCurrentVerseCommand(value), CancellationToken.None)
                        );
                    }

                    _verseChange = value;

#pragma warning disable CS4014
                    VerseChangeRerender();
#pragma warning restore CS4014
                    NotifyOfPropertyChange(() => VerseChange);
                }
            }
        }

        #endregion BCV

        public new string? Title
        {
            get => base.Title;
            set  
            {
                base.Title = value;
                if (EnhancedViewLayout != null)
                {
                    EnhancedViewLayout.Title = value;
                }
               
                NotifyOfPropertyChange(Title);

            }
        }

        #endregion //Public Properties

        #region Observable Properties

        private string _currentCorpusName = string.Empty;
        public string CurrentCorpusName
        {
            get => _currentCorpusName;
            set => Set(ref _currentCorpusName, value);
        }

        private string? _message;
        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private BindableCollection<TokensTextRow>? _verses;
        public BindableCollection<TokensTextRow>? Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        private bool _enableBcvControl;
        public bool EnableBcvControl
        {
            get => _enableBcvControl;
            set => Set(ref _enableBcvControl, value);
        }

        #region DrawerProperties

        private int _titleFontSizeValue = Settings.Default.TitleFontSizeValue;
        public int TitleFontSizeValue
        {
            get => _titleFontSizeValue;
            set
            {
                _titleFontSizeValue = value;
                Settings.Default.TitleFontSizeValue = value;
                NotifyOfPropertyChange(() => TitleFontSizeValue);
            }
        }

        private int _targetFontSizeValue = Settings.Default.TargetFontSizeValue;
        public int TargetFontSizeValue
        {
            get => _targetFontSizeValue;
            set
            {
                _targetFontSizeValue = value;
                Settings.Default.TargetFontSizeValue = value;
                NotifyOfPropertyChange(() => TargetFontSizeValue);
            }
        }


        private int _targetVerticalValue = Settings.Default.TargetVerticalValue;
        public int TargetVerticalValue
        {
            get => _targetVerticalValue;
            set
            {
                _targetVerticalValue = value;
                Settings.Default.TargetVerticalValue = value;
                NotifyOfPropertyChange(() => TargetVerticalValue);
            }
        }


        private int _targetHorizontalValue = Settings.Default.TargetHorizontalValue;
        public int TargetHorizontalValue
        {
            get => _targetHorizontalValue;
            set
            {
                _targetHorizontalValue = value;
                Settings.Default.TargetHorizontalValue = value;
                NotifyOfPropertyChange(() => TargetHorizontalValue);
            }
        }


        private int _sourceFontSizeValue = Settings.Default.SourceFontSizeValue;
        public int SourceFontSizeValue
        {
            get => _sourceFontSizeValue;
            set
            {
                _sourceFontSizeValue = value;
                Settings.Default.SourceFontSizeValue = value;
                NotifyOfPropertyChange(() => SourceFontSizeValue);
            }
        }


        private int _sourceVerticalValue = Settings.Default.SourceVerticalValue;
        public int SourceVerticalValue
        {
            get => _sourceVerticalValue;
            set
            {
                _sourceVerticalValue = value;
                Settings.Default.SourceVerticalValue = value;
                NotifyOfPropertyChange(() => SourceVerticalValue);
            }
        }


        private int _sourceHorizontalValue = Settings.Default.SourceHorizontalValue;
        public int SourceHorizontalValue
        {
            get => _sourceHorizontalValue;
            set
            {
                _sourceHorizontalValue = value;
                Settings.Default.SourceHorizontalValue = value;
                NotifyOfPropertyChange(() => SourceHorizontalValue);
            }
        }


        private int _translationsFontSizeValue = Settings.Default.TranslationsFontSizeValue;
        public int TranslationsFontSizeValue
        {
            get => _translationsFontSizeValue;
            set
            {
                _translationsFontSizeValue = value;
                Settings.Default.TranslationsFontSizeValue = value;
                NotifyOfPropertyChange(() => TranslationsFontSizeValue);
            }
        }


        private int _noteIndicatorsSizeValue = Settings.Default.NoteIndicatorSizeValue;
        public int NoteIndicatorsSizeValue
        {
            get => _noteIndicatorsSizeValue;
            set
            {
                _noteIndicatorsSizeValue = value;
                Settings.Default.NoteIndicatorSizeValue = value;
                NotifyOfPropertyChange(() => NoteIndicatorsSizeValue);
            }
        }

        #endregion

        #endregion Observable Properties

        #region IPaneViewModel

        public ICommand RequestCloseCommand { get; set; }

        private string? _contentId;
        private bool _isSelected;
        private bool _isActive;
        #endregion Member Variables

        #region Public Properties

        public Guid PaneId { get; set; }

        public DockSide DockSide { get; set; }

        public ImageSource? IconSource { get; protected set; }

        public string? ContentId
        {
            get => _contentId;
            set => Set(ref _contentId, value);
        }
 
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    Set(ref _isActive, value);

                    if (this.ContentId == "ENHANCEDVIEW" && value)
                    {
                        // send out a notice that the active document has changed
                        EventAggregator.PublishOnUIThreadAsync(new ActiveDocumentMessage(PaneId));
                    }
                }
            }
        }
        public async Task RequestClose(object obj)
        {
            await EventAggregator.PublishOnUIThreadAsync(new CloseDockingPane(this.PaneId));
        }
        #endregion

        #region Constructor

        // ReSharper disable once UnusedMember.Global
#pragma warning disable CS8618
        public EnhancedViewModel(INavigationService navigationService, 
            ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager, 
            NoteManager noteManager, 
            VerseManager verseManager, 
            SelectionManager selectionManager, 
            IEventAggregator? eventAggregator, 
            IMediator mediator,
            ILifetimeScope? lifetimeScope
            ) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
#pragma warning restore CS8618
        {
            _logger = logger;
            _projectManager = projectManager;
            NoteManager = noteManager;
            VerseManager = verseManager;
            SelectionManager = selectionManager;

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger!);

            ContentId = "ENHANCEDVIEW";

            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);
            RequestCloseCommand = new RelayCommandAsync(RequestClose);

            TokenDisplay.EventAggregator = eventAggregator;
            VerseDisplay.EventAggregator = eventAggregator;
            PaneId = Guid.NewGuid();
        }

        public async Task Initialize(EnhancedViewLayout enhancedViewLayout)
        {
            EnableBcvControl = true;
            EnhancedViewLayout = enhancedViewLayout;

            Title = enhancedViewLayout.Title;
            VerseOffsetRange = enhancedViewLayout.VerseOffset;
            BcvDictionary = ProjectManager!.CurrentParatextProject.BcvDictionary;
            ParatextSync = enhancedViewLayout.ParatextSync;

            EventAggregator.SubscribeOnPublishedThread(this);

            await Task.CompletedTask;
        }

        public async Task AddItem(EnhancedViewItemMetadatum item, CancellationToken cancellationToken)
        {
            EnhancedViewLayout!.EnhancedViewItems.Add(item);
            await ActivateNewVerseAwareViewItem1(item, cancellationToken);
        }

        public async Task LoadData(CancellationToken token)
        {
            await Parallel.ForEachAsync(EnhancedViewLayout!.EnhancedViewItems, new ParallelOptions(), async (enhancedViewItemMetadatum, cancellationToken) =>
            {
                await ActivateNewVerseAwareViewItem1(enhancedViewItemMetadatum, cancellationToken);

            });
        }

        private async Task ActivateNewVerseAwareViewItem(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                var verseAwareEnhancedViewItemViewModel =
                    await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>(cancellationToken);
                await verseAwareEnhancedViewItemViewModel!.GetData(enhancedViewItemMetadatum, cancellationToken);
            });
        }

        private async Task ActivateNewVerseAwareViewItem1(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                var enhancedViewItemViewModel =
                    await ActivateItemAsync1(enhancedViewItemMetadatum, cancellationToken); //FIXME: should not be named with ending "1".
                await enhancedViewItemViewModel!.GetData(enhancedViewItemMetadatum, cancellationToken);
            });
        }

        //FIXME: should go in ClearApplicationFramework
        private Type ConvertEnhancedViewItemMetadatumToEnhancedViewItemViewModelType(EnhancedViewItemMetadatum enhancedViewItemMetadatum)
        {
            var metadataAssemblyQualifiedName = enhancedViewItemMetadatum.GetEnhancedViewItemMetadatumType().AssemblyQualifiedName 
                ?? throw new Exception($"AssemblyQualifiedName is null for type name {enhancedViewItemMetadatum.GetType().Name}");
            var viewModelAssemblyQualifiedName = metadataAssemblyQualifiedName
                .Replace("EnhancedViewItemMetadatum", "EnhancedViewItemViewModel")
                .Replace("Models.ProjectSerialization", "ViewModels.EnhancedView");
            return Type.GetType(viewModelAssemblyQualifiedName) 
                ?? throw new Exception($"AssemblyQualifiedName {viewModelAssemblyQualifiedName} type not found");

        }

        //FIXME: should go in ClearApplicationFramework
        /// <summary>
        /// Expects Metadatum to be in a 'Models.ProjectSerialization' namespace and looks for a ViewModel in a sibling 'ViewModels.EnhancedView' namespace by replacing
        /// EnhancedViewItemMetadatum suffix with EnhancedViewItemViewModel suffix.
        /// </summary>
        /// <param name="enhancedViewItemMetadatum"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<EnhancedViewItemViewModel> ActivateItemAsync1(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnhancedViewItemViewModel viewModel = (EnhancedViewItemViewModel) LifetimeScope.Resolve(ConvertEnhancedViewItemMetadatumToEnhancedViewItemViewModelType(enhancedViewItemMetadatum));
            viewModel.Parent = this;
            viewModel.ConductWith(this);
            UIElement view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync((EnhancedViewItemViewModel)(object)viewModel, cancellationToken);
            return viewModel;
        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";

            // await ActivateItemAsync<TestEnhancedViewItemViewModel>(cancellationToken);
            await base.OnInitializeAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            // grab the dictionary of all the verse lookups
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;

                var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                    .Select(g => g.First())
                    .ToList();

                foreach (var book in books)
                {
                    var bookId = book.Substring(0, 3);

                    var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                    CurrentBcv.BibleBookList?.Add(bookName);
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }

            CurrentBcv.SetVerseFromId(_projectManager!.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = _projectManager.CurrentVerse;

            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Methods

        private async Task VerseChangeRerender()
        {
            var sw =Stopwatch.StartNew();

            EnableBcvControl = false;

            try
            {
                await ReloadData();
            }
            finally
            {
                EnableBcvControl = true;
            }
           

            sw.Stop();
            _logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
        }

        private async Task ReloadData(ReloadType reloadType = ReloadType.Refresh)
        {
            await Parallel.ForEachAsync(VerseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, cancellationToken) =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    await viewModel.RefreshData(reloadType, cancellationToken);
                });

            });
        }

        private void MoveCorpusUp(object obj)
        {
            var row = obj as EnhancedViewItemViewModel;
            var index = MoveableItems.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index < 1)
            {
                return;
            }

            MoveableItems.Move(index, index - 1);
            EnhancedViewLayout!.EnhancedViewItems.Move(index, index - 1);
        }

        private void MoveCorpusDown(object obj)
        {
            var row = obj as EnhancedViewItemViewModel;
            var index = MoveableItems.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

            if (index == Items.Count - 1)
            {
                return;
            }
            MoveableItems.Move(index, index + 1);
            EnhancedViewLayout!.EnhancedViewItems.Move(index, index + 1);

        }

        private void DeleteCorpusRow(object obj)
        {
            var item = (EnhancedViewItemViewModel)obj;
            
            var index = Items.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(item))?.index ?? -1;

            Items.RemoveAt(index);
            EnhancedViewLayout!.EnhancedViewItems.RemoveAt(index);

        }

        public static ParatextProjectMetadata HebrewManuscriptMetadata => new ParatextProjectMetadata
        {
            Id = ManuscriptIds.HebrewManuscriptId,
            CorpusType = CorpusType.ManuscriptHebrew,
            Name = "Macula Hebrew",
            AvailableBooks = BookInfo.GenerateScriptureBookList(),
        };

        public static ParatextProjectMetadata GreekManuscriptMetadata => new ParatextProjectMetadata
        {
            Id = ManuscriptIds.GreekManuscriptId,
            CorpusType = CorpusType.ManuscriptGreek,
            Name = "Macula Greek",
            AvailableBooks = BookInfo.GenerateScriptureBookList(),
        };


        #region IHandle

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (ParatextSync == false)
            {
                return;
            }
            
            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                CurrentBcv.SetVerseFromId(message.Verse);
            }
        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                DashboardProjectManager.IncomingChangesStarted = true;

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                NotifyOfPropertyChange(() => CurrentBcv);
                DashboardProjectManager.IncomingChangesStarted = false;
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }
        }


        public Task HandleAsync(BCVLoadedMessage message, CancellationToken cancellationToken)
        {
            BcvDictionary = _projectManager!.CurrentParatextProject.BcvDictionary;

            return Task.CompletedTask;
        }

        public Task HandleAsync(VerseSelectedMessage message, CancellationToken cancellationToken)
        {
            SelectedVerseDisplayViewModel = message.SelectedVerseDisplayViewModel;
            return Task.CompletedTask;
        }

        public async Task HandleAsync(ReloadDataMessage message, CancellationToken cancellationToken)
        {
            await ReloadData(message.ReloadType);
        }


        public async  Task HandleAsync(TokenizedCorpusUpdatedMessage message, CancellationToken cancellationToken)
        {
            var verseAwareEnhancedViewItemViewModels =
                VerseAwareEnhancedViewItemViewModels.Where(vm =>
                    vm.CorpusId == message.TokenizedTextCorpusId.CorpusId?.Id);

            await Task.Factory.StartNew(async () =>
            {
                await Parallel.ForEachAsync(verseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, token) =>
                {
                    await Execute.OnUIThreadAsync(async () =>
                    {
                        await viewModel.RefreshData(ReloadType.Force, token);
                    });

                });
            }, cancellationToken);
          
        }

        #endregion

        #endregion // Methods

        #region Event Handlers

        #region VerseDisplayControl

        public void TokenClicked(object sender, TokenEventArgs e)
        {
            SelectionManager.UpdateSelection(e.TokenDisplay, e.SelectedTokens, e.IsControlPressed);
            NoteControlVisibility = SelectionManager.AnySelectedNotes ? Visibility.Visible : Visibility.Collapsed;
        }

        public void TokenRightButtonDown(object sender, TokenEventArgs e)
        {
            SelectionManager.UpdateRightClickSelection(e.TokenDisplay);
            NoteControlVisibility = SelectionManager.AnySelectedNotes ? Visibility.Visible : Visibility.Collapsed;
        }

        public void TokenMouseEnter(object sender, TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
        }

        public void TokenMouseLeave(object sender, TokenEventArgs e)
        {
            Message = string.Empty;
        }

        public void TokenJoin(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenJoinAsync(e).GetAwaiter());
        }

        public async Task TokenJoinAsync(TokenEventArgs e)
        {
            await VerseManager.JoinTokensAsync(e.SelectedTokens.TokenCollection, e.TokenDisplay.VerseDisplay.ParallelCorpusId);
        }

        public void TokenUnjoin(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenUnjoinAsync(e).GetAwaiter());
        }

        public async Task TokenUnjoinAsync(TokenEventArgs e)
        {
            await VerseManager.UnjoinTokenAsync(e.TokenDisplay.CompositeToken, e.TokenDisplay.VerseDisplay.ParallelCorpusId);
        }

        public void TranslationMouseEnter(object sender, TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
        }

        public void TranslationMouseLeave(object sender, TranslationEventArgs e)
        {
            Message = string.Empty;
        }

        public void NoteCreate(object sender, NoteEventArgs e)
        {
            NoteControlVisibility = Visibility.Visible;
        }
    
        public void FilterPins(object sender, NoteEventArgs e)
        {
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.SurfaceText));
        }

        public void TranslateQuick(object sender, NoteEventArgs e)
        {
            try
            {
                var surfaceText = e.SelectedTokens.CombinedSurfaceText.Replace(',', ' ');
                var result = TranslateText(surfaceText);
                Message = $"Quick Translation: '{result}'";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Quick translation failed.");
            }
        }

        public string TranslateText(string input)
        {
            string url = string.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "auto", "en", Uri.EscapeUriString(input));
            HttpClient httpClient = new HttpClient();
            string result = httpClient.GetStringAsync(url).Result;

            var items = result.Split("\"");
            var translation = items[1];
            return translation;
        }

        #endregion

        #region NoteControl

        public void NoteAdded(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteAddedAsync(e).GetAwaiter());
        }

        public async Task NoteAddedAsync(NoteEventArgs e)
        {
            OnUIThread(async () =>
            {
                await NoteManager.AddNoteAsync(e.Note, e.EntityIds);
                NotifyOfPropertyChange(() => Items);
            });

            Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public void NoteUpdated(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteUpdatedAsync(e).GetAwaiter());
        }

        public async Task NoteUpdatedAsync(NoteEventArgs e)
        {
            await NoteManager.UpdateNoteAsync(e.Note);
            Message = $"Note '{e.Note.Text}' updated on tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public void NoteSendToParatext(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteSendToParatextAsync(e).GetAwaiter());
        }

        public async Task NoteSendToParatextAsync(NoteEventArgs e)
        {
            try
            {
                await NoteManager.SendToParatextAsync(e.Note);
                Message = $"Note '{e.Note.Text}' sent to Paratext.";
            }
            catch (Exception ex)
            {
                Message = $"Could not send note to Paratext: {ex.Message}";
            }
        }

        public void NoteDeleted(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteDeletedAsync(e).GetAwaiter());
        }

        public async Task NoteDeletedAsync(NoteEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                OnUIThread(async () =>
                {
                    await NoteManager.DeleteNoteAsync(e.Note, e.EntityIds);
                    NotifyOfPropertyChange(() => Items);
                });
            }
            Message = $"Note '{e.Note.Text}' deleted from tokens ({string.Join(", ", e.EntityIds.Select(id => id.ToString()))})";
        }

        public void NoteEditorMouseEnter(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteEditorMouseEnterAsync(e).GetAwaiter());
        }

        public async Task NoteEditorMouseEnterAsync(NoteEventArgs e)
        {
            await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds);
        }

        public void NoteEditorMouseLeave(object sender, NoteEventArgs e)
        {
            Task.Run(() => NoteEditorMouseLeaveAsync(e).GetAwaiter());
        }

        public async Task NoteEditorMouseLeaveAsync(NoteEventArgs e)
        {
            await NoteManager.NoteMouseLeaveAsync(e.Note, e.EntityIds);
        }

        public void LabelAdded(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelAddedAsync(e).GetAwaiter());
        }

        public async Task LabelAddedAsync(LabelEventArgs e)
        {
            if (SelectedVerseDisplayViewModel is null)
            {
                return;
            }

            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await NoteManager.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            }
            Message = $"Label '{e.Label.Text}' added for note";
        }


        public void LabelSelected(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelSelectedAsync(e).GetAwaiter());
        }

        public async Task LabelSelectedAsync(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.AssociateNoteLabelAsync(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' selected for note";
        }

        public void LabelRemoved(object sender, LabelEventArgs e)
        {
            Task.Run(() => LabelRemovedAsync(e).GetAwaiter());
        }

        public async Task LabelRemovedAsync(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.DetachNoteLabel(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' removed for note";
        }

        public void CloseNotePaneRequested(object sender, RoutedEventArgs args)
        {
            NoteControlVisibility = Visibility.Collapsed;
        }

        #endregion

        // ReSharper restore UnusedMember.Global

        #endregion

        #region VerseControlMethods

        private Visibility _noteControlVisibility = Visibility.Collapsed;
        private EnhancedViewLayout? _enhancedViewLayout;
        

        public Visibility NoteControlVisibility
        {
            get => _noteControlVisibility;
            set => Set(ref _noteControlVisibility, value);
        }

        #endregion


    }
}
