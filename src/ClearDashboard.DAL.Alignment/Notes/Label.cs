using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class Label
    {
        public LabelId? LabelId { get; private set; }
        public string? Text { get; set; }

        public Label()
        {
        }

        internal Label(LabelId labelId, string text)
        {
            LabelId = labelId;
            Text = text;
        }

        public async Task<Label> CreateOrUpdate(IMediator mediator, CancellationToken token = default)
        {     
            if (string.IsNullOrEmpty(Text))
            {
                throw new MediatorErrorEngineException($"Unable to create Label - Text property has not been set");
            }

            var command = new CreateOrUpdateLabelCommand(LabelId, Text);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            LabelId = result.Data!;
            return this;
        }

        public async void Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LabelId == null)
            {
                return;
            }

            var command = new DeleteLabelAndAssociationsByLabelIdCommand(LabelId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<IEnumerable<Label>> GetAll(
            IMediator mediator)
        {
            var command = new GetAllLabelsQuery();

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task<IEnumerable<Label>> Get(
            IMediator mediator,
            string partialText)
        {
            var command = new GetLabelsByPartialTextQuery(partialText);

            var result = await mediator.Send(command);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
