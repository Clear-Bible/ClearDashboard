using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class LabelNoteAssociation
    {
        private readonly IMediator mediator_;

        public LabelNoteAssociationId? LabelNoteAssociationId { get; private set; }
        public LabelId LabelId { get; set; }
        public NoteId NoteId { get; set; }

        internal LabelNoteAssociation(IMediator mediator, LabelNoteAssociationId labelNoteAssociationId, LabelId labelId, NoteId noteId)
        {
            this.mediator_ = mediator;

            LabelNoteAssociationId = labelNoteAssociationId;
            LabelId = labelId;
            NoteId = noteId;
        }

        public async void Create(CancellationToken token = default)
        {      
            if (LabelNoteAssociationId == null)
            {
                var command = new CreateLabelNoteAssociationCommand(LabelId, NoteId);

                var result = await mediator_.Send(command, token);
                if (result.Success)
                {
                    LabelNoteAssociationId = result.Data!;
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            else
            {
                throw new MediatorErrorEngineException($"LabelNoteAssociation already created having id '{LabelNoteAssociationId}'");
            }
        }

        public async void Delete(CancellationToken token = default)
        {
            if (LabelNoteAssociationId == null)
            {
                return;
            }

            var command = new DeleteLabelNoteAssociationByLabelNoteAssociationIdCommand(LabelNoteAssociationId);

            var result = await mediator_.Send(command, token);
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<LabelNoteAssociation>> GetAll(
            IMediator mediator,
            LabelId labelId)
        {
            var command = new GetLabelNoteAssociationsByLabelIdOrNoteIdQuery(labelId, null);

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

        public static async Task<IEnumerable<LabelNoteAssociation>> GetAll(
            IMediator mediator,
            NoteId noteId)
        {
            var command = new GetLabelNoteAssociationsByLabelIdOrNoteIdQuery(null, noteId);

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
