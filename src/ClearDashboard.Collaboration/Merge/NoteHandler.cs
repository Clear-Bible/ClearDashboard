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

public class NoteHandler : DefaultMergeHandler<IModelSnapshot<Models.Note>>
{
    public static readonly Func<Guid, Guid, ProjectDbContext, ILogger, Task<Guid>> NoteIdUserIdToNoteSeenAssociationId = async (noteId, userId, projectDbContext, logger) =>
    {
        var noteUserSeenAssociationId = await projectDbContext.NoteUserSeenAssociations
            .Where(e => e.NoteId == noteId)
            .Where(e => e.UserId == userId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted NoteUserSeenAssociation NoteId ('{noteId}') / UserId ('{userId}') to Id ('{noteUserSeenAssociationId}')");
        return noteUserSeenAssociationId;
    };

    public NoteHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.NoteUserSeenAssociation), nameof(Models.NoteUserSeenAssociation.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.TryGetGuidPropertyValue(nameof(Models.NoteUserSeenAssociation.NoteId), out var noteId) &&
                    modelSnapshot.TryGetGuidPropertyValue(nameof(Models.NoteUserSeenAssociation.UserId), out var userId))
                {
                    var noteUserSeenId = await NoteIdUserIdToNoteSeenAssociationId(noteId, userId, projectDbContext, logger);
                    return (noteUserSeenId != default) ? noteUserSeenId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"UserSeen snapshot does not have both NoteId and UserId property values, which are required for Id resolution.");
                }

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.NoteUserSeenAssociation), NoteBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.NoteUserSeenAssociation.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.NoteUserSeenAssociation), NoteBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.NoteUserSeenAssociation.Id) });
    }
}