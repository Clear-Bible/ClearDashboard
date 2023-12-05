using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Collaboration.Merge;

public class SemanticDomainHandler : DefaultMergeHandler<IModelSnapshot<Models.Lexicon_SemanticDomain>>
{
    public static readonly Func<string, ProjectDbContext, ILogger, Task<Guid>> ValuesToSemanticDomainId = async (semanticDomainText, projectDbContext, logger) =>
    {
        var semanticDomainId = await projectDbContext.Lexicon_SemanticDomains
            .Where(e => e.Text! == semanticDomainText)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Label Text ('{semanticDomainText}') to SemanticDomainId ('{semanticDomainId}')");
        return semanticDomainId;
    };

    public static readonly Func<string, Guid, ProjectDbContext, ILogger, Task<Guid>> ValuesToSemanticDomainMeaningAssociationId = async (semanticDomainText, meaningId, projectDbContext, logger) =>
    {
        var semanticDomainMeaningAssociationId = await projectDbContext.Lexicon_SemanticDomainMeaningAssociations
            .Include(e => e.SemanticDomain)
            .Where(e => e.SemanticDomain!.Text! == semanticDomainText)
            .Where(e => e.Meaning!.Id == meaningId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Semantic Domain Text ('{semanticDomainText}') / Meaning Id ('{meaningId}') to association Id ('{semanticDomainMeaningAssociationId}')");
        return semanticDomainMeaningAssociationId;
    };

    public SemanticDomainHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_SemanticDomain), nameof(Models.Lexicon_SemanticDomain.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var semanticDomainText = LexiconHandler.ExtractStringProperty(modelSnapshot, nameof(Models.Lexicon_SemanticDomain.Text));
                var semanticDomainId = await ValuesToSemanticDomainId(semanticDomainText, projectDbContext, logger);
                return (semanticDomainId != default) ? semanticDomainId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexiconHandler.LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var meaningId = await LexiconHandler.MeaningRefLexemeIdToMeaningId(modelSnapshot, lexemeId, projectDbContext, cache, logger);
                if (meaningId == default)
                    return null;

                var semanticDomainRefName = SemanticDomainBuilder.BuildPropertyRefName(SemanticDomainBuilder.SEMANTIC_DOMAIN_REF_PREFIX);
                var semanticDomainRef = LexiconHandler.ExtractStringProperty(modelSnapshot, semanticDomainRefName);
                var semanticDomainText = SemanticDomainBuilder.DecodeSemanticDomainRef(semanticDomainRef);

                var associationId = await ValuesToSemanticDomainMeaningAssociationId(semanticDomainText, meaningId, projectDbContext, logger);
                return (associationId != default) ? associationId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), nameof(Models.Lexicon_SemanticDomainMeaningAssociation.SemanticDomainId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var semanticDomainRefName = SemanticDomainBuilder.BuildPropertyRefName(SemanticDomainBuilder.SEMANTIC_DOMAIN_REF_PREFIX);
                var semanticDomainRef = LexiconHandler.ExtractStringProperty(modelSnapshot, semanticDomainRefName);
                var semanticDomainText = SemanticDomainBuilder.DecodeSemanticDomainRef(semanticDomainRef);

                var semanticDomainId = await ValuesToSemanticDomainId(semanticDomainText, projectDbContext, logger);
                return (semanticDomainId != default) ? semanticDomainId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), nameof(Models.Lexicon_SemanticDomainMeaningAssociation.MeaningId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var lexemeId = await LexiconHandler.LexemeRefToLexemeId(modelSnapshot, projectDbContext, cache, logger);
                if (lexemeId == default)
                    return null;

                var meaningId = await LexiconHandler.MeaningRefLexemeIdToMeaningId(modelSnapshot, lexemeId, projectDbContext, cache, logger);
                return (meaningId != default) ? meaningId : null;

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomain), SemanticDomainBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_SemanticDomain.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomain), SemanticDomainBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_SemanticDomain.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), SemanticDomainBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), SemanticDomainBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Lexicon_SemanticDomainMeaningAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), LabelBuilder.BuildPropertyRefName(SemanticDomainBuilder.SEMANTIC_DOMAIN_REF_PREFIX)),
            new[] { nameof(Models.Lexicon_SemanticDomainMeaningAssociation.SemanticDomainId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), LabelBuilder.BuildPropertyRefName(LexiconBuilder.MEANING_REF_PREFIX)),
            new[] { nameof(Models.Lexicon_SemanticDomainMeaningAssociation.MeaningId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Lexicon_SemanticDomainMeaningAssociation), LabelBuilder.BuildPropertyRefName(LexiconBuilder.LEXEME_REF_PREFIX)),
            Enumerable.Empty<string>());
    }

}