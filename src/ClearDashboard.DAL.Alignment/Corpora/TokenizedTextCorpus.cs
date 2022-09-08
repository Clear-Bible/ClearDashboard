using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class TokenizedTextCorpus : ScriptureTextCorpus
    {
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; set; }
        public CorpusId CorpusId { get; set; }
        internal TokenizedTextCorpus(TokenizedTextCorpusId tokenizedCorpusId, CorpusId corpusId, IMediator mediator, IEnumerable<string> bookAbbreviations)
        {
            TokenizedTextCorpusId = tokenizedCorpusId;
            CorpusId = corpusId;

            Versification = ScrVers.Original;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedTextCorpusId, mediator, Versification, bookAbbreviation));
            }

        }
        public override ScrVers Versification { get; }

        public async void Update()
        {
            // call the update handler to update the r/w metadata on the TokenizedTextCorpusId
        }

        public static async Task<IEnumerable<TokenizedTextCorpusId>> GetAllTokenizedCorpusIds(IMediator mediator, CorpusId corpusId)
        {
            var result = await mediator.Send(new GetAllTokenizedCorpusIdsByCorpusIdQuery(corpusId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
        public static async Task<TokenizedTextCorpus> Get(
            IMediator mediator,
            TokenizedTextCorpusId tokenizedTextCorpusId)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return new TokenizedTextCorpus(result.Data.tokenizedTextCorpusId, result.Data.corpusId, mediator, result.Data.bookIds);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
