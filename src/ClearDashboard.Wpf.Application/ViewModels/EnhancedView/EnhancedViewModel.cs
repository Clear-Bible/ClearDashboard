using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Collections;
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
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using Uri = System.Uri;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class EnhancedViewModel : DashboardConductorAllActive<EnhancedViewItemViewModel>, IPaneViewModel,
        IHandle<VerseSelectedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<BCVLoadedMessage>
    {

        #region Commands

        public ICommand MoveCorpusDownRowCommand { get; set; }
        public ICommand MoveCorpusUpRowCommand { get; set; }
        public ICommand DeleteCorpusRowCommand { get; set; }

        #endregion

        #region Member Variables
        private readonly ILogger<EnhancedViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
       
     
      
        private string? _message;
      

        private string CurrentBookDisplay => string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";
        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels => Items.Where(item => item.GetType() == typeof(VerseAwareEnhancedViewItemViewModel)).Cast<VerseAwareEnhancedViewItemViewModel>();



        #endregion //Member Variables

        #region Public Properties

        public EnhancedViewLayout? EnhancedViewLayout
        {
            get => _enhancedViewLayout;
            set => Set(ref _enhancedViewLayout, value);
        }

        public bool IsRtl { get; set; }

   
        public NoteManager NoteManager { get; set; }

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


        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());

        public EngineStringDetokenizer TargetDetokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());

        public IEnumerable<Translation> CurrentTranslations { get; set; }

        //private IEnumerable<Label> _labelSuggestions;
        //public IEnumerable<Label> LabelSuggestions
        //{
        //    get => _labelSuggestions;
        //    set => Set(ref _labelSuggestions, value);
        //}

        private TokenDisplayViewModel _currentToken;
        public TokenDisplayViewModel TokenForTranslation
        {
            get => _currentToken;
            set => Set(ref _currentToken, value);
        }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set => Set(ref _selectedTokens, value);
        }


        private IEnumerable<TranslationOption> _translationOptions;
        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            set => Set(ref _translationOptions, value);
        }

        private TranslationOption? _currentTranslationOption;
        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            set => Set(ref _currentTranslationOption, value);
        }

        #endregion //Observable Properties

        #region IPaneViewModel

        public ICommand RequestCloseCommand { get; set; }

        //private string _title = null;
        private string? _contentId;
        private bool _isSelected;
        private bool _isActive;
        #endregion //Member Variables

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
        public EnhancedViewModel(INavigationService navigationService, ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager, NoteManager noteManager, IEventAggregator? eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager,
                eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
#pragma warning restore CS8618
        {

            _logger = logger;
            _projectManager = projectManager;
            NoteManager = noteManager;
          

            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger!);

            ContentId = "ENHANCEDVIEW";

            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);
            RequestCloseCommand = new RelayCommandAsync(RequestClose);

            VerseDisplay.EventAggregator = eventAggregator;
            PaneId = Guid.NewGuid();
        }

        public async Task Initialize(EnhancedViewLayout enhancedViewLayout)
        {
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
            await ActivateNewVerseAwareViewItem(item, cancellationToken);
        }

        public async Task LoadData(CancellationToken token)
        {
            await Parallel.ForEachAsync(EnhancedViewLayout!.EnhancedViewItems, new ParallelOptions(), async (enhancedViewItemMetadatum, cancellationToken) =>
            {
                await ActivateNewVerseAwareViewItem(enhancedViewItemMetadatum, cancellationToken);

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


        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //no-op for now.
           return base.OnDeactivateAsync(close, cancellationToken);
        }


        #endregion //Constructor

        #region Methods


        private async Task VerseChangeRerender()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            await ReloadData();

            sw.Stop();
            _logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
        }

        private async Task ReloadData()
        {
            await Parallel.ForEachAsync(VerseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, cancellationToken) =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    await viewModel.RefreshData(cancellationToken);
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //public void VerseSelected(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        if (e.AddedItems[0] is VerseDisplayViewModel verseDisplayViewModel)
        //        {
        //            SelectedVerseDisplayViewModel = verseDisplayViewModel;
        //        }
        //    }
        //}


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

            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{DisplayName}: Project Change"), cancellationToken);
                CurrentBcv.SetVerseFromId(message.Verse);
            }
        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{DisplayName}: Project Change"), cancellationToken);

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

        #endregion

        #endregion // Methods

        #region Event Handlers

        #region VerseDisplayControl

        private void UpdateSelection(TokenDisplayViewModel token, TokenDisplayViewModelCollection selectedTokens, bool addToSelection)
        {
            if (addToSelection)
            {
                foreach (var selectedToken in selectedTokens)
                {
                    if (!SelectedTokens.Contains(selectedToken))
                    {
                        SelectedTokens.Add(selectedToken);
                    }
                }

                if (!token.IsSelected)
                {
                    SelectedTokens.Remove(token);
                }
            }
            else
            {
                SelectedTokens = selectedTokens;
            }
            EventAggregator.PublishOnUIThreadAsync(new SelectionUpdatedMessage(SelectedTokens));
        }

        public void TokenClicked(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenClickedAsync(e).GetAwaiter());
        }

        public async Task TokenClickedAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplay, e.SelectedTokens, (e.ModifierKeys & ModifierKeys.Control) > 0);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NoteControlVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId})";
        }

        public void TokenRightButtonDown(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenRightButtonDownAsync(e).GetAwaiter());
        }

        public async Task TokenRightButtonDownAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplay, e.SelectedTokens, false);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NoteControlVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) right-clicked";
        }

        public void TokenMouseEnter(object sender, TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
        }

        public void TokenMouseLeave(object sender, TokenEventArgs e)
        {
            Message = string.Empty;
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
