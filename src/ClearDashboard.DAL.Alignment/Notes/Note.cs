using System.Collections.ObjectModel;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
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
            // RELEASE MODIFIED
            //private set;
            set;
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

        /// <summary>
        /// Contains the EntityId<NoteId> of the leading note of a note thread 
        /// where there are one or more reply Notes.  For a standalone Note
        /// having no replies, this will be null.  
        /// </summary>
        public EntityId<NoteId>? ThreadId { get; internal set; }

#if DEBUG
        private ObservableCollection<Label> labels_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Label> labels_;
        private ObservableCollection<Label> labels_;
#endif

        public ObservableCollection<Label> Labels
        {
            get { return labels_; }
#if DEBUG
            set { labels_ = value; }
#else
            // RELEASE MODIFIED
            //set { labels_ = value; }
            set { labels_ = value; }
#endif
        }

        private readonly ICollection<IId> domainEntityIds_;
        public IEnumerable<IId> DomainEntityIds { get { return domainEntityIds_; } }

        public ICollection<Guid> SeenByUserIds { get; private set; }

        public Note(Note? noteInThread = null)
        {
            if (noteInThread is not null)
            {
                if (noteInThread.NoteId is null)
                {
                    throw new MediatorErrorEngineException("'CreateOrUpdate noteInThread argument before passing to Note constructor");
                }
                if (noteInThread.ThreadId is null)
                {
                    ThreadId = new EntityId<NoteId>() { Id = noteInThread.NoteId.Id };
                    noteInThread.ThreadId = ThreadId;
                }
                else
                {
                    ThreadId = noteInThread.ThreadId;
                }
            }

            noteStatus_ = Models.NoteStatus.Open;
            labels_ = new ObservableCollection<Label>();
            domainEntityIds_ = new HashSet<IId>(new IIdEqualityComparer());
            SeenByUserIds = new HashSet<Guid>();
        }
        internal Note(NoteId noteId, string text, string? abbreviatedText, Models.NoteStatus noteStatus, EntityId<NoteId>? threadId, ICollection<Label> labels, ICollection<IId> domainEntityIds, ICollection<Guid> seenByUserIds)
        {
            NoteId = noteId;
            Text = text;
            AbbreviatedText = abbreviatedText;
            ThreadId = threadId;
            noteStatus_ = noteStatus;
            labels_ = new ObservableCollection<Label>(labels.DistinctBy(l => l.LabelId)); ;
            domainEntityIds_ = new HashSet<IId>(domainEntityIds, new IIdEqualityComparer());
            SeenByUserIds = new HashSet<Guid>(seenByUserIds);
        }

        /// <summary>
        /// Returns true for any Note instance that is a reply of a 
        /// Note thread.  ThreadId property will be non-null if this 
        /// method returns true.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
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

        public async Task<IEnumerable<Note>> GetReplyNotes(IMediator mediator, CancellationToken token = default)
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before calling GetReplyNotes");
            }

            if (IsReply())
            {
                throw new MediatorErrorEngineException("GetReplyNotes is unsupported for a note that is itself a reply note");
            }

            return (await GetNotesInThread(mediator, ThreadId ?? NoteId!, token))
                .Where(note => note.NoteId!.Id != NoteId.Id)
                .ToList();
        }

        public async Task<Dictionary<IId, Dictionary<string, string>>> GetDomainEntityContexts(IMediator mediator, CancellationToken token = default)
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before retrieving domain entity contexts");
            }

            return await GetDomainEntityContexts(mediator, this.domainEntityIds_, token);
        }

        public async Task<IEnumerable<IId>> GetFullDomainEntityIds(IMediator mediator, CancellationToken token = default)
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before retrieving full domain entity ids");
            }

            return await GetFullDomainEntityIds(mediator, this.domainEntityIds_, token);
        }

        public async Task<Note> CreateOrUpdate(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateOrUpdateNoteCommand(NoteId, Text ?? string.Empty, AbbreviatedText, noteStatus_, ThreadId, SeenByUserIds);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            NoteId = result.Data!;
            return this;
        }

        public async Task<Label> CreateAssociateLabel(IMediator mediator, string labelText, string? templateText, CancellationToken token = default)
        {
            if (NoteId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Note before associating with Label");
            }

            var label = await new Label { Text = labelText, TemplateText = templateText }.CreateOrUpdate(mediator, token);
            await AssociateLabel(mediator, label, token);

            return label;
        }

        /// <summary>
        /// NOTE:  this method alters a the "Labels" ObservableCollection that was created in the Note constructor.  
        /// For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(note.Labels, theLockObject)), and use it in a “lock” statement every 
        /// time those methods are called.  Summarized from:
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.bindingoperations.enablecollectionsynchronization
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
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
            result.ThrowIfCanceledOrFailed();

            labels_.Add(label);
        }

        /// <summary>
        /// NOTE:  this method alters a the "Labels" ObservableCollection that was created in the Note constructor.  
        /// For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(note.Labels, theLockObject)), and use it in a “lock” statement every 
        /// time those methods are called.  Summarized from:
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.bindingoperations.enablecollectionsynchronization
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="label"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
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
            result.ThrowIfCanceledOrFailed();
                
            labels_.Remove(labelMatch);
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
            result.ThrowIfCanceledOrFailed();

            domainEntityIds_.Add(domainEntityId);
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
            result.ThrowIfCanceledOrFailed();

            domainEntityIds_.Remove(domainEntityId);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (NoteId == null)
            {
                return;
            }

            var command = new DeleteNoteAndAssociationsByNoteIdCommand(NoteId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<Note> Get(
            IMediator mediator,
            NoteId noteId,
            CancellationToken token = default)
        {
            var command = new GetNoteByNoteIdQuery(noteId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public static async Task<IEnumerable<Note>> GetAllNotes(
            IMediator mediator,
            CancellationToken token = default)
        {
            var command = new GetNotesByDomainEntityIIdsQuery(null);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        public static async Task<IOrderedEnumerable<Note>> GetNotesInThread(
            IMediator mediator,
            EntityId<NoteId> threadId,
            CancellationToken token = default)
        {
            var command = new GetNotesInThreadQuery(threadId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        /// <summary>
        /// If Note (per the given NoteId) is associated with:
        ///   domain entity ids representing one or more tokens (no other domain entity id type(s)),
        ///   the associated tokens are all part of the same TokenizedCorpus + Book + Chapter + Verse,
        ///   the associated tokens are contiguous within the verse,
        /// returns the Corpus ParatextGuid value.  
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="noteId"></param>
        /// <param name="token"></param>
        /// <returns>(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)?</returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task<(string paratextId, TokenizedTextCorpusId tokenizedTextCorpusId, IEnumerable<Token> verseTokens)?> GetParatextIdIfAssociatedContiguousTokensOnly(
            IMediator mediator,
            NoteId noteId,
            CancellationToken token = default)
        {
            var command = new GetParatextProjectIdForNoteIdQuery(noteId);

            var result = await mediator.Send(command, token);
            if (result.Canceled)
            {
                throw new OperationCanceledException();
            }
            else if (result.Success)
            {
                return result.Data!;
            }

            return null;
        }

        public static async Task<IEnumerable<IId>> GetFullDomainEntityIds(
            IMediator mediator,
            IEnumerable<IId> domainEntityIIds,
            CancellationToken token = default)
        {
            var command = new GetFullDomainEntityIdsByIIdsQuery(domainEntityIIds);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        /// <summary>
        /// Returns domain entity contextual information (e.g. which corpus/tokenized corpus
        /// a given Token is contained in) for display along with a Note's other details.  
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="domainEntityIIds"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<Dictionary<IId, Dictionary<string, string>>> GetDomainEntityContexts(
            IMediator mediator,
            IEnumerable<IId> domainEntityIIds,
            CancellationToken token = default)
        {
            var command = new GetDomainEntityContextsByIIdsQuery(domainEntityIIds);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }

        /// <summary>
        /// Returns all note-associated domain entity ids mapped to their 
        /// respective associated Note instances.  Does not include Notes
        /// that are only associated indirectly, i.e. only associated by
        /// being in a reply thread where the directly-associated note is
        /// the leading Note.  
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="domainEntityIIds"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<Dictionary<IId, IEnumerable<NoteId>>> GetDomainEntityNoteIds(
            IMediator mediator,
            IEnumerable<IId>? domainEntityIIds,
            CancellationToken token = default)
        {
            var command = new GetNotesByDomainEntityIIdsQuery(domainEntityIIds);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            var iidNoteGroups = result.Data!
                .Where(note => note.DomainEntityIds.Any())
                .SelectMany(note => note.DomainEntityIds, (note, iid) => new { iid, note })
                .GroupBy(pair => pair.iid, new IIdEqualityComparer());

            if (domainEntityIIds is not null && domainEntityIIds.Any())
            {
                iidNoteGroups = iidNoteGroups.Where(g => domainEntityIIds.Select(d => d.Id).Contains(g.Key.Id));
            }

            var domainEntityNoteIds = iidNoteGroups
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(iidNote => iidNote.note.NoteId!),
                    new IIdEqualityComparer()
                );

            return domainEntityNoteIds;
        }

        /// <summary>
        /// Returns all note-associated domain entity ids mapped to their 
        /// respective associated Note instances.  Note that the Note instances
        /// for a given domain entity id will include Notes directly associated
        /// (standalone notes and/or leading notes in a thread) as well as any
        /// reply notes.  
        /// 
        /// To determine which notes are directly associated to a given domain 
        /// entity id (vs replies), the caller can check which notes have
        /// Note.IsReply() == false.  To see which leading Note a reply Note
        /// is a reply to, the Note.ThreadId property can be used:  for all 
        /// notes in a thread, Note.ThreadId will contain the NoteId.Id of 
        /// the leading note.  
        /// </summary>
        /// <see cref="GetDomainEntityNotesThreadsFlattened"/>
        /// <param name="mediator"></param>
        /// <returns></returns>
        public static async Task<Dictionary<IId, IEnumerable<Note>>> GetAllDomainEntityIdNotes(
            IMediator mediator,
            CancellationToken token = default)
        {
            return await GetDomainEntityNotesThreadsFlattened(mediator, null, token);
        }

        /// <summary>
        /// Returns all note-associated domain entity ids mapped to their 
        /// respective associated Note instances.  Note that the Note instances
        /// for a given domain entity id will include Notes directly associated
        /// (standalone notes and/or leading notes in a thread) as well as any
        /// reply notes.  
        /// 
        /// To determine which notes are directly associated to a given domain 
        /// entity id (vs replies), the caller can check which notes have
        /// Note.IsReply() == false.  To see which leading Note a reply Note
        /// is a reply to, the Note.ThreadId property can be used:  for all 
        /// notes in a thread, Note.ThreadId will contain the NoteId.Id of 
        /// the leading note.  
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="domainEntityIIds">used as filter for keys of returned dictionary</param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<Dictionary<IId, IEnumerable<Note>>> GetDomainEntityNotesThreadsFlattened(
            IMediator mediator,
            IEnumerable<IId>? domainEntityIIds,
            CancellationToken token = default)
        {
            var command = new GetNotesByDomainEntityIIdsQuery(null);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            var domainEntityIdNotesThreads = ToDomainEntityIdNotesThreads(result.Data!, domainEntityIIds);

            var idNotes = domainEntityIdNotesThreads
                .ToDictionary(
                    iidNotes => iidNotes.Key,
                    iidNotes => iidNotes.Value
                        .SelectMany(kvp => kvp.Value
                            .Append(kvp.Key)
                            .OrderBy(note => note.NoteId!.Created)),
                new IIdEqualityComparer());
            // result.Data is in the form:  IEnumerable<Note>
            // This linq uses SelectMany to extract out note / domain entity id pairs,
            // groups them by domain entity id (using an IIdEquatable comparer) and writes
            // the resulting IIdEquatable, INumerable<Note> groups to a dictionary:
            //var idNotes = result.Data!
            //    .SelectMany(note => note.DomainEntityIds, (note, iid) => new { iid, note })
            //    .GroupBy(pair => pair.iid, new IIdEqualityComparer())
            //    .ToDictionary(
            //        g => g.Key!,
            //        g => g.Select(g => g.note),
            //        new IIdEqualityComparer());

            // result.Data is in the form:  Dictionary<Note, IdEquatableCollection>
            // This linq reverses that, grouping the appropriate Note references
            // under their associated IIdEquatable(s):
            //var idNotes = result.Data!
            //    .SelectMany(nd => nd.Value, (nd, iid) => new { iid, note = nd.Key })
            //    .GroupBy(pair => pair.iid)
            //    .ToDictionary(g => g.Key, g => g.Select(g => g.note) /*.ToList() as ICollection<Note>*/);
            return idNotes;
        }

        /// <summary>
        /// Returns all note-associated domain entity ids mapped to their 
        /// respective associated Note instances.  The associated Note instances
        /// are returned as a Dictionary<Note, IEnumerable<Note>>, where the
        /// Note key has a direct association to the domain entity id and any
        /// Note values (empty for standalone Notes) are replies to the respective
        /// Note key.  
        /// 
        /// Any given Note key + values pair share a common ThreadId value but 
        /// different IsReply() values:  false for the key and true for the values.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="domainEntityIIds">used as filter for keys of returned dictionary</param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<Dictionary<IId, Dictionary<Note, IEnumerable<Note>>>> GetDomainEntityNotesThreads(
            IMediator mediator,
            IEnumerable<IId>? domainEntityIIds,
            CancellationToken token = default)
        {
            var command = new GetNotesByDomainEntityIIdsQuery(null);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            var domainEntityIdNotesAndThreads = ToDomainEntityIdNotesThreads(result.Data!, domainEntityIIds);
            return domainEntityIdNotesAndThreads;
        }

        private static Dictionary<IId, Dictionary<Note, IEnumerable<Note>>> ToDomainEntityIdNotesThreads(
            IEnumerable<Note> allNotes, 
            IEnumerable<IId>? domainEntityIIds)
        {
            var noteIdNoteThreads = allNotes
                .GroupBy(keySelector: note => note.ThreadId?.Id ?? note.NoteId!.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Where(note => note.ThreadId != null && note.ThreadId!.Id != note.NoteId!.Id)
                        .OrderBy(note => note.NoteId!.Created));

            var iidNoteGroups = allNotes
                .Where(note => note.DomainEntityIds.Any())
                .SelectMany(note => note.DomainEntityIds, (note, iid) => new { iid, note })
                .GroupBy(pair => pair.iid, new IIdEqualityComparer());

            if (domainEntityIIds is not null && domainEntityIIds.Any())
            {
                iidNoteGroups = iidNoteGroups.Where(g => domainEntityIIds.Select(d => d.Id).Contains(g.Key.Id));
            }

            var domainEntityIdNotesAndThreads = iidNoteGroups
                .ToDictionary(
                    g => g.Key!,
                    g => g.ToDictionary(
                        keySelector: pair => pair.note,
                        elementSelector: pair => {
                            if (noteIdNoteThreads.TryGetValue(pair.note.NoteId!.Id, out var replyNotes))
                            {
                                return replyNotes;
                            }
                            return System.Linq.Enumerable.Empty<Note>();
                        }),
                    new IIdEqualityComparer());

            return domainEntityIdNotesAndThreads;
        }
    }
}
