using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.DAL.Alignment.Notes;

namespace ClearDashboard.Collaboration.Merge;

public class LabelGroupHandler : DefaultMergeHandler<IModelSnapshot<Models.LabelGroup>>
{
    public static readonly Func<string, ProjectDbContext, ILogger, Task<Guid>> LabelGroupNameToId = async (labelGroupName, projectDbContext, logger) =>
    {
        var labelGroupId = await projectDbContext.LabelGroups
            .Where(e => e.Name! == labelGroupName)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted LabelGroup Name ('{labelGroupName}') to LabelId ('{labelGroupId}')");
        return labelGroupId;
    };

    public static readonly Func<string, string, ProjectDbContext, ILogger, Task<Guid>> LabelGroupAssociationToId = async (labelGroupName, labelText, projectDbContext, logger) =>
    {
        var labelGroupAssociationId = await projectDbContext.LabelGroupAssociations
            .Include(e => e.Label)
            .Include(e => e.LabelGroup)
            .Where(e => e.Label!.Text == labelText)
            .Where(e => e.LabelGroup!.Name == labelGroupName)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted LabelGroup Name ('{labelGroupName}') / Label Text ('{labelText}') to Id ('{labelGroupAssociationId}')");
        return labelGroupAssociationId;
    };

    public LabelGroupHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelGroup), nameof(Models.LabelGroup.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelGroupName = LexiconHandler.ExtractStringProperty(modelSnapshot, nameof(Models.LabelGroup.Name));
                var labelGroupId = await LabelGroupNameToId(labelGroupName, projectDbContext, logger);
                return (labelGroupId != default) ? labelGroupId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelGroupAssociation), nameof(Models.LabelGroupAssociation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelGroupRefName = LabelGroupBuilder.BuildPropertyRefName(LabelGroupBuilder.LABELGROUP_REF_PREFIX);
                var labelGroupRef = LexiconHandler.ExtractStringProperty(modelSnapshot, labelGroupRefName);
                var labelGroupName = LabelGroupBuilder.DecodeLabelGroupRef(labelGroupRef);

                var labelRefName = LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX);
                var labelRef = LexiconHandler.ExtractStringProperty(modelSnapshot, labelRefName);
                var labelText = LabelBuilder.DecodeLabelRef(labelRef);

                var labelGroupAssociationId = await LabelGroupAssociationToId(labelGroupName, labelText, projectDbContext, logger);
                return (labelGroupAssociationId != Guid.Empty) ? labelGroupAssociationId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelGroupAssociation), nameof(Models.LabelGroupAssociation.LabelGroupId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelGroupRefName = LabelGroupBuilder.BuildPropertyRefName(LabelGroupBuilder.LABELGROUP_REF_PREFIX);
                var labelGroupRef = LexiconHandler.ExtractStringProperty(modelSnapshot, labelGroupRefName);
                var labelGroupName = LabelGroupBuilder.DecodeLabelGroupRef(labelGroupRef);

                var labelGroupId = await LabelGroupNameToId(labelGroupName, projectDbContext, logger);
                return (labelGroupId != default) ? labelGroupId : null;

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelGroupAssociation), nameof(Models.LabelGroupAssociation.LabelId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelRefName = LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX);
                var labelRef = LexiconHandler.ExtractStringProperty(modelSnapshot, labelRefName);
                var labelText = LabelBuilder.DecodeLabelRef(labelRef);

                var labelId = await LabelHandler.LabelTextToId(labelText, projectDbContext, logger);
                return (labelId != default) ? labelId : null;

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.LabelGroup), LabelGroupBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelGroup.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelGroup), LabelGroupBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelGroup.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.LabelGroupAssociation), LabelGroupBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelGroupAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelGroupAssociation), LabelGroupBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelGroupAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelGroupAssociation), LabelGroupBuilder.BuildPropertyRefName(LabelGroupBuilder.LABELGROUP_REF_PREFIX)),
            new[] { nameof(Models.LabelGroupAssociation.LabelGroupId) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelGroupAssociation), LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX)),
            new[] { nameof(Models.LabelGroupAssociation.LabelId) });
    }

}