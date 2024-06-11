using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class SplitTokenDialogViewModel : DashboardApplicationScreen
    {
        public VerseManager? VerseManager { get; }


        public BindableCollection<SplitInstructionViewModel> SplitInstructionsViewModels { get; private set; } = new();

        //private SplitInstructions? _splitInstructions;

        public SplitInstructionsViewModel SplitInstructionsViewModel
        {
            get => _splitInstructionsViewModel;
            private set => Set(ref _splitInstructionsViewModel, value);
        }

        public List<KeyValuePair<SplitTokenPropagationScope, string>> PropagationOptions { get; } = new();
        public SplitTokenPropagationScope SelectedPropagationOption { get; set; } = SplitTokenPropagationScope.None;

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
                SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(_tokenDisplay.SurfaceText, []);
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
            set
            {
                _wordGloss = value;
                NotifyOfPropertyChange();
            }
        }

        public string DialogTitle => $"{LocalizationService!["EnhancedView_SplitToken"]}: {TokenDisplay.SurfaceText}";

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        public bool ApplyEnabled
        {
            get
            {
                var enabled = true;
            

                return  enabled;
            }
        }

        private bool _cancelEnabled = true;
        private SplitInstructionsViewModel _splitInstructionsViewModel;

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            private set => Set(ref _cancelEnabled, value);
        }

        //private bool _isLoaded;
        //public bool IsLoaded
        //{
        //    get => _isLoaded;
        //    private set => Set(ref _isLoaded, value);
        //}



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

                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes);
                    NotifyOfPropertyChange(nameof(ApplyEnabled));
                }
            }
            else
            {

                if (SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
                {
                    SplitInstructionsViewModel.SplitIndexes.Remove(splitIndex);
                    var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();
                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes);
                    NotifyOfPropertyChange(nameof(ApplyEnabled));
                }

            }

        }

      

        public async void ApplySplit()
        {
            try
            {
                OnUIThread(() =>
                {
                    ProgressBarVisibility = Visibility.Visible;
                    CancelEnabled = false;
                });

                async Task SaveSplitToken()
                {
                    // TODO: Implement this method

                    //await VerseManager.SplitToken(TokenDisplay.Corpus!,
                    //    TokenDisplay.Token.TokenId,
                    //    Subtoken3Enabled ? CharacterThreshold1 : 0,
                    //    Subtoken3Enabled ? CharacterThreshold2 : CharacterThreshold1,
                    //    Subtoken1,
                    //    Subtoken2,
                    //    Subtoken3,
                    //    !TokenDisplay.IsCorpusView,
                    //    SelectedPropagationOption);
                    await Task.CompletedTask;
                }
                await Task.Run(async () => await SaveSplitToken());
            }
            finally
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
                await TryCloseAsync(true);
            }
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
            settings.Left = System.Windows.Application.Current.MainWindow!.ActualWidth/2 - 258;
            settings.Width = 1000;
            settings.Height = 800;
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
            PropagationOptions.Add(new KeyValuePair<SplitTokenPropagationScope, string>(SplitTokenPropagationScope.None, localizationService["None"]));
            PropagationOptions.Add(new KeyValuePair<SplitTokenPropagationScope, string>(SplitTokenPropagationScope.BookChapterVerse, $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Verse"]}"));
            PropagationOptions.Add(new KeyValuePair<SplitTokenPropagationScope, string>(SplitTokenPropagationScope.BookChapter, $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Chapter"]}"));
            PropagationOptions.Add(new KeyValuePair<SplitTokenPropagationScope, string>(SplitTokenPropagationScope.BookChapter, $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Book"]}"));
            PropagationOptions.Add(new KeyValuePair<SplitTokenPropagationScope, string>(SplitTokenPropagationScope.BookChapter, $"{localizationService["BiblicalTermsBcv_All"]}"));
        }
    }
}
