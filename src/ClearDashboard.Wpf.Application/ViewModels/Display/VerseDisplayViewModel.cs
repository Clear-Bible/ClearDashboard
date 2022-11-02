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
using ClearDashboard.DAL.Alignment.Translation;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.ObjectModel;
using static ClearBible.Engine.Persistence.FileGetBookIds;

// These need to be specified explicitly to resolve ambiguity with ClearDashboard.DataAccessLayer.Models.
using Alignment = ClearDashboard.DAL.Alignment.Translation.Alignment;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;
using Token = ClearBible.Engine.Corpora.Token;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

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

        private IReadOnlyList<Token> SourceTokens { get; set; }
        private EngineStringDetokenizer SourceDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        private bool IsSourceRtl { get; set; }
        private IReadOnlyList<Token>? TargetTokens { get; set; } = null;
        private EngineStringDetokenizer? TargetDetokenizer { get; set; } = new(new LatinWordDetokenizer());
        private bool IsTargetRtl { get; set; }

        private TranslationSet? TranslationSet { get; set; }
        private IEnumerable<Translation>? Translations { get; set; }

        private AlignmentSet? AlignmentSet { get; set; }
        public IEnumerable<Alignment>? Alignments { get; set; }

        private Dictionary<IId, IEnumerable<Note>>? AllNotes { get; set; }

        private static List<BookId>? _bookIds;
        private static IEnumerable<BookId> BookIds
        {
            get
            {
                if (_bookIds == null)
                {
                    _bookIds = ClearBible.Engine.Persistence.FileGetBookIds.BookIds;
                }
                return _bookIds;
            }
        }

        #region Public Properties

        /// <summary>
        /// Gets a collection of source <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection SourceTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of target <see cref="TokenDisplayViewModel"/>s to be rendered.
        /// </summary>
        public TokenDisplayViewModelCollection TargetTokenDisplayViewModels { get; private set; } = new();

        /// <summary>
        /// Gets a collection of <see cref="Label"/>s that can be used for auto-completion of labels.
        /// </summary>
        public LabelCollection LabelSuggestions { get; private set; } = new();

        public Guid Id { get; } = Guid.NewGuid();

        #endregion Public Properties

        #region Private methods
        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)> GetPaddedTokens(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer)
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
                Logger?.LogInformation($"Retrieved padded tokens from {detokenizer.GetType().Name} detokenizer in {stopwatch.ElapsedMilliseconds} ms");
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
            return Translations?.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id) ?? null;
