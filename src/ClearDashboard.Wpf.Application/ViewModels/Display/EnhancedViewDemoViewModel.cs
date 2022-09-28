// Uncomment this preprocessor definition to use mock data for dev/test purposes.
//#define MOCK

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        #region Mock data
#if MOCK
        private static readonly string _testDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string _usfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "Tes");
        private static IEnumerable<TokensTextRow>? _mockCorpus;

        private static IEnumerable<TokensTextRow>? MockCorpus => _mockCorpus ??=
            new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _usfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Transform<SetTrainingBySurfaceLowercase>()
                .Cast<TokensTextRow>();

        // ReSharper disable once InconsistentNaming
        private static TokensTextRow? MockVerseTextRow(int BBBCCCVVV)
        {
            return MockCorpus?.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
        }

        private static readonly List<string> _mockOogaWords = new()
            { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };

        private static int _mockTranslationWordIndexer;

        private static string MockTranslationWord
        {
            get
            {
                var result = _mockOogaWords[_mockTranslationWordIndexer++];
                if (_mockTranslationWordIndexer == _mockOogaWords.Count) _mockTranslationWordIndexer = 0;
                return result;
            }
        }

        private static string MockTranslationStatus => new Random().Next(3) switch
        {
            0 => "FromTranslationModel",
            1 => "FromOther",
            _ => "Assigned"
        };

        private IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        {
            var result = new List<TranslationOption>();

            var random = new Random();
            var optionCount = random.Next(4) + 2; // 2-5 options
            var remainingPercentage = 100d;

            var basePercentage = random.NextDouble() * remainingPercentage;
            result.Add(new TranslationOption { Word = sourceTranslation, Probability = basePercentage });
            remainingPercentage -= basePercentage;

            for (var i = 1; i < optionCount - 1; i++)
            {
                var percentage = random.NextDouble() * remainingPercentage;
                result.Add(new TranslationOption { Word = MockTranslationWord, Probability = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = MockTranslationWord, Probability = remainingPercentage });

            return result.OrderByDescending(to => to.Probability);
        }

        private ObservableCollection<Label> MockLabels
        {
            get
            {
                var labelSuggestions = LabelSuggestions.ToList();
                var labelTexts = new List<string>();
                var numLabels = new Random().Next(4);

                for (var i = 0; i <= numLabels; i++)
                {
                    var labelIndex = new Random().Next(LabelSuggestions.Count());
                    if (!labelTexts.Contains(labelSuggestions[labelIndex].Text))
                    {
                        labelTexts.Add(labelSuggestions[labelIndex].Text);
                    }
                }

                return new ObservableCollection<Label>(labelTexts.OrderBy(lt => lt)
                    .Select(lt => new Label { Text = lt }));
            }
        }

        private ObservableCollection<Note> MockNotes
        {
            get
            {
                var result = new ObservableCollection<Note>();
                var random = new Random().NextDouble();
                if (random < 0.2)
                {
                    result.Add(new Note
                    {
                        Text = "This is a note",
                        //NoteId = new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new UserId(Guid.NewGuid())),
                        //Labels = MockLabels
                    });
                }

                if (random < 0.1)
                {
                    result.Add(new Note
                    {
                        Text = "Here's another note",
                        //NoteId = new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new UserId(Guid.NewGuid())),
                        //Labels = MockLabels
                    });
                }

                return result;
            }
        }

        private static ObservableCollection<Label> MockLabelSuggestions => new()
        {
            new Label { Text = "alfa" },
            new Label { Text = "bravo" },
            new Label { Text = "charlie" },
            new Label { Text = "delta" },
            new Label { Text = "echo" }
        };
#endif
#endregion

        private async Task<ObservableCollection<Label>> GetLabelSuggestions()
        {
#if MOCK
            return MockLabelSuggestions;
#else
            if (Mediator == null)
            {
                Logger?.LogCritical("Mediator is null; could not retrieve label suggestions from database");
                return new ObservableCollection<Label>();
            }
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var labels = await Label.GetAll(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved label suggestions in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return new ObservableCollection<Label>(labels);
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }

        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)> GetPaddedTokens(
            IEnumerable<Token> tokens)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var result = Detokenizer.Detokenize(tokens);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved padded tokens from {Detokenizer.GetType().Name} detokenizer in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private async Task<IEnumerable<TranslationOption>> GetTranslationOptions(Translation translation)
        {
#if MOCK
            return GetMockTranslationOptions(translation.TargetTranslationText);
#else
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var translationModelEntry = await CurrentTranslationSet.GetTranslationModelEntryForToken(translation.SourceToken);
                var translationOptions = translationModelEntry.OrderByDescending(option => option.Value)
                    .Select(option => new TranslationOption { Word = option.Key, Probability = option.Value })
                    .Take(4)    // FORNOW
                    .ToList();
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved translation options for {translation.SourceToken.SurfaceText} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return translationOptions;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }
        private Translation GetTranslationForToken(Token token)
        {
#if MOCK
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? MockTranslationWord
                : String.Empty;
            return new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: MockTranslationStatus);
#else
            var translation = CurrentTranslations.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id);
            return translation;
