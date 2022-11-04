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
using System.Text;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using SIL.Extensions;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class NoteManager : PropertyChangedBase
    {
        private ILogger<NoteManager>? Logger { get; }
        private IMediator Mediator { get; }

        // TODO: localize
        private static string GetNoteAssociationDescription(IId associatedEntityId, IReadOnlyDictionary<string, string> entityContext)
        {
            if (associatedEntityId is TokenId tokenId)
            {
                var sb = new StringBuilder();
                sb.Append($"Tokenized Corpus {entityContext[EntityContextKeys.Corpus.DisplayName]} - {entityContext[EntityContextKeys.TokenizedCorpus.DisplayName]}");
                sb.Append($" {entityContext[EntityContextKeys.TokenId.BookId]} {entityContext[EntityContextKeys.TokenId.ChapterNumber]}:{entityContext[EntityContextKeys.TokenId.VerseNumber]}");
                sb.Append($" {entityContext[EntityContextKeys.TokenId.BookId]} {entityContext[EntityContextKeys.TokenId.ChapterNumber]}:{entityContext[EntityContextKeys.TokenId.VerseNumber]}");
                sb.Append($" word {entityContext[EntityContextKeys.TokenId.WordNumber]} part {entityContext[EntityContextKeys.TokenId.SubwordNumber]}");
                return sb.ToString();
            }

            return string.Empty;
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
        /// To get the list of notes associated with the entity, call <see cref="GetNoteIdsAsync(IId)"/>.
        /// To get the detailed notes associated with the entity, call <see cref="GetNoteDetailsAsync(NoteId)"/>.
        /// </remarks>
        /// <param name="entityId">The entity ID to query.</param>
        /// <returns>True if the entity has at least one note associated to it; false otherwise.</returns>
        public async Task<bool> HasNote(IId entityId)
        {
            return (await GetNoteIdsAsync(entityId)).Any();
        }

        /// <summary>
        /// Gets the note IDs associated with a collection of entity IDs.
        /// </summary>
        /// <remarks>
        /// This returns a dictionary of note IDs, which indicate the presence of notes on the entities.  To retrieve the details
        /// of the referenced note, call <see cref="GetNoteDetailsAsync(NoteId)"/>.
        /// </remarks>
        /// <param name="entityIds">A collection of entity IDs for which to retrieve note IDs.</param>
        /// <returns>A <see cref="EntityNoteIdDictionary"/> containing the note IDs.</returns>
        public async Task<EntityNoteIdDictionary> GetNoteIdsAsync(IEnumerable<IId> entityIds)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var notes = await Note.GetDomainEntityNoteIds(Mediator, entityIds);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved domain entity note IDs in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return new EntityNoteIdDictionary(notes);
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets the note IDs associated with an entity ID.
        /// </summary>
        /// <remarks>
        /// This returns a collection of note IDs, which indicate the presence of notes on the entities.  To retrieve the details
        /// of the referenced note, call <see cref="GetNoteDetailsAsync(NoteId)"/>.
        /// </remarks>
        /// <param name="entityId">An entity IDs for which to retrieve note IDs.</param>
        /// <returns>A <see cref="NoteIdCollection"/> containing the note IDs.</returns>
        public async Task<NoteIdCollection> GetNoteIdsAsync(IId entityId)
        {
            var dictionary = await GetNoteIdsAsync(entityId.ToEnumerable());
            return dictionary.ContainsKey(entityId) ? dictionary[entityId] : new NoteIdCollection();
        }

        /// <summary>
        /// Gets the note details for a specific note ID.
        /// </summary>
        /// <param name="noteId">A note ID for which to retrieve the note details.</param>
        /// <returns>A <see cref="NoteViewModel"/> containing the note details.</returns>
        public async Task<NoteViewModel> GetNoteDetailsAsync(NoteId noteId)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                var note = await Note.Get(Mediator, noteId);
                var noteViewModel = new NoteViewModel(note);

                var associatedEntityIds = await note.GetFullDomainEntityIds(Mediator);
                var domainEntityContexts = await note.GetDomainEntityContexts(Mediator);
                foreach (var associatedEntityId in associatedEntityIds)
                {
                    var association = new NoteAssociationViewModel
                    {
                        AssociatedEntityId = associatedEntityId
                    };
                    if (domainEntityContexts.ContainsKey(associatedEntityId))
                    {
                        association.Description = GetNoteAssociationDescription(associatedEntityId, domainEntityContexts[associatedEntityId]);
                    }
                    noteViewModel.Associations.Add(association);
                }
                noteViewModel.Replies = new NoteViewModelCollection((await note.GetReplyNotes(Mediator)).Select(n => new NoteViewModel(n)));

#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved details for note {noteId} in {stopwatch.ElapsedMilliseconds}ms");
#endif
                return noteViewModel;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets the unique note details for a collection of note IDs.
        /// </summary>
        /// <remarks>
        /// Each note ID will be included in the resulting collection only once, for example in the case where two entities are associated to the same note.
        /// </remarks>
        /// <param name="noteIds">A collection of note IDs for which to retrieve the notes details.</param>
        /// <returns>A <see cref="NoteViewModelCollection"/> containing the notes details.</returns>

        public async Task<NoteViewModelCollection> GetNoteDetailsAsync(IEnumerable<IId> noteIds)
        {
            var result = new NoteViewModelCollection();

            foreach (var id in noteIds)
            {
                var noteId = id as NoteId;
                if (noteId != null)
                {
                    if (!result.Any(n => n.NoteId != null && n.NoteId.Id == noteId.Id))
                    {
                        result.Add(await GetNoteDetailsAsync(noteId as NoteId));
                    }
                }
                else
                {
                    Logger?.LogError($"GetNoteDetails: ID {id} is not a NoteId");
                }
            }

            return result;
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
        /// <param name="note">The <see cref="NoteViewModel"/> to update.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UpdateNoteAsync(NoteViewModel note)
        {
            try
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await note.Entity.CreateOrUpdate(Mediator);
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

        public async Task InitializeAsync()
        {
            LabelSuggestions = await GetLabelSuggestionsAsync();
        }

        public NoteManager(ILogger<NoteManager>? logger, IMediator mediator)
        {
            Logger = logger;
            Mediator = mediator;
        }
    }
}
