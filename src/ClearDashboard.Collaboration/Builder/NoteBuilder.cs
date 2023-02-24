using System.Text.Json;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class NoteBuilder : GeneralModelBuilder<Models.Note>
{
    private readonly Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>> _ndaByNoteId;
    private readonly Dictionary<Guid, IEnumerable<Models.Note>> _repliesByThreadId;

    public NoteBuilder(ProjectDbContext projectDbContext)
    {
        _ndaByNoteId = GetNoteDomainEntityAssociationsByNoteId(projectDbContext);
        _repliesByThreadId = GetRepliesByThreadId(projectDbContext);
    }

    public GeneralModel<Models.Note> BuildModelSnapshot(Models.Note note, BuilderContext builderContext)
    {
        return BuildModelSnapshotInternal(note, builderContext, _ndaByNoteId, _repliesByThreadId);
    }

    public static IEnumerable<GeneralModel<Models.Note>> BuildModelSnapshot(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.Note>>();

        var notes = GetNotes(builderContext.ProjectDbContext);
        var ndaByNoteId = GetNoteDomainEntityAssociationsByNoteId(builderContext.ProjectDbContext);
        var repliesByThreadId = GetRepliesByThreadId(builderContext.ProjectDbContext);

        GetNotes(builderContext.ProjectDbContext).ToList().ForEach(n =>
        {
            modelSnapshot.Add(BuildModelSnapshotInternal(n, builderContext, ndaByNoteId, repliesByThreadId));
        });

        return modelSnapshot;
    }

    private static GeneralModel<Models.Note> BuildModelSnapshotInternal(
        Models.Note note,
        BuilderContext builderContext,
        Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>> ndaByNoteId,
        Dictionary<Guid, IEnumerable<Models.Note>> repliesByThreadId)
    {
        var modelSnapshot = ExtractUsingModelIds(note);

        if (ndaByNoteId.TryGetValue(note.Id, out var ndas))
        {
            modelSnapshot.AddChild<NoteModelRef>("NoteModelRefs", ExtractNoteModelRefs(ndas, builderContext));
        }

        if (repliesByThreadId.TryGetValue(note.Id, out var value))
        {
            var replyModelSnapshots = new GeneralListModel<GeneralModel<Models.Note>>();
            foreach (var reply in value)
            {
                var replyModelSnapshot = ExtractUsingModelIds(reply);
                if (ndaByNoteId.TryGetValue(reply.Id, out var rdas))
                {
                    replyModelSnapshot.AddChild<NoteModelRef>("NoteModelRefs", ExtractNoteModelRefs(rdas, builderContext));
                }

                replyModelSnapshots.Add(replyModelSnapshot);
            }

            modelSnapshot.AddChild("Replies", replyModelSnapshots.AsModelSnapshotChildrenList());
        }

        return modelSnapshot;
    }

    public static IEnumerable<NoteModelRef> ExtractNoteModelRefs(
        Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>> nda,
        BuilderContext builderContext)
    {
        var noteModelRefs = new GeneralListModel<NoteModelRef>();
        foreach (var kvp in nda)
        {
            var typeName = kvp.Key.CreateInstanceByNameAndSetId(Guid.Empty).GetType().FindEntityIdGenericType()?.Name;
            var guids = kvp.Value.Select(nd => (Guid)nd.DomainEntityIdGuid!);

            if (typeName == typeof(TokenId).Name || typeName == typeof(CompositeTokenId).Name)
            {
                var tokenComponents = builderContext.ProjectDbContext.TokenComponents.Where(e => guids.Contains(e.Id)).ToDictionary(e => e.Id, e => e);

                foreach (var nd in kvp.Value)
                {
                    var tokenComponent = tokenComponents[(Guid)nd.DomainEntityIdGuid!];

                    noteModelRefs.Add(new NoteModelRef<TokenRef>
                    (
                        nd.Id,
                        nd.NoteId,
                        TokenBuilder.BuildTokenRef(tokenComponent, builderContext)
                    ));
                }
            }
            else if (typeName == typeof(AlignmentId).Name)
            {
                var alignments = builderContext.ProjectDbContext.Alignments
                    .Include(e => e.SourceTokenComponent)
                    .Include(e => e.TargetTokenComponent)
                    .Where(e => guids.Contains(e.Id))
                    .ToDictionary(e => e.Id, e => e);

                foreach (var nd in kvp.Value)
                {
                    var alignment = alignments[(Guid)nd.DomainEntityIdGuid!];

                    noteModelRefs.Add(new NoteModelRef<AlignmentRef>
                    (
                        nd.Id,
                        nd.NoteId,
                        new AlignmentRef
                        {
                            AlignmentSetId = alignment.AlignmentSetId,
                            SourceTokenRef = TokenBuilder.BuildTokenRef(alignment.SourceTokenComponent!, builderContext),
                            TargetTokenRef = TokenBuilder.BuildTokenRef(alignment.TargetTokenComponent!, builderContext)
                        }
                    ));
                }
            }
            else if (typeName == typeof(TranslationId).Name)
            {
                var translations = builderContext.ProjectDbContext.Translations
                    .Include(e => e.SourceTokenComponent)
                    .Where(e => guids.Contains(e.Id))
                    .ToDictionary(e => e.Id, e => e);

                foreach (var nd in kvp.Value)
                {
                    var translation = translations[(Guid)nd.DomainEntityIdGuid!];

                    noteModelRefs.Add(new NoteModelRef<TranslationRef>
                    (
                        nd.Id,
                        nd.NoteId,
                        new TranslationRef
                        {
                            TranslationSetId = translation.TranslationSetId,
                            SourceTokenRef = TokenBuilder.BuildTokenRef(translation.SourceTokenComponent!, builderContext)
                        }
                    ));
                }
            }
            else
            {
                foreach (var nd in kvp.Value)
                {
                    var domainEntityId = nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!);
                    var domainEntityType = domainEntityId.GetType().GetGenericArguments()[0];
                    noteModelRefs.Add(new NoteModelRef<DomainEntityRef>
                    (

                        nd.Id,
                        nd.NoteId,
                        new DomainEntityRef
                        {
                            DomainEntityIdGuid = domainEntityId.Id,
                            DomainEntityIdName = $"{domainEntityType.FullName}, {domainEntityType.Assembly.GetName().Name}"
                        }
                    ));
                }
            }
        }

        return noteModelRefs;
    }

    public static IEnumerable<Models.Note> GetNotes(ProjectDbContext projectDbContext)
    {
        return projectDbContext.Notes.Where(n => n.ThreadId == null || n.ThreadId == n.Id).OrderBy(n => n.Created).ToList();
    }

    public static Dictionary<Guid, IEnumerable<Models.Note>> GetRepliesByThreadId(ProjectDbContext projectDbContext)
    {
        return projectDbContext.Notes
            .Where(n => n.ThreadId != null && n.ThreadId != n.Id)
            .GroupBy(n => (Guid)n.ThreadId!)
            .ToDictionary(g => g.Key, g => g.Select(n => n).OrderBy(n => n.Created).AsEnumerable());
    }

    public static Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>> GetNoteDomainEntityAssociationsByNoteId(ProjectDbContext projectDbContext)
    {
        return projectDbContext.NoteDomainEntityAssociations
            .ToList()
            .GroupBy(e => e.NoteId)
            .Select(g => new
            {
                NoteId = g.Key,
                DomainEntityTypes = g.ToList()
                    .GroupBy(gg => gg.DomainEntityIdName!)
                    .ToDictionary(gg => gg.Key, gg => gg.Select(e => e))
            })
            .ToDictionary(g => g.NoteId, g => g.DomainEntityTypes);
    }
}
