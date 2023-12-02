using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class LabelHandler : DefaultMergeHandler<IModelSnapshot<Models.Label>>
{
    public static readonly Func<string, ProjectDbContext, ILogger, Task<Guid>> LabelTextToId = async (labelText, projectDbContext, logger) =>
    {
        var labelId = await projectDbContext.Labels
            .Where(e => e.Text! == labelText)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Label Text ('{labelText}') to LabelId ('{labelId}')");
        return labelId;
    };

    public static readonly Func<string, Guid, ProjectDbContext, ILogger, Task<Guid>> LabelNoteAssociationToId = async (labelText, noteId, projectDbContext, logger) =>
    {
        var labelNoteAssociationId = await projectDbContext.LabelNoteAssociations
            .Include(e => e.Label)
            .Where(e => e.Label!.Text! == labelText)
            .Where(e => e.NoteId == noteId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Label Text ('{labelText}') / NoteId ('{noteId}') to Id ('{labelNoteAssociationId}')");
        return labelNoteAssociationId;
    };

    public LabelHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Label), nameof(Models.Label.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.TryGetStringPropertyValue(nameof(Models.Label.Text), out var labelText))
                {
                    var labelId = await LabelTextToId(labelText, projectDbContext, logger);
                    return (labelId != default) ? labelId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Label snapshot does not have Text property value, which is required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelNoteAssociation), nameof(Models.LabelNoteAssociation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelRefName = LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX);
                if (modelSnapshot.TryGetStringPropertyValue(labelRefName, out var labelText) &&
                    modelSnapshot.TryGetGuidPropertyValue(nameof(Models.LabelNoteAssociation.NoteId), out var noteId))
                {
                    var labelNoteAssociationId = await LabelNoteAssociationToId(labelText, noteId, projectDbContext, logger);
                    return (labelNoteAssociationId != default) ? labelNoteAssociationId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"LabelNoteAssociation snapshot does not have both Text and NoteId property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.LabelNoteAssociation), nameof(Models.LabelNoteAssociation.LabelId)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var labelRefName = LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX);
                if (modelSnapshot.TryGetStringPropertyValue(labelRefName, out var labelText))
                {
                    var labelId = await LabelTextToId(labelText, projectDbContext, logger);
                    return (labelId != default) ? labelId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"LabelNoteAssociation snapshot does not have Text property value, which is required for LabelId resolution.");
                }

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Label), LabelBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Label.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Label), LabelBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Label.Id) });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.LabelNoteAssociation), LabelBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelNoteAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelNoteAssociation), LabelBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.LabelNoteAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.LabelNoteAssociation), LabelBuilder.BuildPropertyRefName(LabelBuilder.LABEL_REF_PREFIX)),
            new[] { nameof(Models.LabelNoteAssociation.LabelId) });
    }

}