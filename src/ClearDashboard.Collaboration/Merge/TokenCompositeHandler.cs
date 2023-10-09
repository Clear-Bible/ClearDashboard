using System;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.Collaboration.Exceptions;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration.Builder;
using System.Data.Common;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using System.Reflection.Metadata.Ecma335;
using SIL.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections;
using System.Threading;

namespace ClearDashboard.Collaboration.Merge;

public class TokenCompositeHandler : DefaultMergeHandler<IModelSnapshot<Models.TokenComposite>>
{
    public static readonly Type TABLE_ENTITY_TYPE = typeof(Models.TokenComponent);
    public static readonly string DISCRIMINATOR_COLUMN_VALUE = "TokenComposite";
    public static readonly string DISCRIMINATOR_COLUMN_NAME = "Discriminator";

    public TokenCompositeHandler(MergeContext mergeContext) : base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.TokenComposite),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "TokenComposite")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.VerseRowId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenCompositeBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.TokenizedCorpusId), out var tokenizedCorpusId))
                {
                    var verseRowId = await projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)verseRowLocation!)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync();

                    logger.LogDebug($"Converted TokenComposite TokenizedCorpusId ('{tokenizedCorpusId}') / VerseRowLocation ('{verseRowLocation}') to VerseRowId ('{verseRowId}')");
                    return (verseRowId != Guid.Empty) ? verseRowId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"TokenComposite snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowLocation conversion.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.TokenComposite>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.TokenComposite>");
                }

                return await ResolveTokenCompositeId((IModelSnapshot<Models.TokenComposite>)modelSnapshot, dbConnection, logger);
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.TokenComposite), "Ref"),
            new[] { nameof(Models.TokenComposite.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), "Ref"),
            new[] { nameof(Models.TokenComposite.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(  
            (typeof(Models.TokenComposite), TokenCompositeBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.TokenComposite.VerseRowId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), TokenCompositeBuilder.TOKEN_LOCATIONS),
            Enumerable.Empty<string>());
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteCompositesByVerseRowIdQueryAsync(Guid verseRowId)
    {
        /*
         * Query to run before deleting a VerseRow (and using its real Id):
DELETE FROM TokenComponent WHERE Id IN
(
    SELECT DISTINCT tco.Id
    FROM TokenComponent tco
    JOIN TokenCompositeTokenAssociation tca on tca.TokenCompositeId = tco.Id
    JOIN TokenComponent tc on tca.TokenId = tc.Id
    JOIN VerseRow vr on tc.VerseRowId = vr.Id
    WHERE vr.Id = '[... some id ...]'
)
         */
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                projectDbContext.RemoveRange(projectDbContext.TokenComposites
                    .Where(tc => tc.TokenCompositeTokenAssociations
                        .Where(tca => tca.Token!.VerseRowId == verseRowId).Any()));

                await Task.CompletedTask;
            };
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteTokenComponentsByVerseRowIdQueryAsync(Guid verseRowId)
    {
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                projectDbContext.RemoveRange(projectDbContext.TokenComponents
                    .Where(tc => tc.VerseRowId == verseRowId));

                await Task.CompletedTask;

            };
    }

    protected async static Task<Guid> ResolveTokenCompositeId(IModelSnapshot<Models.TokenComposite> modelSnapshot, DbConnection dbConnection, ILogger logger)
    {
        if (modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.TokenizedCorpusId), out var tokenizedCorpusId) &&
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.EngineTokenId), out var engineTokenId))
        {
            modelSnapshot.PropertyValues.TryGetValue(nameof(Models.TokenComposite.ParallelCorpusId), out var parallelCorpusId);
            var hasDeleted = modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.Deleted), out var tokenDeleted);

            var deleted = (hasDeleted && tokenDeleted is not null) ? DataUtil.NOT_NULL : null;

            var tokenCompositeId = (await DataUtil.SelectEntityValuesAsync(
                dbConnection,
                TABLE_ENTITY_TYPE,
                new List<string> { "Id" },
                new Dictionary<string, object?>             {
                    { "EngineTokenId", (string)engineTokenId! },
                    { "TokenizedCorpusId", tokenizedCorpusId },
                    { "Deleted", deleted },
                    { "ParallelCorpusId", parallelCorpusId },
                    { DISCRIMINATOR_COLUMN_NAME, DISCRIMINATOR_COLUMN_VALUE }
                },
                Enumerable.Empty<(Type, string, string)>(),
                true,
                CancellationToken.None))
                    .Select(e => Guid.Parse((string)e["Id"]!))
                    .FirstOrDefault();

            if (tokenCompositeId == default)
            {
                tokenCompositeId = Guid.NewGuid();
                logger.LogDebug($"No TokenComposite Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / ParallelCorpusId ('{parallelCorpusId}') / EngineTokenId ('{engineTokenId}').  Using: '{tokenCompositeId}'");
            }
            else
            {
                logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / ParallelCorpusId ('{parallelCorpusId}') / EngineTokenId ('{engineTokenId}') to TokenComposite Id ('{tokenCompositeId}')");
            }

            return tokenCompositeId;
        }
        else
        {
            throw new PropertyResolutionException($"TokenComposite snapshot does not have all:  TokenizedCorpusId+EngineTokenId, which are required for Id resolution.");
        }
    }

    // FIXME:  need to talk out this logic with someone.  I think what I need to do
    // is approach this at a low level - no business logic - and assume that the
    // snapshot carries across changes to affected Translations and Alignments.

    protected override async Task<Dictionary<string, object>> HandleDeleteAsync(IModelSnapshot<Models.TokenComposite> itemToDelete, CancellationToken cancellationToken)
    {
        return await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }

    private async Task<IEnumerable<(Guid TokenId, string EngineTokenId)>> FindTokenIds(IEnumerable<string> tokenLocations, Guid tokenizedCorpusId, CancellationToken cancellationToken)
    {
        var tokenIds = new List<(Guid TokenId, string EngineTokenId)>();

        await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
            $"Resolve token id",
            async (DbConnection dbConnection, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                tokenIds.AddRange((await DataUtil.SelectEntityValuesAsync(
                    dbConnection,
                    TokenHandler.TABLE_ENTITY_TYPE,
                    new List<string> { "Id", "EngineTokenId" },
                    new Dictionary<string, object?> {
                        { "EngineTokenId", tokenLocations },
                        { "TokenizedCorpusId", tokenizedCorpusId },
                        { "Deleted", null },
                        { TokenHandler.DISCRIMINATOR_COLUMN_NAME, TokenHandler.DISCRIMINATOR_COLUMN_VALUE }
                    },
                    Enumerable.Empty<(Type, string, string)>(),
                    true,
                    CancellationToken.None))
                        .Select(e => (
                            TokenId: Guid.Parse((string)e["Id"]!),
                            EngineTokenId: (string)e["EngineTokenId"]!))
                        .ToList());
            },
            cancellationToken
        );

        return tokenIds; ;
    }

    protected override async Task<Guid> HandleCreateAsync(IModelSnapshot<Models.TokenComposite> itemToCreate, CancellationToken cancellationToken)
    {
        var id = await base.HandleCreateAsync(itemToCreate, cancellationToken);

        // Not only do we need to create the TokenComposite record itself,
        // but we also need to find all TokenLocation tokens and set their
        // TokenCompositeId.

        var tokenizedCorpusId = (Guid)itemToCreate.PropertyValues["TokenizedCorpusId"]!;
        var parallelCorpusId = (Guid?)itemToCreate.PropertyValues["ParallelCorpusId"];
        var tokenLocations = (IEnumerable<string>)itemToCreate.PropertyValues["TokenLocations"]!;

        var tokenIds = await FindTokenIds(tokenLocations, tokenizedCorpusId, cancellationToken);

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"In HandleCreateAsync associating new TokenComposite with child Tokens",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var leftover = tokenLocations.Except(tokenIds.Select(e => e.EngineTokenId));
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", leftover)}");
                }

                var existingAssociatedTokenCompositeIds = projectDbContext.TokenCompositeTokenAssociations
                    .Include(e => e.TokenComposite)
                    .Select(e => new { e.TokenCompositeId, e.TokenComposite!.ParallelCorpusId })
                    .Where(e => e.ParallelCorpusId == parallelCorpusId)
                    .Select(e => e.TokenCompositeId)
                    .ToList();

                if (existingAssociatedTokenCompositeIds.Any())
                {
                    logger.LogInformation($"CompositeToken '{id}' snapshot contains tokens already part of a composite having a ParallelCorpusId (null or non-null) that matches '{parallelCorpusId}'.  Deleting existing/conflicting composite");
                    projectDbContext.RemoveRange(projectDbContext.TokenComposites.Where(e => existingAssociatedTokenCompositeIds.Contains(e.Id)));
                }

                // Add the tokens to the composite:
                var tokenAssocationsToAdd = tokenIds
                    .Select(l => new Models.TokenCompositeTokenAssociation
                    {
                        TokenId = l.TokenId,
                        TokenCompositeId = id
                    })
                    .ToList();

                projectDbContext.TokenCompositeTokenAssociations.AddRange(tokenAssocationsToAdd);
                await Task.CompletedTask;
            },
            cancellationToken);

        return id;
    }

    public override async Task<bool> HandleModifyPropertiesAsync(IModelDifference<IModelSnapshot<Models.TokenComposite>> modelDifference, IModelSnapshot<Models.TokenComposite> itemToModify, CancellationToken cancellationToken = default)
    {
        var modified = await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        var tokenLocationDifference = modelDifference.PropertyDifferences
            .Where(pd => pd.PropertyName == "TokenLocations")
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ListDifference<string>)))
            .FirstOrDefault();
        if (tokenLocationDifference is null)
        {
            return modified;
        }

        // If the modifications included changes to which tokens were part of
        // the composite, we need to do two things:
        //  - remove the previous child tokens
        //  - add the new child tokens
        var tokenCompositeId = default(Guid);

        await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
            $"Resolve token id",
            async (DbConnection dbConnection, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                tokenCompositeId = await ResolveTokenCompositeId(itemToModify, dbConnection, logger);
            },
            cancellationToken
        );

        var valueListDifference = (ListDifference<string>)tokenLocationDifference.PropertyValueDifference;
        var tokenLocationsOnlyIn1 = valueListDifference.ListMembershipDifference.OnlyIn1;
        var tokenLocationsOnlyIn2 = valueListDifference.ListMembershipDifference.OnlyIn2;
        var tokenLocationsBoth = tokenLocationsOnlyIn1.Union(tokenLocationsOnlyIn2);
        var tokenizedCorpusId = (Guid)itemToModify.PropertyValues["TokenizedCorpusId"]!;
        var parallelCorpusId = (Guid?)itemToModify.PropertyValues["ParallelCorpusId"];

        var tokenIds = await FindTokenIds(tokenLocationsBoth, tokenizedCorpusId, cancellationToken);

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Applying TokenComposite 'TokenLocations' property ListMembershipDifference (OnlyIn1: {string.Join(", ", tokenLocationsOnlyIn1)}, OnlyIn2: {string.Join(", ", tokenLocationsOnlyIn2)})", 
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                var leftover = tokenLocationsBoth.Except(tokenIds.Select(e => e.EngineTokenId));
                if (leftover.Any())
                {
                    throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", leftover)}");
                }

                // Remove the OnlyIn1 tokens from the composite:
                var tokenIdsOnlyIn1 = tokenIds
                    .Where(e => tokenLocationsOnlyIn1.Contains(e.EngineTokenId))
                    .Select(e => e.TokenId)
                    .ToList();

                var tokenAssocationsToRemove = projectDbContext.TokenCompositeTokenAssociations
                    .Where(a => tokenIdsOnlyIn1.Contains(a.TokenId))
                    .Where(a => a.TokenCompositeId == tokenCompositeId);

                projectDbContext.RemoveRange(tokenAssocationsToRemove);

                var tokenIdsOnlyIn2 = tokenIds
                    .Where(e => tokenLocationsOnlyIn2.Contains(e.EngineTokenId))
                    .Select(e => e.TokenId)
                    .ToList();

                var existingAssociatedTokenCompositeIds = projectDbContext.TokenCompositeTokenAssociations
                    .Include(e => e.TokenComposite)
                    .Where(e => tokenIdsOnlyIn2.Contains(e.TokenId))
                    .Select(ta => new { ta.TokenCompositeId, ta.TokenComposite!.ParallelCorpusId })
                    .Where(tap => tap.ParallelCorpusId == parallelCorpusId)
                    .Where(tap => tap.TokenCompositeId != tokenCompositeId)
                    .Select(tap => tap.TokenCompositeId)
                    .ToList();

                if (existingAssociatedTokenCompositeIds.Any())
                {
                    throw new PropertyResolutionException($"Merge conflict:  CompositeToken '{tokenCompositeId}' snapshot contains tokens already part of a composite having a ParallelCorpusId (null or non-null) that matches '{parallelCorpusId}'.");
                }

                // Add the OnlyIn2 tokens to the composite:
                var tokenAssocationsToAdd = tokenIdsOnlyIn2
                    .Select(e => new Models.TokenCompositeTokenAssociation
                    {
                        TokenId = e,
                        TokenCompositeId = tokenCompositeId
                    })
                    .ToList();

                projectDbContext.TokenCompositeTokenAssociations.AddRange(tokenAssocationsToAdd);
                await Task.CompletedTask;
            },
            cancellationToken);

        return modified;
    }

    public Expression<Func<Models.TokenComposite, bool>> BuildTokenChildWhereExpression(IModelSnapshot<Models.TokenComposite> snapshot, Guid tokenizedCorpusId, IEnumerable<string> tokenLocations)
    {
        return e => e.GetType() == typeof(Models.Token) &&
                    e.TokenizedCorpusId == tokenizedCorpusId &&
                    tokenLocations.Contains(e.EngineTokenId);
    }
}

