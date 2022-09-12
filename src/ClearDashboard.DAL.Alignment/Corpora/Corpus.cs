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
        public string? DisplayName { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public string CorpusType { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; set; }

        // FIXME:  Should this be a string?  A different (higher level) enum?
        public Corpus(CorpusId corpusId, IMediator mediator, bool isRtl, string? name, string? displayName,
            string? language, string? paratextGuid, string corpusType, Dictionary<string, object> metadata,
            DateTimeOffset? created, UserId userId)
        {
            CorpusId = corpusId;
            IsRtl = isRtl;
            Name = name;
            DisplayName = displayName;
            Language = language;
            ParatextGuid = paratextGuid;
            CorpusType = corpusType;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }

        public async void Update()
        {
            // call the update handler
            // update 'this' instance with the metadata from the handler (the ones with setters only)
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
            string ParatextId,
            CancellationToken token = default)
        {
            var command = new CreateCorpusCommand(IsRtl, Name, Language, CorpusType, ParatextId);

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
