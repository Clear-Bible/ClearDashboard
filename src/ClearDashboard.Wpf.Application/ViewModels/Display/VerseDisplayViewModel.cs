// Uncomment this preprocessor definition to use mock data for dev/test purposes.  If defined, no database operations are performed.
//#define MOCK

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
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
    public class VerseDisplayViewModel : PropertyChangedBase
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

        private TranslationSet? TranslationSet { get; set; }

        private AlignmentSet? AlignmentSet { get; set; }

        public IEnumerable<Alignment>? Alignments { get; set; }

        private IReadOnlyList<Token> Tokens { get; set; }

        private IReadOnlyList<Token>? TargetTokens { get; set; } = null;
        private IEnumerable<Translation>? Translations { get; set; }
        private EngineStringDetokenizer SourceDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        private EngineStringDetokenizer? TargetDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        private Dictionary<IId, IEnumerable<Note>>? AllEntityNotes { get; set; }
        private bool IsRtl { get; set; }

        private bool IsTargetRtl { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TokenDisplayViewModels { get; private set; } = new();

        public TokenDisplayViewModelCollection TargetTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of <see cref="Label"/>s that can be used for auto-completion of labels.
        /// </summary>
        public ObservableCollection<Label> LabelSuggestions { get; private set; } = new();

        #region Public Properties

        public Guid Id { get; set; } = Guid.NewGuid();

        #endregion Public Properties


        #region Private methods
        private static IEnumerable<(Token token, string paddingBefore, string paddingAfter)> GetPaddedTokens(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer, ILogger? logger)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var result = detokenizer.Detokenize(tokens);
#if DEBUG
                stopwatch.Stop();
                logger?.LogInformation($"Retrieved padded tokens from {detokenizer.GetType().Name} detokenizer in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private static Translation? GetTranslationForToken(Token token, IEnumerable<Translation>? translations)
        {
#if MOCK
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? GetMockTranslationWord()
                : String.Empty;
            return new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: GetMockTranslationStatus());
#else
            return translations?.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id) ?? null;
#endif
        }

        private static NoteViewModelCollection GetNotesForToken(Token token, Dictionary<IId, IEnumerable<Note>>? allEntityNotes)
        {
#if MOCK
            return GetMockNotes();
#else
            var matches = allEntityNotes?.FirstOrDefault(kvp => kvp.Key.Id == token.TokenId.Id);
            return matches is { Key: { } } ? new NoteViewModelCollection(matches.Value.Value.Select(n => new NoteViewModel(n))) : new NoteViewModelCollection();
#endif
        }

        private static TokenDisplayViewModelCollection BuildTokenDisplayViewModels(
            IReadOnlyList<Token> tokens, 
            EngineStringDetokenizer detokenizer, 
            IEnumerable<Translation>? translations,
            Dictionary<IId, IEnumerable<Note>>? allEntityNotes,
            bool isRtl,
            bool isSource,
            ILogger? logger)
        {
            var paddedTokens = GetPaddedTokens(tokens, detokenizer, logger);

            var tokenDisplayViewModelCollection = new TokenDisplayViewModelCollection();
            tokenDisplayViewModelCollection.AddRange(from paddedToken in paddedTokens
                let translation = GetTranslationForToken(paddedToken.token, translations)
                let notes = GetNotesForToken(paddedToken.token, allEntityNotes)
                select new TokenDisplayViewModel
                {
                    Token = paddedToken.token,
                    // For right-to-left languages, the padding before and padding after should be swapped.
                    PaddingBefore = !isRtl ? paddedToken.paddingBefore : paddedToken.paddingAfter,
                    PaddingAfter = !isRtl ? paddedToken.paddingAfter : paddedToken.paddingBefore,
                    Translation = translation,
                    Notes = notes,
                    IsSource = isSource
                });
            return tokenDisplayViewModelCollection;
        }

        private static async Task<IEnumerable<Translation>> GetTranslations(TranslationSet translationSet, IEnumerable<TokenId> tokens, ILogger?logger)
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
                var result = await translationSet!.GetTranslations(tokens);
#if DEBUG
                stopwatch.Stop();
                logger?.LogInformation($"Retrieved translations in {stopwatch.ElapsedMilliseconds} ms");
