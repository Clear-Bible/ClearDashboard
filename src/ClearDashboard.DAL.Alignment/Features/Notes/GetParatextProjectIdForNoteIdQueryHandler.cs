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
        /// 1) All of the associated entities need to be Tokens or CompositeTokens;
        /// 2) All of the associated Tokens must originate from the same Paratext corpus;
        /// 3) All of the associated Tokens must be part of the same book+chapter+verse and
        /// contiguous in the corpus.  
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

            // Ensure that all domain entity associations are of type TokenId or CompositeTokenId:
            foreach (var iid in iids) 
            {
                var t = iid.GetType().FindEntityIdGenericType();
                if (t is null || !t.IsAssignableTo(typeof(TokenId)))
                {
                    var domainEntityType = t?.Name ?? "Unknown";
                    return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                    (
                        success: false,
                        message: $"NoteId '{request.NoteId.Id}' associated with at least one non-TokenId domain entity of type '{domainEntityType}'"
                    );
                }
            }

            // Query for all Tokens/TokenComposites associated with the given Note:
            var associatedTokenComponents = ProjectDbContext!.TokenComponents
                .Include(e => e.TokenizedCorpus)
                    .ThenInclude(e => e!.Corpus)
                        .ThenInclude(c => c!.User)
                .Include(e => e.TokenizedCorpus)
                    .ThenInclude(e => e!.User)
                .Include(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.Deleted == null)
                .Where(e => iids.Select(i => i.Id).Contains(e.Id))
                .ToList();

            // Extract associated Tokens
            var associatedTokens = associatedTokenComponents
                .Where(e => e.GetType() == typeof(Models.Token))
                .Cast<Models.Token>()
                .ToList();

            // Extract associated TokenComposites
            var associatedTokenComposites = associatedTokenComponents
                .Where(e => e.GetType() == typeof(Models.TokenComposite))
                .Cast<Models.TokenComposite>()
                .ToList();

            if (!associatedTokens.Any() && !associatedTokenComposites.Any())
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' associated with at least one TokenId, but no matching TokenComponents found"
                );
            }

            // Decide on VerseRowId - either from first Token,
            // or from first Token of first TokenComposite:
            var verseRowId = associatedTokens.Any()
                ? (Guid)associatedTokens.First().VerseRowId!
                : (Guid)associatedTokenComposites.First().Tokens.First().VerseRowId!;

            // Return failure if any associated Token is not part of VerseRow:
            if (associatedTokens.Where(e => e.VerseRowId != verseRowId).Any())
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' is associated with Tokens not of a common VerseRow (i.e. common TokenizedCorpus, Book, Chapter and Verse)"
                );
            }

            // Return failure if any associated CompositeToken has no Token 
            // children part of the verseRow:
            if (associatedTokenComposites.Where(e => e.Tokens.All(e => e.VerseRowId != verseRowId)).Any())
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' is associated with CompositeTokens not of a common VerseRow (i.e. common TokenizedCorpus, Book, Chapter and Verse)"
                );
            }

            // When checking that Note Tokens/TokenComposites are contiguous,
            // use any Token children from the TokenComposite(s) that are in 
            // the VerseRow:
            var tokensToCheckForContiguousness = associatedTokenComposites
                .SelectMany(e => e.Tokens.Where(e => e.VerseRowId == verseRowId))
                .ToList();

            // As well as all Note-associated Tokens:
            tokensToCheckForContiguousness.AddRange(associatedTokens);
            tokensToCheckForContiguousness = tokensToCheckForContiguousness.DistinctBy(e => e.Id).ToList();

            var allVerseRowTokens = ProjectDbContext.Tokens
                .Include(e => e.TokenComposites)
                    .ThenInclude(e => e.Tokens)
                .Where(e => e.VerseRowId == verseRowId)
                .ToList();

            if (tokensToCheckForContiguousness.Count > 1)
            {
                // Ensure that Tokens are contiguous within the VerseRow.  Order the
                // set of 'all VerseRow Tokens' by word, subword and put Id-Index
                // into a dictionary:
                var verseRowTokenIdToIndex = allVerseRowTokens
                    .OrderBy(t => t.WordNumber)
                    .OrderBy(t => t.SubwordNumber)
                    .ToList()
                    .Select((t, index) => new { t, index })
                    .ToDictionary(c => c.t.Id, c => c.index);

                // Order 'Tokens to check' per dictionary index:
                var associatedTokenVerseRowIndexes = tokensToCheckForContiguousness
                    .Select(t => verseRowTokenIdToIndex[t.Id])
                    .OrderBy(i => i)
                    .ToList();

                // Look for missing indexes:
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
                        message: $"NoteId '{request.NoteId.Id}' associated with Tokens of either Macula Hebrew or Greek (Corpus Id: '{associatedTokenComponents.First().TokenizedCorpus!.CorpusId}')"
                    );
                }

                // Build CompositeTokens part of 'verseTokens':
                var tokenComposites = allVerseRowTokens
                    .SelectMany(e => e.TokenComposites)
                    .DistinctBy(e => e.Id)
                    .Select(e => ModelHelper.BuildCompositeToken(
                        e,
                        e.Tokens.Where(t => t.VerseRowId == verseRowId),
                        e.Tokens.Where(t => t.VerseRowId != verseRowId)
                     ))
                    .ToList();

                // Determine which individual Token Ids are in Composite
                // children, to use for filtering those out of Tokens
                // part of 'verseTokens':
                var tokenIdsInComposites = tokenComposites
                    .SelectMany(e => e.Tokens)
                    .Select(e => e.TokenId.Id)
                    .Distinct()
                    .ToList();

                // Build Tokens part of 'verseTokens':
                var verseTokens = allVerseRowTokens
                    .Where(e => !tokenIdsInComposites.Contains(e.Id))
                    .Select(e => ModelHelper.BuildToken(e))
                    .ToList();

                // and add CompositeTokens:
                verseTokens.AddRange(tokenComposites);

                var tokenizedTextCorpusId = ModelHelper.BuildTokenizedTextCorpusId(associatedTokens.First().TokenizedCorpus!);

                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>((
                    (paratextId, tokenizedTextCorpusId, verseTokens)
                ));
            }
            else
            {
                return new RequestResult<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)>
                (
                    success: false,
                    message: $"NoteId '{request.NoteId.Id}' associated with Tokens of Corpus '{associatedTokenComponents.First().TokenizedCorpus!.CorpusId}', which has a null or empty ParatextGuid value"
                );
            }
        }
    }
}
