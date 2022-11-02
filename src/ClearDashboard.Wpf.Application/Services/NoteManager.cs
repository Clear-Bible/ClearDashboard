using ClearBible.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.Display;
using ClearBible.Engine.Corpora;
using MediatR;
using System.Diagnostics;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class NoteManager : PropertyChangedBase
    {
        private ILogger<NoteManager>? Logger { get; }
        private IMediator Mediator { get; }
        private AsyncLazy<EntityNoteDictionary> EntityNoteSummaries { get; }

        private async Task<EntityNoteDictionary> GetAllEntityNoteSummariesAsync()
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var notes = await Note.GetAllDomainEntityIdNotes(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved all domain entity notes in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return new EntityNoteDictionary(notes);
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private async Task<IEnumerable<Note>> GetNoteSummariesForEntity(IId entityGuid)
        {
            var allNoteSummaries = await EntityNoteSummaries;
            return allNoteSummaries.ContainsKey(entityGuid) ? allNoteSummaries[entityGuid] : new List<Note>();
        }

        // TODO: localize
        private static string GetDescriptionForNoteAssociation(IId associatedEntityId)
        {
            if (associatedEntityId is TokenId tokenId)
            {
                return $"Token in {BookLookup.GetBookAbbreviation(tokenId.BookNumber)} {tokenId.ChapterNumber}:{tokenId.VerseNumber} word {tokenId.WordNumber} part {tokenId.SubWordNumber}";
            }

            return string.Empty;
        }

        public async Task<NoteViewModelCollection> GetNoteDetailsForEntityAsync(IId entityId)
        {
            var result = new NoteViewModelCollection();

            var notesList = (await GetNoteSummariesForEntity(entityId)).OrderBy(n => n.NoteId?.Created).ToList();
            foreach (var parentNote in notesList.Where(note => !note.IsReply()))
            {
                var noteViewModel = new NoteViewModel(parentNote);
                var associatedEntityIds = await parentNote.GetFullDomainEntityIds(Mediator);
                foreach (var associatedEntityId in associatedEntityIds)
                {
                    noteViewModel.Associations.Add(new NoteAssociationViewModel
                    {
                        AssociatedEntityId = associatedEntityId,
                        Description = GetDescriptionForNoteAssociation(associatedEntityId)

                    });
                }
                result.Add(noteViewModel);
            }

            foreach (var replyNote in notesList.Where(note => note.IsReply()))
            {
                var parentNote = result.FirstOrDefault(n => n.ThreadId == replyNote.ThreadId);
                if (parentNote != null)
                {
                    parentNote.Replies.Add(new NoteViewModel(replyNote));
                }
                else
                {
                    Logger?.LogError($"Could not find thread ID {replyNote.ThreadId} for reply note ID {replyNote.NoteId}");
                }
            }

            return result;
        }

        private async Task<LabelCollection> GetLabelSuggestionsAsync()
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var labels = await Label.GetAll(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved label suggestions in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return new LabelCollection(labels.OrderBy(l => l.Text));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="Label"/>s that can be used for auto-completion.
        /// </summary>
        public LabelCollection LabelSuggestions { get; private set; } = new();

        /// <summary>
        /// Determines whether the specified entity ID has at least one note associated to it.
        /// </summary>
        /// <remarks>
        /// To get the detailed notes associated with the 
        /// </remarks>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public async Task<bool> HasNote(IId entityId)
        {
            return (await GetNoteSummariesForEntity(entityId)).Any();
        }

        /// <summary>
        /// Adds a note to a collection of entities.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to add.</param>
        /// <param name="entityIds">The entity IDs to which to associate the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AddNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
        {
            try
            {
#if !MOCK
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.Entity.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Added note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    await note.Entity.AssociateDomainEntity(Mediator, entityId);
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated note {note.NoteId?.Id} with entity {entityId} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
                if (note.Labels.Any())
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    foreach (var label in note.Labels)
                    {
                        if (label.LabelId == null)
                        {
                            await label.CreateOrUpdate(Mediator);
                        }

                        await note.Entity.AssociateLabel(Mediator, label);
                    }
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation($"Associated labels with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                }
                foreach (var entityId in entityIds)
                {
                    //var token = SourceTokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                    //token?.NoteAdded(note);
                }
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Updates a note.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to update.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UpdateNoteAsync(Note note)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.CreateOrUpdate(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Updated note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Deletes a note.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to delete.</param>
        /// <param name="entityIds">The entity IDs from which to remove the note.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DeleteNoteAsync(NoteViewModel note, EntityIdCollection entityIds)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.Entity.Delete(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Deleted note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                foreach (var entityId in entityIds)
                {
                    //var token = SourceTokenDisplayViewModels.FirstOrDefault(vt => vt.Token.TokenId.Id == entityId.Id);
                   // token?.NoteDeleted(note);
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Label"/> and associates it with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="labelText">The text of the new <see cref="Label"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateAssociateNoteLabelAsync(NoteViewModel note, string labelText)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, because Note.CreateAssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                async void CreateAssociateLabel()
                {
                    var newLabel = await note.Entity.CreateAssociateLabel(Mediator, labelText);
                    LabelSuggestions.Add(newLabel);
                    LabelSuggestions = new LabelCollection(LabelSuggestions.OrderBy(l => l.Text));
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(CreateAssociateLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Created label {labelText} and associated it with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Associates a <see cref="Label"/> with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="label">The <see cref="Label"/> to associate.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AssociateNoteLabelAsync(NoteViewModel note, Label label)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, because Note.AssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                async void AssociateLabel()
                {
                    await note.Entity.AssociateLabel(Mediator, label);
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(AssociateLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Associated label {label.Text} with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Detaches a <see cref="Label"/> from a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> from which to detach the label.</param>
        /// <param name="label">The <see cref="Label"/> to detach.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DetachNoteLabel(NoteViewModel note, Label label)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, because Note.DetachLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                async void DetachLabel()
                {
                    await note.Entity.DetachLabel(Mediator, label);
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(DetachLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Detached label {label.Text} from note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public NoteManager(ILogger<NoteManager>? logger, IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;

            EntityNoteSummaries = new AsyncLazy<EntityNoteDictionary>(GetAllEntityNoteSummariesAsync);
        }
    }
}
