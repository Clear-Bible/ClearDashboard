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

            await DataUtil.DeleteIdentifiableEntityAsync(
                dbConnection,
                TokenCompositeHandler.TABLE_ENTITY_TYPE,
                tokenCompositeIds,
                cancellationToken);
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
