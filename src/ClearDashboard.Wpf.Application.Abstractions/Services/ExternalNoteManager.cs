using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.Services
{
    public class ExternalNoteManager
    {
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// Gets relevant External drafting tool project information for the entities associated with a note.
        /// </summary>
        /// <remarks>
        /// In order for external information to be returned:
        /// 
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same external corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// </remarks>
        /// <param name="mediator">The <see cref="Mediator"/> to use to populate the project information.</param>
        /// <param name="noteId">The note for which to get the external information.</param>
        /// <param name="logger">A <see cref="ILogger"/> for logging the operation.</param>
        /// <returns>
        /// If all three conditions are met, a tuple containing:
        /// * The external project ID;
        /// * The <see cref="TokenizedTextCorpusId"/> for the related external corpus;
        /// * An <see cref="IEnumerable{Token}"/> of the tokens making up the surrounding verse.
        /// Otherwise, null.
        /// </returns>
        public static async Task<ExternalSendNoteInformation?> GetExternalSendNoteInformationAsync(IMediator mediator, NoteId noteId, IUserProvider userProvider, ILogger? logger = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(mediator, noteId);

                stopwatch.Stop();
                logger?.LogInformation($"Retrieved External information for note {noteId.Id} in {stopwatch.ElapsedMilliseconds} ms");

                var externalUserName = userProvider.CurrentUser?.ParatextUserName;
                var canAddNoteForProjectAndUerQuery = false;
                if (result != null && !string.IsNullOrWhiteSpace(externalUserName))
                {
                    stopwatch.Restart();
                    var r = await mediator.Send(new CanAddNoteForProjectAndUserQuery(externalUserName, result.Value.paratextId));
                    stopwatch.Stop();

                    if (r.Success)
                    {
                        canAddNoteForProjectAndUerQuery = r.Data;
                        logger?.LogInformation($"Checked can add note for project {result.Value.paratextId} and user {externalUserName}. Result is {canAddNoteForProjectAndUerQuery}");
                    }
                    else
                    {
                        logger?.LogCritical($"Error checking can add note for project {result.Value.paratextId} and user {externalUserName}: {r.Message}");
                        //throw new MediatorErrorEngineException(r.Message);
                    }
                }

                return (result != null && canAddNoteForProjectAndUerQuery) ? new ExternalSendNoteInformation(ExternalProjectId: result.Value.paratextId, 
                                                                        TokenizedTextCorpusId: result.Value.tokenizedTextCorpusId,
                                                                        VerseTokens: result.Value.verseTokens)
                                      : null;
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Sends a note to the external drafting tool.
        /// </summary>
        /// <param name="mediator">The <see cref="Mediator"/> to use to send the note to external.</param>
        /// <param name="note">The note to send to the external drafting tool.</param>
        /// <param name="logger">A <see cref="ILogger"/> for logging the operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public static async Task SendToExternalAsync(IMediator mediator, NoteViewModel note, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            if (note.ParatextSendNoteInformation == null) throw new ArgumentException($"Cannot send note {note.NoteId?.Id} to external.");

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var sortedVerseTokens = note.ParatextSendNoteInformation.VerseTokens.GetPositionalSortedBaseTokens().ToList();
                var verseFirstTokenId = sortedVerseTokens.First().TokenId;
                var associatedTokens = sortedVerseTokens.Where(t => note.Entity.DomainEntityIds.Contains(t.TokenId));

                var addNoteCommandParam = new AddNoteCommandParam();
                addNoteCommandParam.SetProperties(
                    note.ParatextSendNoteInformation.ExternalProjectId.ToString(),
                    sortedVerseTokens,
                    associatedTokens,
                    note.ParatextSendNoteInformation.TokenizedTextCorpusId.Detokenizer,
                    note.Text,
                    verseFirstTokenId.BookNumber,
                    verseFirstTokenId.ChapterNumber,
                    verseFirstTokenId.VerseNumber
                );

                var result = await mediator.Send(new AddNoteCommand(addNoteCommandParam), cancellationToken);
                stopwatch.Stop();
                if (result.Success)
                {
                    logger?.LogInformation($"Sent note {note.NoteId?.Id} to external drafting tool in {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    logger?.LogCritical($"Error sending {note.NoteId?.Id} to external drafting tool: {result.Message}");
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public static async Task<List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>> GetNotesForChapterFromExternalAsync(
            IMediator mediator, 
            TokenizedTextCorpusId tokenizedTextCorpusId, 
            int bookNumber, 
            int chapterNumber, 
            ILogger? logger,
            CancellationToken cancellationToken)
        {
            var engineStringDetokenizer = tokenizedTextCorpusId.Detokenizer;
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var bookAbbreviation = BookIds
                  .Where(b => b.silCannonBookNum.Equals(bookNumber.ToString()))
                  .Select(b => b.silCannonBookAbbrev)
                  .FirstOrDefault() 
                    ?? throw new InvalidBookMappingEngineException(message: "Doesn't exist", name: "silCannonBookNum", value: bookNumber.ToString());

                var tokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator, tokenizedTextCorpusId, false, cancellationToken);
                var tokenTextRows = tokenizedTextCorpus.GetRows(new List<string>() { bookAbbreviation })
                    .Where(r => ((VerseRef)r.Ref).ChapterNum == chapterNumber)
                    .Cast<TokensTextRow>()
                    .ToList();

                var getNotesCommandParam = new GetNotesQueryParam()
                {
                    ExternalProjectId = tokenizedTextCorpusId?.CorpusId?.ParatextGuid 
                        ?? throw new InvalidDataEngineException(name: "corpusId.ParatextGuid", value: "null"),
                    BookNumber = bookNumber,
                    ChapterNumber = chapterNumber,
                    IncludeResolved = true
                };

                var result = await mediator.Send(new GetNotesQuery(getNotesCommandParam), cancellationToken);
                stopwatch.Stop();
                if (result.Success)
                {
                    var externalNotesGroupedByVerse = result?.Data
                        ?.GroupBy(e => e.VerseRefString)
                            ?? throw new InvalidDataEngineException(name: "result.Data", value: "null", message: "result of GetNotesQuery is successful yet result.Data is null");

                    logger?.LogInformation($"Got notes from external authoring tool for book {bookNumber} chapter {chapterNumber} in {stopwatch.ElapsedMilliseconds} ms");

                    return externalNotesGroupedByVerse
                        ?.Select(g => g
                            .Select(en => en)
                            .Where(en => tokenTextRows
                                .Select(ttr => (VerseRef) ttr.Ref)
                                .Contains(new VerseRef(en.VerseRefString)))
                            .AddVerseAndTokensContext(tokenTextRows.First(ttr => ((VerseRef)ttr.Ref).ToString() == g.Key), engineStringDetokenizer))
                        .SelectMany(r => r)
                        .ToList()
                            ?? throw new InvalidDataEngineException(name: "result.Data", value: "null", message: "result of GetNotesQuery is successful yet result.Data is null");
                }
                else
                {
                    logger?.LogCritical($"Error getting notes from external authoring tool for book {bookNumber} chapter {chapterNumber}: {result.Message}");
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private record Chapter(int BookNumber, int ChapterNumber);

        private Dictionary<string, Dictionary<Chapter, List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>>>  ExternalProjectIdToChapterToExternalNotesMap { get; set; } = new();

        public ExternalNoteManager(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="tokenizedTextCorpusIds"></param>
        /// <param name="verseRefs"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of list of external notes, where the first list groups lists of external notes by the tokenizedTextCorpusIds parameter, in order.</returns>
        /// <exception cref="EngineException"></exception>
        /// <exception cref="InvalidStateEngineException"></exception>
        public List<List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>>
            GetExternalNotes(
                IMediator mediator,
                IEnumerable<TokenizedTextCorpusId> tokenizedTextCorpusIds,
                IEnumerable<VerseRef> verseRefs,
                ILogger? logger = null,
                CancellationToken cancellationToken = default)
        {
            bool obtainedLock = false;
            try
            {
                Monitor.TryEnter(ExternalProjectIdToChapterToExternalNotesMap, TimeSpan.FromSeconds(60), ref obtainedLock);
                if (!obtainedLock)
                {
                    logger?.LogWarning($"couldn't obtain lock on ExternalProjectIdToChapterToExternalNotesMap for verseref {verseRefs.First()}");
                    throw new EngineException("Couldn't obtain lock on cache within 60 seconds");
                }
                                
                return tokenizedTextCorpusIds
                    .Select(ttcid =>
                    {
                        Dictionary<Chapter, List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>>? chapterToExternalNotesMap = null;
                        if (ExternalProjectIdToChapterToExternalNotesMap.TryGetValue(ttcid?.CorpusId?.ParatextGuid
                            ?? throw new InvalidStateEngineException(name: "tokenizedTextCorpus.CorpusId or ParatextGuid", value: "null")
                            , out chapterToExternalNotesMap))
                        {
                        }
                        else
                        {
                            chapterToExternalNotesMap = new Dictionary<Chapter, List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>>();
                            ExternalProjectIdToChapterToExternalNotesMap.Add(ttcid!.CorpusId!.ParatextGuid, chapterToExternalNotesMap);
                        }

                        var externalNotes = verseRefs
                            .SelectMany(vr =>
                            {
                                var chapter = new Chapter(vr.BookNum, vr.ChapterNum);
                                List<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>? externalNotes = null;
                                if (chapterToExternalNotesMap.TryGetValue(chapter, out externalNotes))
                                {
                                    return externalNotes
                                        .Where(en => en.verseRef.BookNum == vr.BookNum &&
                                            en.verseRef.ChapterNum == vr.ChapterNum &&
                                            en.verseRef.VerseNum == vr.VerseNum);
                                }
                                else
                                {
                                    var task = GetNotesForChapterFromExternalAsync(mediator, ttcid, vr.BookNum, vr.ChapterNum, logger, cancellationToken);
                                    task.Wait(cancellationToken);
                                    externalNotes = task.Result;
                                    chapterToExternalNotesMap.Add(chapter, externalNotes);
                                    return externalNotes
                                        .Where(en => en.verseRef.BookNum == vr.BookNum &&
                                            en.verseRef.ChapterNum == vr.ChapterNum &&
                                            en.verseRef.VerseNum == vr.VerseNum);

                                }
                            })
                            .ToList();
                        return externalNotes;
                    })
                    .ToList();
            }
            finally
            {
                if (obtainedLock)
                {
                    Monitor.Exit(ExternalProjectIdToChapterToExternalNotesMap);
                }
            }
        }

        public async Task<bool> InvalidateExternalNotesCache(CorpusId? corpusId, CancellationToken cancellationToken = default)
        {
            bool obtainedLock = false;
            try
            {
                Monitor.TryEnter(ExternalProjectIdToChapterToExternalNotesMap, TimeSpan.FromSeconds(60), ref obtainedLock);
                if (!obtainedLock)
                {
                    throw new EngineException("Couldn't obtain lock on cache within 60 seconds");
                }

                if (corpusId == null)
                {
                    ExternalProjectIdToChapterToExternalNotesMap.Clear();
                    return true;
                }
                else
                {
                    return ExternalProjectIdToChapterToExternalNotesMap.Remove(corpusId?.ParatextGuid
                        ?? throw new InvalidStateEngineException(name: "tokenizedTextCorpus.CorpusId or ParatextGuid", value: "null"));
                }
            }
            finally
            {
                if (obtainedLock)
                {
                    Monitor.Exit(ExternalProjectIdToChapterToExternalNotesMap);
                }
                await _eventAggregator.PublishOnUIThreadAsync(new ExternalNotesUpdatedMessage(corpusId), cancellationToken);
            }
        }
    }
}
