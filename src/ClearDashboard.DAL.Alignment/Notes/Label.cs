using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class Label
    {
        private readonly IMediator mediator_;

        public LabelId? LabelId { get; private set; }
        public string Text { get; set; }

        public Label(IMediator mediator, string text)
        {
            mediator_ = mediator;

            Text = text;
        }

        internal Label(IMediator mediator, LabelId labelId, string text)
        {
            this.mediator_ = mediator;

            LabelId = labelId;
            Text = text;
        }

        public async void CreateOrUpdate(CancellationToken token = default)
        {            
            var command = new CreateOrUpdateLabelCommand(LabelId, Text);

            var result = await mediator_.Send(command, token);
            if (result.Success)
            {
                LabelId = result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async void Delete(CancellationToken token = default)
        {
            if (LabelId == null)
            {
                return;
            }

            var command = new DeleteLabelByLabelIdCommand(LabelId);

            var result = await mediator_.Send(command, token);
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<Label>> Get(
            IMediator mediator,
            string partialText)
        {
            var command = new GetLabelsByPartialTextQuery(partialText);

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