#endif
        }

        // TODO: localize
        private static string GetDescriptionForNoteAssociation(IId associatedEntityId)
        {
            if (associatedEntityId.GetType() == typeof(TokenId))
            {
                if (associatedEntityId is TokenId tokenId)
                {
                    var bookNumberString = tokenId.BookNumber.ToString();
                    var bookId = BookIds.FirstOrDefault(b => b.silCannonBookNum == bookNumberString);
                    if (bookId != null)
                    {
                        return $"Token in {bookId.silCannonBookAbbrev} {tokenId.ChapterNumber}:{tokenId.VerseNumber} word {tokenId.WordNumber} part {tokenId.SubWordNumber}";
                    }
                }
            }

            return string.Empty;
        }

        private async Task<NoteViewModelCollection> GetNotesForEntityAsync(IId entityId)
        {
#if MOCK
            return GetMockNotes();
#else
            var result = new NoteViewModelCollection();
            var tokenMatch = AllNotes?.FirstOrDefault(kvp => kvp.Key.Id == entityId.Id);
            if (tokenMatch is { Key: { } })
            {
                var notesList = tokenMatch.Value.Value.OrderBy(n => n.NoteId?.Created).ToList();
                foreach (var parentNote in notesList.Where(note => !note.IsReply()))
                {
                    var noteViewModel = new NoteViewModel(parentNote);
                    var associatedEntityIds = await parentNote.GetFullDomainEntityIds(Mediator);
                    foreach (var associatedEntityId in associatedEntityIds)
                    {
                        noteViewModel.Associations.Add(new NoteAssociationViewModel
                        {
                            AssociatedEntityId = associatedEntityId, 
                            Description = GetDescriptionForNoteAssociation(associatedEntityId)

                        });
                    }
                    result.Add(noteViewModel);
                }

                foreach (var replyNote in notesList.Where(note => note.IsReply()))
                {
                    var parentNote = result.FirstOrDefault(n => n.ThreadId == replyNote.ThreadId);
                    if (parentNote != null)
                    {
                        parentNote.Replies.Add(new NoteViewModel(replyNote));
                    }
                    else
                    {
                        Logger?.LogError($"Could not find thread ID {replyNote.ThreadId} for reply note ID {replyNote.NoteId}");
                    }
                }
            }

            return result;
#endif
        }

        private async Task<TokenDisplayViewModelCollection> BuildTokenDisplayViewModelsAsync(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer, bool isRtl, bool isSource)
        {
            var result = new TokenDisplayViewModelCollection();
            
            var paddedTokens = GetPaddedTokens(tokens, detokenizer);
            foreach (var paddedToken in paddedTokens)
            {
                result.Add(new TokenDisplayViewModel
                {
                    Token = paddedToken.token,
                    // For right-to-left languages, the padding before and padding after should be swapped.
                    PaddingBefore = !isRtl ? paddedToken.paddingBefore : paddedToken.paddingAfter,
                    PaddingAfter = !isRtl ? paddedToken.paddingAfter : paddedToken.paddingBefore,
                    Translation = GetTranslationForToken(paddedToken.token),
                    Notes = await GetNotesForEntityAsync(paddedToken.token.TokenId),
                    IsSource = isSource
                });
            }
            return result;
        }

        private async Task<IEnumerable<Translation>> GetTranslations(TranslationSet translationSet, IEnumerable<TokenId> tokens)
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

        private async Task<Dictionary<IId, IEnumerable<Note>>?> GetAllNotes()
        {
#if MOCK
            return new Dictionary<IId, IEnumerable<Note>>();
#else
            return await Note.GetAllDomainEntityIdNotes(Mediator);
#endif
        }

        private async Task<LabelCollection> GetLabelSuggestions()
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
                return new LabelCollection(labels.OrderBy(l => l.Text));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
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
                    Translations = await GetTranslations(TranslationSet, SourceTokens.Select(t => t.TokenId));

                    SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokens, SourceDetokenizer, IsSourceRtl, true);
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
                await note.Entity.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    await note.Entity.AssociateDomainEntity(Mediator, entityId);
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

                        await note.Entity.AssociateLabel(Mediator, label);
                    }
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated labels with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
                foreach (var entityId in entityIds)
                {
                    var token = SourceTokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
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
            await UpdateNoteAsync(note.Entity);
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
                await note.Entity.Delete(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Deleted note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
                    var token = SourceTokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
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
                    var newLabel = await note.Entity.CreateAssociateLabel(Mediator, labelText);
                    LabelSuggestions.Add(newLabel);
                    LabelSuggestions = new LabelCollection(LabelSuggestions.OrderBy(l => l.Text));
                }

                // For thread safety, because Note.CreateAssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(CreateAssociateLabel);
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
                    await note.Entity.AssociateLabel(Mediator, label);
                }

                // For thread safety, because Note.AssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(AssociateLabel);
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
                    await note.Entity.DetachLabel(Mediator, label);
                }

                // For thread safety, because Note.CreateAssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(DetachLabel);
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
            EngineStringDetokenizer sourceDetokenizer, 
            bool isRtl)
        {
            SourceTokens = textRow.Tokens;
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isRtl;
            IsTargetRtl = false;

            TranslationSet = null;
            
            AlignmentSet = null;

            AllNotes = await GetAllNotes();
            LabelSuggestions = await GetLabelSuggestions();

            SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokens, sourceDetokenizer, IsSourceRtl, true);
        }

        public async Task ShowTranslationAsync(
            EngineParallelTextRow engineParallelTextRow,
            TranslationSet translationSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl)
        {
            SourceTokens = engineParallelTextRow.SourceTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isSourceRtl;
            IsTargetRtl = false;

            TranslationSet = translationSet;
            Translations = await GetTranslations(TranslationSet, SourceTokens.Select(t => t.TokenId));
            
            AlignmentSet = null;
            
            AllNotes = await GetAllNotes();
            LabelSuggestions = await GetLabelSuggestions();

            SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokens, sourceDetokenizer, IsSourceRtl, true);
        }

        public async Task ShowAlignmentsAsync(
            EngineParallelTextRow engineParallelTextRow,
            AlignmentSet alignmentSet,
            EngineStringDetokenizer sourceDetokenizer,
            bool isSourceRtl,
            EngineStringDetokenizer targetDetokenizer,
            bool isTargetRtl)
        {
            SourceTokens = engineParallelTextRow.SourceTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            SourceDetokenizer = sourceDetokenizer;
            IsSourceRtl = isSourceRtl;

            TargetTokens = engineParallelTextRow.TargetTokens ?? throw new InvalidOperationException("Text row has no source tokens");
            TargetDetokenizer = targetDetokenizer;
            IsTargetRtl = isTargetRtl;

            TranslationSet = null;

            AlignmentSet = alignmentSet;
            Alignments = await alignmentSet.GetAlignments(new List<EngineParallelTextRow>() { engineParallelTextRow });
            
            AllNotes = await GetAllNotes();
            LabelSuggestions = await GetLabelSuggestions();

            SourceTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(SourceTokens, sourceDetokenizer, isSourceRtl, true);
            TargetTokenDisplayViewModels = await BuildTokenDisplayViewModelsAsync(TargetTokens, targetDetokenizer, isTargetRtl, false);
        }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Unless the other constructor is used (via dependency injection), then this view model will not be able to perform database operations. 
        /// </remarks>
        public VerseDisplayViewModel()
        {
            SourceTokens = new ReadOnlyList<Token>(new List<Token>());
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
