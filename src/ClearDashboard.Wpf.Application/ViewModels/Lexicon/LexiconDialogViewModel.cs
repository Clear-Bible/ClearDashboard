using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconDialogViewModel : DashboardApplicationScreen
    {
        private LexiconManager? LexiconManager { get; }
        public string DialogTitle => $"Translation/Lexeme: {TokenDisplay?.TranslationSurfaceText}";

        public TokenDisplayViewModel? TokenDisplay { get; set; }

        public InterlinearDisplayViewModel? InterlinearDisplay { get; set; }

        private LexemeViewModel? _lexeme;
        public LexemeViewModel? Lexeme
        {
            get => _lexeme;
            set => Set(ref _lexeme, value);
        }

        private SemanticDomainCollection _semanticDomainSuggestions = new();
        public SemanticDomainCollection SemanticDomainSuggestions
        {
            get => _semanticDomainSuggestions;
            set => Set(ref _semanticDomainSuggestions, value);
        }

        private LexiconTranslationViewModelCollection _concordance = new();
        public LexiconTranslationViewModelCollection Concordance
        {
            get => _concordance;
            set => Set(ref _concordance, value);
        }

        private IEnumerable<TranslationOption> _translationOptions = new List<TranslationOption>();
        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            private set => Set(ref _translationOptions, value);
        }

        private TranslationOption? _currentTranslationOption;
        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            private set => Set(ref _currentTranslationOption, value);
        }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        private async Task OnLoadedAsync()
        {
            //if (TokenDisplay.Translation != null && !TokenDisplay.Translation.IsDefault)
            //{
            //    TranslationOptions = await InterlinearDisplay.GetTranslationOptionsAsync(TokenDisplay.Token);
            //    CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == TokenDisplay.TargetTranslationText);

            //    OnUIThread(() =>
            //    {
            //        if (CurrentTranslationOption == null)
            //        {
            //            TranslationSelectorControl.TranslationValue.Text = TokenDisplay.TargetTranslationText;
            //            TranslationSelectorControl.TranslationValue.SelectAll();
            //        }
            //        TranslationSelectorControl.TranslationOptionsVisibility = Visibility.Visible;
            //    });
            //}
            //OnUIThread(() =>
            //{
            //    TranslationSelectorControl.TranslationControlsVisibility = Visibility.Visible;
            //    ProgressBarVisibility = Visibility.Collapsed;
            //    TranslationSelectorControl.TranslationValue.SelectAll();
            //    TranslationSelectorControl.TranslationValue.Focus();
            //});
        }

        public async void ApplyTranslation()
        {
            try
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Visible);

                async Task SaveTranslation()
                {
                    //await InterlinearDisplay.PutTranslationAsync(e.Translation, e.TranslationActionType);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(SaveTranslation);

                //System.Windows.Application.Current.Dispatcher.Invoke(() => DialogResult = true);
            }
            catch (Exception ex)
            {
                var exception = ex;
            }
            finally
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
                await TryCloseAsync(true);
            }
        }

        public async void CancelTranslation()
        {
            await TryCloseAsync(false);
        }

        public async void OnLexemeAdded(object sender, LexemeEventArgs e)
        {
            if (LexiconManager != null && ! string.IsNullOrWhiteSpace(e.Lexeme.Lemma))
            {
                if (e.Lexeme.LexemeId == null)
                {
                    if (!string.IsNullOrWhiteSpace(e.Lexeme.Lemma))
                    {
                        Lexeme = await LexiconManager.CreateLexemeAsync(e.Lexeme.Lemma, e.Lexeme.Language, e.Lexeme.Type);
                    }
                }
                else
                {
                    Logger?.LogError($"Cannot create new lexeme because {e.Lexeme.Lemma} already has a LexemeId.");
                }
            }
            else
            {
                Logger?.LogCritical("Cannot create lexeme because LexiconManager is null.");
            }
        }

        public async void OnLexemeFormAdded(object sender, LexemeFormEventArgs e)
        {

        }

        public async void OnLexemeFormRemoved(object sender, LexemeFormEventArgs e)
        {

        }

        public async void OnMeaningAdded(object sender, MeaningEventArgs e)
        {

        }

        public async void OnMeaningDeleted(object sender, MeaningEventArgs e)
        {

        }

        public async void OnMeaningUpdated(object sender, MeaningEventArgs e)
        {

        }

        public async void OnSemanticDomainAdded(object sender, SemanticDomainEventArgs e)
        {

        }

        public async void OnSemanticDomainSelected(object sender, SemanticDomainEventArgs e)
        {

        }

        public async void OnSemanticDomainRemoved(object sender, SemanticDomainEventArgs e)
        {

        }

        public async void OnTranslationDeleted(object sender, LexiconTranslationEventArgs e)
        {

        }

        public async void OnTranslationDropped(object sender, LexiconTranslationEventArgs e)
        {

        }

        public async void OnTranslationSelected(object sender, LexiconTranslationEventArgs e)
        {

        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);

            if (Lexeme == null && TokenDisplay != null)
            {
                Lexeme = await LexiconManager!.GetLexemeAsync(TokenDisplay.TranslationSurfaceText);
            }
            OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
        }

        public dynamic DialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.SingleBorderWindow;
            settings.ShowInTaskbar = false;
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.CanResizeWithGrip;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.Placement = PlacementMode.Center;
            settings.Width = 1000;
            settings.Width = 800;
            settings.Title = DialogTitle;
            return settings;
        }

        public LexiconDialogViewModel()
        {
            // Required for designer support.
        }

        public LexiconDialogViewModel(
            LexiconManager lexiconManager,
            DashboardProjectManager? projectManager, 
            INavigationService navigationService,
            ILogger<LexiconDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            LexiconManager = lexiconManager;
        }
    }
}
