using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class Corpus
    {
        public CorpusId CorpusId { get; set; }
        public bool IsRtl { get; set; }
        public string? Name { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public CorpusType CorpusType { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        
        // FIXME:  Should this be a string?  A different (higher level) enum?
        internal Corpus(CorpusId corpusId, IMediator mediator, bool isRtl, string? name, string? language, string paratextGuid, CorpusType corpusType, Dictionary<string, object> metadata)
        {
            CorpusId = corpusId;
            IsRtl = isRtl;
            Name = name;
            Language = language;
            ParatextGuid = paratextGuid;
            CorpusType = corpusType;
            Metadata = metadata;
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
            string CorpusType, 
            CancellationToken token=default)
        {
            var command = new CreateCorpusCommand(IsRtl, Name, Language, CorpusType);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<Corpus>> GetAll(IMediator mediator)
        {
            var command = new GetAllCorporaQuery();

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
