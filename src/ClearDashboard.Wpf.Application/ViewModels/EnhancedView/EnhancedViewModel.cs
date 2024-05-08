using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Converters;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure.EnhancedView;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.UserControls.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using Uri = System.Uri;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    using ClearDashboard.DAL.Alignment;
    using ClearDashboard.DAL.Alignment.Notes;
    using ClearDashboard.DataAccessLayer.Features.DashboardProjects;
    using ClearDashboard.Wpf.Application.Events.Notes;
    using ClearDashboard.Wpf.Application.ViewModels.Notes;
    using System.Dynamic;
    using System.Linq;  //  needed to move this into the namespace to allow the .Reverse() to use this over the SIL.Linq

    public class EnhancedViewModel : VerseAwareConductorOneActive, IEnhancedViewModel, IPaneViewModel,
        IHandle<VerseSelectedMessage>,
        IHandle<VerseChangedMessage>,
        IHandle<ProjectChangedMessage>,
        IHandle<BCVLoadedMessage>,
        IHandle<ReloadDataMessage>,
        IHandle<TokenizedCorpusUpdatedMessage>,
        IHandle<HighlightTokensMessage>,
        IHandle<UnhighlightTokensMessage>,
        IHandle<ParallelCorpusDeletedMessage>,
        IHandle<CorpusDeletedMessage>,
        IHandle<ExternalNotesUpdatedMessage>,
        IHandle<ReloadExternalNotesDataMessage>
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

   #endregion

        #region Member Variables

        protected IEnhancedViewManager EnhancedViewManager { get; }
        private VerseManager VerseManager { get; }
        public SelectionManager SelectionManager { get; }
        public IWindowManager WindowManager { get; }

        private IEnumerable<VerseAwareEnhancedViewItemViewModel> VerseAwareEnhancedViewItemViewModels => Items.Where(item => item is VerseAwareEnhancedViewItemViewModel).Cast<VerseAwareEnhancedViewItemViewModel>();

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

        private bool _showNoteIndicatorsCheckbox = Settings.Default.NotesIndicatorVisibility;
        public bool ShowNoteIndicatorsCheckbox
        {
            get => _showNoteIndicatorsCheckbox;
            set
            {
                _showNoteIndicatorsCheckbox = value;
                Settings.Default.NotesIndicatorVisibility = value;
                NotifyOfPropertyChange(() => ShowNoteIndicatorsCheckbox);
            }
        }


        private bool _paragraphMode = Settings.Default.ParagraphMode;
        public bool ParagraphMode
        {
            get => _paragraphMode;
            set
            {
                if (_paragraphMode != value)
                {
                    _paragraphMode = value;
                    Settings.Default.ParagraphMode = value;
                    NotifyOfPropertyChange(() => ParagraphMode);
                    if (VerseOffsetRange > 0)
                    {
                        Task.Run(() => EventAggregator.PublishOnUIThreadAsync(new ReloadDataMessage()).GetAwaiter());
                    }
                }
            }
        }


        //private bool _showExternalNotes = AbstractionsSettingsHelper.GetShowExternalNotes();
        //public bool ShowExternalNotes
        //{
        //    get => _showExternalNotes;
        //    set
        //    {
        //        if (_showExternalNotes != value)
        //        {
        //            _showExternalNotes = value;
        //            AbstractionsSettingsHelper.SaveShowExternalNotes(value);
        //            NotifyOfPropertyChange(() => ShowExternalNotes);

        //            EventAggregator.PublishOnUIThreadAsync(new ReloadDataMessage()).GetAwaiter();
        //        }
        //    }
        //}


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
            IEnhancedViewManager enhancedViewManager,
            ILogger<EnhancedViewModel> logger,
            DashboardProjectManager? projectManager,
            //NoteManager noteManager, 
            VerseManager verseManager,
            SelectionManager selectionManager,
            IEventAggregator? eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService, IWindowManager windowManager) :
            base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
