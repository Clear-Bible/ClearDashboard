using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public class TokenCompositeHandler : TokenComponentHandler<IModelSnapshot<Models.TokenComposite>>
{
    public static readonly string DISCRIMINATOR_COLUMN_VALUE = nameof(Models.TokenComposite);

    public TokenCompositeHandler(MergeContext mergeContext) : base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.TokenComposite),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "TokenComposite")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenComposite), nameof(Models.TokenComposite.VerseRowId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) =>
            {

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
			(typeof(Models.TokenComposite), nameof(Models.TokenComposite.GrammarId)),
			entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

				if (modelSnapshot.PropertyValues.TryGetValue(TokenCompositeBuilder.GRAMMAR_SHORT_NAME, out var grammarShortName))
				{
					if (string.IsNullOrEmpty((string?)grammarShortName)) return null;
					var grammarId = await GrammarHandler.GrammarShortNameToId((string)grammarShortName, projectDbContext, logger);
					return (grammarId != default) ? grammarId : null;
				}
				else
				{
					throw new PropertyResolutionException($"TokenComposite snapshot does not have Grammer ShortName, which is required for Grammar conversion.");
				}

			});

		mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.TokenComposite), TokenCompositeBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.TokenComposite.VerseRowId) });

		mergeContext.MergeBehavior.AddPropertyNameMapping(
			(typeof(Models.TokenComposite), TokenCompositeBuilder.GRAMMAR_SHORT_NAME),
			new[] { nameof(Models.Token.GrammarId) });

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
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {

                projectDbContext.RemoveRange(projectDbContext.TokenComposites
                    .Where(tc => tc.TokenCompositeTokenAssociations
                        .Where(tca => tca.Token!.VerseRowId == verseRowId).Any()));

                await Task.CompletedTask;
            };
    }

    public static ProjectDbContextMergeQueryAsync GetDeleteTokenComponentsByVerseRowIdQueryAsync(Guid verseRowId)
    {
        return
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {

                projectDbContext.RemoveRange(projectDbContext.TokenComponents
                    .Where(tc => tc.VerseRowId == verseRowId));

                await Task.CompletedTask;

            };
    }

    protected override async Task<Dictionary<string, object?>> HandleDeleteAsync(IModelSnapshot<Models.TokenComposite> itemToDelete, CancellationToken cancellationToken)
    {
        var resolvedWhereClause = await base.HandleDeleteAsync(itemToDelete, cancellationToken);

        // Since we did a DbConnection-style delete, detach any matching entities in 
        // ProjectDbContext (since they could otherwise be out-of-sync):
        await DetachTokenComponents(Enumerable.Empty<Guid>(), new Guid[] { (Guid)resolvedWhereClause[nameof(Models.IdentifiableEntity.Id)]! }, cancellationToken);

        return resolvedWhereClause;
    }

    /// <summary>
    /// Given a set of token locations, finds matching token ids.  Along with each returned token 
    /// id, if the token is contained within a composite that matches that composite filter arguments 
    /// (tokenized corpus id, parallel corpus id, deleted null/not null),  the composite and
    /// association id is also returned.  For token ids that are not contained within such a
    /// composite, the composite and association ids will have a default(Guid) value.  
    /// </summary>
    /// <param name="tokenLocations"></param>
    /// <param name="tokenizedCorpusId"></param>
    /// <param name="parallelCorpusId"></param>
    /// <param name="compositeDeleted"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IEnumerable<(Guid TokenCompositeId, Guid TokenCompositeTokenAssociationId, Guid TokenId, string EngineTokenId)>> FindTokensComposites(IEnumerable<string> tokenLocations, Guid tokenizedCorpusId, Guid? parallelCorpusId, bool compositeDeleted, CancellationToken cancellationToken)
    {
        var results = new List<(Guid TokenCompositeId, Guid TokenCompositeTokenAssociationId, Guid TokenId, string EngineTokenId)>();

        await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
            $"Find token ids, token composite ids from token locations",
            async (DbConnection dbConnection, IModel metadataModel, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                await using var command = dbConnection.CreateCommand();
                command.CommandType = CommandType.Text;

                var entityType = metadataModel.ToEntityType(typeof(Models.TokenComposite));

				var whereClause = new List<(IProperty PropertyInfo, object? ParameterValue)>
				{
                    { (entityType.ToProperty(nameof(Models.TokenComposite.ParallelCorpusId)), parallelCorpusId) },
                    { (entityType.ToProperty(nameof(Models.TokenComposite.Deleted)), null) },
                    { (entityType.ToProperty(nameof(Models.TokenComposite.TokenizedCorpusId)), tokenizedCorpusId) },
                    { (entityType.ToProperty(nameof(Models.TokenComposite.EngineTokenId)), tokenLocations) },
                    { (entityType.ToProperty(TokenHandler.DISCRIMINATOR_COLUMN_NAME), TokenHandler.DISCRIMINATOR_COLUMN_VALUE) }
                };

                var whereEngineTokenIds = DataUtil.BuildWhereInParameterString(
                    command,
                    nameof(Models.Token.EngineTokenId),
                    tokenLocations.Count());

                string andWhereParallelCorpusId = (parallelCorpusId is not null)
                    ? "tc.ParallelCorpusId = @ParallelCorpusId"
                    : "tc.ParallelCorpusId IS @ParallelCorpusId";

                string andWhereCompositeDeleted = compositeDeleted 
                    ? "tc.Deleted IS NOT @Deleted"  // I.e. tcDeleted IS NOT NULL
                    : "tc.Deleted IS @Deleted";     // I.e. tcDeleted IS NULL

                command.CommandText =
                    $@"
                        SELECT 
                              COALESCE(t2.Id, t1.Id) as TokenId
                            , COALESCE(t2.EngineTokenId, t1.EngineTokenId) as EngineTokenId
                            , tc.Id as TokenCompositeId
                        --  , tc.ParallelCorpusId
                            , CASE WHEN tc.Id IS NOT NULL
                                  THEN ta2.Id
                                  ELSE null
                              END AS TokenCompositeTokenAssociationId
                        FROM TokenComponent t1
                        LEFT JOIN TokenCompositeTokenAssociation ta1 ON t1.Id = ta1.TokenId
                        LEFT JOIN TokenCompositeTokenAssociation ta2 ON ta2.TokenCompositeId = ta1.TokenCompositeId
                        LEFT JOIN TokenComponent t2 ON ta2.TokenId = t2.Id
                        LEFT JOIN TokenComponent tc ON ta2.TokenCompositeId = tc.Id AND {andWhereCompositeDeleted} AND {andWhereParallelCorpusId}
                        WHERE t1.EngineTokenId IN ({whereEngineTokenIds})
                        AND t1.TokenizedCorpusId = @TokenizedCorpusId
                        AND t1.Discriminator = @Discriminator
                        AND t1.Deleted IS NULL
                        GROUP BY tc.Id, COALESCE(t2.Id, t1.Id)
                        ORDER BY tc.Id, COALESCE(t2.EngineTokenId, t1.EngineTokenId)
                    ";

                DataUtil.AddWhereClauseParameters(
                    command,
                    new IProperty[] {
						entityType.ToProperty(nameof(Models.TokenComposite.ParallelCorpusId)),
						entityType.ToProperty(nameof(Models.TokenComposite.Deleted)),
						entityType.ToProperty(nameof(Models.TokenComposite.TokenizedCorpusId)),
						entityType.ToProperty(TokenHandler.DISCRIMINATOR_COLUMN_NAME)
                    },
                    new (IProperty propertyInfo, int count)[] { (entityType.ToProperty(nameof(Models.TokenComposite.EngineTokenId)), tokenLocations.Count()) });

                command.Prepare();

                DataUtil.AddWhereClauseParameterValues(command, whereClause);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                results = DataUtil.ReadSelectDbDataReader(reader)
                    .Select(e => (
                        TokenCompositeId: Guid.TryParse((string?)e[nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId)], out var tc) ? tc : default,
                        TokenCompositeTokenAssociationId: Guid.TryParse((string?)e[nameof(Models.TokenCompositeTokenAssociation)+nameof(Models.TokenCompositeTokenAssociation.Id)], out var tca) ? tca : default,
                        TokenId: Guid.Parse((string)e[nameof(Models.TokenCompositeTokenAssociation.TokenId)]!),
                        EngineTokenId: (string)e[nameof(Models.Token.EngineTokenId)]!
                    ))
                    .ToList();
            },
            cancellationToken
        );

        return results;
    }

    protected override async Task<Guid> HandleCreateAsync(IModelSnapshot<Models.TokenComposite> itemToCreate, CancellationToken cancellationToken)
    {
        // Delete existing composites that reference any of the token locations in itemToCreate.
        // Note that if both source and target systems had a composite with the same Id, it would
        // not be considered a create.  Instead HandleModifyAsync would be used.

        var tokenizedCorpusId = (Guid)itemToCreate.PropertyValues[nameof(Models.TokenComposite.TokenizedCorpusId)]!;
        var parallelCorpusId = (Guid?)itemToCreate.PropertyValues[nameof(Models.TokenComposite.ParallelCorpusId)];
        var tokenLocations = (IEnumerable<string>)itemToCreate.PropertyValues[TokenCompositeBuilder.TOKEN_LOCATIONS]!;
        var hasDeleted = itemToCreate.PropertyValues.TryGetValue(nameof(Models.TokenComposite.Deleted), out var tokenDeleted);

        var tokensComposites = await FindTokensComposites(
            tokenLocations,
            tokenizedCorpusId,
            parallelCorpusId,
            hasDeleted,
            cancellationToken
        );

        var notFound = tokenLocations.Except(tokensComposites.Select(e => e.EngineTokenId));
        if (notFound.Any())
        {
            throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", notFound)}");
        }

        // Existing composites attached to incoming token locations:
        var existingTokenCompositeIds = tokensComposites
            .Where(e => tokenLocations.Contains(e.EngineTokenId))
            .Where(e => e.TokenCompositeId != default)
            .Select(e => e.TokenCompositeId)
            .Distinct()
            .ToList();

        if (existingTokenCompositeIds.Any())
        {
            await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
                $"Delete any existing composites that reference TokenLocations",
                async (DbConnection dbConnection, IModel metadataModel, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
                {
                    await DeleteComposites(dbConnection, metadataModel, existingTokenCompositeIds, cancellationToken);
                },
                cancellationToken
            );

            await DetachTokenComponents(Enumerable.Empty<Guid>(), existingTokenCompositeIds, cancellationToken);
        }

        // Not only do we need to create the TokenComposite record itself,
        // but we also need to find all TokenLocation tokens and set their
        // TokenCompositeId.

        var id = await base.HandleCreateAsync(itemToCreate, cancellationToken);

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"In HandleCreateAsync associating new TokenComposite with child Tokens",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                // Add the tokens to the composite:
                var tokenAssocationsToCreate = tokensComposites
                    .Where(e => tokenLocations.Contains(e.EngineTokenId))
                    .Select(e => TokenCompositeBuilder.BuildModelSnapshot(
                        new Models.TokenCompositeTokenAssociation
                        {
                            Id = Guid.NewGuid(),
                            TokenId = e.TokenId,
                            TokenCompositeId = id
                        }))
                    .ToList();

                if (tokenAssocationsToCreate.Any())
                {
                    _mergeContext.MergeBehavior.StartInsertModelCommand(tokenAssocationsToCreate.First());
                    foreach (var modelSnapshot in tokenAssocationsToCreate)
                    {
                        _ = await _mergeContext.MergeBehavior.RunInsertModelCommand(modelSnapshot, cancellationToken);
                    }
                    _mergeContext.MergeBehavior.CompleteInsertModelCommand(tokenAssocationsToCreate.First().EntityType);
                }
            },
            cancellationToken);

        return id;
    }

    public override async Task<(bool, Dictionary<string, object?>?)> HandleModifyPropertiesAsync(IModelDifference<IModelSnapshot<Models.TokenComposite>> modelDifference, IModelSnapshot<Models.TokenComposite> itemToModify, CancellationToken cancellationToken = default)
    {
        var (modified, where) = await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        var tokenLocationDifference = modelDifference.PropertyDifferences
            .Where(pd => pd.PropertyName == TokenCompositeBuilder.TOKEN_LOCATIONS)
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ListDifference<string>)))
            .FirstOrDefault();
        if (tokenLocationDifference is null)
        {
            return (modified, where);
        }

        var valueListDifference = (ListDifference<string>)tokenLocationDifference.PropertyValueDifference;
        var tokenLocationsOnlyIn1 = valueListDifference.ListMembershipDifference.OnlyIn1;
        var tokenLocationsOnlyIn2 = valueListDifference.ListMembershipDifference.OnlyIn2;

        var tokenCompositeId = (Guid)itemToModify.GetId();
        var tokenizedCorpusId = (Guid)itemToModify.PropertyValues[nameof(Models.TokenComposite.TokenizedCorpusId)]!;
        var parallelCorpusId = (Guid?)itemToModify.PropertyValues[nameof(Models.TokenComposite.ParallelCorpusId)];
        var tokenLocations = (IEnumerable<string>)itemToModify.PropertyValues[TokenCompositeBuilder.TOKEN_LOCATIONS]!;
        var hasDeleted = itemToModify.PropertyValues.TryGetValue(nameof(Models.TokenComposite.Deleted), out var tokenDeleted);

        var tokensComposites = await FindTokensComposites(
            tokenLocationsOnlyIn1.Union(tokenLocationsOnlyIn2),
            tokenizedCorpusId,
            parallelCorpusId,
            hasDeleted,
            cancellationToken
        );

        var notFound = tokenLocationsOnlyIn2.Except(tokensComposites.Select(e => e.EngineTokenId));
        if (notFound.Any())
        {
            throw new PropertyResolutionException($"Tokenized corpus {tokenizedCorpusId} does not contain token locations: {string.Join(", ", notFound)}");
        }

        await _mergeContext.MergeBehavior.RunDbConnectionQueryAsync(
            $"Applying TokenComposite 'TokenLocations' property ListMembershipDifference (OnlyIn1: {string.Join(", ", tokenLocationsOnlyIn1)}, OnlyIn2: {string.Join(", ", tokenLocationsOnlyIn2)})",
            async (DbConnection dbConnection, IModel metadataModel, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) =>
            {
                var tcaIdsToRemove = new List<Guid>();
                var tcIdsToRemove = new List<Guid>();
                var tcaIdsToAdd = new List<(Guid TokenId, Guid TokenCompositeId)>();

                foreach (var (TokenCompositeId, TokenCompositeTokenAssociationId, TokenId, EngineTokenId) in tokensComposites)
                {
                    if (tokenLocationsOnlyIn1.Contains(EngineTokenId) && tokenCompositeId == TokenCompositeId)
                    {
                        tcaIdsToRemove.Add(TokenCompositeTokenAssociationId);
                    }

                    if (tokenLocationsOnlyIn2.Contains(EngineTokenId))
                    {
                        if (TokenCompositeId != default)
                        {
                            tcIdsToRemove.Add(TokenCompositeId);
                        }
                        
                        tcaIdsToAdd.Add((TokenId, TokenCompositeId));
                    }
                }

                var entityType = metadataModel.ToEntityType(typeof(Models.TokenCompositeTokenAssociation));
                foreach (var tcaId in tcaIdsToRemove)
                {
                    await DataUtil.DeleteIdentifiableEntityAsync(dbConnection, entityType, new Guid[] { tcaId }, cancellationToken);
                }
                foreach (var tcId in tcIdsToRemove)
                {
                    await DataUtil.DeleteIdentifiableEntityAsync(dbConnection, entityType, new Guid[] { tcId }, cancellationToken);
                }

                if (tcaIdsToAdd.Any())
                {
                    var tokenAssocationsToAdd = tcaIdsToAdd
                        .Select(e => TokenCompositeBuilder.BuildModelSnapshot(
                            new Models.TokenCompositeTokenAssociation
                            {
                                TokenId = e.TokenId,
                                TokenCompositeId = e.TokenCompositeId
                            }))
                        .ToList();

                    _mergeContext.MergeBehavior.StartInsertModelCommand(tokenAssocationsToAdd.First());
                    foreach (var modelSnapshot in tokenAssocationsToAdd)
                    {
                        _ = await _mergeContext.MergeBehavior.RunInsertModelCommand(modelSnapshot, cancellationToken);
                    }
                    _mergeContext.MergeBehavior.CompleteInsertModelCommand(tokenAssocationsToAdd.First().EntityType);
                }

                await Task.CompletedTask;
            },
            cancellationToken);

        // Since we did a DbConnection-style modify, detach any matching entities in 
        // ProjectDbContext (since they could otherwise be out-of-sync):
        await DetachTokenComponents(Enumerable.Empty<Guid>(), new Guid[] { tokenCompositeId }, cancellationToken);

        // I can't think of a scenario where there are multiple composites having the same token locations
        // (say one deleted and one not, or two completely overlapping composites).  So leaving this commented:
//        AddTokenDeleteChangeToCache(modelDifference, itemToModify, _mergeContext.MergeBehavior.MergeCache);

        return (modified, where);
    }

    public Expression<Func<Models.TokenComposite, bool>> BuildTokenChildWhereExpression(IModelSnapshot<Models.TokenComposite> snapshot, Guid tokenizedCorpusId, IEnumerable<string> tokenLocations)
    {
        return e => e.GetType() == typeof(Models.Token) &&
                    e.TokenizedCorpusId == tokenizedCorpusId &&
                    tokenLocations.Contains(e.EngineTokenId);
    }
}

