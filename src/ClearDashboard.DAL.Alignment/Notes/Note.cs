using System.Collections.ObjectModel;
using System.Linq;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Notes;
using MediatR;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public class Note
    {
        public NoteId? NoteId 
        { 
            get;
#if DEBUG
            set;
#else
            private set;
#endif
        }
        public string? Text { get; set; }
        public string? AbbreviatedText { get; set; }

        private Models.NoteStatus noteStatus_;
        public string NoteStatus
        {
            get { return noteStatus_.ToString(); }
            set
            {
                if (!Enum.TryParse(value, out noteStatus_))
                {
                    throw new ArgumentException(
                        $"Invalid NoteStatus '{value}'.  Must be one of: " + 
                        $"{string.Join(", ", ((Models.NoteStatus[])Enum.GetValues(typeof(Models.NoteStatus))).Select(ns => $"'{ns}'"))}");
                }
            }
        }
        public EntityId<NoteId>? ThreadId { get; private set; }

#if DEBUG
        private ObservableCollection<Label> labels_;
#else
        private readonly ObservableCollection<Label> labels_;
#endif

        public ObservableCollection<Label> Labels
        {
            get { return labels_; }
#if DEBUG
            set { labels_ = value; }
#endif
        }

        private readonly ICollection<IId> domainEntityIds_;
        public IEnumerable<IId> DomainEntityIds { get { return domainEntityIds_; } }

        public Note(Note? noteInThread = null)
        {
            if (noteInThread is not null)
            {
                if (noteInThread.NoteId is null)
                {
                    throw new MediatorErrorEngineException("'CreateOrUpdate NoteInThread before passing to Note constructor");
                }
                ThreadId = noteInThread.ThreadId ?? new EntityId<NoteId>() { Id = noteInThread.NoteId.Id };
            }

            noteStatus_ = Models.NoteStatus.Open;
            labels_ = new ObservableCollection<Label>();
            domainEntityIds_ = new HashSet<IId>(new IIdEquatableComparer());
        }
        internal Note(NoteId noteId, string text, string? abbreviatedText, Models.NoteStatus noteStatus, EntityId<NoteId>? threadId, ICollection<Label> labels, ICollection<IId> domainEntityIds)
        {
            NoteId = noteId;
            Text = text;
            AbbreviatedText = abbreviatedText;
            ThreadId = threadId;
            noteStatus_ = noteStatus;
            labels_ = new ObservableCollection<Label>(labels.DistinctBy(l => l.LabelId)); ;
            domainEntityIds_ = new HashSet<IId>(domainEntityIds, new IIdEquatableComparer());
        }

        public bool IsReply()
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before calling IsReply");
            }

            return ThreadId is not null && !NoteId.IdEquals(ThreadId);
        }

        public IEnumerable<string> GetAttachmentContentTypes()
        {
            // No domain model here yet - just a placeholder.  Thinking
            // that a note won't carry attachments/content in properties, 
            // it'll require a query handler call to retrieve
            return new List<string>();
        }

        public async Task<Note> CreateOrUpdate(IMediator mediator, CancellationToken token = default)
        {            
            var command = new CreateOrUpdateNoteCommand(NoteId, Text ?? string.Empty, AbbreviatedText, noteStatus_, ThreadId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                NoteId = result.Data!;
                return this;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task<Label> CreateAssociateLabel(IMediator mediator, string labelText, CancellationToken token = default)
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before associating with Label");
            }

            var label = await new Label { Text = labelText }.CreateOrUpdate(mediator, token);
            await AssociateLabel(mediator, label, token);

            return label;
        }

        public async Task AssociateLabel(IMediator mediator, Label label, CancellationToken token = default)
        {
            if (labels_.Any(l => l.LabelId == label.LabelId))
            {
                return;
            }

            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("Create Note before associating with given Label");
            }
            if (label.LabelId is null)
            {
                throw new MediatorErrorEngineException("Create given Label before associating with Note");
            }

            var command = new CreateLabelNoteAssociationCommand(label.LabelId, NoteId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                labels_.Add(label);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task DetachLabel(IMediator mediator, Label label, CancellationToken token = default)
        {
            var labelMatch = labels_.FirstOrDefault(l => l.LabelId == label.LabelId);
            if (labelMatch is null)
            {
                return;
            }

            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before Note has been created");
            }
            if (label.LabelId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before it has been created/attached");
            }

            var command = new DeleteLabelNoteAssociationCommand(label.LabelId, NoteId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                labels_.Remove(labelMatch);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task AssociateDomainEntity(IMediator mediator, IId domainEntityId, CancellationToken token = default)
        {
            if (domainEntityIds_.Contains(domainEntityId))
            {
                return;
            }

            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before associating with domain entity id");
            }

            var command = new CreateNoteDomainEntityAssociationCommand(NoteId, domainEntityId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                domainEntityIds_.Add(domainEntityId);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task DetachDomainEntity(IMediator mediator, IId domainEntityId, CancellationToken token = default)
        {
            if (!domainEntityIds_.Contains(domainEntityId))
            {
                return;
            }

            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach domain entity id before Note has been created");
            }

            var command = new DeleteNoteDomainEntityAssociationCommand(NoteId, domainEntityId);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                domainEntityIds_.Remove(domainEntityId);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (NoteId == null)
            {
                return;
            }

            var command = new DeleteNoteAndAssociationsByNoteIdCommand(NoteId);

            var result = await mediator.Send(command, token);
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<Note> Get(
            IMediator mediator,
            NoteId noteId)
        {
            var command = new GetNoteByNoteIdQuery(noteId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IEnumerable<Note>> GetAllNotes(
            IMediator mediator)
        {
            var command = new GetAllNotesQuery();

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<IOrderedEnumerable<Note>> GetNotesInThread(EntityId<NoteId> threadId,
            IMediator mediator)
        {
            var command = new GetNotesInThreadQuery(threadId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                return result.Data!;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public static async Task<Dictionary<IId, IEnumerable<Note>>> GetAllDomainEntityIdNotes(
            IMediator mediator)
        {
            var command = new GetAllNotesQuery();

            var result = await mediator.Send(command);
            if (result.Success)
            {
                // result.Data is in the form:  IEnumerable<Note>
                // This linq uses SelectMany to extract out note / domain entity id pairs,
                // groups them by domain entity id (using an IIdEquatable comparer) and writes
                // the resulting IIdEquatable, INumerable<Note> groups to a dictionary:
                var idNotes = result.Data!
                    .SelectMany(note => note.DomainEntityIds, (note, iid) => new { iid, note })
                    .GroupBy(pair => pair.iid, new IIdEquatableComparer())
                    .ToDictionary(g => (g.Key as IId)!, g => g.Select(g => g.note));

                // result.Data is in the form:  Dictionary<Note, IdEquatableCollection>
                // This linq reverses that, grouping the appropriate Note references
                // under their associated IIdEquatable(s):
                //var idNotes = result.Data!
                //    .SelectMany(nd => nd.Value, (nd, iid) => new { iid, note = nd.Key })
                //    .GroupBy(pair => pair.iid)
                //    .ToDictionary(g => g.Key, g => g.Select(g => g.note) /*.ToList() as ICollection<Note>*/);
                return idNotes;
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
