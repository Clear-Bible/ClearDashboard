using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class SemanticDomain
    {
        public SemanticDomainId? SemanticDomainId { get; private set; }
        public string? Text { get; set; }

        public SemanticDomain()
        {
        }

        internal SemanticDomain(SemanticDomainId semanticDomainId, string text)
        {
            SemanticDomainId = semanticDomainId;
            Text = text;
        }

        public async Task<SemanticDomain> Create(IMediator mediator, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(Text))
            {
                throw new MediatorErrorEngineException($"Unable to create SemanticDomain - Text property has not been set");
            }

            var command = new CreateOrUpdateSemanticDomainCommand(null, Text);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            SemanticDomainId = result.Data!;
            return this;
        }

        public async void Delete(IMediator mediator, CancellationToken token = default)
        {
            if (SemanticDomainId == null)
            {
                return;
            }

            var command = new DeleteSemanticDomainAndAssociationsCommand(SemanticDomainId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<IEnumerable<SemanticDomain>> GetAll(
            IMediator mediator)
        {
            var command = new GetAllSemanticDomainsQuery();

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task<IEnumerable<SemanticDomain>> Get(
            IMediator mediator,
            string partialText)
        {
            var command = new GetSemanticDomainsByPartialTextQuery(partialText);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