#endif
        }

        private ObservableCollection<Note> GetNotesForToken(Token token)
        {
#if MOCK
            return MockNotes;
#else
            var matches = NotesDictionary.FirstOrDefault(kvp => kvp.Key.Id == token.TokenId.Id);
            return matches.Key != null
                ? new ObservableCollection<Note>(matches.Value)
                : new ObservableCollection<Note>();
#endif
        }

        private List<TokenDisplayViewModel> GetTokenDisplayViewModels(IEnumerable<Token> tokens)
        {
            var tokenDisplays = new List<TokenDisplayViewModel>();
            var paddedTokens = GetPaddedTokens(tokens);

            if (paddedTokens != null)
            {
                tokenDisplays.AddRange(from paddedToken in paddedTokens
                    let translation = GetTranslationForToken(paddedToken.token)
                    let notes = GetNotesForToken(paddedToken.token)
                    select new TokenDisplayViewModel
                    {
                        Token = paddedToken.token,
                        PaddingBefore = paddedToken.paddingBefore,
                        PaddingAfter = paddedToken.paddingAfter,
                        Translation = translation,
                        Notes = notes
                    });
            }

            return tokenDisplays;
        }

        public async Task<ParallelCorpus> GetParallelCorpus(ParallelCorpusId? corpusId = null)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                if (corpusId == null)
                {
                    var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator);
                    corpusId = corpusIds.First();
                }
                var corpus = await ParallelCorpus.Get(Mediator, corpusId);
                Detokenizer = corpus.Detokenizer;
