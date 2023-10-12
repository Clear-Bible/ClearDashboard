using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration.DifferenceModel;
using System.Threading;
using ClearDashboard.DAL.Alignment.Features.Common;
using System.Data.Entity.Core.Objects;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace ClearDashboard.Collaboration.Merge;

public class TokenHandler : TokenComponentHandler<IModelSnapshot<Models.Token>>
{
    public static readonly string DISCRIMINATOR_COLUMN_VALUE = nameof(Models.Token);
    public TokenHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.Token),
            (
                TableEntityType: TABLE_ENTITY_TYPE, 
                DiscriminatorColumnName: DISCRIMINATOR_COLUMN_NAME, 
                DiscriminatorColumnValue: DISCRIMINATOR_COLUMN_VALUE
            )
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.VerseRowId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId))
                {
                    var verseRowId = await projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)verseRowLocation!)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    logger.LogDebug($"Converted Token TokenizedCorpusId ('{tokenizedCorpusId}') / VerseRowLocation ('{verseRowLocation}') to VerseRowId ('{verseRowId}')");
                    return (verseRowId != Guid.Empty) ? verseRowId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Token snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowLocation conversion.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Token>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Token>");
                }

                return await ResolveTokenId((IModelSnapshot<Models.Token>)modelSnapshot, dbConnection, cache, logger);
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), TokenBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.Token.VerseRowId) });
    }

    protected async static Task<Guid> ResolveTokenId(IModelSnapshot<Models.Token> modelSnapshot, DbConnection dbConnection, MergeCache cache, ILogger logger)
    {
        if (modelSnapshot.PropertyValues.TryGetValue("Ref", out var refValue) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.EngineTokenId), out var engineTokenId))
        {
            if (cache.TryLookupCacheEntry(
                TokenizedCorpusTokenRefCacheKey((Guid)tokenizedCorpusId!), (string)refValue!, out var id))
            {
                return (Guid)id!;
            }

            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.OriginTokenLocation), out var originTokenLocation);
            var hasIndex = int.TryParse(((string)refValue!).Split("_").LastOrDefault(), out int index);
            var hasDeleted = modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.Deleted), out var tokenDeleted);

            Guid tokenId;
            var deleted = (hasDeleted && tokenDeleted is not null) ? DataUtil.NOT_NULL : null;
            if (originTokenLocation is not null)
            {
                var tokenResult = (await DataUtil.SelectEntityValuesAsync(
                    dbConnection,
                    TABLE_ENTITY_TYPE,
                    new List<string> { nameof(Models.Token.Id), nameof(Models.Token.EngineTokenId) },
                    whereClause: new Dictionary<string, object?> {
                        { nameof(Models.Token.OriginTokenLocation), (string)originTokenLocation },
                        { nameof(Models.Token.TokenizedCorpusId), tokenizedCorpusId },
                        { nameof(Models.Token.Deleted), deleted },
                        { DISCRIMINATOR_COLUMN_NAME, DISCRIMINATOR_COLUMN_VALUE }
                    },
                    Enumerable.Empty<(Type, string, string)>(),
                    true,
                    CancellationToken.None))
                        .Select(e => (
                            EngineTokenId: (string)e[nameof(Models.Token.EngineTokenId)]!,
                            Id: Guid.Parse((string)e[nameof(Models.Token.Id)]!)))
                        .OrderBy(e => e.EngineTokenId)
                        .Skip(hasIndex ? index : 0)
                        .FirstOrDefault();

                if (tokenResult == default)
                {
                    tokenId = Guid.NewGuid();
                    logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}).  Using: '{tokenId}'");
                }
                else
                {
                    tokenId = tokenResult.Id;
                    //projectDbContext.Entry(token).State = EntityState.Detached;
                    logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}) to Token Id ('{tokenId}')");
                }
            }
            else
            {
                var tokenResult = (await DataUtil.SelectEntityValuesAsync(
                    dbConnection,
                    TABLE_ENTITY_TYPE,
                    new List<string> { nameof(Models.Token.Id), nameof(Models.Token.EngineTokenId) },
                    new Dictionary<string, object?> {
                        { nameof(Models.Token.EngineTokenId), (string)engineTokenId! },
                        { nameof(Models.Token.TokenizedCorpusId), tokenizedCorpusId },
                        { nameof(Models.Token.Deleted), deleted },
                        { DISCRIMINATOR_COLUMN_NAME, DISCRIMINATOR_COLUMN_VALUE }
                    },
                    Enumerable.Empty<(Type, string, string)>(),
                    true,
                    CancellationToken.None))
                        .Select(e => (
                            EngineTokenId: (string)e[nameof(Models.Token.EngineTokenId)]!,
                            Id: Guid.Parse((string)e[nameof(Models.Token.Id)]!)))
                        .FirstOrDefault();

                if (tokenResult == default)
                {
                    tokenId = Guid.NewGuid();
                    logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}').  Using: '{tokenId}'");
                }
                else
                {
                    tokenId = tokenResult.Id;
                    //projectDbContext.Entry(token).State = EntityState.Detached;
                    logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}') to Token Id ('{tokenId}')");
                }
            }

            return tokenId;
        }
        else
        {
            throw new PropertyResolutionException($"Token snapshot does not have all:  Ref+TokenizedCorpusId+EngineTokenId, which are required for Id resolution.");
        }
    }

    protected override async Task<Dictionary<string, object>> HandleDeleteAsync(IModelSnapshot<Models.Token> itemToDelete, CancellationToken cancellationToken)
    {
        // If deleting a Token that is associated with a composite...
        // my best guess is we should delete the composite since it
        // is now invalid

        var tokenId = default(Guid);
        var deletedTokenCompositeIds = new List<Guid>();

        await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
            $"Resolve token id",
            async (DbConnection dbConnection, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                tokenId = await ResolveTokenId(itemToDelete, dbConnection, cache, logger);

                var alignmentSetDenormalizationTasks = await BuildDenormalizationTasksForTokenAlignments(
                    dbConnection, 
                    new Guid[] { tokenId }, 
                    cancellationToken);

                await InsertDenormalizationTasks(alignmentSetDenormalizationTasks, cancellationToken);

                var tokenCompositeIds = await FindTokenCompositeIds(dbConnection, new Guid[] { tokenId }, cancellationToken);
                await DeleteComposites(dbConnection, tokenCompositeIds, cancellationToken);

                deletedTokenCompositeIds.AddRange(tokenCompositeIds);
            },
            cancellationToken
        );

        // Since we are deleting the Token entity using DbCommand (which will cascade delete any
        // related TokenComponentTokenAssociation entities), DbContext won't know anything about
        // it and if projectDbContext is tracking the token or its associations, they will get out
        // of sync with the changes made directly to the database.  So...detach, just to be safe:
        await DetachTokenComponents(new Guid[] { tokenId }, deletedTokenCompositeIds, cancellationToken);

        // Deletes the actual Token entity.  
        return await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }

    /// <summary>
    /// Handles two cases:
    /// 1.  Any remaining tokens having the same OriginTokenLocation are being deleted (contained in OnlyIn1)
    /// 2.  All tokens in the incoming snapshot for a given OriginTokenLocation are being created (contained in OnlyIn2)
    /// In this method we are trying to think of each split tokens set as a unit.  So for both cases, delete any
    /// 'leftover' tokens in the current database (either: tokens leftover after deleting a set, or additional tokens
    /// of a given set in the current database that aren't on in the incoming snapshot).  
    /// </summary>
    /// <param name="listDifference"></param>
    /// <param name="childrenInCurrentSnapshot"></param>
    /// <param name="childrenInTargetCommitSnapshot"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDifferenceStateException"></exception>
    public async Task DeleteOriginTokenLocationLeftovers(
        IListDifference listDifference,
        IEnumerable<IModelDistinguishable>? childrenInCurrentSnapshot,
        IEnumerable<IModelDistinguishable>? childrenInTargetCommitSnapshot,
        CancellationToken cancellationToken)
    {
        if (!listDifference.GetType().IsAssignableTo(typeof(IListDifference<IModelSnapshot<Models.Token>>)))
        {
            throw new InvalidDifferenceStateException($"Encountered child IListDifference without any generic model type!");
        }

        if (childrenInCurrentSnapshot is not null && !childrenInCurrentSnapshot.GetType().IsAssignableTo(typeof(IEnumerable<IModelSnapshot<Models.Token>>)))
        {
            throw new InvalidDifferenceStateException($"Encountered non-IModelSnapshot children in current snapshot list!");
        }

        if (childrenInTargetCommitSnapshot is not null && !childrenInTargetCommitSnapshot.GetType().IsAssignableTo(typeof(IEnumerable<IModelSnapshot<Models.Token>>)))
        {
            throw new InvalidDifferenceStateException($"Encountered non-IModelSnapshot children in target commit snapshot list!");
        }

        var difference = (IListDifference<IModelSnapshot<Models.Token>>)listDifference;
        var currentChildren = (IEnumerable<IModelSnapshot<Models.Token>>?)childrenInCurrentSnapshot;
        var targetCommitChildren = (IEnumerable<IModelSnapshot<Models.Token>>?)childrenInTargetCommitSnapshot;

        var otlSetsInCurrentDatabase = FindOriginTokenLocationSets(currentChildren);
        var otlSetsInTargetCommitSnapshot = FindOriginTokenLocationSets(targetCommitChildren);

        // For deletes (OnlyIn1): for any OriginTokenLocation having no remaining values in the latest
        // commit snapshot, make sure all are getting deleted from the current database (i.e. make sure
        // any refValuesInCurrentDatabase.Except(OnlyIn1) are deleted from the current database).  
        var otlDeletes = FindOriginTokenLocationSets(difference.OnlyIn1);

        foreach (var otlDelete in otlDeletes
            .Where(e => !otlSetsInTargetCommitSnapshot.ContainsKey(e.Key)))
        {
            if (otlSetsInCurrentDatabase.TryGetValue(otlDelete.Key, out var otlSetCurrentDatabase))
            {
                var leftoverRefsInCurrentDatabase = otlSetCurrentDatabase.Except(otlDelete.Value);
                foreach (var leftoverRef in leftoverRefsInCurrentDatabase)
                {
                    var itemInCurrentSnapshot = currentChildren.FindById(leftoverRef);
                    if (itemInCurrentSnapshot is not null)
                    {
                        await HandleDeleteAsync(itemInCurrentSnapshot, cancellationToken);
                    }
                }
            }
        }

        // For inserts (OnlyIn2): for any OriginTokenLocation that contains all Ref values from the latest
        // commit snapshot, make sure there are no extras in the current database (i.e. make sure any
        // refValuesInCurrentDatabase.Except(refValuesInCurrentSnapshot) are deleted from the current database).  
        var otlInserts = FindOriginTokenLocationSets(difference.OnlyIn2);

        foreach (var otlInsert in otlInserts)
        {
            if (otlSetsInTargetCommitSnapshot.TryGetValue(otlInsert.Key, out var otlSetTargetSnapshot) &&
                otlSetTargetSnapshot.All(e => otlInsert.Value.Contains(e)) &&
                otlSetsInCurrentDatabase.TryGetValue(otlInsert.Key, out var otlSetCurrentDatabase))
            {
                var leftoverRefsInCurrentDatabase = otlSetCurrentDatabase.Except(otlSetTargetSnapshot);
                foreach (var leftoverRef in leftoverRefsInCurrentDatabase)
                {
                    var itemInCurrentSnapshot = currentChildren.FindById(leftoverRef);
                    if (itemInCurrentSnapshot is not null)
                    {
                        await HandleDeleteAsync(itemInCurrentSnapshot, cancellationToken);
                    }
                }
            }
        }

        // Delete any existing token composites from the current database that reference tokens
        // from the OriginTokenLocation sets being inserted:
        var deletedTokenCompositeIds = new List<Guid>();
        foreach (var otlInsert in otlInserts)
        {
            if (otlSetsInCurrentDatabase.TryGetValue(otlInsert.Key, out var otlSetCurrentDatabase))
            {
                await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
                    $"Find all database composite ids related to incoming token snapshots",
                    async (DbConnection dbConnection, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
                    {
                        var tokenIds = new List<Guid>();
                        foreach (var tokenSnapshot in otlSetCurrentDatabase
                            .Select(e => e.Snapshot))
                        {
                            var tokenId = await ResolveTokenId(tokenSnapshot, dbConnection, cache, logger);
                            if (tokenId != default)
                            {
                                tokenIds.Add(tokenId);
                            }
                        }

                        var tokenCompositeIds = await FindTokenCompositeIds(dbConnection, tokenIds, cancellationToken);
                        await DeleteComposites(dbConnection, tokenCompositeIds, cancellationToken);

                        deletedTokenCompositeIds.AddRange(tokenCompositeIds);
                    },
                    cancellationToken);
            }
        }

        await DetachTokenComponents(Enumerable.Empty<Guid>(), deletedTokenCompositeIds, cancellationToken);
    }

    private static async Task<IEnumerable<Guid>> FindTokenCompositeIds(DbConnection dbConnection, IEnumerable<Guid> tokenIds, CancellationToken cancellationToken)
    {
        return (await DataUtil.SelectEntityValuesAsync(
            dbConnection,
            typeof(Models.TokenCompositeTokenAssociation),
            new List<string> { 
                nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId)
            },
            new Dictionary<string, object?> {
                { nameof(Models.TokenCompositeTokenAssociation.TokenId), tokenIds },
                { nameof(Models.TokenCompositeTokenAssociation.Deleted), null }
            },
            Enumerable.Empty<(Type, string, string)>(),
            true,
            cancellationToken))
                .Select(e => Guid.Parse((string)e[nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId)]!))
                .Distinct()
                .ToList();
    }

    private static IDictionary<string, IEnumerable<(string RefValue, IModelSnapshot<Models.Token> Snapshot)>> FindOriginTokenLocationSets(IEnumerable<IModelSnapshot<Models.Token>>? tokenSnapshots)
    {
        if (tokenSnapshots is null)
        {
            return new Dictionary<string, IEnumerable<(string, IModelSnapshot<Models.Token>)>>();
        }

        return tokenSnapshots
            .Select(e =>
            {
                if (e.PropertyValues.TryGetValue("Ref", out var refValue) &&
                    e.PropertyValues.TryGetValue(nameof(Models.Token.OriginTokenLocation), out var originTokenLocation) &&
                    !string.IsNullOrEmpty((string?)originTokenLocation) &&
                    int.TryParse(((string)refValue!).Split("_").LastOrDefault(), out int index))
                {
                    return (OriginTokenLocation: (string)originTokenLocation, RefValue: (string)refValue, Index: index, Snapshot: e);
                }
                else
                {
                    return (OriginTokenLocation: string.Empty, RefValue: string.Empty, Index: 0, Snapshot: e);
                }
            })
            .Where(e => !string.IsNullOrEmpty(e.OriginTokenLocation))
            .GroupBy(e => e.OriginTokenLocation)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(e => e.Index).Select(e => (e.RefValue, e.Snapshot)));
    }
}