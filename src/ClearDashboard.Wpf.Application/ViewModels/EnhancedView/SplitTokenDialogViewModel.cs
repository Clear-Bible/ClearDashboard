using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DataAccessLayer.Features.Grammar;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.UserControls.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Token = ClearBible.Engine.Corpora.Token;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

// ReSharper disable UnusedMember.Global

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class SplitTokenDialogViewModel : DashboardApplicationScreen
    {
        public VerseManager? VerseManager { get; }


        public BindableCollection<SplitInstructionViewModel> SplitInstructionsViewModels { get; private set; } = new();

        public BindableCollection<Grammar> GrammarSuggestions { get; private set; } = new();

        //private SplitInstructions? _splitInstructions;

        public SplitInstructionsViewModel SplitInstructionsViewModel
        {
            get => _splitInstructionsViewModel;
            private set => Set(ref _splitInstructionsViewModel, value);
        }

        public List<SplitTokenPropagationComboItem> PropagationOptions { get; } = new();

        public SplitTokenPropagationScope SelectedPropagationOption
        {
            get => _selectedPropagationOption;
            set => Set(ref _selectedPropagationOption, value);
        }

        public SplitTokenPropagationComboItem SelectedSplitTokenPropagationComboItem
        {
            get => _selectedPropagationKeyComboItem;
            set
            {
                Set(ref _selectedPropagationKeyComboItem, value);
                SelectedPropagationOption = value.SelectedValuePath;
            }
        }

        private FontFamily? _tokenFontFamily;
        public FontFamily? TokenFontFamily
        {
            get => _tokenFontFamily;
            set
            {
                _tokenFontFamily = value;
                NotifyOfPropertyChange();
            }
        }

        private TokenDisplayViewModel? _tokenDisplay;
        public TokenDisplayViewModel TokenDisplay
        {
            get
            {
                if (_tokenDisplay == null) throw new Exception("Cannot perform operation as TokenDisplay is null");
                return _tokenDisplay;
            }
            set
            {
                _tokenDisplay = value;
                //_splitInstructions ??= SplitInstructions.CreateSplits(_tokenDisplay.SurfaceText, []);
                SplitInstructionsViewModel =
                    SplitInstructionViewModelFactory.CreateSplits(_tokenDisplay.SurfaceText, [], LifetimeScope!);
                NotifyOfPropertyChange(nameof(TokenCharacters));
            }
        }

        private TokenCharacterViewModelCollection? _tokenCharacters;
        public TokenCharacterViewModelCollection TokenCharacters
        {
            get
            {
                return _tokenCharacters ??= new TokenCharacterViewModelCollection(TokenDisplay.SurfaceText);
            }
        }

      

        /// <summary>
        /// The <see cref="InterlinearDisplayViewModel"/> .
        /// </summary>
        public InterlinearDisplayViewModel? InterlinearDisplay { get; set; }

        /// <summary>
        /// The <see cref="Translation"/> to which this event pertains.
        /// </summary>
        public Translation? Translation { get; set; }

        public string TranslationActionType { get; set; } = string.Empty;


        private string _wordGloss = string.Empty;
        public string WordGloss
        {
            get => _wordGloss;
            set => Set(ref _wordGloss, value);
        }

        public bool HasWordGloss => !string.IsNullOrEmpty(WordGloss);

        public string DialogTitle => $"{LocalizationService!["EnhancedView_SplitToken"]}: {TokenDisplay.SurfaceText}";

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        public bool ApplyEnabled
        {
            get => _applyEnabled;
            set => Set(ref _applyEnabled, value);
        }

        private bool _cancelEnabled = true;
        private SplitInstructionsViewModel _splitInstructionsViewModel;
        private SplitTokenPropagationComboItem _selectedPropagationKeyComboItem;
        private SplitTokenPropagationScope _selectedPropagationOption = SplitTokenPropagationScope.All;
        private bool _applyEnabled;

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            private set => Set(ref _cancelEnabled, value);
        }

        public void CharacterClicked(object source, RoutedEventArgs args)
        {
            var tokenCharacterArgs = args as TokenCharacterEventArgs;

            var tokenCharacter = tokenCharacterArgs!.TokenCharacter;
            var splitIndex = tokenCharacter.Index + 1;

            //SplitInstructionsViewModel.UpdateInstructions(tokenCharacter.Index, tokenCharacter.IsSelected);

            if (tokenCharacter.IsSelected)
            {

                if (!SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
                {
                    SplitInstructionsViewModel.SplitIndexes.Add(splitIndex);
                    var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();

                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope);
                    ApplyEnabled = true;
                }
            }
            else
            {

                if (SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
                {
                    SplitInstructionsViewModel.SplitIndexes.Remove(splitIndex);
                    var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();
                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope!);
                    ApplyEnabled = true;
                }

            }

        }

        private class SplitInstructionComparer : IEqualityComparer<SplitInstructionViewModel>

        {
            public bool Equals(SplitInstructionViewModel x, SplitInstructionViewModel y)
            {
                return x.TokenText.Equals(y.TokenText);
            }
            // GetHashCode() must return the same value for equal objects.
            public int GetHashCode(SplitInstructionViewModel vm)
            {
                return vm.TokenText.GetHashCode();
            }
        }

        public async void ApplySplits()
        {
            try
            {
                OnUIThread(() =>
                {
                    ProgressBarVisibility = Visibility.Visible;
                    CancelEnabled = false;
                });

                async Task SaveSplitTokens()
                {
                    var result = await VerseManager!.SplitTokensAsync(TokenDisplay.Corpus!, TokenDisplay.Token.TokenId,
                        SplitInstructionsViewModel.Entity, /*!TokenDisplay.IsCorpusView*/ false,
                        SelectedPropagationOption);

                    var applyToAll = true;
                  
                    foreach (var splitInstruction in SplitInstructionsViewModel.Instructions)
                    {
                        if (splitInstruction.HasGloss)
                        {
                            var translation = new LexiconTranslationViewModel { Text = splitInstruction.Gloss };
                            await SaveGloss(translation, applyToAll);
                        }
                    }

                    if (HasWordGloss)
                    {
                        var translation = new LexiconTranslationViewModel { Text = WordGloss };
                        await SaveGloss(translation, applyToAll);
                    }

                    await Task.CompletedTask;
                }

                await Task.Run(async () => await SaveSplitTokens());
            }
            finally
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
                await TryCloseAsync(true);
            }
        }

        private async Task SaveGloss(LexiconTranslationViewModel translation, bool applyToAll)
        {
            var lexiconTranslationId = (translation.TranslationId.IsInDatabase) ? translation.TranslationId : null;
            await InterlinearDisplay.PutTranslationAsync(new Translation(TokenDisplay.TokenForTranslation, translation.Text, Translation.OriginatedFromValues.Assigned, lexiconTranslationId),
                applyToAll ? TranslationActionTypes.PutPropagate : TranslationActionTypes.PutNoPropagate);
        }


        private LexiconDialogViewModel? lexiconDialogViewModel_;

        public async void OnDropDownOpening(object sender, RoutedEventArgs e)
        {
            var args = e as SplitTokenEventArgs;
            var lexiconDialogView = args!.LexiconDialogView;
            var splitInstructionViewModel = args.SplitInstructionViewModel;
            var lexiconDialogViewModel_ = LifetimeScope!.Resolve<LexiconDialogViewModel>();
            lexiconDialogViewModel_.Mode = LexiconDialogMode.DropDown;
            var tokenId = TokenDisplay.Token.TokenId;
            var token = new Token(
                new TokenId(tokenId.BookNumber, tokenId.ChapterNumber, tokenId.VerseNumber, tokenId.WordNumber,
                    tokenId.SubWordNumber), args.TokenText, args.TrainingText);
            var tokenDisplay = new TokenDisplayViewModel(token)
            {
                VerseDisplay = TokenDisplay.VerseDisplay,
                //TargetTranslationText = args.TokenText
            };

            lexiconDialogViewModel_.InterlinearDisplay = InterlinearDisplay;
            lexiconDialogViewModel_.TokenDisplay = tokenDisplay;
            args!.LexiconDialogView.DataContext = lexiconDialogViewModel_;

            //ViewModelBinder.Bind(lexiconDialogViewModel, args!.LexiconDialogView, null);

            await lexiconDialogViewModel_.Initialize();



        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var results = await Mediator!.Send(new GetGrammarSuggestionsQuery(ProjectManager!.CurrentProject!.ProjectName!), cancellationToken);
            if (results is { Success: true, HasData: true })
            {
                GrammarSuggestions = new BindableCollection<Grammar>(results.Data);
            }

            SelectedSplitTokenPropagationComboItem = PropagationOptions.Last();

            await base.OnActivateAsync(cancellationToken);
        }

        public async void CancelSplit()
        {
            await TryCloseAsync(false);
        }


        public dynamic DialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.SingleBorderWindow;
            settings.ShowInTaskbar = false;
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.CanResizeWithGrip;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            //settings.Top = 0;
            settings.Left = System.Windows.Application.Current.MainWindow!.ActualWidth / 2 - 258;
            settings.Width = 1000;
            settings.Height = 820;
            settings.Title = DialogTitle;

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            return settings;
        }

        public SplitTokenDialogViewModel()
        {
            // Required for designer support.
        }

        public SplitTokenDialogViewModel(
            DashboardProjectManager? projectManager,
            VerseManager verseManager,
            INavigationService navigationService,
            ILogger<SplitTokenDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            VerseManager = verseManager;
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.None, DisplayMemberPath = localizationService["None"] });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapterVerse, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Verse"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Chapter"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Book"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = localizationService["BiblicalTermsBcv_All"] });

            SelectedSplitTokenPropagationComboItem = PropagationOptions.Last();
        }
    }

    public class SplitTokenPropagationComboItem : PropertyChangedBase
    {
        private string? _displayMemberPath;
        private SplitTokenPropagationScope _selectedValuePath;

        public SplitTokenPropagationScope SelectedValuePath
        {
            get => _selectedValuePath;
            set => Set(ref _selectedValuePath, value);
        }

        public string? DisplayMemberPath
        {
            get => _displayMemberPath;
            set => Set(ref _displayMemberPath, value);
        }
    }
}
