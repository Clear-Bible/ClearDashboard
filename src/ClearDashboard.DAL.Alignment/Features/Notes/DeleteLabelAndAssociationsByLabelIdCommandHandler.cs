﻿using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class DeleteLabelAndAssociationsByLabelIdCommandHandler : ProjectDbContextCommandHandler<DeleteLabelAndAssociationsByLabelIdCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DeleteLabelAndAssociationsByLabelIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteLabelAndAssociationsByLabelIdCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DeleteLabelAndAssociationsByLabelIdCommand request,
            CancellationToken cancellationToken)
        {
            var label = ProjectDbContext!.Labels.FirstOrDefault(c => c.Id == request.LabelId.Id);
            if (label == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid LabelId '{request.LabelId.Id}' found in request"
                );
            }

            // The data model should be set up to do a cascade delete of
            // any LabelNoteAssociations when the following executes:
            ProjectDbContext.Remove(label);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<object>(new());
        }
    }
}