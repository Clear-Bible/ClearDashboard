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
    public const string NOTEUSERSEENASSOCIATION_REF_PREFIX = "UserSeen";

    private Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>>? _ndaByNoteId = null;
    private Dictionary<Guid, IEnumerable<Models.Note>>? _repliesByThreadId = null;
    private Dictionary<Guid, IEnumerable<Models.NoteUserSeenAssociation>>? _nusaDbModelsByNoteId = null;

    public Func<ProjectDbContext, IEnumerable<Models.Note>> GetNotes = (projectDbContext) =>
    {
        return projectDbContext.Notes.Where(n => n.ThreadId == null || n.ThreadId == n.Id).OrderBy(n => n.Created).ToList();
    };

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.Note>>> GetRepliesByThreadId = (projectDbContext) =>
    {
        return projectDbContext.Notes
            .Where(n => n.ThreadId != null && n.ThreadId != n.Id)
            .GroupBy(n => (Guid)n.ThreadId!)
            .ToDictionary(g => g.Key, g => g.Select(n => n).OrderBy(n => n.Created).AsEnumerable());
    };

    public Func<ProjectDbContext, Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>>> GetNoteDomainEntityAssociationsByNoteId = (projectDbContext) =>
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
    };

    public Func<ProjectDbContext, Dictionary<Guid, IEnumerable<Models.NoteUserSeenAssociation>>> GetNoteUserSeenAssociationsByNoteId = (projectDbContext) =>
    {
        return projectDbContext.NoteUserSeenAssociations
            .ToList()
            .GroupBy(e => e.NoteId)
            .ToDictionary(g => g.Key, g => g.Select(e => e));
    };

    public override IEnumerable<GeneralModel<Models.Note>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshot = new GeneralListModel<GeneralModel<Models.Note>>();

        var notes = GetNotes(builderContext.ProjectDbContext);
        var ndaByNoteId = GetNoteDomainEntityAssociationsByNoteId(builderContext.ProjectDbContext);
        var repliesByThreadId = GetRepliesByThreadId(builderContext.ProjectDbContext);
        var nusaDbModelsByNoteId = GetNoteUserSeenAssociationsByNoteId(builderContext.ProjectDbContext);

        GetNotes(builderContext.ProjectDbContext).ToList().ForEach(n =>
        {
            modelSnapshot.Add(BuildModelSnapshotInternal(n, builderContext, ndaByNoteId, repliesByThreadId, nusaDbModelsByNoteId));
        });

        return modelSnapshot;
    }

    public GeneralModel<Models.Note> BuildModelSnapshot(Models.Note note, BuilderContext builderContext)
    {
        if (_ndaByNoteId is null) _ndaByNoteId = GetNoteDomainEntityAssociationsByNoteId(builderContext.ProjectDbContext);
        if (_repliesByThreadId is null) _repliesByThreadId = GetRepliesByThreadId(builderContext.ProjectDbContext);
        if (_nusaDbModelsByNoteId is null) _nusaDbModelsByNoteId = GetNoteUserSeenAssociationsByNoteId(builderContext.ProjectDbContext);

        return BuildModelSnapshotInternal(note, builderContext, _ndaByNoteId, _repliesByThreadId, _nusaDbModelsByNoteId);
    }

    private GeneralModel<Models.Note> BuildModelSnapshotInternal(
        Models.Note note,
        BuilderContext builderContext,
        Dictionary<Guid, Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>> ndaByNoteId,
        Dictionary<Guid, IEnumerable<Models.Note>> repliesByThreadId,
        Dictionary<Guid, IEnumerable<Models.NoteUserSeenAssociation>> nusaDbModelsByNoteId)
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

                if (nusaDbModelsByNoteId.TryGetValue(reply.Id, out var rusas))
                {
                    modelSnapshot.AddChild<IModelSnapshot<Models.NoteUserSeenAssociation>>("NoteUserSeenAssociations", ExtractNoteUserSeenAssociations(rusas, builderContext));
                }

                replyModelSnapshots.Add(replyModelSnapshot);
            }

            modelSnapshot.AddChild("Replies", replyModelSnapshots.AsModelSnapshotChildrenList());
        }

        if (nusaDbModelsByNoteId.TryGetValue(note.Id, out var nusas))
        {
            modelSnapshot.AddChild<IModelSnapshot<Models.NoteUserSeenAssociation>>("NoteUserSeenAssociations", ExtractNoteUserSeenAssociations(nusas, builderContext));
        }

        return modelSnapshot;
    }

    public Func<Dictionary<string, IEnumerable<Models.NoteDomainEntityAssociation>>, BuilderContext, IEnumerable<NoteModelRef>> ExtractNoteModelRefs = (nda, builderContext) =>
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
    };

    public static string GetNoteSeenAssociationRef(Guid noteId, Guid userId) => HashPartsToRef(NOTEUSERSEENASSOCIATION_REF_PREFIX, noteId.ToString(), userId.ToString());

    public Func<IEnumerable<Models.NoteUserSeenAssociation>, BuilderContext, IEnumerable<GeneralModel<Models.NoteUserSeenAssociation>>> ExtractNoteUserSeenAssociations = (nusas, builderContext) =>
    {
        var noteUserSeenAssociationModelSnapshots =
            new GeneralListModel<GeneralModel<Models.NoteUserSeenAssociation>>();

        foreach (var nusa in nusas)
        {
            var nusaSnapshot = BuildRefModelSnapshot(
                nusa,
                GetNoteSeenAssociationRef(nusa.NoteId, nusa.UserId),
                null,
                null,
                builderContext);

            noteUserSeenAssociationModelSnapshots.Add(nusaSnapshot);
        }

        return noteUserSeenAssociationModelSnapshots;
    };
}
