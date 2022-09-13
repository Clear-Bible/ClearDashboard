using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class CreateLabelNoteAssociationCommandHandler : ProjectDbContextCommandHandler<CreateLabelNoteAssociationCommand,
        RequestResult<LabelNoteAssociationId>, LabelNoteAssociationId>
    {
        private readonly IMediator _mediator;

        public CreateLabelNoteAssociationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateLabelNoteAssociationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LabelNoteAssociationId>> SaveDataAsync(CreateLabelNoteAssociationCommand request,
            CancellationToken cancellationToken)
        {
            var labelNoteAssociation = ProjectDbContext!.LabelNoteAssociations
                .FirstOrDefault(ln => ln.LabelId == request.LabelId.Id && ln.NoteId == request.NoteId.Id);

            if (labelNoteAssociation == null)
            {
                labelNoteAssociation = new Models.LabelNoteAssociation
                {
                    LabelId = request.LabelId.Id,
                    NoteId = request.NoteId.Id
                };

                ProjectDbContext.LabelNoteAssociations.Add(labelNoteAssociation);
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<LabelNoteAssociationId>(new LabelNoteAssociationId(labelNoteAssociation.Id));
        }
    }
}