using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class LabelGroup
    {
        public LabelGroupId? LabelGroupId { get; private set; }
        public string? Name { get; set; }
        public LabelGroup()
        {
        }

        internal LabelGroup(LabelGroupId labelGroupId, string name)
        {
            LabelGroupId = labelGroupId;
            Name = name;
        }

        public async Task<LabelGroup> CreateOrUpdate(IMediator mediator, CancellationToken token = default)
        {     
            if (string.IsNullOrEmpty(Name))
            {
                throw new MediatorErrorEngineException($"Unable to create LabelGroup - Name property has not been set");
            }

            var command = new CreateOrUpdateLabelGroupCommand(LabelGroupId, Name);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            LabelGroupId = result.Data!;
            return this;
        }

        public async void PutAsUserDefault(IMediator mediator, UserId userId, CancellationToken token = default)
        {
            if (LabelGroupId == null)
            {
                throw new MediatorErrorEngineException("Cannot assign as user default before LabelGroup has been created");
            }

            await PutUserDefault(mediator, LabelGroupId, userId, token);
        }

        public async Task<IEnumerable<LabelId>> GetLabelIds(IMediator mediator, CancellationToken token = default)
        {
            if (LabelGroupId == null)
            {
                throw new MediatorErrorEngineException("Create LabelGroup before getting labels in group");
            }

            var command = new GetAllLabelsQuery(LabelGroupId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!.Select(e => e.LabelId!);
        }

        public async Task<Label> CreateAssociateLabel(IMediator mediator, string labelText, string? labelTemplateText, CancellationToken token = default)
        {
            if (LabelGroupId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate LabelGroup before associating with Label");
            }

            var label = await new Label { Text = labelText, TemplateText = labelTemplateText }.CreateOrUpdate(mediator, token);
            await AssociateLabel(mediator, label, token);

            return label;
        }

        public async Task AssociateLabel(IMediator mediator, Label label, CancellationToken token = default)
        {
            if (LabelGroupId is null)
            {
                throw new MediatorErrorEngineException("Create LabelGroup before associating with given Label");
            }
            if (label.LabelId is null)
            {
                throw new MediatorErrorEngineException("Create given Label before associating with LabelGroup");
            }

            var command = new CreateLabelGroupAssociationCommand(label.LabelId, LabelGroupId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public async Task DetachLabel(IMediator mediator, Label label, CancellationToken token = default)
        {
            if (LabelGroupId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before LabelGroup has been created");
            }
            if (label.LabelId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before it has been created/attached");
            }

            var command = new DeleteLabelGroupAssociationCommand(label.LabelId, LabelGroupId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public async void Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LabelGroupId == null)
            {
                return;
            }

            var command = new DeleteLabelGroupAndAssociationsByLabelGroupIdCommand(LabelGroupId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<IEnumerable<LabelGroup>> GetAll(
            IMediator mediator, 
            CancellationToken token = default)
        {
            var command = new GetAllLabelGroupsQuery();

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task<LabelGroupId?> GetUserDefault(
            IMediator mediator, 
            UserId userId,
            CancellationToken token = default)
        {
            var command = new GetLabelGroupDefaultForUserQuery(userId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task PutUserDefault(IMediator mediator, LabelGroupId? labelGroupId, UserId userId, CancellationToken token = default)
        {
            var command = new PutLabelGroupAsUserDefaultCommand(labelGroupId, userId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
