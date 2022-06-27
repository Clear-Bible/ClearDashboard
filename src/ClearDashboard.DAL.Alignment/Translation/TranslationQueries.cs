using MediatR;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Alignment.DataServices.Features.Translation;

namespace ClearBible.Alignment.DataServices.Translation
{
    public class TranslationQueries : IITranslationQueriable
    {
        private readonly IMediator mediator_;

        public TranslationQueries(IMediator mediator)
        {
            mediator_ = mediator;
        }

        public async Task<IEnumerable<(Token, Token, double)>?> GetAlignemnts(ParallelCorpusId parallelCorpusId)
        {
            var result = await mediator_.Send(new GetAlignmentsByParallelCorpusIdQuery(parallelCorpusId));
            if (result.Success && result.Data != null)
            {
                return result.Data;
            }
            else if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
            else
            {
                return null;
            }
        }
    }
}
