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
using ClearDashboard.DAL.Alignment.Features.Corpora;
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
        public VerseManager VerseManager { get; }
        public List<KeyValuePair<SplitTokenPropagationScope, string>> PropagationOptions { get; } = new();
        public SplitTokenPropagationScope SelectedPropagationOption { get; set; } = SplitTokenPropagationScope.None;

        private FontFamily _tokenFontFamily;
        public FontFamily TokenFontFamily
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
                CharacterThreshold2 = TokenLength;
                NotifyOfPropertyChange(nameof(TokenCharacters));
            } 
        }

        private int TokenLength => _tokenDisplay != null ? _tokenDisplay.SurfaceText.Length : 0;

        private TokenCharacterViewModelCollection? _tokenCharacters;
        public TokenCharacterViewModelCollection TokenCharacters
        {
            get
            {
                return _tokenCharacters ??= new TokenCharacterViewModelCollection(TokenDisplay.SurfaceText);
            }
        }

        private int _characterThreshold1 = 0;
        public int CharacterThreshold1
        {
            get => _characterThreshold1;
            set
            {
                _characterThreshold1 = Math.Min(value, CharacterThreshold2);
                NotifyOfPropertyChange();
                CalculateSubtokens();
            }
        }

        private int _characterThreshold2 = int.MaxValue;
        public int CharacterThreshold2
        {
            get => _characterThreshold2;
            set
            {
                _characterThreshold2 = Math.Max(value, CharacterThreshold1);
                NotifyOfPropertyChange();
                CalculateSubtokens();
            }
        }

        private bool _threeWaySplit;
        public bool ThreeWaySplit
        {
            get => _threeWaySplit;
            set
            {
                _threeWaySplit = value;
                NotifyOfPropertyChange();
            }
        }

        private void CalculateSubtokens()
        {
            Subtoken1 = TokenDisplay.SurfaceText[..CharacterThreshold1];

            if (CharacterThreshold2 < TokenLength)
            {
                Subtoken2 = TokenDisplay.SurfaceText[CharacterThreshold1..CharacterThreshold2];
                Subtoken3 = TokenDisplay.SurfaceText[CharacterThreshold2..];
            }
            else
            {
                Subtoken2 = TokenDisplay.SurfaceText[CharacterThreshold1..];
                Subtoken3 = string.Empty;
            }

            NotifyOfPropertyChange(nameof(Subtoken1));
            NotifyOfPropertyChange(nameof(Subtoken2));
            NotifyOfPropertyChange(nameof(Subtoken3));

            NotifyOfPropertyChange(nameof(Subtoken1Enabled));
            NotifyOfPropertyChange(nameof(Subtoken2Enabled));
            NotifyOfPropertyChange(nameof(Subtoken3Enabled));

            NotifyOfPropertyChange(nameof(ApplyEnabled));
        }

        private string _subtoken1 = string.Empty;
        public string Subtoken1
        {
            get => _subtoken1;
            set
            {
                _subtoken1 = value;
                NotifyOfPropertyChange();
            }
        }

        private string _subtoken2 = string.Empty;
        public string Subtoken2
        {
            get => _subtoken2;
            set
            {
                _subtoken2 = value;
                NotifyOfPropertyChange();
            }
        }

        private string _subtoken3 = string.Empty;
        public string Subtoken3
        {
            get => _subtoken3;
            set
            {
                _subtoken3 = value;
                NotifyOfPropertyChange();
            }
        }

        public bool Subtoken1Enabled => CharacterThreshold1 > 0 && CharacterThreshold1 < TokenLength;
        public bool Subtoken2Enabled => CharacterThreshold1 != CharacterThreshold2 && (CharacterThreshold1 > 0 || CharacterThreshold2 < TokenLength);
        public bool Subtoken3Enabled => CharacterThreshold2 > 0 && CharacterThreshold2 < TokenLength;

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
                var enabledCount = 0;
                if (Subtoken1Enabled)
                {
                    enabledCount++;
                    enabled &= !string.IsNullOrWhiteSpace(Subtoken1);
                }
                if (Subtoken2Enabled)
                {
                    enabledCount++;
                    enabled &= !string.IsNullOrWhiteSpace(Subtoken2);
                }
                if (Subtoken3Enabled)
                {
                    enabledCount++;
                    enabled &= !string.IsNullOrWhiteSpace(Subtoken3);
                }

                return enabledCount >= 2 && enabled;
            }
        }

        private bool _cancelEnabled = true;
        public bool CancelEnabled
        {
            get => _cancelEnabled;
            private set => Set(ref _cancelEnabled, value);
        }

        private bool _isLoaded = false;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => Set(ref _isLoaded, value);
        }

        public void CharacterClicked(object source, RoutedEventArgs args)
        {
            var tokenCharacterArgs = args as TokenCharacterEventArgs;
            if (tokenCharacterArgs!.IsShiftPressed)
            {
                CharacterThreshold2 = tokenCharacterArgs!.TokenCharacter.Index;
                ThreeWaySplit = true;

            }
            else
            {
                CharacterThreshold1 = tokenCharacterArgs!.TokenCharacter.Index + 1;
            }
        }

        public void ThreeWaySplitUnchecked(object source, RoutedEventArgs args)
        {
            CharacterThreshold2 = TokenDisplay.SurfaceText.Length;
        }

        public void TrainingTextChanged(object source, RoutedEventArgs args)
        {
            NotifyOfPropertyChange(nameof(ApplyEnabled));
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
                    await VerseManager.SplitToken(TokenDisplay.Corpus!,
                        TokenDisplay.Token.TokenId,
                        Subtoken3Enabled ? CharacterThreshold1 : 0,
                        Subtoken3Enabled ? CharacterThreshold2 : CharacterThreshold1,
                        Subtoken1,
                        Subtoken2,
                        Subtoken3,
                        !TokenDisplay.IsCorpusView,
                        SelectedPropagationOption);
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
            settings.Left = App.Current.MainWindow.ActualWidth/2 - 258;
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
            ILogger<LexiconDialogViewModel> logger,
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
