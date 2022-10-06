// Uncomment this preprocessor definition to use mock data for dev/test purposes.  If defined, no database operations are performed.
//#define MOCK

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;

#if MOCK
// Additional using statements for mock data
using System.IO;
using System.Text;
using SIL.Machine.Corpora;
using SIL.Scripture;
using ClearDashboard.DAL.Alignment.Corpora;
#endif

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    /// <summary>
    /// A class containing the need information to render a verse of <see cref="Token"/>s in the UI.
    /// </summary>
    public class VerseDisplayViewModel
    {
        #region Mock data construction
#if MOCK

        private static readonly string _testDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string _usfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "Tes");
        private static IEnumerable<TokensTextRow>? _mockCorpus;

        private static IEnumerable<TokensTextRow> MockCorpus => _mockCorpus ??=
            new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _usfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Transform<SetTrainingBySurfaceLowercase>()
                .Cast<TokensTextRow>();

        // ReSharper disable once InconsistentNaming
        private static TokensTextRow? GetMockVerseTextRow(int BBBCCCVVV)
        {
            return MockCorpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
        }

        // ReSharper disable StringLiteralTypo
        // ReSharper disable once IdentifierTypo
        private static readonly List<string> _mockOogaWords = new() { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };
        // ReSharper restore StringLiteralTypo

        private static int _mockTranslationWordIndexer;

        private static string GetMockTranslationWord()
        {
            var result = _mockOogaWords[_mockTranslationWordIndexer++];
            if (_mockTranslationWordIndexer == _mockOogaWords.Count) _mockTranslationWordIndexer = 0;
            return result;
        }

        private static string GetMockTranslationStatus()
        {
            return new Random().Next(3) switch
            {
                0 => "FromTranslationModel",
                1 => "FromOther",
                _ => "Assigned"
            };
        }

        private static IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        {
            var result = new List<TranslationOption>();

            var random = new Random();
            var optionCount = random.Next(4) + 2; // 2-5 options
            var remainingPercentage = 100d;

            var basePercentage = random.NextDouble() * remainingPercentage;
            result.Add(new TranslationOption { Word = sourceTranslation, Count = basePercentage });
            remainingPercentage -= basePercentage;

            for (var i = 1; i < optionCount - 1; i++)
            {
                var percentage = random.NextDouble() * remainingPercentage;
                result.Add(new TranslationOption { Word = GetMockTranslationWord(), Count = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = GetMockTranslationWord(), Count = remainingPercentage });

            return result.OrderByDescending(to => to.Count);
        }

        private static NoteId GetMockNoteId()
        {
            return new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new UserId(Guid.NewGuid(), "Joe Schmoe"));

        }
        private static ObservableCollection<Label> GetMockLabels()
        {
            var mockLabelSuggestions = MockLabelSuggestions.ToList();
            var mockLabels = new List<string>();
            var numMockLabels = new Random().Next(4);

            for (var i = 0; i <= numMockLabels; i++)
            {
                var mockLabelIndex = new Random().Next(mockLabelSuggestions.Count);
                var mockLabel = mockLabelSuggestions[mockLabelIndex].Text;
                if (mockLabel != null && !mockLabels.Contains(mockLabel))
                {
                    mockLabels.Add(mockLabel);
                }
            }

            return new ObservableCollection<Label>(mockLabels.OrderBy(lt => lt)
                .Select(lt => new Label { Text = lt }));
        }

        private static NoteCollection GetMockNotes()
        {
            var result = new NoteCollection();
            var random = new Random().NextDouble();
            if (random < 0.2)
            {
                result.Add(new Note
                {
                    Text = "This is a note",
                    NoteId = GetMockNoteId(),
                    Labels = GetMockLabels()
                });
            }

            if (random < 0.1)
            {
                result.Add(new Note
                {
                    Text = "Here's another note",
                    NoteId = GetMockNoteId(),
                    Labels = GetMockLabels()
                });
            }

            return result;
        }

        // ReSharper disable StringLiteralTypo
        private static ObservableCollection<Label> MockLabelSuggestions => new()
        {
            new Label { Text = "alfa" },
            new Label { Text = "bravo" },
            new Label { Text = "charlie" },
            new Label { Text = "delta" },
            new Label { Text = "echo" }
        };
        // ReSharper restore StringLiteralTypo
