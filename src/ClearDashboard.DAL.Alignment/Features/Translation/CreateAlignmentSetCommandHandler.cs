using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ModelVerificationType = ClearDashboard.DataAccessLayer.Models.AlignmentVerification;
using ModelOriginatedType = ClearDashboard.DataAccessLayer.Models.AlignmentOriginatedFrom;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Diagnostics;
using ClearDashboard.DAL.Alignment.Features.Events;
using System;
using ClearDashboard.DAL.Alignment.Features.Common;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class CreateAlignmentSetCommandHandler : ProjectDbContextCommandHandler<CreateAlignmentSetCommand,
        RequestResult<AlignmentSet>, AlignmentSet>
    {
        private readonly IMediator _mediator;

        public CreateAlignmentSetCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateAlignmentSetCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<AlignmentSet>> SaveDataAsync(CreateAlignmentSetCommand request,
            CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var sourceTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.SourceToken.TokenId.Id);
            var targetTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.TargetToken.TokenId.Id);

            var parallelCorpus = ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext!)
                // -------------------------------------------------------
                // For manuscript -> zz_sur, including the tokens in this
                // query was making it take upward of 15 minutes.
                // Removing this means we are unable to validate that all
                // alignment tokens are actually in the given parallel 
                // corpus (as source or target tokens, respectively).
                // -------------------------------------------------------
                //.Include(pc => pc.SourceTokenizedCorpus)
                //    .ThenInclude(stc => stc!.TokenComponents /*.Where(tc => sourceTokenIds.Contains(tc.Id)) */)
                //.Include(pc => pc.TargetTokenizedCorpus)
                //    .ThenInclude(ttc => ttc!.TokenComponents /*.Where(tc => targetTokenIds.Contains(tc.Id)) */)
                .FirstOrDefault(c => c.Id == request.ParallelCorpusId.Id);

#if DEBUG
            sw.Stop();
#endif

            if (parallelCorpus == null)
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

            //var notFoundSourceTokens = sourceTokenIds
            //    .Except(parallelCorpus!.SourceTokenizedCorpus!.TokenComponents
            //        .Select(tc => tc.Id));

            //if (notFoundSourceTokens.Any())
            //{
            //    return new RequestResult<AlignmentSet>
            //    (
            //        success: false,
            //        message: $"Requested alignment token pair source Id(s) not found in parallel corpus: '{string.Join(",", notFoundSourceTokens)}'"
            //    );
            //}

            //var notFoundTargetTokens = targetTokenIds
            //    .Except(parallelCorpus!.TargetTokenizedCorpus!.TokenComponents
            //        .Select(tc => tc.Id));

            //if (notFoundTargetTokens.Any())
            //{
            //    return new RequestResult<AlignmentSet>
            //    (
            //        success: false,
            //        message: $"Requested alignment token pair target Id(s) not found in parallel corpus: '{string.Join(",", notFoundTargetTokens)}'"
            //    );
            //}

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Insert AlignmentSet '{request.DisplayName}' and alignments (start) [token counts - source: {sourceTokenIds.Count()}, target: {targetTokenIds.Count()}]");
            sw.Restart();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE BULK INSERT): {proc.PrivateMemorySize64}");
#endif

            //var result = await AlignmentUtil.FillInVerificationAndOriginatedTypes(request.Alignments);

            //if (result.Success)
            //{
            //    request.Alignments = result.Data;
            //}
            //else
            //{
            //    return new RequestResult<AlignmentSet>
            //    (
            //        success: result.Success,
            //        message: result.Message
            //    );
            //}

            var verificationTypes = new Dictionary<string, ModelVerificationType>();
            var originatedTypes = new Dictionary<string, ModelOriginatedType>();

            var result = await AlignmentUtil.FillInVerificationAndOriginatedTypes(request.Alignments, verificationTypes, originatedTypes);

            if (!result.Success)
            {
                return new RequestResult<AlignmentSet>
                (
                    success: result.Success,
                    message: result.Message
                );
            }

            //foreach (var al in request.Alignments)
            //{
            //    if (Enum.TryParse(al.Verification, out ModelVerificationType verificationType))
            //    {
            //        verificationTypes[al.Verification] = verificationType;
            //    }
            //    else
            //    {
            //        return new RequestResult<AlignmentSet>
            //        (
            //            success: false,
            //            message: $"Invalid alignment verification type '{al.Verification}' found in request"
            //        );
            //    }

            //    if (Enum.TryParse(al.OriginatedFrom, out ModelOriginatedType originatedType))
            //    {
            //        originatedTypes[al.OriginatedFrom] = originatedType;
            //    }
            //    else
            //    {
            //        return new RequestResult<AlignmentSet>
            //        (
            //            success: false,
            //            message: $"Invalid alignment originated from type '{al.OriginatedFrom}' found in request"
            //        );
            //    }
            //}

            var alignmentSetId = Guid.NewGuid();
            var alignmentSet = new Models.AlignmentSet
            {
                Id = alignmentSetId,
                ParallelCorpusId = request.ParallelCorpusId.Id,
                DisplayName = request.DisplayName,
                SmtModel = request.SmtModel,
                IsSyntaxTreeAlignerRefined = request.IsSyntaxTreeAlignerRefined,
                IsSymmetrized = request.IsSymmetrized,
                Metadata = request.Metadata,
                //DerivedFrom = ,
                //EngineWordAlignment = ,
                Alignments = request.Alignments
                    .Select(al => new Models.Alignment
                    {
                        SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                        TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                        Score = al.AlignedTokenPair.Score,
                        AlignmentVerification = verificationTypes[al.Verification],
                        AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                    }).ToList()
            };

            cancellationToken.ThrowIfCancellationRequested();

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await ProjectDbContext.Database.GetDbConnection().BeginTransactionAsync(cancellationToken);

                using var alignmentSetInsertCommand = AlignmentUtil.CreateAlignmentSetInsertCommand(connection);
                using var alignmentInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);

                await AlignmentUtil.PrepareInsertAlignmentSetAsync(
                    alignmentSet,
                    alignmentSetInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                await AlignmentUtil.InsertAlignmentsAsync(
                    alignmentSet.Alignments, 
                    alignmentSetId,
                    alignmentInsertCommand, 
                    ProjectDbContext.UserProvider!.CurrentUser!.Id, 
                    cancellationToken);

                // Explicitly setting the DatabaseFacade transaction to match
                // the underlying DbConnection transaction in case any event handlers
                // need to alter data as part of the current transaction:
                ProjectDbContext.Database.UseTransaction(transaction);
                await _mediator.Publish(new AlignmentSetCreatingEvent(alignmentSetId, ProjectDbContext), cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                ProjectDbContext.Database.UseTransaction(null);
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }

#if DEBUG
            proc.Refresh();
            Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            var alignmentSetFromDb = ProjectDbContext!.AlignmentSets
                .Include(ast => ast.User)
                .First(ast => ast.Id == alignmentSetId);

            var parallelCorpusId = ModelHelper.BuildParallelCorpusId(parallelCorpus);

            await _mediator.Publish(new AlignmentSetCreatedEvent(alignmentSetId), cancellationToken);

            return new RequestResult<AlignmentSet>(new AlignmentSet(
                ModelHelper.BuildAlignmentSetId(alignmentSetFromDb, parallelCorpusId, alignmentSetFromDb.User!),
                parallelCorpusId,
                _mediator));
        }
    }
}