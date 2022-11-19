using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class Corpus
    {
        public const string DefaultFontFamily = "Segoe UI";

        public CorpusId CorpusId { get; set; }

        public Corpus(CorpusId corpusId)
        {
            CorpusId = corpusId;
        }

        public async void Update()
        {
            // call the update handler
            // update 'this' instance with the metadata from the handler (the ones with setters only)
        }

        public static async Task<IEnumerable<CorpusId>> GetAllCorpusIds(IMediator mediator)
        {
            var result = await mediator.Send(new GetAllCorpusIdsQuery());
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public static async Task<Corpus> Create(
            IMediator mediator,
            bool IsRtl,
            string Name,
            string Language,
            string CorpusType,
            string ParatextId,
            string FontFamily = DefaultFontFamily,
            CancellationToken token = default)
        {
            var command = new CreateCorpusCommand(IsRtl, FontFamily, Name, Language, CorpusType, ParatextId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return await Corpus.Get(mediator, result.Data!);
        }

        public static async Task<IEnumerable<Corpus>> GetAll(IMediator mediator)
        {
            var command = new GetAllCorporaQuery();

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
        public static async Task<Corpus> Get(
            IMediator mediator,
            CorpusId corpusId)
        {
            var command = new GetCorpusByCorpusIdQuery(corpusId);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
