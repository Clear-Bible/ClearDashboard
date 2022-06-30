using ClearBible.Engine.Exceptions;
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
        private string projectName_;
        internal TokenizedTextCorpus(TokenizedCorpusId tokenizedCorpusId, CorpusId corpusId, IMediator mediator, IEnumerable<string> bookAbbreviations, string projectName)
        {
            TokenizedCorpusId = tokenizedCorpusId;
            CorpusId = corpusId;

            Versification = ScrVers.Original;

            foreach (var bookAbbreviation in bookAbbreviations)
            {
                AddText(new TokenizedText(TokenizedCorpusId, mediator, Versification, bookAbbreviation, projectName));
            }

            projectName_ = projectName;
        }
        public override ScrVers Versification { get; }

        public static async Task<IEnumerable<(CorpusVersionId corpusVersionId, CorpusId corpusId)>> GetAllCorpusVersionIds(string projectName, IMediator mediator)
        {
            var result = await mediator.Send(new GetAllCorpusVersionIdsQuery(projectName));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<TokenizedCorpusId>> GetAllTokenizedCorpusIds(string projectName, IMediator mediator, CorpusVersionId corpusVersionId)
        {
            var result = await mediator.Send(new GetAllTokenizedCorpusIdsByCorpusVersionIdQuery(projectName, corpusVersionId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
        public static async Task<TokenizedTextCorpus> Get(string projectName,
            IMediator mediator,
            TokenizedCorpusId tokenizedCorpusId)
        {
            var command = new GetBookIdsByTokenizedCorpusIdQuery(projectName, tokenizedCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {                                                      
                return new TokenizedTextCorpus(command.TokenizedCorpusId, result.Data.corpusId, mediator, result.Data.bookIds, command.ProjectName);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
