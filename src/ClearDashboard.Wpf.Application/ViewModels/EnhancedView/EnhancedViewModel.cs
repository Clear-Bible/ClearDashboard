using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClearApplicationFoundation.Framework.Input;
using Uri = System.Uri;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{

    public class EnhancedViewModel : VerseAwareConductorOneActive, IEnhancedViewModel, IPaneViewModel,
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
        public ICommand IncreaseTextSizeCommand => new RelayCommand(IncreaseTextSize);

        private void IncreaseTextSize(object? commandParameter)
        {
            SourceFontSizeValue += 1;
            TargetFontSizeValue += 1;
            TitleFontSizeValue += 1;
            TranslationsFontSizeValue += 1;
        }

        public ICommand DecreaseTextSizeCommand => new RelayCommand(DecreaseTextSize);

        private void DecreaseTextSize(object? commandParameter)
        {
            SourceFontSizeValue -= 1;
            TargetFontSizeValue -= 1;
            TitleFontSizeValue -= 1;
            TranslationsFontSizeValue -= 1;
        }

        public ICommand ResetTextSizeCommand => new RelayCommand(ResetTextSize);

        private void ResetTextSize(object? commandParameter)
        {
            SourceFontSizeValue = _originalSourceFontSizeValue;
            TargetFontSizeValue = _originalTargetFontSizeValue;
            TitleFontSizeValue = _originalTitleFontSizeValue;
            TranslationsFontSizeValue = _originalTranslationsFontSizeValue;
        }

        public ICommand InsertNoteCommand => new RelayCommand(InsertNote);

        private void InsertNote(object? commandParameter)
        {
            if (SelectionManager.SelectedEntityIds.Count != 0)
            {
                NoteCreate(null, null);
            }
        }

        #endregion

        #region Member Variables

        public NoteManager NoteManager { get; }
        private VerseManager VerseManager { get; }
        public SelectionManager SelectionManager { get; }

        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels => Items.Where(item => item.GetType() == typeof(VerseAwareEnhancedViewItemViewModel)).Cast<VerseAwareEnhancedViewItemViewModel>();

        private int _originalSourceFontSizeValue = 14;
        private int _originalTargetFontSizeValue = 14;
        private int _originalTitleFontSizeValue = 14;
        private int _originalTranslationsFontSizeValue = 16;
        
        #endregion //Member Variables

        #region Public Properties

        private VerseDisplayViewModel _selectedVerseDisplayViewModel;
        public VerseDisplayViewModel SelectedVerseDisplayViewModel
        {
            get => _selectedVerseDisplayViewModel;
            set => Set(ref _selectedVerseDisplayViewModel, value);
        }

        #region BCV

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

   
        public async Task RequestClose(object? obj)
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
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService) :
            base( projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
#pragma warning restore CS8618
        {
            NoteManager = noteManager;
            VerseManager = verseManager;
            SelectionManager = selectionManager;
            
            Title = "⳼ " + LocalizationService!.Get("Windows_EnhancedView");

            ContentId = "ENHANCEDVIEW";

            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);
            RequestCloseCommand = new RelayCommandAsync(RequestClose);

            TokenDisplay.EventAggregator = eventAggregator;
            VerseDisplay.EventAggregator = eventAggregator;
            PaneId = Guid.NewGuid();

            
        }

        public async Task Initialize(EnhancedViewLayout enhancedViewLayout, EnhancedViewItemMetadatum? metadatum, CancellationToken cancellationToken)
        {

            EnableBcvControl = true;
            EnhancedViewLayout = enhancedViewLayout;

            Title = enhancedViewLayout.Title;
            VerseOffsetRange = enhancedViewLayout.VerseOffset;
            BcvDictionary = ProjectManager!.CurrentParatextProject.BcvDictionary;
            ParatextSync = enhancedViewLayout.ParatextSync;
            CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
            VerseChange = ProjectManager.CurrentVerse;

            if (metadatum != null) {

                if (metadatum is TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusEnhancedViewItemMetadatum)
                {
                    CurrentCorpusName = tokenizedCorpusEnhancedViewItemMetadatum.ProjectName!;
                }

                await AddItem(metadatum, cancellationToken);
            }

            EventAggregator.SubscribeOnPublishedThread(this);

            await Task.CompletedTask;
        }

        public async Task AddItem(EnhancedViewItemMetadatum item, CancellationToken cancellationToken)
        {
            EnableBcvControl = false;
            EnhancedViewLayout!.EnhancedViewItems.Add(item);
            try
            {
                await ActivateNewVerseAwareViewItem(item, cancellationToken);
            }
            finally
            {
                EnableBcvControl = true;
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Logger?.LogInformation($"{nameof(EnhancedViewModel)} OnActivateAsync called.");
            await base.OnActivateAsync(cancellationToken);

            if (Items.Count > 0 && !Items.Any(item=>item.HasFocus))
            {
                Items[0].HasFocus = true;
            }
           
        }

        public override async Task LoadData(CancellationToken token)
        {
            await Parallel.ForEachAsync(EnhancedViewLayout!.EnhancedViewItems, new ParallelOptions(), async (enhancedViewItemMetadatum, cancellationToken) =>
            {
                await ActivateNewVerseAwareViewItem(enhancedViewItemMetadatum, cancellationToken);

            });
        }

        //private async Task ActivateNewVerseAwareViewItem(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        //{
        //    await Execute.OnUIThreadAsync(async () =>
        //    {
        //        var verseAwareEnhancedViewItemViewModel =
        //            await ActivateItemAsync<VerseAwareEnhancedViewItemViewModel>(cancellationToken);
        //        await verseAwareEnhancedViewItemViewModel!.GetData(enhancedViewItemMetadatum, cancellationToken);
        //    });
        //}

        private async Task ActivateNewVerseAwareViewItem(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                var enhancedViewItemViewModel = await ActivateItemFromMetadatumAsync(enhancedViewItemMetadatum, cancellationToken); 
                //EnableBcvControl = false;
                await enhancedViewItemViewModel.GetData(enhancedViewItemMetadatum, cancellationToken);

                
               
            });
        }

     

        /// <summary>
        /// Expects Metadatum to be in a 'Models.EnhancedView' namespace and looks for a ViewModel in a sibling 'ViewModels.EnhancedView' namespace by replacing
        /// EnhancedViewItemMetadatum suffix with EnhancedViewItemViewModel suffix.
        /// </summary>
        /// <param name="enhancedViewItemMetadatum"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<EnhancedViewItemViewModel> ActivateItemFromMetadatumAsync(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken = default(CancellationToken))
        {
            var viewModelType = enhancedViewItemMetadatum.ConvertToEnhancedViewItemViewModelType();
            var viewModel = (EnhancedViewItemViewModel) LifetimeScope!.Resolve(viewModelType);
            viewModel.Parent = this;
            viewModel.ConductWith(this);
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);
            await ActivateItemAsync(viewModel, cancellationToken);
            return viewModel;
        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "Enhanced View";
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

                foreach (var bookName in books.Select(book => book.Substring(0, 3)).Select(BookChapterVerseViewModel.GetShortBookNameFromBookNum))
                {
                    CurrentBcv.BibleBookList?.Add(bookName);
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }

            CurrentBcv.SetVerseFromId(ProjectManager!.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = ProjectManager.CurrentVerse;

            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Methods

        protected override async Task ReloadData(ReloadType reloadType = ReloadType.Refresh)
        {
            await Parallel.ForEachAsync(VerseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, cancellationToken) =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    await viewModel.RefreshData(reloadType, cancellationToken);
                });

            });
        }

        private void MoveCorpusUp(object? obj)
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

        private void MoveCorpusDown(object? obj)
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

        private void DeleteCorpusRow(object? obj)
        {
            var item = (EnhancedViewItemViewModel)obj!;
            
            var index = Items.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(item))?.index ?? -1;

            Items.RemoveAt(index);
            EnhancedViewLayout!.EnhancedViewItems.RemoveAt(index);

        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<Views.EnhancedView.EnhancedView>.Show(this, actualWidth, actualHeight);
        }


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

            await Task.CompletedTask;
        }

       

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                UpdatingCurrentVerse = true;
                try
                {
                    // set the CurrentBcv prior to listening to the event
                    CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
                    NotifyOfPropertyChange(() => CurrentBcv);
                }
                finally
                {
                    UpdatingCurrentVerse = false;
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }

            await Task.CompletedTask;
        }


        public Task HandleAsync(BCVLoadedMessage message, CancellationToken cancellationToken)
        {
            BcvDictionary = ProjectManager!.CurrentParatextProject.BcvDictionary;

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
            await VerseManager.JoinTokensAsync(e.SelectedTokens.TokenCollection, null);
        }

        public void TokenJoinLanguagePair(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenJoinLanguagePairAsync(e).GetAwaiter());
        }

        public async Task TokenJoinLanguagePairAsync(TokenEventArgs e)
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

        public void NoteCreate(object? sender, NoteEventArgs? e)
        {
            NoteControlVisibility = Visibility.Visible;
        }
    
        public void FilterPins(object? sender, NoteEventArgs e)
        {
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.SurfaceText));
        }

        public void Copy(object sender, NoteEventArgs e)
        {
            var surfaceText = e.SelectedTokens.CombinedSurfaceText.Replace(',', ' ');
            Clipboard.SetText(surfaceText);
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

        public Visibility NoteControlVisibility
        {
            get => _noteControlVisibility;
            set => Set(ref _noteControlVisibility, value);
        }

        #endregion
        
    }
}
