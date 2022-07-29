using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class Corpus
    {
        public CorpusId CorpusId { get; set; }
        public bool IsRtl { get; set; }
        public string? Name { get; set; }
        public string? Language { get; set; }
        
        // FIXME:  Should this be a string?  A different (higher level) enum?
        public int CorpusType { get; set; }
        internal Corpus(CorpusId corpusId, IMediator mediator, bool isRtl, string? name, string? language, int corpusType)
        {
            CorpusId = corpusId;
            IsRtl = isRtl;
            Name = name;
            Language = language;
            CorpusType = corpusType;
        }

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

        public static async Task<Corpus> Create(
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

        public static async Task<Corpus> Get(
            IMediator mediator,
            CorpusId corpusId)
        {
            var command = new GetCorpusByCorpusIdQuery(corpusId);

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
