using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Features.Common;
using System.Data.Common;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;

namespace ClearDashboard.Collaboration.Merge
{
    public abstract class TokenComponentHandler<T> : DefaultMergeHandler<T> where T : IModelSnapshot
    {
        public static readonly Type TABLE_ENTITY_TYPE = typeof(Models.TokenComponent);
        public static readonly string DISCRIMINATOR_COLUMN_NAME = "Discriminator";

        protected TokenComponentHandler(MergeContext mergeContext) : base(mergeContext)
        {
        }

        protected async Task DeleteComposites(DbConnection dbConnection, IEnumerable<Guid> tokenCompositeIds, CancellationToken cancellationToken)
        {
            if (!tokenCompositeIds.Any())
            {
                return;
            }

            var alignmentSetDenormalizationTasks = await BuildDenormalizationTasksForTokenAlignments(
                dbConnection,
                tokenCompositeIds,
                cancellationToken);

            await InsertDenormalizationTasks(alignmentSetDenormalizationTasks, cancellationToken);

            await DataUtil.DeleteIdentifiableEntityAsync(
                dbConnection,
                TokenCompositeHandler.TABLE_ENTITY_TYPE,
                tokenCompositeIds,
                cancellationToken);
        }

        protected async Task InsertDenormalizationTasks(IEnumerable<GeneralModel<Models.AlignmentSetDenormalizationTask>> alignmentSetDenormalizationTasks, CancellationToken cancellationToken)
        {
            if (alignmentSetDenormalizationTasks.Any())
            {
                _mergeContext.MergeBehavior.StartInsertModelCommand(alignmentSetDenormalizationTasks.First());
                foreach (var child in alignmentSetDenormalizationTasks)
                {
                    _ = await _mergeContext.MergeBehavior.RunInsertModelCommand(child, cancellationToken);
                }
                _mergeContext.MergeBehavior.CompleteInsertModelCommand(typeof(Models.AlignmentSetDenormalizationTask));

                _mergeContext.FireAlignmentDenormalizationEvent = true;
            }
        }

        protected static async Task<List<GeneralModel<Models.AlignmentSetDenormalizationTask>>> BuildDenormalizationTasksForTokenAlignments(DbConnection dbConnection, IEnumerable<Guid> tokenComponentIds, CancellationToken cancellationToken)
        {
            List<GeneralModel<Models.AlignmentSetDenormalizationTask>> alignmentSetDenormalizationTasks = new();

            var alignmentSetTokens = (await DataUtil.SelectEntityValuesAsync(
                dbConnection,
                typeof(Models.Alignment),
                selectColumns: new List<string> {
                    nameof(Models.Alignment.AlignmentSetId),
                    nameof(Models.Alignment.SourceTokenComponentId),
                    nameof(Models.TokenComponent.TrainingText)
                },
                new Dictionary<string, object?> {
                    { nameof(Models.Alignment.SourceTokenComponentId), tokenComponentIds },
                    { $"{nameof(Models.Alignment)}.{nameof(Models.Alignment.Deleted)}", null }
                },
                new List<(Type, string, string)> { (
                    TABLE_ENTITY_TYPE,
                    nameof(Models.TokenComponent.Id),
                    nameof(Models.Alignment.SourceTokenComponentId)
                )},
                true,
                cancellationToken))
                    .Select(e => (
                        AlignmentSetId: Guid.Parse((string)e[nameof(Models.Alignment.AlignmentSetId)]!),
                        SourceTokenComponentId: Guid.Parse((string)e[nameof(Models.Alignment.SourceTokenComponentId)]!),
                        TrainingText: (string)e[nameof(Models.TokenComponent.TrainingText)]!))
                    .Where(e => !string.IsNullOrEmpty(e.TrainingText))
                    .GroupBy(e => e.AlignmentSetId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e).DistinctBy(e => e.SourceTokenComponentId));

            foreach (var kvp in alignmentSetTokens)
            {
                foreach (var tokenComponentInfo in kvp.Value)
                {
                    var t = new GeneralModel<Models.AlignmentSetDenormalizationTask>(nameof(Models.AlignmentSetDenormalizationTask.Id), Guid.NewGuid());
                    t.Add(nameof(Models.AlignmentSetDenormalizationTask.AlignmentSetId), kvp.Key);
                    t.Add(nameof(Models.AlignmentSetDenormalizationTask.SourceText), tokenComponentInfo.TrainingText);
                    alignmentSetDenormalizationTasks.Add(t);
                }
            }

            return alignmentSetDenormalizationTasks;
        }

        protected async Task DetachTokenComponents(IEnumerable<Guid> tokenIds, IEnumerable<Guid> tokenCompositeIds, CancellationToken cancellationToken)
        {
            if (!tokenIds.Any() && !tokenCompositeIds.Any())
            {
                return;
            }

            await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
    $"Detach TokenComposites/Tokens/TokenComponentTokenAssocations",
                async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                    foreach (var entry in projectDbContext.ChangeTracker
                        .Entries<Models.TokenCompositeTokenAssociation>()
                        .Where(e =>
                            tokenCompositeIds.Contains(e.Property(e => e.TokenCompositeId).OriginalValue) ||
                            tokenIds.Contains(e.Property(e => e.TokenId).OriginalValue)))
                    {
                        entry.State = EntityState.Detached;
                    }

                    foreach (var entry in projectDbContext.ChangeTracker
                        .Entries<Models.TokenComposite>()
                        .Where(e => tokenCompositeIds.Contains(e.Property(e => e.Id).OriginalValue)))
                    {
                        entry.State = EntityState.Detached;
                    }

                    foreach (var entry in projectDbContext.ChangeTracker
                        .Entries<Models.Token>()
                        .Where(e => tokenIds.Contains(e.Property(e => e.Id).OriginalValue)))
                    {
                        entry.State = EntityState.Detached;
                    }
                    await Task.CompletedTask;

                },
                cancellationToken
            );
        }
    }
}
