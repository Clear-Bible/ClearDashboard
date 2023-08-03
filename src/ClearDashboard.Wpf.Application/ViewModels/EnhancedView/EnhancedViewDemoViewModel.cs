// Uncomment this preprocessor definition to use mock data for dev/test purposes.
//#define MOCK

using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using Translation = ClearDashboard.DAL.Alignment.Lexicon.Translation;
using TranslationCollection = ClearDashboard.Wpf.Application.Collections.Lexicon.TranslationCollection;
using TranslationId = ClearDashboard.DAL.Alignment.Lexicon.TranslationId;
using ClearDashboard.Wpf.Application.ViewModels.Popups;
using ClearDashboard.Wpf.Application.Events.Notes;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        #region Demo data - for demo page only
        public EngineStringDetokenizer Detokenizer { get; set; } = new(new LatinWordDetokenizer());

        public async Task<ParallelCorpus> GetParallelCorpus(ParallelCorpusId? corpusId = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                if (corpusId == null)
                {
                    var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator!);
                    corpusId = corpusIds.First();
                }
                var corpus = await ParallelCorpus.Get(Mediator!, corpusId, useCache: true);

                Detokenizer = corpus.Detokenizer;
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
                return corpus;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<EngineParallelTextRow?> GetVerseTextRow(int bbbcccvvv, ParallelCorpusId? corpusId = null)
        {
            try
            {
                var corpus = await GetParallelCorpus(corpusId);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var verse = corpus.GetByVerseRange(new VerseRef(bbbcccvvv), 0, 0);
                var row = verse.parallelTextRows.FirstOrDefault() as EngineParallelTextRow;
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus verse {bbbcccvvv} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
                return row;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<TranslationSet?> GetFirstTranslationSet()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var translationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!);
                var translationSet = await TranslationSet.Get(translationSetIds.First(), Mediator!);
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved first translation set {translationSet.TranslationSetId.Id} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
                return translationSet;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private async Task PopulateData()
        {
            try
            {
                //VerseDisplayViewModel = ServiceProvider.GetService<VerseDisplayViewModel>()!;
#if MOCK
                await VerseDisplayViewModel.BindMockVerseAsync();
#else
                await ProjectManager!.LoadProject("EnhancedViewDemo4");
                //var row = await GetVerseTextRow(40001001);
                var row = await GetVerseTextRow(001001001);
                var translationSet = await GetFirstTranslationSet();

                VerseDisplayViewModel = await InterlinearDisplayViewModel.CreateAsync(LifetimeScope!, row.SourceTokens, new ParallelCorpusId(Guid.Empty), Detokenizer, true, translationSet.TranslationSetId);

                //await VerseDisplayViewModel!.ShowTranslationAsync(row, translationSet, Detokenizer, false);
#endif
                NotifyOfPropertyChange(nameof(VerseDisplayViewModel));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion Demo data

        private IServiceProvider ServiceProvider { get; }

        public NoteManager NoteManager { get; }
        public SelectionManager SelectionManager { get; }
        public VerseDisplayViewModel VerseDisplayViewModel { get; set; }
        private IWindowManager WindowManager { get; }


        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set => Set(ref _selectedTokens, value);
        }

        private Visibility _notePaneVisibility = Visibility.Collapsed;
        public Visibility NotePaneVisibility
        {
            get => _notePaneVisibility;
            set => Set(ref _notePaneVisibility, value);
        }

        private Visibility _translationPaneVisibility = Visibility.Collapsed;
        public Visibility TranslationPaneVisibility
        {
            get => _translationPaneVisibility;
            set => Set(ref _translationPaneVisibility, value);
        }

        private static string GetModifierKeysText(ModifierKeys modifierKeys)
        {
            var modifiers = string.Empty;
            if ((modifierKeys & ModifierKeys.Control) > 0) modifiers += "Ctrl+";
            if ((modifierKeys & ModifierKeys.Shift) > 0) modifiers += "Shift+";
            if ((modifierKeys & ModifierKeys.Alt) > 0) modifiers += "Alt+";
            if ((modifierKeys & ModifierKeys.Windows) > 0) modifiers += "Win+";
            return modifiers;
        }

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
                if (!token.IsTokenSelected)
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

        #region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Task.Run(() => TokenClickedAsync(e).GetAwaiter());
        }

        public async Task TokenClickedAsync(TokenEventArgs e)
        {
            SelectionManager.UpdateSelection(e.TokenDisplay, e.SelectedTokens, e.IsControlPressed);

            //UpdateSelection(e.TokenDisplay, e.SelectedTokens, (e.ModifierKeys & ModifierKeys.Control) > 0);
            //await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            //NotePaneVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            NotePaneVisibility = SelectionManager.AnySelectedNotes ? Visibility.Visible : Visibility.Collapsed;

            //Message = $"'{e.TokenDisplay.SurfaceText}' token ({e.TokenDisplay.Token.TokenId}) {GetModifierKeysText(e.ModifierKeys)}clicked";
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Task.Run(() => TokenRightButtonDownAsync(e).GetAwaiter());
        }

        public async Task TokenRightButtonDownAsync(TokenEventArgs e)
        {
            SelectionManager.UpdateRightClickSelection(e.TokenDisplay);
            //UpdateSelection(e.TokenDisplay, e.SelectedTokens, false);
            //await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NotePaneVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) right-clicked";
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
        }

        public void TranslationMouseEnter(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
        }

        public void TranslationMouseLeave(TranslationEventArgs e)
        {
            Message = string.Empty;
        }

        public void NoteLeftButtonDown(NoteEventArgs e)
        {
            e.TokenDisplayViewModel.IsTokenSelected = true;
            SelectedTokens = new TokenDisplayViewModelCollection(e.TokenDisplayViewModel);
            NotePaneVisibility = Visibility.Visible;
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
        }

        public void NoteCreate(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public void FilterPins(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public void FilterPinsByBiblicalTerms(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public void TranslateQuick(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public async Task NoteAdded(NoteEventArgs e)
        {
            await NoteManager.AddNoteAsync(e.Note, e.EntityIds);
            Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ",e.EntityIds.Select(id => id.ToString()))}";
        }

        public async Task NoteDeleted(NoteEventArgs e)
        {
            await NoteManager.DeleteNoteAsync(e.Note, e.EntityIds);
            Message = $"Note '{e.Note.Text}' deleted from tokens ({string.Join(", ", e.EntityIds.Select(id => id.ToString()))})";
        }

        public async Task NoteEditorMouseEnter(NoteEventArgs e)
        {
            await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds);
        }

        public async Task NoteEditorMouseLeave(NoteEventArgs e)
        {
            await NoteManager.NoteMouseLeaveAsync(e.Note, e.EntityIds);
        }

        public async Task NoteSendToParatext(NoteEventArgs e)
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

        public async Task NoteUpdated(NoteEventArgs e)
        {
            await NoteManager.UpdateNoteAsync(e.Note);
            Message = $"Note '{e.Note.Text}' updated on tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public async Task LabelAdded(LabelEventArgs e)
        {
            // If this is a new note, we'll handle any labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await NoteManager.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            }
            Message = $"Label '{e.Label.Text}' added for note";
        }

        public async Task LabelSelected(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.AssociateNoteLabelAsync(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' selected for note";
        }

        public async Task LabelRemoved(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.DetachNoteLabel(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' removed for note";
        }

        public void NoteAssociationClicked(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' clicked";
        }

        public void NoteAssociationDoubleClicked(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' double-clicked";
        }

        public void NoteAssociationRightButtonDown(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' right button down";
        }

        public void NoteAssociationMouseEnter(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' hovered";
        }

        public void NoteAssociationMouseLeave(NoteAssociationEventArgs e)
        {
            Message = string.Empty;
        }

        public void CloseNotePaneRequested(RoutedEventArgs args)
        {
            NotePaneVisibility = Visibility.Collapsed;
            SelectedTokens = new TokenDisplayViewModelCollection();
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            //await PopulateData();
            PopulateLexicon();

        }

        public async void DisplayModal()
        {
            var tokenDisplay = new TokenDisplayViewModel(new Token(new TokenId(1, 1, 1, 1, 1), "Sample", ""));
            var dialogViewModel = LifetimeScope?.Resolve<LexiconDialogViewModel>();
            if (dialogViewModel != null)
            {
                dialogViewModel.TokenDisplay = tokenDisplay;
                dialogViewModel.CurrentLexeme = DemoLexeme;
                dialogViewModel.SemanticDomainSuggestions = SemanticDomainSuggestions;
                dialogViewModel.Concordance = Concordance;

                var result = await WindowManager.ShowDialogAsync(dialogViewModel, null, dialogViewModel.DialogSettings());
            }
        }

        public MeaningViewModel DemoMeaning { get; set; }
        public LexemeViewModel DemoLexeme { get; set; }
        public SemanticDomainCollection SemanticDomainSuggestions { get; set; }
        public LexiconTranslationViewModelCollection Concordance { get; set; }
        private void PopulateLexicon()
        {
            SemanticDomainSuggestions = new SemanticDomainCollection
            {
                new SemanticDomain { Text = "Apple" },
                new SemanticDomain { Text = "Apricot" },
                new SemanticDomain { Text = "Banana" },
                new SemanticDomain { Text = "Cherry" }
            };
            var semanticDomains1 = new SemanticDomainCollection
            {
                new SemanticDomain { Text = "Semantic Domain 1" },
                new SemanticDomain { Text = "Semantic Domain 2" },
                new SemanticDomain { Text = "Semantic Domain 3" }
            };
            var semanticDomains2 = new SemanticDomainCollection
            {
                new SemanticDomain { Text = "Semantic Domain 4" },
                new SemanticDomain { Text = "Semantic Domain 5" },
                new SemanticDomain { Text = "Semantic Domain 6" }
            };
            var forms = new LexemeFormCollection
            {
                new Form { Text = "Form 1" },
                new Form { Text = "Form 2" },
                new Form { Text = "Form 3" },
            };
            var translations1 = new LexiconTranslationViewModelCollection
            {
                new LexiconTranslationViewModel { Text = "Translation 1", Count = 10, 
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 2", Count = 5,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 3", Count = 2,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
            };
            var translations2 = new LexiconTranslationViewModelCollection
            {
                new LexiconTranslationViewModel { Text = "Translation 4", Count = 8,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 5", Count = 3,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 6", Count = 1,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
            };
            Concordance = new LexiconTranslationViewModelCollection
            {
                new LexiconTranslationViewModel { Text = "Translation 7", Count = 8,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 8", Count = 3,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) },
                new LexiconTranslationViewModel { Text = "Translation 9", Count = 1,
                    TranslationId = TranslationId.Create(Guid.NewGuid()) }
            };

            var meaning1 = new MeaningViewModel(new Meaning
            {
                MeaningId = MeaningId.Create(Guid.NewGuid()),
                Text = "Meaning 1",
                SemanticDomains = semanticDomains1,
            })
            {
                Translations = translations1
            };
            var meaning2 = new MeaningViewModel(new Meaning
            {
                Text = "Meaning 2",
                SemanticDomains = semanticDomains2,
            })
            {
                Translations = translations2
            };
            DemoLexeme = new LexemeViewModel
            {
                Lemma = "Sample",
                Forms = forms,
                Meanings = new MeaningViewModelCollection { meaning1, meaning2 }
            };
            //DemoMeaning = meaning1;
        }
        #endregion

#pragma warning disable CS8618
        /// <summary>
        /// Default constructor for design-time support only.
        /// </summary>
        public EnhancedViewDemoViewModel()
        {
        }

        public EnhancedViewDemoViewModel(INavigationService navigationService, ILogger<EnhancedViewDemoViewModel> logger, DashboardProjectManager projectManager, NoteManager noteManager, SelectionManager selectionManager, IEventAggregator eventAggregator, IMediator mediator, IServiceProvider serviceProvider, ILifetimeScope? lifetimeScope, ILocalizationService localizationService, IWindowManager windowManager)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            WindowManager = windowManager;
            NoteManager = noteManager;
            SelectionManager = selectionManager;
            ServiceProvider = serviceProvider;
        }
#pragma warning restore CS8618
    }
}
