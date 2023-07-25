//#define DEMO

using System;
using System.Dynamic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Services;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Events.Lexicon;
using ClearDashboard.Wpf.Application.Infrastructure;
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
        private string _sourceFontFamily = FontNames.DefaultFontFamily;
        public string SourceFontFamily
        {
            get => _sourceFontFamily;
            set
            {
                _sourceFontFamily = value;
                NotifyOfPropertyChange(() => SourceFontFamily);
            }
        }

        private string _targetFontFamily = FontNames.DefaultFontFamily;
        public string TargetFontFamily
        {
            get => _targetFontFamily;
            set
            {
                _targetFontFamily = value;
                NotifyOfPropertyChange(() => TargetFontFamily);
            }
        }

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

        private LexemeViewModel? _currentLexeme;
        public LexemeViewModel? CurrentLexeme
        {
            get => _currentLexeme;
            set => Set(ref _currentLexeme, value);
        }

        private LexemeViewModelCollection _lexemes = new();
        public LexemeViewModelCollection Lexemes
        {
            get => _lexemes;
            set => Set(ref _lexemes, value);
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

        private bool _applyEnabled;
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
                        await InterlinearDisplay.PutTranslationAsync(new Translation(TokenDisplay.TokenForTranslation, SelectedTranslation.Text, Translation.OriginatedFromValues.Assigned, SelectedTranslation.TranslationId),
                                                                     ApplyToAll ? TranslationActionTypes.PutPropagate : TranslationActionTypes.PutNoPropagate);
                    }
                }