#pragma warning restore CS8618
        {

            //NoteManager = noteManager;
            VerseManager = verseManager;
            SelectionManager = selectionManager;
            WindowManager = windowManager;
            EnhancedViewManager = enhancedViewManager;

            Title = "⳼ " + LocalizationService!.Get("Windows_EnhancedView");

            ContentId = "ENHANCEDVIEW";

            MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
            MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
            DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);
            RequestCloseCommand = new RelayCommandAsync(RequestClose);

            TokenDisplay.EventAggregator = eventAggregator;
            VerseDisplay.EventAggregator = eventAggregator;
            VerseDisplay.SelectionManager = selectionManager;
            LabelsEditor.EventAggregator = eventAggregator;
            LabelsDisplay.EventAggregator = eventAggregator;

            LabelSelector.LocalizationService = localizationService;

            PaneId = Guid.NewGuid();
        }

        public async Task Initialize(EnhancedViewLayout enhancedViewLayout, EnhancedViewItemMetadatum? metadatum, CancellationToken cancellationToken)
        {

            EnableBcvControl = true;
            EnhancedViewLayout = enhancedViewLayout;

            DisplayName = enhancedViewLayout.Title;
            Title = enhancedViewLayout.Title;
            VerseOffsetRange = enhancedViewLayout.VerseOffset;

            if (ProjectManager!.CurrentParatextProject is not null)
            {
                BcvDictionary = ProjectManager!.CurrentParatextProject.BcvDictionary;
            }
            else
            {
                // check to see if it has not already been computed
                if (!BcvDictionary.Any())
                {
                    BcvDictionary = await GenerateBcvFromDatabase(cancellationToken);
                }
            }

            ParatextSync = enhancedViewLayout.ParatextSync;
            if (ParatextSync && ProjectManager.CurrentVerse is not null)
            {
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
                VerseChange = ProjectManager.CurrentVerse;
            }
            else
            {
                CurrentBcv.SetVerseFromId(enhancedViewLayout.BBBCCCVVV);
                VerseChange = enhancedViewLayout.BBBCCCVVV!;
            }



            if (metadatum != null)
            {

                if (metadatum is TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusEnhancedViewItemMetadatum)
                {
                    CurrentCorpusName = tokenizedCorpusEnhancedViewItemMetadatum.ProjectName!;
                }

                _ = Task.Run(async () =>
                {
                    await AddItem(metadatum, cancellationToken);
                });


            }

            EventAggregator.SubscribeOnPublishedThread(this);

            await Task.CompletedTask;
        }

        private async Task<Dictionary<string, string>> GenerateBcvFromDatabase(CancellationToken cancellationToken)
        {
            // This is a hack to get the BcvDictionary to be populated when there is no Paratext project loaded.
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIdsAsync(LifetimeScope!, cancellationToken);

            if (topLevelProjectIds.CorpusIds is null)
            {
                return new Dictionary<string, string>();
            }

            // get the first project id
            var firstProjectId = topLevelProjectIds.CorpusIds.FirstOrDefault(t => t.CorpusType == "Standard");

            if (firstProjectId is null)
            {
                return new Dictionary<string, string>();
            }

            var results =
                await ExecuteRequest(
                    new GetProjectBBBCCCVVVQuery(ProjectManager.CurrentDashboardProject.FullFilePath, firstProjectId.Id),
                    CancellationToken.None);

            var bcvDictionary = new Dictionary<string, string>();
            if (results.Success)
            {
                var data = results.Data;
                foreach (var verse in data)
                {
                    bcvDictionary.Add(verse, verse);
                }
            }

            if (ProjectManager!.CurrentParatextProject is null)
            {
                ProjectManager.CurrentParatextProject = new ParatextProject();
                ProjectManager.CurrentParatextProject.Language = new ScrLanguageWrapper();
            }

            ProjectManager!.CurrentParatextProject.BcvDictionary = bcvDictionary;

            return bcvDictionary;
        }

        public async Task AddItem(EnhancedViewItemMetadatum item, CancellationToken cancellationToken)
        {
            EnableBcvControl = false;
            EnhancedViewLayout!.EnhancedViewItems.Add(item);
            await Task.Run(async () =>
            {
                try
                {
                    await ActivateNewVerseAwareViewItem(item, cancellationToken);
                    await Items.Last().GetData(cancellationToken);
                }
                finally
                {
                    await EnhancedViewManager.SaveProjectData();
                    EnableBcvControl = true;
                }
            }, cancellationToken);
        }
        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Logger?.LogInformation($"{nameof(EnhancedViewModel)} OnActivateAsync called.");
            await base.OnActivateAsync(cancellationToken);

            if (Items.Count > 0 && !Items.Any(item => item.HasFocus))
            {
                Items[0].HasFocus = true;
            }
        }

        public override async Task LoadData(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                // Activate (and draw the items on the EnhancedView) in the order they have been defined.
                foreach (var enhancedViewItemMetadatum in EnhancedViewLayout!.EnhancedViewItems)
                {
                    await ActivateNewVerseAwareViewItem(enhancedViewItemMetadatum, token);
                }

                // Then get the data in a parallel fashion.  Note the use of the Items collection, not the EnhancedViewItems
                // from the EnhancedViewLayout.
                await Parallel.ForEachAsync(Items, new ParallelOptions(), async (item, cancellationToken) =>
                {
                    await item.GetData(cancellationToken);
                });

            }, token);
        }

        private async Task ActivateNewVerseAwareViewItem(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                var enhancedViewItemViewModel = await ActivateItemFromMetadatumAsync(enhancedViewItemMetadatum, cancellationToken);
                enhancedViewItemViewModel.EnhancedViewItemMetadatum = enhancedViewItemMetadatum;

            });
        }

        /// <summary>
        /// Expects Metadatum to be in a 'Models.EnhancedView' namespace and looks for a ViewModel in a sibling 'ViewModels.EnhancedView' namespace by replacing
        /// EnhancedViewItemMetadatum suffix with EnhancedViewItemViewModel suffix.
        /// </summary>
        /// <param name="enhancedViewItemMetadatum"></param>
        /// <param name="editMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task<EnhancedViewItemViewModel> ActivateItemFromMetadatumAsync(EnhancedViewItemMetadatum enhancedViewItemMetadatum, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var editMode = (enhancedViewItemMetadatum.GetType() == typeof(AlignmentEnhancedViewItemMetadatum))
                    ? ((AlignmentEnhancedViewItemMetadatum)enhancedViewItemMetadatum).EditMode
                    : EditMode.MainViewOnly;
                var viewModelType = enhancedViewItemMetadatum.ConvertToEnhancedViewItemViewModelType();
                var viewModel = (EnhancedViewItemViewModel)LifetimeScope!.Resolve(viewModelType, new NamedParameter("editMode", editMode));
                viewModel.Parent = this;
                viewModel.ConductWith(this);
                var view = ViewLocator.LocateForModel(viewModel, null, null);
                ViewModelBinder.Bind(viewModel, view, null);
                await ActivateItemAsync(viewModel, cancellationToken);
                return viewModel;
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, "An unexpected error occurred while activating a 'EnhancedViewItemViewModel'");
                throw;
            }

        }
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            DisplayName = "View";
            await base.OnInitializeAsync(cancellationToken);
        }



        protected override async void OnViewAttached(object view, object context)
        {
            if (ProjectManager?.CurrentParatextProject is null)
            {
                BcvDictionary = await GenerateBcvFromDatabase(CancellationToken.None);
            }
            else
            {
                // grab the dictionary of all the verse lookups
                BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
            }

            var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                .Select(g => g.First())
                .ToList();

            foreach (var bookName in books.Select(book => book.Substring(0, 3)).Select(BookChapterVerseViewModel.GetShortBookNameFromBookNum))
            {
                CurrentBcv.BibleBookList?.Add(bookName);
            }

            CurrentBcv.SetVerseFromId(ProjectManager!.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = ProjectManager.CurrentVerse;

            base.OnViewAttached(view, context);
        }

        private ListView EnhancedViewListView { get; set; }

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

        public async Task ReloadExternalNotesData(ReloadType reloadType = ReloadType.Refresh)
        {
            await Parallel.ForEachAsync(VerseAwareEnhancedViewItemViewModels, new ParallelOptions(), async (viewModel, cancellationToken) =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    await viewModel.RefreshExternalNotesData(reloadType, cancellationToken);
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

        private async void DeleteCorpusRow(object? obj)
        {
            var item = (EnhancedViewItemViewModel)obj!;

            var index = Items.Select((element, index) => new { element, index })
                .FirstOrDefault(x => x.element.Equals(item))?.index ?? -1;

            Items.RemoveAt(index);
            EnhancedViewLayout!.EnhancedViewItems.RemoveAt(index);

            await EnhancedViewManager.SaveProjectData();
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<Views.EnhancedView.EnhancedView>.Show(this, actualWidth, actualHeight, this.Title);
        }


        #region IHandle

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (ParatextSync == false && message.OverrideParatextSync == false)
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
                BcvDictionary = await GenerateBcvFromDatabase(cancellationToken);
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


        public async Task HandleAsync(TokenizedCorpusUpdatedMessage message, CancellationToken cancellationToken)
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

        public async Task HandleAsync(HighlightTokensMessage message, CancellationToken cancellationToken)
        {
            foreach (var enhancedViewItemViewModel in Items.Where(item => item is VerseAwareEnhancedViewItemViewModel).Cast<VerseAwareEnhancedViewItemViewModel>())
            {
                await enhancedViewItemViewModel.HighlightTokensAsync(message, cancellationToken);
            }
        }

        public async Task HandleAsync(UnhighlightTokensMessage message, CancellationToken cancellationToken)
        {
            foreach (var enhancedViewItemViewModel in Items.Where(item => item is VerseAwareEnhancedViewItemViewModel).Cast<VerseAwareEnhancedViewItemViewModel>())
            {
                await enhancedViewItemViewModel.UnhighlightTokensAsync(message, cancellationToken);
            }
        }

        public async Task HandleAsync(ExternalNotesUpdatedMessage message, CancellationToken cancellationToken)
        {
            foreach (var enhancedViewItemViewModel in Items.Where(item => item is VerseAwareEnhancedViewItemViewModel).Cast<VerseAwareEnhancedViewItemViewModel>())
            {
                await enhancedViewItemViewModel.GetData(cancellationToken);
            }
        }

        public Task HandleAsync(ParallelCorpusDeletedMessage message, CancellationToken cancellationToken)
        {
            bool deleteHappened = false;
            var parallelCorpusID = message.ParallelCorpusId;

            var verseItems = Items.Where(item => item is VerseAwareEnhancedViewItemViewModel)
                .Cast<VerseAwareEnhancedViewItemViewModel>();

            foreach (var item in verseItems.Reverse())
            {
                if (item.ParallelCorpusId == parallelCorpusID)
                {
                    Items.Remove(item);
                    deleteHappened = true;
                }
            }

            if (deleteHappened)
            {
                NotifyOfPropertyChange(() => VerseAwareEnhancedViewItemViewModels);
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(CorpusDeletedMessage message, CancellationToken cancellationToken)
        {
            bool deleteHappened = false;
            var corpusId = message.CorpusId;

            var verseItems = Items.Where(item => item is VerseAwareEnhancedViewItemViewModel)
                .Cast<VerseAwareEnhancedViewItemViewModel>();

            foreach (var item in verseItems.Reverse())
            {
                if (item.CorpusId == corpusId)
                {
                    Items.Remove(item);
                    deleteHappened = true;
                }
            }

            if (deleteHappened)
            {
                NotifyOfPropertyChange(() => VerseAwareEnhancedViewItemViewModels);
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(ReloadExternalNotesDataMessage message, CancellationToken cancellationToken)
        {
            ReloadExternalNotesData(message.reloadType);
            return Task.CompletedTask;
        }


        #endregion

        #endregion // Methods

        #region Event Handlers

        #endregion

        #region VerseDisplayControl

        public async void TokenClicked(object sender, TokenEventArgs e)
        {
        }

        public async void NoteIndicatorMouseEnter(object? sender, NoteEventArgs? e)
        {
           
            if (SelectionManager.AnySelectedNotes)
            {
                await DisplayJotsEditor(e.MousePosition);
            }
        }

        public async void NoteLeftButtonDown(object? sender, NoteEventArgs? e)
        {
            // select the attached token
            if (e.SelectedTokens.Count == 0)
            {
                e.TokenDisplayViewModel.IsTokenSelected = true;
                e.SelectedTokens.Add(e.TokenDisplayViewModel);
            }

            // 3
            if (SelectionManager.AnySelectedNotes)
            {
                // requires tokens to have been selected
                await DisplayJotsEditor(e.MousePosition);
            }
        }

        public async void NoteLeftButtonDown(object sender, TokenEventArgs e)
        {
            if (SelectionManager.AnySelectedNotes)
            {
                await DisplayJotsEditor(e.MousePosition);
            }
        }

        
        public async void TokenLeftButtonDown(object sender, TokenEventArgs e)
        {
            // 3

            if (e.IsShiftPressed && e.TokenDisplay.VerseDisplay is AlignmentDisplayViewModel alignmentDisplayViewModel)
            {
                if (SelectionManager.AnySourceTokens && SelectionManager.AnyTargetTokens)
                {
                    await EventAggregator.PublishOnUIThreadAsync(new HighlightTokensMessage(e.TokenDisplay.IsSource, e.TokenDisplay.AlignmentToken.TokenId), CancellationToken.None);
                    await alignmentDisplayViewModel.AlignmentManager!.AddAlignment(e.TokenDisplay);
                    var element = (UIElement)sender;
                    EnhancedFocusScope.SetFocusOnActiveElementInScope(element);
                }
                else
                {
                    Logger!.LogInformation("There are no source tokens, so skipping the call to add an alignment.");
                }

            }
            else
            {
                if (e.TokenDisplay.IsTokenSelected)
                {
                    SelectionManager.StartDragSelection(e.TokenDisplay);
                }
                else
                {
                    SelectionManager.EndDragSelection(e.TokenDisplay);
                }
                SelectionManager.UpdateSelection(e.TokenDisplay, e.SelectedTokens, e.IsControlPressed);
            }
        }

        public async void TokenLeftButtonUp(object sender, TokenEventArgs e)
        {
            SelectionManager.UpdateSelection(e.TokenDisplay, e.SelectedTokens, e.IsControlPressed);
            SelectionManager.EndDragSelection(e.TokenDisplay);
        }

        public async void TokenCreateAlignment(object sender, TokenEventArgs e)
        {
            if (SelectionManager.SelectedTokens.SourceTokenCount == 1 && SelectionManager.SelectedTokens.TargetTokenCount == 1)
            {
                if (e is { TokenDisplay.VerseDisplay: AlignmentDisplayViewModel alignmentDisplayViewModel })
                {
                    await alignmentDisplayViewModel.AlignmentManager!.AddAlignment(SelectionManager.SelectedSourceTokens.First(), SelectionManager.SelectedTargetTokens.First());
                }
            }
            else
            {
                Logger.LogError($"Could not create manual alignment with {SelectionManager.SelectedSourceTokens.Count} source tokens and {SelectionManager.SelectedTargetTokens.Count} target tokens selected.");
            }
        }

        public async void TokenDeleteAlignment(object sender, TokenEventArgs e)
        {
            if (e is { TokenDisplay.VerseDisplay: AlignmentDisplayViewModel alignmentDisplayViewModel })
            {
                await alignmentDisplayViewModel.AlignmentManager!.DeleteAlignment(e.TokenDisplay);
            }

        }

        public async void TokenRightButtonDown(object sender, TokenEventArgs e)
        {
            SelectionManager.UpdateRightClickSelection(e.TokenDisplay);
        }

        public void TokenMouseEnter(object sender, TokenEventArgs e)
        {
            if (SelectionManager.IsDragInProcess)
            {
                SelectionManager.UpdateSelection(e.TokenDisplay, e.SelectedTokens, e.IsControlPressed);
            }

            Message = $"'{e.TokenDisplay.SurfaceText}' token ({e.TokenDisplay.Token.TokenId}) hovered";
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

        public void TokenSplit(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenSplitAsync(e).GetAwaiter());
        }

        public async Task TokenSplitAsync(TokenEventArgs args)
        {
            async Task ShowSplitTokenDialog()
            {
                var item = ActiveItem as VerseAwareEnhancedViewItemViewModel;
                var dialogViewModel = LifetimeScope!.Resolve<SplitTokenDialogViewModel>();
                dialogViewModel.TokenDisplay = args.TokenDisplay;
                dialogViewModel.TokenFontFamily = item.SourceFontFamily;
                _ = await WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
            }
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(ShowSplitTokenDialog);
        }

        public void TokenUnjoin(object sender, TokenEventArgs e)
        {
            Task.Run(() => TokenUnjoinAsync(e).GetAwaiter());
        }

        public async Task TokenUnjoinAsync(TokenEventArgs e)
        {
            await VerseManager.UnjoinTokenAsync(e.TokenDisplay.CompositeToken, e.TokenDisplay.VerseDisplay.ParallelCorpusId);
        }

        public async void TranslationClicked(object sender, TranslationEventArgs e)
        {
            SelectionManager.UpdateSelection(e.TokenDisplay!, e.SelectedTokens, e.IsControlPressed);
        }


        //JOTS Refactor -- can probably be deleted
        public async void TranslationRightButtonDown(object sender, TranslationEventArgs e)
        {
            if (e.TokenDisplay is not null)
            {
                //SelectionManager.UpdateRightClickTranslationSelection(e.TokenDisplay);
                //NoteControlVisibility = SelectionManager.AnySelectedTokenTranslationNotes ? Visibility.Visible : Visibility.Collapsed;

                SelectionManager.UpdateRightClickSelection(e.TokenDisplay);
            }
        }

        public void TranslationMouseEnter(object sender, TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
        }

        public void TranslationMouseLeave(object sender, TranslationEventArgs e)
        {
            Message = string.Empty;
        }

        public async void NoteCreate(object? sender, NoteEventArgs? e)
        {
            var dpi = VisualTreeHelper.GetDpi(App.Current.Windows[0]);
            var location = (e.OriginalSource as FrameworkElement).PointToScreen(new Point(0,0));
            await DisplayJotsEditor(e?.MousePosition);
        }

        public async Task DisplayJotsEditor(Point? mousePosition, List<NoteId>? notedIds = null, List<IId>? entityIds = null)
        {
            const int height = 600;
            const int width = 800;

            //Point? mousePosition

            dynamic settings = new ExpandoObject();
            settings.MinWidth = width;
            settings.MinHeight = height;
            settings.Height = height;
            settings.Width = width;
            settings.MaxWidth = width;
            settings.MaxHeight = height;
            settings.Title = "Jot";

#if HIGHDPIDEBUG
            
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            
#else
            if (mousePosition.HasValue)
            {
                settings.Top = mousePosition.Value.Y;
                settings.Left = mousePosition.Value.X;
                settings.WindowStartupLocation = WindowStartupLocation.Manual;
            }
            else
            {
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
#endif

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            var jotsEditorViewModel = LifetimeScope.Resolve<JotsEditorViewModel>();

            // NB:  The following three lines need to be executed in this order
            //  so that the dialog will be drawn while the data is being fetched.
            jotsEditorViewModel.SelectionManager = SelectionManager.Clone();

            await WindowManager.ShowWindowAsync(jotsEditorViewModel, null, settings);
            jotsEditorViewModel.Initialize(notedIds, entityIds);

        }

        public void FilterPins(object? sender, NoteEventArgs e)
        {
            //5
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.SurfaceText));
        }

        public void FilterPinsTarget(object? sender, NoteEventArgs e)
        {
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.TargetTranslationText));
        }

        public void FilterPinsByBiblicalTerms(object? sender, NoteEventArgs e)
        {
            EventAggregator.PublishOnUIThreadAsync(new FilterPinsMessage(e.TokenDisplayViewModel.SurfaceText, XmlSource.BiblicalTerms));
        }

        public void Copy(object sender, NoteEventArgs e)
        {
            TokenDisplayViewModelCollection sortedTokens = new TokenDisplayViewModelCollection(e.SelectedTokens.OrderBy(x => x.Token.Position));
            sortedTokens.Refresh();
            var surfaceText = sortedTokens.CombinedSurfaceText.Replace(",", "");
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
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={"auto"}&tl={"en"}&dt=t&q={Uri.EscapeDataString(input)}";
            var httpClient = new HttpClient();
            var result = httpClient.GetStringAsync(url).Result;

            var items = result.Split("\"");
            var translation = items[1];
            return translation;
        }

        #endregion

       
    }
}
