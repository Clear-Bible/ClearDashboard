//#define DEMO

using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages.Lexicon;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.UserControls.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMember.Global

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconDialogViewModel : DashboardApplicationScreen
    {
        private readonly LexiconManager? _lexiconManager;
        private LexiconManager LexiconManager
        {
            get
            {
                if (_lexiconManager == null) throw new Exception("Cannot perform operation as LexiconManager is null");
                return _lexiconManager!;
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
            set => _tokenDisplay = value;
        }

        private InterlinearDisplayViewModel? _interlinearDisplayViewModel;
        public InterlinearDisplayViewModel InterlinearDisplay
        {
            get
            {
                if (_interlinearDisplayViewModel == null) throw new Exception("Cannot perform operation as InterlinearDisplay is null");
                return _interlinearDisplayViewModel!;
            }
            set => _interlinearDisplayViewModel = value;
        }

        public string DialogTitle => $"Translation/Lexeme: {TokenDisplay.TranslationSurfaceText}";

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

        public bool ApplyToAll { get; set; } = true;

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        private bool _applyEnabled = false;
        public bool ApplyEnabled
        {
            get => _applyEnabled;
            private set => Set(ref _applyEnabled, value);
        }

        public async void ApplyTranslation()
        {
            try
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Visible);

                async Task SaveTranslation()
                {
                    if (SelectedTranslation != null && !string.IsNullOrWhiteSpace(SelectedTranslation.Text))
                    {
                        await InterlinearDisplay.PutTranslationAsync(new Translation(TokenDisplay.TokenForTranslation, SelectedTranslation.Text, Translation.OriginatedFromValues.Assigned),
                                                                     ApplyToAll ? TranslationActionTypes.PutPropagate : TranslationActionTypes.PutNoPropagate);
                    }
                }
#if !DEMO
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(SaveTranslation);
#endif
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
#if !DEMO
            if (e.Lexeme.LexemeId == null)
            {
                if (!string.IsNullOrWhiteSpace(e.Lexeme.Lemma))
                {
                    Lexeme = await LexiconManager.CreateLexemeAsync(e.Lexeme.Lemma, e.Lexeme.Language, e.Lexeme.Type);
                    await EventAggregator.PublishOnUIThreadAsync(new LexemeAddedMessage(Lexeme));
                }
            }
            else
            {
                Logger?.LogError($"Cannot create new lexeme because {e.Lexeme.Lemma} already has a LexemeId.");
            }
#endif
        }
        public async void OnLexemeDeleted(object sender, LexemeEventArgs e)
        {
#if !DEMO
            if (e.Lexeme.LexemeId != null)
            {
                await LexiconManager.DeleteLexemeAsync(e.Lexeme);
            }
            else
            {
                Logger?.LogError($"Cannot delete lexeme because {e.Lexeme.Lemma} does not have a LexemeId.");
            }
#endif 
        }

        public async void OnLexemeFormAdded(object sender, LexemeFormEventArgs e)
        {
#if !DEMO
            await LexiconManager.AddLexemeFormAsync(e.Lexeme, e.Form);
#endif
        }

        public async void OnLexemeFormRemoved(object sender, LexemeFormEventArgs e)
        {
#if !DEMO
            await LexiconManager.DeleteLexemeFormAsync(e.Form);
#endif
        }

        public async void OnMeaningAdded(object sender, MeaningEventArgs e)
        {
#if !DEMO
            await LexiconManager.AddMeaningAsync(e.Lexeme, e.Meaning);
#endif
        }

        public async void OnMeaningDeleted(object sender, MeaningEventArgs e)
        {
#if !DEMO
            await LexiconManager.DeleteMeaningAsync(e.Meaning);
#endif
        }

        public async void OnMeaningUpdated(object sender, MeaningEventArgs e)
        {
#if !DEMO
#endif
        }

        public async void OnSemanticDomainAdded(object sender, SemanticDomainEventArgs e)
        {
#if !DEMO
            if (!string.IsNullOrWhiteSpace(e.SemanticDomain.Text))
            {
                await LexiconManager.AddNewSemanticDomainAsync(e.Meaning, e.SemanticDomain.Text);
            }
#endif
        }

        public async void OnSemanticDomainSelected(object sender, SemanticDomainEventArgs e)
        {
#if !DEMO
            await LexiconManager.AddExistingSemanticDomainAsync(e.Meaning, e.SemanticDomain);
#endif
        }

        public async void OnSemanticDomainRemoved(object sender, SemanticDomainEventArgs e)
        {
#if !DEMO
            await LexiconManager.RemoveSemanticDomainAsync(e.Meaning, e.SemanticDomain);
#endif
        }

        public async void OnTranslationDeleted(object sender, LexiconTranslationEventArgs e)
        {
#if !DEMO
            await LexiconManager.DeleteTranslationAsync(e.Translation);
#endif
        }

        public async void OnTranslationDropped(object sender, LexiconTranslationEventArgs e)
        {
            await EventAggregator.PublishOnUIThreadAsync(new LexiconTranslationMovedMessage(e.Translation, e.Meaning));
#if !DEMO
#endif
        }

        private LexiconTranslationViewModel? SelectedTranslation { get; set; }
        public void OnTranslationSelected(object sender, LexiconTranslationEventArgs e)
        {
            SelectedTranslation = e.Translation;
            ApplyEnabled = !string.IsNullOrWhiteSpace(SelectedTranslation.Text);
        }
        
        public void OnTranslationAdded(object sender, LexiconTranslationEventArgs e)
        {
            SelectedTranslation = e.Translation;
            ApplyEnabled = !string.IsNullOrWhiteSpace(SelectedTranslation.Text);
        }

        public void OnNewTranslationChanged(object sender, LexiconTranslationEventArgs e)
        {
            SelectedTranslation = e.Translation;
            ApplyEnabled = !string.IsNullOrWhiteSpace(SelectedTranslation.Text);
        }

        private async Task BuildConcordance()
        {
            var translationOptions = await InterlinearDisplay.GetTranslationOptionsAsync(TokenDisplay.Token);
            foreach (var translationOption in translationOptions)
            {
                if (Lexeme == null || !Lexeme.ContainsTranslationText(translationOption.Word))
                {
                    Concordance.Add(new LexiconTranslationViewModel(translationOption.Word, Convert.ToInt32(translationOption.Count)));
                }
            }
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);

            Lexeme ??= await LexiconManager!.GetLexemeAsync(TokenDisplay.TranslationSurfaceText);
            await BuildConcordance();

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
            settings.Height = 800;
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
            _lexiconManager = lexiconManager;

            LexemeEditor.EventAggregator = eventAggregator;
            ConcordanceDisplay.EventAggregator = eventAggregator;
            MeaningEditor.EventAggregator = eventAggregator;
        }
    }
}
