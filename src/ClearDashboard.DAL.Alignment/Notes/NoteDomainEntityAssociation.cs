using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class NoteDomainEntityAssociation
    {
        private readonly IMediator mediator_;

        public NoteDomainEntityAssociationId? NoteDomainEntityAssociationId { get; private set; }
        public NoteId NoteId { get; private set; }
        // FIXME:  make a real NoteInfo type!
        public string? NoteInfo { get; private set; }
        public IId DomainEntityId { get; private set; }

        public NoteDomainEntityAssociation(IMediator mediator, NoteId noteId, IId domainEntityId)
        {
            this.mediator_ = mediator;

            NoteId = noteId;
            DomainEntityId = domainEntityId;
        }

        internal NoteDomainEntityAssociation(IMediator mediator, NoteDomainEntityAssociationId noteDomainEntityAssociationId, NoteId noteId, string noteInfo, IId domainEntityId)
        {
            this.mediator_ = mediator;

            NoteDomainEntityAssociationId = noteDomainEntityAssociationId;
            NoteId = noteId;
            DomainEntityId = domainEntityId;
        }

        public async Task<NoteDomainEntityAssociation> Create(CancellationToken token = default)
        {      
            if (NoteDomainEntityAssociationId == null)
            {
                var command = new CreateNoteDomainEntityAssociationCommand(NoteId, DomainEntityId);

                var result = await mediator_.Send(command, token);
                if (result.Success)
                {
                    NoteDomainEntityAssociationId = result.Data!;
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            else
            {
                throw new MediatorErrorEngineException($"NoteDomainEntityAssociation already created having id '{NoteDomainEntityAssociationId}'");
            }
            return this;
        }

        public async void Delete(CancellationToken token = default)
        {
            if (NoteDomainEntityAssociationId == null)
            {
                return;
            }

            var command = new DeleteNoteDomainEntityAssociationByNoteDomainEntityAssociationIdCommand(NoteDomainEntityAssociationId);

            var result = await mediator_.Send(command, token);
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<NoteDomainEntityAssociation>> GetAll(
            IMediator mediator)
        {
            var command = new GetAllNoteDomainEntityAssociationsQuery();

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