#endif
        #endregion

        private ILogger<VerseDisplayViewModel>? Logger { get; }

        private readonly IMediator? _mediator;
        private IMediator Mediator
        {
            get
            {
                if (_mediator == null)
                {
                    throw new InvalidOperationException("Could not perform database operation because Mediator is null. Ensure that the VerseDisplayViewModel is instantiated via the DI ServiceProvider.");
                }
                return _mediator;
            }
        }

        private TranslationSet? _translationSet;
        private TranslationSet? TranslationSet
        {
            get
            {
                if (_translationSet == null)
                {
                    throw new InvalidOperationException("Cannot perform translation operations because the TranslationSet is null.  Ensure that you have called VerseDisplayViewModel.BindAsync() with the current translation set.");
                }
                return _translationSet;
            }
            set => _translationSet = value;
        }

        private IReadOnlyList<Token> Tokens { get; set; }
        private IEnumerable<Translation> Translations { get; set; } = new List<Translation>();
        private EngineStringDetokenizer Detokenizer { get; set; } = new(new LatinWordDetokenizer());
        private Dictionary<IId, IEnumerable<Note>>? AllEntityNotes { get; set; }
        private bool IsRtl { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of <see cref="Label"/>s that can be used for auto-completion of labels.
        /// </summary>
        public ObservableCollection<Label> LabelSuggestions { get; private set; } = new();
        
        #region Private methods
        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)> GetPaddedTokens(IEnumerable<Token> tokens)
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

        private Translation? GetTranslationForToken(Token token)
        {
#if MOCK
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? GetMockTranslationWord()
                : String.Empty;
            return new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: GetMockTranslationStatus());
#else
            return Translations.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id);
