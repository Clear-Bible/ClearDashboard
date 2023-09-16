using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Persistence;
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
using SIL.Scripture;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.Services
{
    public class ParatextNoteManager
    {
        /// <summary>
        /// Gets relevant Paratext project information for the entities associated with a note.
        /// </summary>
        /// <remarks>
        /// In order for Paratext information to be returned:
        /// 
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same Paratext corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// </remarks>
        /// <param name="mediator">The <see cref="Mediator"/> to use to populate the project information.</param>
        /// <param name="noteId">The note for which to get the Paratext information.</param>
        /// <param name="logger">A <see cref="ILogger"/> for logging the operation.</param>
        /// <returns>
        /// If all three conditions are met, a tuple containing:
        /// * The Paratext project ID;
        /// * The <see cref="TokenizedTextCorpusId"/> for the related Paratext corpus;
        /// * An <see cref="IEnumerable{Token}"/> of the tokens making up the surrounding verse.
        /// Otherwise, null.
        /// </returns>
        private static async Task<ParatextSendNoteInformation?> GetParatextSendNoteInformationAsync(IMediator mediator, NoteId noteId, IUserProvider userProvider, ILogger? logger = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await Note.GetParatextIdIfAssociatedContiguousTokensOnly(mediator, noteId);

                stopwatch.Stop();
                logger?.LogInformation($"Retrieved Paratext information for note {noteId.Id} in {stopwatch.ElapsedMilliseconds} ms");

                var paratextUserName = userProvider.CurrentUser?.ParatextUserName;
                var canAddNoteForProjectAndUerQuery = false;
                if (result != null && !string.IsNullOrWhiteSpace(paratextUserName))
                {
                    stopwatch.Restart();
                    var r = await mediator.Send(new CanAddNoteForProjectAndUserQuery(paratextUserName, result.Value.paratextId));
                    stopwatch.Stop();

                    if (r.Success)
                    {
                        canAddNoteForProjectAndUerQuery = r.Data;
                        logger?.LogInformation($"Checked can add note for project {result.Value.paratextId} and user {paratextUserName}. Result is {canAddNoteForProjectAndUerQuery}");
                    }
                    else
                    {
                        logger?.LogCritical($"Error checking can add note for project {result.Value.paratextId} and user {paratextUserName}: {r.Message}");
                        //throw new MediatorErrorEngineException(r.Message);
                    }
                }

                return (result != null && canAddNoteForProjectAndUerQuery) ? new ParatextSendNoteInformation(ParatextId: result.Value.paratextId, 
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
        /// Populates a <see cref="NoteViewModel"/> with information needed to send the note to Paratext.
        /// </summary>
        /// <remarks>
        /// In order for a note to be sent to Paratext:
        /// 
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same Paratext corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// 
        /// If any of these three conditions are false, then <see cref="NoteViewModel.ParatextSendNoteInformation"/> will be null.
        /// </remarks>
        /// <param name="mediator">The <see cref="Mediator"/> to use to populate the project information.</param>
        /// <param name="note">The note for which to populate the Paratext project ID.</param>
        /// <param name="userProvider">User provider for obtaining user details.</param>
        /// <param name="logger">A <see cref="ILogger"/> for logging the operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public static async Task PopulateParatextDetailsAsync(IMediator mediator, NoteViewModel note, IUserProvider userProvider, ILogger? logger = null)
        {
            note.ParatextSendNoteInformation = await GetParatextSendNoteInformationAsync(mediator, note.NoteId!, userProvider, logger);
        }

        /// <summary>
        /// Sends a note to Paratext.
        /// </summary>
        /// <param name="mediator">The <see cref="Mediator"/> to use to send the note to Paratext.</param>
        /// <param name="note">The note to send to Paratext.</param>
        /// <param name="logger">A <see cref="ILogger"/> for logging the operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public static async Task SendToParatextAsync(IMediator mediator, NoteViewModel note, ILogger? logger = null, CancellationToken cancellationToken = default)
        {
            if (note.ParatextSendNoteInformation == null) throw new ArgumentException($"Cannot send note {note.NoteId?.Id} to Paratext.");

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var sortedVerseTokens = note.ParatextSendNoteInformation.VerseTokens.GetPositionalSortedBaseTokens().ToList();
                var verseFirstTokenId = sortedVerseTokens.First().TokenId;
                var associatedTokens = sortedVerseTokens.Where(t => note.Entity.DomainEntityIds.Contains(t.TokenId));

                var addNoteCommandParam = new AddNoteCommandParam();
                addNoteCommandParam.SetProperties(
                    note.ParatextSendNoteInformation.ParatextId.ToString(),
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
                    logger?.LogInformation($"Sent note {note.NoteId?.Id} to Paratext in {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    logger?.LogCritical($"Error sending {note.NoteId?.Id} to Paratext: {result.Message}");
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            catch (Exception e)
            {
                logger?.LogCritical(e.ToString());
                throw;
            }
        }


        public static async Task<IEnumerable<(VerseRef verseRef, List<TokenId> tokenIds, ExternalNote externalNote)>> GetNotesForChapterFromParatextAsync(
            IMediator mediator, 
            TokenizedTextCorpusId tokenizedTextCorpusId, 
            int bookNumber, 
            int chapterNumber, 
            EngineStringDetokenizer engineStringDetokenizer,
            ILogger? logger = null,
            CancellationToken cancellationToken = default)
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
    }
}
