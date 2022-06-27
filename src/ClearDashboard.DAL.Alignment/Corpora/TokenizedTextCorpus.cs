using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearBible.Engine.Exceptions;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearBible.Alignment.DataServices.Corpora
{
    public class TokenizedTextCorpus : ScriptureTextCorpus
    {
        public TokenizedCorpusId TokenizedCorpusId { get; set; }
        public CorpusId CorpusId { get; set; }
        internal TokenizedTextCorpus(TokenizedCorpusId tokenizedCorpusId, CorpusId corpusId, IMediator mediator, IEnumerable<string> bookAbbreviations)
        {
            TokenizedCorpusId = tokenizedCorpusId;
            CorpusId = corpusId;

            Versification = ScrVers.Original;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedCorpusId, mediator, Versification, bookAbbreviation));
            }
        }
        public override ScrVers Versification { get; }

        public static async Task<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>> GetAllCorpusVersionIds(IMediator mediator)
        {
            var result = await mediator.Send(new GetAllCorpusVersionIdsQuery());
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<TokenizedCorpusId>> GetAllTokenizedCorpusIds(IMediator mediator, CorpusVersionId corpusVersionId)
        {
            var result = await mediator.Send(new GetAllTokenizedCorpusIdsByCorpusVersionIdQuery(corpusVersionId));
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
            TokenizedCorpusId tokenizedCorpusId)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {                                                      
                return new TokenizedTextCorpus(command.TokenizedCorpusId, result.Data.corpusId, mediator, result.Data.bookIds);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