#endif
        }

        private NoteCollection GetNotesForToken(Token token)
        {
#if MOCK
            return GetMockNotes();
#else
            var matches = AllEntityNotes?.FirstOrDefault(kvp => kvp.Key.Id == token.TokenId.Id);
            return matches is { Key: { } } ? new NoteCollection(matches.Value.Value) : new NoteCollection();
#endif
        }

        private void BuildTokenDisplayViewModels()
        {
            TokenDisplayViewModels = new TokenDisplayViewModelCollection();
            var paddedTokens = GetPaddedTokens(Tokens);

            TokenDisplayViewModels.AddRange(from paddedToken in paddedTokens
                let translation = GetTranslationForToken(paddedToken.token)
                let notes = GetNotesForToken(paddedToken.token)
                select new TokenDisplayViewModel
                {
                    Token = paddedToken.token,
                    // For right-to-left languages, the padding before and padding after should be swapped.
                    PaddingBefore = !IsRtl ? paddedToken.paddingBefore : paddedToken.paddingAfter,
                    PaddingAfter = !IsRtl ? paddedToken.paddingAfter : paddedToken.paddingBefore,
                    Translation = translation,
                    Notes = notes
                });

        }

        private async Task<IEnumerable<Translation>> GetTranslations(IEnumerable<TokenId> tokens)
        {
#if MOCK
            return new List<Translation>();
#else
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var result = await TranslationSet!.GetTranslations(tokens);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved translations in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }

        private async Task PopulateTranslations()
        {
            if (_translationSet != null)
            {
                Translations = await GetTranslations(Tokens.Select(t => t.TokenId));
            }
        }

        private async Task<Dictionary<IId, IEnumerable<Note>>?> GetAllNotes()
        {
#if MOCK
            return new Dictionary<IId, IEnumerable<Note>>();
#else
            return await Note.GetAllDomainEntityIdNotes(Mediator);
#endif
        }

        private async Task<ObservableCollection<Label>> GetLabelSuggestions()
        {
#if MOCK
            return MockLabelSuggestions;
#else
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

        private async Task PopulateLabelSuggestions()
        {
            LabelSuggestions = await GetLabelSuggestions();
        }

        private async Task BindAsync(TranslationSet? translationSet = null, EngineStringDetokenizer? detokenizer = null, bool isRtl = false)
        {
            TranslationSet = translationSet;
            if (detokenizer != null)
            {
                Detokenizer = detokenizer;
            }
            IsRtl = isRtl;
            AllEntityNotes = await GetAllNotes();
            await PopulateLabelSuggestions();
            await PopulateTranslations();
            BuildTokenDisplayViewModels();
        }

        #endregion
        #region Public API

        /// <summary>
        /// Gets a collection of <see cref="TranslationOption"/>s for a given translation.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> for which to provide options.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> containing a <see cref="IEnumerable{T}"/> of <see cref="TranslationOption"/>s.</returns>
        public async Task<IEnumerable<TranslationOption>> GetTranslationOptionsAsync(Translation translation)
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
                if (TranslationSet == null)
                {
                    throw new InvalidOperationException("Cannot retrieve translation options because the translation set is null.  Ensure that you have called BindAsync() with the current translation set.");
                }

                var translationModelEntry = await TranslationSet.GetTranslationModelEntryForToken(translation.SourceToken);
                if (translationModelEntry == null)
                {
                    Logger?.LogCritical($"Cannot find translation options for {translation.SourceToken.SurfaceText}");
                    return new List<TranslationOption>();
                }

                var translationOptions = translationModelEntry.OrderByDescending(option => option.Value)
                    .Select(option => new TranslationOption { Word = option.Key, Count = option.Value })
                    .Take(4)        // For now, we'll just return the top four options; may be configurable in the future
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

        /// <summary>
        /// Saves a selected translation for a token to the database.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> to save to the database.</param>
        /// <param name="translationActionType"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task PutTranslationAsync(Translation translation, string translationActionType)
        {
#if MOCK
            return;
#else
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                if (TranslationSet == null)
                {
                    throw new InvalidOperationException("Cannot save translation because the translation set is null.  Ensure that you have called BindAsync() with the current translation set.");
                }

                await TranslationSet.PutTranslation(translation, translationActionType);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Saved translation options for {translation.SourceToken.SurfaceText} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                // If translation propagates to other translations, then we need a fresh call to PopulateTranslations() and to rebuild the token displays.
                if (translationActionType == TranslationActionTypes.PutPropagate)
                {
                    await PopulateTranslations();
                    BuildTokenDisplayViewModels();
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }

        /// <summary>
        /// Adds a note to a specified entity.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to add.</param>
        /// <param name="entityId">The entity ID to which to add the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AddNoteAsync(Note note, IId entityId)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
#endif
                await note.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Associated note {note.NoteId?.Id} with entity {entityId.Id} in {stopwatch.ElapsedMilliseconds} ms");
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
                    Logger?.LogInformation($"Associated labels with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                    var token = TokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                    token?.NoteAdded();
                }
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a note to a collection of tokens.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to add.</param>
        /// <param name="tokens">The entity ID to which to add the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AddNoteAsync(Note note, TokenDisplayViewModelCollection tokens)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Restart();
#endif
                //await note.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                stopwatch.Stop();
                //Logger?.LogInformation($"Associated note {note.NoteId?.Id} with entity {entityId.Id} in {stopwatch.ElapsedMilliseconds} ms");
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
                    Logger?.LogInformation($"Associated labels with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                    //var token = TokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                    //token?.NoteAdded();
                }
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Updates a note.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to update.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UpdateNoteAsync(Note note)
        {
            try
            {
#if MOCK
                return;
#endif
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Updated note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Deletes a note.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to update.</param>
        /// <param name="entityId">The entity ID from which to add the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DeleteNoteAsync(Note note, IId entityId)
        {
            try
            {
#if MOCK
                return;
#endif
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.Delete(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Deleted note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                var token = TokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                token?.NoteDeleted();
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Label"/> and associates it with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to which to associate the label.</param>
        /// <param name="labelText">The text of the new <see cref="Label"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateAssociateNoteLabelAsync(Note note, string labelText)
        {
            try
            {
#if MOCK
                var newLabel = new Label { Text = labelText };
#else
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var newLabel = await note.CreateAssociateLabel(Mediator, labelText);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Created label {labelText} and associated it with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
#endif
                LabelSuggestions.Add(newLabel);
                LabelSuggestions = new ObservableCollection<Label>(LabelSuggestions.OrderBy(l => l.Text));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Associates a <see cref="Label"/> with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to which to associate the label.</param>
        /// <param name="label">The <see cref="Label"/> to associate.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AssociateNoteLabelAsync(Note note, Label label)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.AssociateLabel(Mediator, label);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Associated label {label.Text} with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Detaches a <see cref="Label"/> from a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> from which to detach the label.</param>
        /// <param name="label">The <see cref="Label"/> to detach.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DetachNoteLabel(Note note, Label label)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.DetachLabel(Mediator, label);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Detached label {label.Text} from note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

#if MOCK
        // ReSharper disable once UnusedMember.Global
        public async Task BindMockVerseAsync(int BBBCCCVVV = 40001001)
        {
            var row = GetMockVerseTextRow(BBBCCCVVV);
            await BindAsync(row);
    }
#endif

        /// <summary>
        /// Binds an <see cref="EngineParallelTextRow"/> to this view model.
        /// </summary>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> to display.</param>
        /// <param name="translationSet">The <see cref="TranslationSet"/> from which to obtain token translations.</param>
        /// <param name="detokenizer">The detokenizer to use for the text row (which can be obtained from TokenizedCorpus.Detokenizer).  Defaults to <see cref="LatinWordDetokenizer"/>.</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        /// <remarks>Unless a <paramref name="translationSet"/> is provided, then no translations can be provided. </remarks>
        /// <exception cref="InvalidOperationException">Thrown is <paramref name="parallelTextRow"/> has no tokens.</exception>
        public async Task BindAsync(EngineParallelTextRow parallelTextRow, TranslationSet? translationSet = null, EngineStringDetokenizer? detokenizer = null, bool isRtl = false)
        {
            Tokens = parallelTextRow.SourceTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            await BindAsync(translationSet, detokenizer, isRtl);
        }

        /// <summary>
        /// Binds an <see cref="TokensTextRow"/> to this view model.
        /// </summary>
        /// <param name="textRow">The <see cref="TokensTextRow"/> to display.</param>
        /// <param name="translationSet">The <see cref="TranslationSet"/> from which to obtain token translations.</param>
        /// <param name="detokenizer">The detokenizer to use for the text row (which can be obtained from TokenizedCorpus.Detokenizer).  Defaults to <see cref="LatinWordDetokenizer"/>.</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        /// <remarks>Unless a <paramref name="translationSet"/> is provided, then no translations can be provided. </remarks>
        public async Task BindAsync(TokensTextRow textRow, TranslationSet? translationSet = null, EngineStringDetokenizer? detokenizer = null, bool isRtl = false)
        {
            Tokens = textRow.Tokens;
            await BindAsync(translationSet, detokenizer, isRtl);
        }
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Unless the following constructor is used (via dependency injection), then this view model will not be able to perform database operations. 
        /// </remarks>
        public VerseDisplayViewModel()
        {
            Tokens = new ReadOnlyList<Token>(new List<Token>());
        }

        public VerseDisplayViewModel(ILogger<VerseDisplayViewModel>? logger, IMediator mediator) : this()
        {
            Logger = logger;
            _mediator = mediator;
        }
    }
}