#if !DEMO
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(SaveTranslation);
#endif
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
            if (e.Lexeme.LexemeId == null)
            {
                if (!string.IsNullOrWhiteSpace(e.Lexeme.Lemma))
                {
                    CurrentLexeme = await LexiconManager.CreateLexemeAsync(e.Lexeme.Lemma, e.Lexeme.Language, e.Lexeme.Type);
                }
            }
            else
            {
                Logger?.LogError($"Cannot create new lexeme because {e.Lexeme.Lemma} already has a LexemeId.");
            }
        }
        public async void OnLexemeDeleted(object sender, LexemeEventArgs e)
        {
            if (e.Lexeme.LexemeId != null)
            {
                await LexiconManager.DeleteLexemeAsync(e.Lexeme);
            }
            else
            {
                Logger?.LogError($"Cannot delete lexeme because {e.Lexeme.Lemma} does not have a LexemeId.");
            }
        }

        public async void OnLexemeFormAdded(object sender, LexemeFormEventArgs e)
        {
            await LexiconManager.AddLexemeFormAsync(e.Lexeme, e.Form);
        }

        public async void OnLexemeFormRemoved(object sender, LexemeFormEventArgs e)
        {
            await LexiconManager.DeleteLexemeFormAsync(e.Form);
        }

        public async void OnMeaningAdded(object sender, MeaningEventArgs e)
        {
            await LexiconManager.AddMeaningAsync(e.Lexeme, e.Meaning);
        }

        public async void OnMeaningDeleted(object sender, MeaningEventArgs e)
        {
            await LexiconManager.DeleteMeaningAsync(e.Meaning);
        }

        public async void OnMeaningUpdated(object sender, MeaningEventArgs e)
        {
            await LexiconManager.UpdateMeaningAsync(e.Lexeme, e.Meaning);
        }

        public async void OnSemanticDomainAdded(object sender, SemanticDomainEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.SemanticDomain.Text))
            {
                await LexiconManager.AddNewSemanticDomainAsync(e.Meaning, e.SemanticDomain.Text);
                if (SemanticDomainSuggestions.All(sd => sd.Text != e.SemanticDomain.Text))
                {
                    SemanticDomainSuggestions.Add(e.SemanticDomain);
                }
            }
        }

        public async void OnSemanticDomainSelected(object sender, SemanticDomainEventArgs e)
        {
            await LexiconManager.AddExistingSemanticDomainAsync(e.Meaning, e.SemanticDomain);
        }

        public async void OnSemanticDomainRemoved(object sender, SemanticDomainEventArgs e)
        {
            await LexiconManager.RemoveSemanticDomainAsync(e.Meaning, e.SemanticDomain);
        }

        public async void OnTranslationDeleted(object sender, LexiconTranslationEventArgs e)
        {
            await LexiconManager.DeleteTranslationAsync(e.Translation);
        }

        public async void OnTranslationDropped(object sender, LexiconTranslationEventArgs e)
        {
            await LexiconManager.MoveTranslationAsync(e.Translation, e.Meaning);
        }

        private LexiconTranslationViewModel? SelectedTranslation { get; set; }
        public LexiconTranslationViewModel? NewTranslation { get; set; }
        public void OnTranslationSelected(object sender, LexiconTranslationEventArgs e)
        {
            SelectedTranslation = e.Translation;
            ApplyEnabled = !string.IsNullOrWhiteSpace(SelectedTranslation.Text);
        }
        
        public async void OnMeaningTranslationAdded(object sender, LexiconTranslationEventArgs e)
        {
            await LexiconManager.AddTranslationAsync(e.Translation, e.Meaning);
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
                if (CurrentLexeme == null || !CurrentLexeme.ContainsTranslationText(translationOption.Word))
                {
                    Concordance.Add(new LexiconTranslationViewModel(translationOption.Word, Convert.ToInt32(translationOption.Count)));
                }
            }
        }

        private bool ContainsTranslationText(string translationText)
        {
            return (CurrentLexeme != null && CurrentLexeme.ContainsTranslationText(translationText)) || Concordance.ContainsText(translationText);
        }

        private void SelectCurrentTranslation()
        {
            SelectedTranslation = CurrentLexeme?.SelectTranslationText(TokenDisplay.TargetTranslationText);
            var concordanceSelection = Concordance.SelectIfContainsText(TokenDisplay.TargetTranslationText);
            SelectedTranslation ??= concordanceSelection;

            if (SelectedTranslation == null && TokenDisplay.TargetTranslationText != Translation.DefaultTranslationText)
            {
                SelectedTranslation = new LexiconTranslationViewModel { Text = TokenDisplay.TargetTranslationText, IsSelected = true};
                //Concordance.Insert(0, SelectedTranslation);
                NewTranslation = SelectedTranslation;
                NotifyOfPropertyChange(nameof(NewTranslation));
            }

            ApplyEnabled = SelectedTranslation != null;
        }

        private async Task<string?> GetFontFamily(string paratextProjectId)
        {
            var result = await Mediator.Send(new GetProjectFontFamilyQuery(paratextProjectId));
            if (result is { HasData: true })
            {
                return result.Data;
            }

            return FontNames.DefaultFontFamily;
        }

        private string? GetSourceLanguage()
        {
            return TokenDisplay.VerseDisplay is InterlinearDisplayViewModel verseDisplay ? verseDisplay.SourceLanguage : string.Empty;
        }

        private string? GetTargetLanguage()
        {
            return TokenDisplay.VerseDisplay is InterlinearDisplayViewModel verseDisplay ? verseDisplay.TargetLanguage : string.Empty;
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            
            Lexemes = await LexiconManager.GetLexemesAsync(TokenDisplay.TranslationSurfaceText, GetSourceLanguage(), GetTargetLanguage());

            if (TokenDisplay?.Translation?.LexiconTranslationId != null)
            {
                CurrentLexeme = Lexemes.GetLexemeWithTranslation(TokenDisplay.Translation.LexiconTranslationId);
            }
            CurrentLexeme ??= Lexemes.FirstOrDefault();

            if (Concordance.Count == 0)
            {
                await BuildConcordance();
            }
            SelectCurrentTranslation();

            if (SemanticDomainSuggestions.Count == 0)
            {
                SemanticDomainSuggestions = await LexiconManager.GetAllSemanticDomainsAsync();
            }

            SourceFontFamily = await GetFontFamily(TokenDisplay.VerseDisplay.ParallelCorpusId.SourceTokenizedCorpusId.CorpusId.ParatextGuid);
            TargetFontFamily = await GetFontFamily(TokenDisplay.VerseDisplay.ParallelCorpusId.TargetTokenizedCorpusId.CorpusId.ParatextGuid);

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
            LexemeEditor.LexiconManager = lexiconManager;

            LexemeEditor.EventAggregator = eventAggregator;
            ConcordanceDisplay.EventAggregator = eventAggregator;
            MeaningEditor.EventAggregator = eventAggregator;
        }
    }
}