#if DEBUG
                stopwatch.Stop();
                Logger.LogInformation($"Retrieved parallel corpus {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return corpus;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<EngineParallelTextRow?> GetVerseTextRow(int BBBCCCVVV, ParallelCorpusId corpusId = null)
        {
            try
            {
                var corpus = await GetParallelCorpus(corpusId);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var verse = corpus.GetByVerseRange(new VerseRef(BBBCCCVVV), 0, 0);
                var row = verse.parallelTextRows.FirstOrDefault() as EngineParallelTextRow;
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus verse {BBBCCCVVV} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return row;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<TranslationSet?> GetTranslationSet()
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var translationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator);
                var translationSet = await TranslationSet.Get(translationSetIds.First().translationSetId, Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved translation set {translationSet.TranslationSetId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return translationSet;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddNoteToDatabase(Note note, IId entityId)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
#endif
                await note.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Associated note {note.NoteId.Id} with entity {entityId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                if (note.Labels.Any())
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    foreach (var label in note.Labels)
                    {
                        if (label.LabelId == null)
                        {
                            await label.CreateOrUpdate(Mediator);
                        }
                        await note.AssociateLabel(Mediator, label);
                    }
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated labels with note {note.NoteId.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
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
#if MOCK
                var row = MockVerseTextRow(40001001);
                VerseTokens = GetTokenDisplayViewModels(row.Tokens);
#else
                await ProjectManager.LoadProject("EnhancedViewDemo");
                var row = await GetVerseTextRow(01001001);
                NotesDictionary = await Note.GetAllDomainEntityIdNotes(Mediator);
                CurrentTranslationSet = await GetTranslationSet();
                CurrentTranslations = await CurrentTranslationSet.GetTranslations(row.SourceTokens.Select(t => t.TokenId));
                VerseTokens = GetTokenDisplayViewModels(row.SourceTokens);
#endif
                LabelSuggestions = await GetLabelSuggestions();
                NotifyOfPropertyChange(nameof(VerseTokens));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IEnumerable<TokenDisplayViewModel>? VerseTokens { get; set; }
        public TokenDisplayViewModel CurrentToken { get; set; }
        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());
        public TranslationOption CurrentTranslationOption { get; set; }
        public TranslationSet CurrentTranslationSet { get; set; }
        public Dictionary<IId, IEnumerable<Note>> NotesDictionary { get; set; }
        public IEnumerable<Translation> CurrentTranslations { get; set; }
        public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        public IEnumerable<Label> LabelSuggestions { get; set; }

        public string? Message { get; set; }

        public Visibility NotePaneVisibility { get; set; } = Visibility.Collapsed;
        public Visibility TranslationPaneVisibility { get; set; } = Visibility.Collapsed;

        private void SetCurrentToken(TokenDisplayViewModel viewModel)
        {
            CurrentToken = viewModel;
            NotifyOfPropertyChange(nameof(CurrentToken));
        }

        private async Task DisplayTranslationPane(TranslationEventArgs e)
        {
            SetCurrentToken(e.TokenDisplayViewModel);

            TranslationPaneVisibility = Visibility.Visible;
            NotifyOfPropertyChange(nameof(TranslationPaneVisibility));

            TranslationOptions = await GetTranslationOptions(e.Translation);
            NotifyOfPropertyChange(nameof(TranslationOptions));

            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText) ?? null;
            NotifyOfPropertyChange(nameof(CurrentTranslationOption));
        }

        private void DisplayNotePane(TokenDisplayViewModel tokenDisplayViewModel)
        {
            SetCurrentToken(tokenDisplayViewModel);

            NotePaneVisibility = Visibility.Visible;
            NotifyOfPropertyChange(nameof(NotePaneVisibility));
        }

#region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplayViewModel.HasNote)
            {
                DisplayNotePane(e.TokenDisplayViewModel);
            }

            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationClicked(TranslationEventArgs e)
        {
            DisplayTranslationPane(e);

            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseEnter(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseLeave(TranslationEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            DisplayNotePane(e.TokenDisplayViewModel);

            Message = $"'Note indicator for token {e.EntityId} hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteCreate(NoteEventArgs e)
        {
            DisplayNotePane(e.TokenDisplayViewModel);

            Message = $"Opening new note panel for token {e.EntityId}";
            NotifyOfPropertyChange(nameof(Message));
        }

        public async Task TranslationApplied(TranslationEventArgs e)
        {
            await CurrentTranslationSet.PutTranslation(e.Translation, e.TranslationActionType);

            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplayViewModel.SurfaceText}' ({e.TokenDisplayViewModel.Token.TokenId})";
            NotifyOfPropertyChange(nameof(Message));

            TranslationPaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationPaneVisibility));
        }

        public void TranslationCancelled(RoutedEventArgs e)
        {
            Message = "Translation cancelled.";
            NotifyOfPropertyChange(nameof(Message));

            TranslationPaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationPaneVisibility));
        }        
        
        public async Task NoteAdded(NoteEventArgs e)
        {
#if !MOCK
            await AddNoteToDatabase(e.Note, e.EntityId);
#endif
            var token = VerseTokens.FirstOrDefault(vt => vt.Token.TokenId == e.EntityId);
            if (token != null)
            {
                token.NoteAdded(e.Note);
            }

            Message = $"Note '{e.Note.Text}' added to token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NotePaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NotePaneVisibility));
        }

        public async Task NoteUpdated(NoteEventArgs e)
        {
#if !MOCK
            await e.Note.CreateOrUpdate(Mediator);
#endif
            Message = $"Note '{e.Note.Text}' updated on token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NotePaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NotePaneVisibility));
        }

        public async Task NoteDeleted(NoteEventArgs e)
        {
#if !MOCK
            await e.Note.Delete(Mediator);
#endif
            Message = $"Note '{e.Note.Text}' deleted from token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NotePaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NotePaneVisibility));
        }

        public async Task LabelSelected(LabelEventArgs e)
        {
#if !MOCK
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await e.Note.AssociateLabel(Mediator, e.Label);
            }
#endif
            Message = $"Label '{e.Label.Text}' selected for note on token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public async Task LabelAdded(LabelEventArgs e)
        {
#if !MOCK
            // If this is a new note, we'll handle the labels when the note is added.
            if (e.Note.NoteId != null)
            {
                e.Label = await e.Note.CreateAssociateLabel(Mediator, e.Label.Text);
            }
#endif
            Message = $"Label '{e.Label.Text}' added for note on token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void CloseNotePaneRequested(RoutedEventArgs args)
        {
            NotePaneVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NotePaneVisibility));
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await PopulateData();
        }
        // ReSharper restore UnusedMember.Global

#endregion

        // ReSharper disable UnusedMember.Global
        public EnhancedViewDemoViewModel()
        {
        }

        public EnhancedViewDemoViewModel(INavigationService navigationService, ILogger<EnhancedViewDemoViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
        }
    }
}
