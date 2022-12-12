using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class GetParatextProjectIdForNoteIdQueryHandler : ProjectDbContextQueryHandler<
        GetParatextProjectIdForNoteIdQuery,
        RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>,
        (string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
    {
        private readonly IMediator _mediator;

        public GetParatextProjectIdForNoteIdQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetParatextProjectIdForNoteIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Gets the Paratext project ID for the entities associated with a note.
        /// </summary>
        /// <remarks>
        /// In order for a Paratext project ID to be returned:
        /// 1) All of the associated entities need to be tokens;
        /// 2) All of the tokens must originate from the same Paratext corpus;
        /// 3) All of the tokens must be contiguous in the corpus.
        /// </remarks>
        /// <param name="noteId">The note for which to get the Paratext project ID.</param>
        /// <returns>The Paratext project ID if all three conditions are met; null otherwise.</returns>
        protected override async Task<RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>> GetDataAsync(GetParatextProjectIdForNoteIdQuery request, CancellationToken cancellationToken)
        {
            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            // Get domain entity associations for NoteId:
            var iids = ProjectDbContext.NoteDomainEntityAssociations
                .Where(na => na.NoteId == request.NoteId.Id)
                .Select(nd => nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!))
                .ToList();

            if (!iids.Any())
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' not associated with any domain entity ids"
                );
            }

            // Ensure that all domain entity associations are of type TokenId
            foreach (var iid in iids) 
            {
                var t = iid.GetType().FindEntityIdGenericType();
                if (t is null || t.Name != typeof(TokenId).Name)
                {
                    var domainEntityType = t?.Name ?? "Unknown";
                    return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                    (
                        success: false,
                        message: $"NoteId '{request.NoteId.Id}' associated with at least one non-TokenId domain entity of type '{domainEntityType}'"
                    );
                }
            }

            // Ensure that all associated TokenIds are of the same TokenizedCorpus, Book, Chapter, Verse
            // by doing a GroupBy(VerseRow):
            // (Could they be part of the same Corpus, but different TokenizedCorpora?  Not sure if the 
            // contiguous word check below would be valid in that case)
            var tokenGroups = ProjectDbContext!.Tokens
                .Include(e => e.TokenizedCorpus)
                    .ThenInclude(e => e!.Corpus)
                        .ThenInclude(c => c!.User)
                .Include(e => e.TokenizedCorpus)
                    .ThenInclude(e => e!.User)
                .Include(e => e.VerseRow)
                    .ThenInclude(e => e!.TokenComponents.Where(tc => tc.Deleted == null))
                .Where(e => e.Deleted == null)
                .Where(e => iids.Select(i => i.Id).Contains(e.Id))
                .ToList()
                .GroupBy(e => new { e.VerseRowId })
                .Select(g => g.Select(t => t));

            if (tokenGroups.Count() != 1)
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' associated with Tokens not of a common VerseRow (i.e. common TokenizedCorpus, Book, Chapter and Verse)"
                );
            }

            var associatedTokens = tokenGroups.Single();
            var allTokensInVerse = associatedTokens.First().VerseRow!.TokenComponents.ToList();

            // Ensure that Tokens are contiguous within the VerseRow:
            var verseRowTokenIdToIndex = allTokensInVerse
                .Where(tc => tc.GetType() == typeof(Models.Token))
                .Select(t => (Models.Token)t)
                .OrderBy(t => t.WordNumber)
                .OrderBy(t => t.SubwordNumber)
                .Select((t, index) => new { t, index })
                .ToDictionary(c => c.t.Id, c => c.index);

            var associatedTokenVerseRowIndexes = associatedTokens
                .Select(t => verseRowTokenIdToIndex[t.Id])
                .OrderBy(i => i)
                .ToList();

            var missing = Enumerable.Range(associatedTokenVerseRowIndexes.Min(), associatedTokenVerseRowIndexes.Count)
                .Except(associatedTokenVerseRowIndexes);

            if (missing.Any())
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' associated with non-contiguous VerseRow Tokens"
                );
            }

            // Extract and parse the paratextGuid from the Corpus:
            var paratextId = associatedTokens.First().TokenizedCorpus!.Corpus!.ParatextGuid;
            if (!string.IsNullOrEmpty(paratextId))
            {
                if (paratextId == ManuscriptIds.GreekManuscriptId ||
                    paratextId == ManuscriptIds.HebrewManuscriptId)
                {
                    return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                    (
                        success: false,
                        message: $"NoteId '{request.NoteId.Id}' associated with Tokens of either Macula Hebrew or Greek (Corpus Id: '{tokenGroups.Single().First().TokenizedCorpus!.CorpusId}')"
                    );
                }

                var tokenizedTextCorpusId = ModelHelper.BuildTokenizedTextCorpusId(associatedTokens.First().TokenizedCorpus!);
                var verseTokens = allTokensInVerse.Select(t => ModelHelper.BuildToken(t));

                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>((
                    (paratextId, tokenizedTextCorpusId, verseTokens)
                ));
            }
            else
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' associated with Tokens of Corpus '{tokenGroups.Single().First().TokenizedCorpus!.CorpusId}', which has a null or empty ParatextGuid value"
                );
            }
        }
    }
}