#endif
                return result;
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }

        private static async Task<Dictionary<IId, IEnumerable<Note>>?> GetAllNotes(IMediator mediator)
        {
#if MOCK
            return new Dictionary<IId, IEnumerable<Note>>();
#else
            return await Note.GetAllDomainEntityIdNotes(mediator);
#endif
        }

        private static async Task<ObservableCollection<Label>> GetLabelSuggestions(IMediator mediator, ILogger? logger)
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
                var labels = await Label.GetAll(mediator);
#if DEBUG
                stopwatch.Stop();
                logger?.LogInformation($"Retrieved label suggestions in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return new ObservableCollection<Label>(labels.OrderBy(l => l.Text));
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
#endif
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
                    throw new InvalidOperationException("Cannot retrieve translation options because the translation set is null.  Ensure that you have called ShowTranslationAsync() with the current translation set.");
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
                    throw new InvalidOperationException("Cannot save translation because the translation set is null.  Ensure that you have called ShowTranslationAsync() with the current translation set.");
                }

                await TranslationSet.PutTranslation(translation, translationActionType);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Saved translation options for {translation.SourceToken.SurfaceText} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                // If translation propagates to other translations, then we need a fresh call to PopulateTranslations() and to rebuild the token displays.
                if (translationActionType == TranslationActionTypes.PutPropagate)
                {
                    Translations = await GetTranslations(TranslationSet, Tokens.Select(t => t.TokenId), Logger);

                    TokenDisplayViewModels = BuildTokenDisplayViewModels(Tokens, SourceDetokenizer, Translations, AllEntityNotes, IsRtl, true, Logger);
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
#endif
        }

        ///// <summary>
        ///// Adds a note to a specified entity.
        ///// </summary>
        ///// <param name="note">The <see cref="Note"/> to add.</param>
        ///// <param name="entityId">The entity ID to which to add the note.</param>
        ///// <returns>An awaitable <see cref="Task"/>.</returns>
        //public async Task AddNoteAsync(Note note, IId entityId)
        //{
        //    await AddNoteAsync(note, new EntityIdCollection(entityId.ToEnumerable()));
        //}

        ///// <summary>
        ///// Adds a note to a collection of entities.
        ///// </summary>
        ///// <param name="note">The <see cref="NoteViewModel"/> to add.</param>
        ///// <param name="entityIds">The entity IDs to which to associate the note.</param>
        ///// <returns>An awaitable <see cref="Task"/>.</returns>
        //public async Task AddNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
        //{
        //    await AddNoteAsync(note.Note, entityIds);
        //}

        /// <summary>
        /// Adds a note to a collection of entities.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to add.</param>
        /// <param name="entityIds">The entity IDs to which to associate the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AddNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.Note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    await note.Note.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated note {note.NoteId?.Id} with entity {entityId} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
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

                        await note.Note.AssociateLabel(Mediator, label);
                    }
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated labels with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
                foreach (var entityId in entityIds)
                {
                    var token = TokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                    token?.NoteAdded(note);
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
        /// <param name="note">The <see cref="NoteViewModel"/> to update.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UpdateNoteAsync(NoteViewModel note)
        {
            await UpdateNoteAsync(note.Note);
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

        ///// <summary>
        ///// Deletes a note.
        ///// </summary>
        ///// <param name="note">The <see cref="NoteViewModel"/> to delete.</param>
        ///// <param name="entityIds">The entity IDs from which to remove the note.</param>
        ///// <returns>An awaitable <see cref="Task"/>.</returns>
        //public async Task DeleteNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
        //{
        //    await DeleteNoteAsync(note.Note, entityIds);
        //}

        /// <summary>
        /// Deletes a note.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to delete.</param>
        /// <param name="entityIds">The entity IDs from which to remove the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DeleteNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
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
                await note.Note.Delete(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Deleted note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
                    var token = TokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                    token?.NoteDeleted(note);
                }
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
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="labelText">The text of the new <see cref="Label"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateAssociateNoteLabelAsync(NoteViewModel note, string labelText)
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
                async void CreateAssociateLabel()
                {
                    var newLabel = await note.Note.CreateAssociateLabel(Mediator, labelText);
                    LabelSuggestions.Add(newLabel);
                    LabelSuggestions = new ObservableCollection<Label>(LabelSuggestions.OrderBy(l => l.Text));
                }

                await App.Current.Dispatcher.InvokeAsync(CreateAssociateLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Created label {labelText} and associated it with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
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
        /// Associates a <see cref="Label"/> with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="label">The <see cref="Label"/> to associate.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AssociateNoteLabelAsync(NoteViewModel note, Label label)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                async void AssociateLabel()
                {
                    await note.Note.AssociateLabel(Mediator, label);
                }

                await App.Current.Dispatcher.InvokeAsync(AssociateLabel);
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
        /// <param name="note">The <see cref="NoteViewModel"/> from which to detach the label.</param>
        /// <param name="label">The <see cref="Label"/> to detach.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DetachNoteLabel(NoteViewModel note, Label label)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                async void DetachLabel()
                {
                    await note.Note.DetachLabel(Mediator, label);
                }

                await App.Current.Dispatcher.InvokeAsync(DetachLabel);
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

        public async Task ShowCorpusAsync(
            TokensTextRow textRow, 
            EngineStringDetokenizer detokenizer, 
            bool isRtl)
        {
            Tokens = textRow.Tokens;
            TranslationSet = null;
            AlignmentSet = null;
            SourceDetokenizer = detokenizer;
            IsRtl = isRtl;
            IsTargetRtl = false;
            AllEntityNotes = await GetAllNotes(Mediator);
            LabelSuggestions = await GetLabelSuggestions(Mediator, Logger);

            TokenDisplayViewModels = BuildTokenDisplayViewModels(Tokens, detokenizer, null, AllEntityNotes, IsRtl, true, Logger);
        }

        public async Task ShowTranslationAsync(
            EngineParallelTextRow engineParallelTextRow,
            TranslationSet translationSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl)
        {
            Tokens = engineParallelTextRow.SourceTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            TranslationSet = translationSet;
            AlignmentSet = null;
            SourceDetokenizer = sourceDetokenizer;
            IsRtl = isSourceRtl;
            IsTargetRtl = false;
            AllEntityNotes = await GetAllNotes(Mediator);
            LabelSuggestions = await GetLabelSuggestions(Mediator, Logger);
            Translations = await GetTranslations(TranslationSet, Tokens.Select(t => t.TokenId), Logger);

            TokenDisplayViewModels = BuildTokenDisplayViewModels(Tokens, sourceDetokenizer, Translations, AllEntityNotes, IsRtl, true, Logger);
        }

        public async Task ShowAlignmentsAsync(
            EngineParallelTextRow engineParallelTextRow,
            AlignmentSet alignmentSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl,
            EngineStringDetokenizer targetDetokenizer,
            bool isTargetRtl)
        {
            Tokens = engineParallelTextRow.SourceTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            TargetTokens = engineParallelTextRow.TargetTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            TranslationSet = null;
            AlignmentSet = alignmentSet;
            Alignments = await alignmentSet.GetAlignments(new List<EngineParallelTextRow>() { engineParallelTextRow });
            SourceDetokenizer = sourceDetokenizer;
            IsRtl = isSourceRtl;
            IsTargetRtl = isTargetRtl;
            AllEntityNotes = await GetAllNotes(Mediator);
            LabelSuggestions = await GetLabelSuggestions(Mediator, Logger);

            TokenDisplayViewModels = BuildTokenDisplayViewModels(Tokens, sourceDetokenizer, null, AllEntityNotes, isSourceRtl, true, Logger);
            TargetTokenDisplayViewModels = BuildTokenDisplayViewModels(TargetTokens, targetDetokenizer, null, AllEntityNotes, isTargetRtl, false, Logger);
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

        /// <summary>
        /// Constructor used via dependency injection.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mediator"></param>
        // ReSharper disable once UnusedMember.Global
        public VerseDisplayViewModel(ILogger<VerseDisplayViewModel>? logger, IMediator mediator) : this()
        {
            Logger = logger;
            _mediator = mediator;
        }
    }
}
