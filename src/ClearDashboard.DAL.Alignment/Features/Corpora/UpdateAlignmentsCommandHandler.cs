using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System.Data.Common;
using System.Threading.Tasks;
using SIL.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using SIL.Machine.SequenceAlignment;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using SIL.Machine.FiniteState;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateAlignmentsCommandHandler : ProjectDbContextCommandHandler<
        UpdateAlignmentsCommand, 
        RequestResult<AlignmentSet>,
        AlignmentSet>
    {
        private readonly IMediator _mediator;
        private readonly TranslationCommands _translationCommands;

        public UpdateAlignmentsCommandHandler(
            IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            TranslationCommands translationCommands,
            ILogger<UpdateAlignmentsCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
            _translationCommands = translationCommands;
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

                //var verificationTypes = new Dictionary<string, Models.AlignmentVerification>();
                //var originatedTypes = new Dictionary<string, Models.AlignmentOriginatedFrom>();

                //var result =
                //    await AlignmentUtil.FillInVerificationAndOriginatedEnums(redoneAlignments, verificationTypes,
                //        originatedTypes);

                //if (!result.Success)
                //{
                //    return new RequestResult<AlignmentSet>
                //    (
                //        success: result.Success,
                //        message: result.Message
                //    );
                //}

                cancellationToken.ThrowIfCancellationRequested();

                //await using var deleteOldMachineAlignmentsCommand = AlignmentUtil.CreateDeleteMachineAlignmentsByAlignmentSetIdCommand(connection);
                await AlignmentUtil.DeleteMachineAlignmentsByAlignmentSetIdAsync(
                    request.AlignmentSetToRedo.Id,
                    connection,
                    //deleteOldMachineAlignmentsCommand,
                    cancellationToken);


                //_ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                //var oldAlignmentSet = await AlignmentSet.Get(request.AlignmentSetToRedo, _mediator);
                //var oldAlignments = await oldAlignmentSet.GetAlignments(request.OldParallelTextRows, token: cancellationToken); //SLOW

                ////Get alignments to be replaced
                //var alignmentsToReplace = oldAlignments.Where(x => x.OriginatedFrom == "FromAlignmentModel");

                //var tokenIdsToReplace = alignmentsToReplace.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId.Id).ToList();

                //var dbAlignmentsToReplace = ProjectDbContext!.Alignments
                //    .Where(dba => tokenIdsToReplace.Contains(dba.SourceTokenComponentId));

                //var alignmentsRemoving = new List<Models.Alignment>();
                //dbAlignmentsToReplace.ForEach(dbAlignmentToReplace =>
                //{
                //    if (dbAlignmentToReplace.Deleted is null)
                //    {
                //        dbAlignmentToReplace.Deleted = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
                //        alignmentsRemoving.Add(dbAlignmentToReplace);
                //    }
                //});

                //Should we do a hard delete to prevent database bloating?
                //change to database query, keep ProjectDbContext aware though
                //if (alignmentsRemoving.Any())
                //{
                //    await _mediator.Publish(new AlignmentAddingRemovingEvent(alignmentsRemoving, null, ProjectDbContext), cancellationToken);
                //    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                //    await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsRemoving, null), cancellationToken);
                //}
                //else
                //{
                //    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
                //}

                //Get Replacement alignments
                var redoneAlignmentsModel = redoneAlignments
                    .Select(al => new Models.Alignment
                    {
                        SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                        TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                        Score = al.AlignedTokenPair.Score,
                        AlignmentVerification = Models.AlignmentVerification.Unverified,
                        AlignmentOriginatedFrom = Models.AlignmentOriginatedFrom.FromAlignmentModel
                    }).ToList();

                await using var redoneAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
                await AlignmentUtil.InsertAlignmentsAsync(
                    redoneAlignmentsModel,
                    request.AlignmentSetToRedo.Id,
                    redoneAlignmentsInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                ////Insert Replacement Alignments
                //var replacementAlignmentsModel = redoneAlignmentsModel.Where(na => tokenIdsToReplace.Contains(na.SourceTokenComponentId));
                //await using var replacementAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
                //await AlignmentUtil.InsertAlignmentsAsync(
                //    replacementAlignmentsModel,
                //    oldAlignmentSet.AlignmentSetId.Id,
                //    replacementAlignmentsInsertCommand,
                //    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                //    cancellationToken);

                ////Get New Alignments
                //var displayName = "Temp Alignment";

                //var newAlignmentSetId = Guid.NewGuid();
                //var redoneAlignmentSetModel = new Models.AlignmentSet
                //{
                //    Id = newAlignmentSetId,
                //    ParallelCorpusId = request.AlignmentSetToRedo.ParallelCorpusId.Id,
                //    DisplayName = displayName,
                //    SmtModel = request.AlignmentSetToRedo.SmtModel,
                //    IsSyntaxTreeAlignerRefined = request.AlignmentSetToRedo.IsSyntaxTreeAlignerRefined,
                //    IsSymmetrized = request.AlignmentSetToRedo.IsSymmetrized,
                //    Metadata = new Dictionary<string, object>(),
                //    Alignments = redoneAlignmentsModel
                //};

                //var oldTokenIds = oldAlignments.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId.Id).ToList();
                ////get rid of this and assume all redone alignments are good?
                //var newAlignments = redoneAlignmentSetModel.Alignments.Where(ra => !oldTokenIds.Contains(ra.SourceTokenComponentId));

                ////Insert New Alignments
                //await using var newAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
                //await AlignmentUtil.InsertAlignmentsAsync(
                //    newAlignments,
                //    oldAlignmentSet.AlignmentSetId.Id,
                //    newAlignmentsInsertCommand,
                //    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                //    cancellationToken);

                ////Add task for Translations refresh
                //ProjectDbContext.AlignmentSetDenormalizationTasks.Add(new Models.AlignmentSetDenormalizationTask()
                //{
                //    AlignmentSetId = request.AlignmentSetToRedo.Id
                //});

                //await ProjectDbContext.SaveChangesAsync(cancellationToken);

                //if (alignmentsRemoving.Any())
                //{
                // Explicitly setting the DatabaseFacade transaction to match
                // the underlying DbConnection transaction in case any event handlers
                // need to alter data as part of the current transaction:
                ProjectDbContext.Database.UseTransaction(transaction);
                await _mediator.Publish(new AlignmentSetCreatingEvent(request.AlignmentSetToRedo.Id,ProjectDbContext), cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                ProjectDbContext.Database.UseTransaction(null);
                //}
                //else
                //{
                //await transaction.CommitAsync(cancellationToken);
                //}
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
