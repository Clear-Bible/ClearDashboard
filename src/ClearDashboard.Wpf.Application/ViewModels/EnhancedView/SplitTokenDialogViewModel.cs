using System;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
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

        private int _characterThreshold = 0;

        public int CharacterThreshold
        {
            get => _characterThreshold;
            set
            {
                _characterThreshold = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Subtoken1));
                NotifyOfPropertyChange(nameof(Subtoken2));
                NotifyOfPropertyChange(nameof(ApplyEnabled));
            }
        }

        public string Subtoken1 => TokenDisplay.SurfaceText[..CharacterThreshold];
        public string Subtoken2 => TokenDisplay.SurfaceText[CharacterThreshold..];

        public string DialogTitle => $"{LocalizationService!["EnhancedView_SplitToken"]}: {TokenDisplay.SurfaceText}";

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        public bool ApplyEnabled => CharacterThreshold > 0 && CharacterThreshold < TokenDisplay.SurfaceText.Length;

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
            CharacterThreshold = tokenCharacterArgs!.TokenCharacter.Index + 1;
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
            return settings;
        }

        public SplitTokenDialogViewModel()
        {
            // Required for designer support.
        }

        public SplitTokenDialogViewModel(
            DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger<LexiconDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
        }
    }
}
