using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.Services
{
    public class ExternalNoteManager
    {
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

        public static async Task<IEnumerable<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>> GetNotesForChapterFromExternalAsync(
            IMediator mediator, 
            TokenizedTextCorpusId tokenizedTextCorpusId, 
            int bookNumber, 
            int chapterNumber, 
            EngineStringDetokenizer engineStringDetokenizer,
            ILogger? logger,
            CancellationToken cancellationToken)
        {
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
                    .Cast<TokensTextRow>();

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
                            .Select(r => r)
                            .AddVerseAndTokensContext(tokenTextRows.First(ttr => ((VerseRef)ttr.Ref).VerseNum.ToString() == g.Key), engineStringDetokenizer))
                        .SelectMany(r => r)
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

        private record Chapter(int BookNumber, int BhapterNumber);

        private Dictionary<Chapter, IEnumerable<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>> ChapterExternalNotesMap { get; set; } = new();

        public async Task<IEnumerable<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>> GetExternalNotes(
            IMediator mediator,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            int bookNumber,
            int chapterNumber,
            int verseNumber,
            EngineStringDetokenizer engineStringDetokenizer,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
        {
            var chapter = new Chapter(bookNumber, chapterNumber);
            IEnumerable<(VerseRef verseRef, List<TokenId>? tokenIds, ExternalNote externalNote)>? externalNotesForChapter;
            if (ChapterExternalNotesMap.TryGetValue(chapter, out externalNotesForChapter)) 
            {
            }
            else
            {
                externalNotesForChapter = await GetNotesForChapterFromExternalAsync(mediator, tokenizedTextCorpusId, bookNumber, chapterNumber, engineStringDetokenizer, logger, cancellationToken);
                ChapterExternalNotesMap.Add(chapter, externalNotesForChapter);
            }

            return externalNotesForChapter
                .Where(t => t.verseRef.VerseNum == verseNumber);
        }

        public void InvalidateExternalNotesCache()
        {
            ChapterExternalNotesMap.Clear();
        }
    }
}
