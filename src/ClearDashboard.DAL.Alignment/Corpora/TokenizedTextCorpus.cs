using ClearBible.Engine.Exceptions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Corpora
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

        public static async Task<IEnumerable<CorpusId>> GetAllCorpusIds(IMediator mediator)
        {
            var result = await mediator.Send(new GetAllCorpusIdsQuery());
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<TokenizedCorpusId>> GetAllTokenizedCorpusIds(IMediator mediator, CorpusId corpusId)
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

        public static async Task<CorpusId> CreateCorpus(
            IMediator mediator,
            bool IsRtl,
            string Name,
            string Language,
            string CorpusType)
        {
            var command = new CreateCorpusCommand(IsRtl, Name, Language, CorpusType);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
