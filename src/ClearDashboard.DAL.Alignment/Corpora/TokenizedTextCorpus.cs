using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class TokenizedTextCorpus : ScriptureTextCorpus
    {
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; set; }
        public override ScrVers Versification { get; }

        internal TokenizedTextCorpus(TokenizedTextCorpusId tokenizedCorpusId, IMediator mediator, IEnumerable<string> bookAbbreviations, ScrVers versification)
        {
            TokenizedTextCorpusId = tokenizedCorpusId;
            Versification = versification;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedTextCorpusId, mediator, Versification, bookAbbreviation));
            }

        }

        public async void Update()
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
        }

        public static async Task<IEnumerable<TokenizedTextCorpusId>> GetAllTokenizedCorpusIds(IMediator mediator, CorpusId? corpusId)
        {
            var result = await mediator.Send(new GetAllTokenizedCorpusIdsByCorpusIdQuery(corpusId));
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
        public static async Task<TokenizedTextCorpus> Get(
            IMediator mediator,
            TokenizedTextCorpusId tokenizedTextCorpusId)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpusId);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return new TokenizedTextCorpus(result.Data.tokenizedTextCorpusId, mediator, result.Data.bookIds, result.Data.versification);
        }
    }
}
