using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateAlignmentsCommandHandler : ProjectDbContextCommandHandler<
        UpdateAlignmentsCommand, 
        RequestResult<AlignmentSet>,
        AlignmentSet>
    {
        private readonly IMediator _mediator;

        public UpdateAlignmentsCommandHandler(
            IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<UpdateAlignmentsCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<AlignmentSet>> SaveDataAsync(UpdateAlignmentsCommand request, CancellationToken cancellationToken)
        {
            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var redoneAlignments = request.TrainSmtModelSet.AlignedTokenPairs.Select(a =>
                    new Alignment.Translation.Alignment(a, "Unverified", "FromAlignmentModel"));

                cancellationToken.ThrowIfCancellationRequested();

                //await using var deleteOldMachineAlignmentsCommand = AlignmentUtil.CreateDeleteMachineAlignmentsByAlignmentSetIdCommand(connection);
                await AlignmentUtil.DeleteMachineAlignmentsByAlignmentSetIdAsync(
                    request.AlignmentSetToRedo.Id,
                    connection,
                    ProjectDbContext.Model,
                    cancellationToken);

                var redoneAlignmentsModel = redoneAlignments
                    .Select(al => new Models.Alignment
                    {
                        SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                        TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                        Score = al.AlignedTokenPair.Score,
                        AlignmentVerification = Models.AlignmentVerification.Unverified,
                        AlignmentOriginatedFrom = Models.AlignmentOriginatedFrom.FromAlignmentModel
                    }).ToList();

                await using var redoneAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection, ProjectDbContext.Model);
                await AlignmentUtil.InsertAlignmentsAsync(
                    redoneAlignmentsModel,
                    request.AlignmentSetToRedo.Id,
                    redoneAlignmentsInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                ProjectDbContext.Database.UseTransaction(transaction);
                await _mediator.Publish(new AlignmentSetCreatingEvent(request.AlignmentSetToRedo.Id,ProjectDbContext), cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                ProjectDbContext.Database.UseTransaction(null);
       
                await _mediator.Publish(new AlignmentSetCreatedEvent(request.AlignmentSetToRedo.Id), cancellationToken);
            }
            catch (Exception ex)
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: ex.Message
                );
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
            
            return new RequestResult<AlignmentSet>
            (
                success: true,
                message: $"Alignments Updated!"
            );
        }
    }
}
