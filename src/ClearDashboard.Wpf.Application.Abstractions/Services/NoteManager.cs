using ClearBible.Engine.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Notes;
using ClearBible.Engine.Corpora;
using MediatR;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using Microsoft.Extensions.Logging;
using SIL.Extensions;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Models;
using Label = ClearDashboard.DAL.Alignment.Notes.Label;
using LabelGroup = ClearDashboard.DAL.Alignment.Notes.LabelGroup;
using Note = ClearDashboard.DAL.Alignment.Notes.Note;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class NoteManager : PropertyChangedBase,
        IHandle<SelectionUpdatedMessage>, IHandle<LabelsUpdatedMessage>
    {
        private NoteViewModelCollection _currentNotes = new();

        private NoteViewModel _newNote = new();

        private NoteViewModel? _selectedNote;

        private IEventAggregator EventAggregator { get; }
        private ILogger<NoteManager>? Logger { get; }
        private IMediator Mediator { get; }
        private IUserProvider UserProvider { get; }
        private ILocalizationService LocalizationService { get; }

        public ExternalNoteManager ExternalNoteManager { get; }

        public EntityIdCollection SelectedEntityIds
        {
            get => _selectedEntityIds;
            set => Set(ref _selectedEntityIds, value);
        }

        public ILifetimeScope LifetimeScope { get; }

        private ConcurrentDictionary<Guid, NoteViewModel> NotesCache { get; } = new();
      

        private UserId? _currentUserId;
        private LabelCollection _labelSuggestions = new();
        private LabelGroupViewModelCollection _labelGroups = new();

        private LabelGroupViewModel? _defaultLabelGroup;
        private bool _isBusy;
        private EntityIdCollection _selectedEntityIds = new EntityIdCollection();
        private bool _isBusyBackground;
        private LabelGroupViewModelCollection _domainLabelGroups = new();


        public NoteManager(IEventAggregator eventAggregator,
            ILogger<NoteManager>? logger,
            IMediator mediator,
            IUserProvider userProvider,
            ILocalizationService localizationService,
            ILifetimeScope lifetimeScope)
        {
            LocalizationService = localizationService;
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
            UserProvider = userProvider;
            ExternalNoteManager = new ExternalNoteManager(EventAggregator);

            NoneLabelGroup.Name = $"<{LocalizationService["None"]}>";
            AllLabelGroup.Name = $"<{LocalizationService["All"]}>";

            LifetimeScope = lifetimeScope;

            EventAggregator.SubscribeOnUIThread(this);
        }

        /// <summary>
        /// Gets the <see cref="UserId"/> of the current user.
        /// </summary>
        public UserId? CurrentUserId
        {
            get
            {
                if (_currentUserId == null &&  UserProvider.CurrentUser != null)
                {
                    _currentUserId = new UserId(UserProvider.CurrentUser!.Id, UserProvider.CurrentUser!.FullName!);
                }
                return _currentUserId;
            }
        }

        /// <summary>
        /// Gets the LabelGroup which contains all label suggestions.
        /// </summary>
        public static LabelGroupViewModel AllLabelGroup { get; } = new LabelGroupViewModel();

        /// <summary>
        /// Gets the LabelGroup which contains label suggestions which are not part of a LabelGroup.
        /// </summary>
        public static LabelGroupViewModel NoneLabelGroup { get; } = new LabelGroupViewModel() ;



        /// <summary>
        /// Gets the default <see cref="LabelGroupViewModel"/> for the current user, if any.
        /// </summary>
        public LabelGroupViewModel? DefaultLabelGroup
        {
            get => _defaultLabelGroup;
            set => Set(ref _defaultLabelGroup, value);
        }

        /// <summary>
        /// Gets a collection of <see cref="Label"/>s that can be used for auto-completion.
        /// </summary>
        public LabelCollection LabelSuggestions
        {
            get => _labelSuggestions;
            private set => Set(ref _labelSuggestions, value);
        }

        /// <summary>
        /// Gets a collection of <see cref="LabelGroupViewModel"/>s that can be used for auto-completion.
        /// </summary>
        public LabelGroupViewModelCollection LabelGroups
        {
            get => _labelGroups;
            private set => Set(ref _labelGroups, value);
        }

        private LabelGroupViewModelCollection DomainLabelGroups
        {
            get => _domainLabelGroups;
            set => Set(ref _domainLabelGroups, value);
        }

        /// <summary>
        /// Gets the collection of notes associated with the current selection.
        /// </summary>
        public NoteViewModelCollection CurrentNotes
        {
            get => _currentNotes;
            private set => Set(ref _currentNotes, value);
        }

      

        public NoteViewModel? SelectedNote
        {
            get => _selectedNote;
            set => Set(ref _selectedNote, value);
        }

        public NoteViewModel? NewNote
        {
            get => _newNote;
            set => Set(ref _newNote, value);
        }

        public void ClearNotesCache()
        {
            NotesCache.Clear();
        }

        private string GetNoteAssociationDescription(IId associatedEntityId, IReadOnlyDictionary<string, string> entityContext)
        {
            if (associatedEntityId is TokenId)
            {
                var sb = new StringBuilder();

                // JOTS Refactor
                //sb.Append($"{LocalizationService["Notes_TokenizedCorpus"]} {entityContext[EntityContextKeys.TokenizedCorpus.DisplayName]}");

                sb.Append($" {entityContext[EntityContextKeys.TokenizedCorpus.DisplayName]}");
                sb.Append($" {entityContext[EntityContextKeys.TokenId.BookId]} {entityContext[EntityContextKeys.TokenId.ChapterNumber]}:{entityContext[EntityContextKeys.TokenId.VerseNumber]}");
                sb.Append($" {LocalizationService["Notes_Word"]} {entityContext[EntityContextKeys.TokenId.WordNumber]} {LocalizationService["Notes_Part"]} {entityContext[EntityContextKeys.TokenId.SubwordNumber]}");
                if (entityContext.ContainsKey(EntityContextKeys.TokenId.SurfaceText))
                {
                    sb.Append($" ({entityContext[EntityContextKeys.TokenId.SurfaceText]})");
                }
                
                return sb.ToString();
            }
            else if (associatedEntityId is TranslationId)
            {
                var sb = new StringBuilder();

                // JOTS Refactor
                //sb.Append($"{LocalizationService["Notes_TranslationSet"]} {entityContext[EntityContextKeys.TranslationSet.DisplayName]}");

                sb.Append($" {entityContext[EntityContextKeys.TranslationSet.DisplayName]}");
                sb.Append($" {entityContext[EntityContextKeys.TokenId.BookId]} {entityContext[EntityContextKeys.TokenId.ChapterNumber]}:{entityContext[EntityContextKeys.TokenId.VerseNumber]}");
                sb.Append($" {LocalizationService["Notes_Word"]} {entityContext[EntityContextKeys.TokenId.WordNumber]} {LocalizationService["Notes_Part"]} {entityContext[EntityContextKeys.TokenId.SubwordNumber]}");
                return sb.ToString();
            }

            return string.Empty;
        }


        private async Task SetIsBusy(bool isBusy)
        {
            await Task.Run(async () =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    IsBusy = isBusy;
                    await Task.CompletedTask;
                });
            });
          
        }

        private async Task SetIsBusyBackground(bool isBusy)
        {
            await Task.Run(async () =>
            {
                await Execute.OnUIThreadAsync(async () =>
                {
                    IsBusyBackground = isBusy;
                    await Task.CompletedTask;
                });
            });

        }

        private async Task<LabelCollection> GetLabelSuggestionsAsync()
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var labels = await Label.GetAll(Mediator);

                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved label suggestions in {stopwatch.ElapsedMilliseconds}ms");

                return new LabelCollection(labels.OrderBy(l => l.Text));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        private async Task<LabelGroupViewModelCollection> GetLabelGroupsAsync()
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                //var result = new LabelGroupViewModelCollection { NoneLabelGroup, AllLabelGroup };

                var result = new LabelGroupViewModelCollection { NoneLabelGroup };

                DomainLabelGroups = new LabelGroupViewModelCollection();

                var labelGroups = await LabelGroup.GetAll(Mediator);
                foreach (var labelGroup in labelGroups.OrderBy(lg => lg.Name))
                {
                    var labelIds = await labelGroup.GetLabelIds(Mediator);
                    var labels = new LabelCollection(LabelSuggestions.Where(ls => labelIds.Contains(ls.LabelId)));
                    DomainLabelGroups.Add(new LabelGroupViewModel(labelGroup) { Labels = labels });
                }

                result.AddRange(DomainLabelGroups);

                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved label groups in {stopwatch.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        public async Task<string> ExportLabelGroupsAsync()
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await LabelGroup.Export(Mediator);

                stopwatch.Stop();
                Logger?.LogInformation($"Exported label groups in {stopwatch.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        public async Task ImportLabelGroupsAsync(string labelGroupJson)
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var data = LabelGroup.Extract(labelGroupJson);
                await LabelGroup.Import(Mediator, data);

                await PopulateLabelsAsync();

                stopwatch.Stop();
                Logger?.LogInformation($"Imported label groups in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false); 
            }
        }

        /// <summary>
        /// Gets the default label group for the current user.
        /// </summary>
        /// <returns>A <see cref="LabelGroupViewModel"/> to use by default.</returns>
        private async Task<LabelGroupViewModel?> GetUserDefaultLabelGroupAsync()
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var labelGroupId = await LabelGroup.GetUserDefault(Mediator, CurrentUserId!);

                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved label groups in {stopwatch.ElapsedMilliseconds}ms");

                var matchingLabelGroup = LabelGroups.FirstOrDefault(lg => lg.LabelGroupId == labelGroupId);
                return matchingLabelGroup ?? NoneLabelGroup;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false); 
            }
        }

        /// <summary>
        /// Gets the note IDs associated with a collection of entity IDs.
        /// </summary>
        /// <remarks>
        /// This returns a dictionary of note IDs, which indicate the presence of notes on the entities.  To retrieve the details
        /// of the referenced note, call <see cref="GetNoteDetailsAsync(NoteId,bool)"/>.
        /// </remarks>
        /// <param name="entityIds">A collection of entity IDs for which to retrieve note IDs.</param>
        /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
        /// <returns>A <see cref="EntityNoteIdDictionary"/> containing the note IDs.</returns>
        public async Task<EntityNoteIdDictionary> GetNoteIdsAsync(IEnumerable<IId>? entityIds = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var notes = await Note.GetDomainEntityNoteIds(Mediator, entityIds, cancellationToken);

                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved domain entity note IDs in {stopwatch.ElapsedMilliseconds}ms");

                return new EntityNoteIdDictionary(notes);
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Gets the note IDs associated with an entity ID.
        /// </summary>
        /// <remarks>
        /// This returns a collection of note IDs, which indicate the presence of notes on the entities.  To retrieve the details
        /// of the referenced note, call <see cref="GetNoteDetailsAsync(NoteId,bool)"/>.
        /// </remarks>
        /// <param name="entityId">An entity IDs for which to retrieve note IDs.</param>
        /// <returns>A <see cref="NoteIdCollection"/> containing the note IDs.</returns>
        public async Task<NoteIdCollection> GetNoteIdsAsync(IId entityId)
        {
            var dictionary = await GetNoteIdsAsync(entityId.ToEnumerable());
            return dictionary.TryGetValue(entityId, out var noteIds) ? noteIds : new NoteIdCollection();
        }

        public void UpdateNoteInCache(Note note)
        {
            if (NotesCache.ContainsKey(note.NoteId.Id))
            {
                var existing = NotesCache[note.NoteId.Id];
                existing.Entity = note;
                existing.Labels = new LabelCollection(note.Labels);
            }
        }

        /// <summary>
        /// Gets the note details for a specific note ID.
        /// </summary>
        /// <param name="noteId">A note ID for which to retrieve the note details.</param>
        /// <param name="doGetParatextSendNoteInformation">If true, also retrieve information needed for sending the note to Paratext.</param>
        /// <returns>A <see cref="NoteViewModel"/> containing the note details.</returns>
        public async Task<NoteViewModel> GetNoteDetailsAsync(NoteId noteId, bool doGetParatextSendNoteInformation = true, bool collabUpdate = false, bool setIsBusy = true)
        {
            try
            {
                if (setIsBusy)
                {
                    await SetIsBusy(true);
                }

                if (!collabUpdate && NotesCache.TryGetValue(noteId.Id, out var noteDetails))
                {
                    if (doGetParatextSendNoteInformation && noteDetails.ParatextSendNoteInformation == null)
                    {
                        noteDetails.ParatextSendNoteInformation = await ExternalNoteManager.GetExternalSendNoteInformationAsync(Mediator, noteDetails.NoteId!, UserProvider, Logger);

                        foreach (var reply in noteDetails.Replies)
                        {
                            reply.ParatextSendNoteInformation = await ExternalNoteManager.GetExternalSendNoteInformationAsync(Mediator, noteDetails.NoteId!, UserProvider, Logger);
                        }
                    }
                    Logger?.LogInformation($"Returning cached details for note \"{noteDetails.Text}\" ({noteId.Id})");
                    return noteDetails;
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var note = await Note.Get(Mediator, noteId);
                var noteViewModel = new NoteViewModel(note);

                var associatedEntityIds = await note.GetFullDomainEntityIds(Mediator);
                var domainEntityContexts = new EntityContextDictionary(await note.GetDomainEntityContexts(Mediator));

                noteViewModel.Associations = GetNoteAssociations(associatedEntityIds, domainEntityContexts);

                noteViewModel.Replies = new NoteViewModelCollection((await note.GetReplyNotes(Mediator)).Select(n => new NoteViewModel(n)));

                if (doGetParatextSendNoteInformation)
                {
                    noteViewModel.ParatextSendNoteInformation = await ExternalNoteManager.GetExternalSendNoteInformationAsync(Mediator, noteViewModel.NoteId!, UserProvider, Logger);
                    
                    foreach(var reply in noteViewModel.Replies)
                    {
                        reply.ParatextSendNoteInformation = await ExternalNoteManager.GetExternalSendNoteInformationAsync(Mediator, noteViewModel.NoteId!, UserProvider, Logger);
                    }
                }
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved details for note \"{note.Text}\" ({noteId.Id}) in {stopwatch.ElapsedMilliseconds}ms");

                NotesCache[noteId.Id] = noteViewModel;

                return noteViewModel;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                if (setIsBusy)
                {
                    await SetIsBusy(false);
                }
            }
        }

        private NoteAssociationViewModelCollection GetNoteAssociations(IEnumerable<IId> associatedEntityIds, EntityContextDictionary domainEntityContexts)
        {
            
            var noteAssociationViewModelCollection = new NoteAssociationViewModelCollection();
                
            foreach (var associatedEntityId in associatedEntityIds)
            {
                var association = new NoteAssociationViewModel
                {
                    AssociatedEntityId = associatedEntityId
                };
                if (domainEntityContexts.TryGetValue(associatedEntityId, out var entityContext))
                {
                    association.Book = entityContext[EntityContextKeys.TokenId.BookId];
                    association.Chapter = entityContext[EntityContextKeys.TokenId.ChapterNumber];
                    association.Verse = entityContext[EntityContextKeys.TokenId.VerseNumber];
                    association.Word = entityContext[EntityContextKeys.TokenId.WordNumber];
                    association.Part = entityContext[EntityContextKeys.TokenId.SubwordNumber];
                    association.Description = GetNoteAssociationDescription(associatedEntityId, entityContext);
                }
                noteAssociationViewModelCollection.Add(association);
            }

            var orderedList = noteAssociationViewModelCollection.OrderBy(a => a.Book)
                .ThenBy(a => a.Chapter)
                .ThenBy(a => a.Verse)
                .ThenBy(a => a.Word)
                .ThenBy(a => a.Part);

            return new NoteAssociationViewModelCollection(orderedList);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public bool IsBusyBackground
        {
            get => _isBusyBackground;
            set => Set(ref _isBusyBackground, value);
        }

        

        public async Task GetNotes(IEnumerable<IId> noteIds)
        {
            CurrentNotes = await GetNotesDetailsAsync(noteIds, doGetParatextSendNoteInformation:false);

            // Get any paratext information in the background.
            try
            {
                await SetIsBusyBackground(true);
                foreach (var note in CurrentNotes)
                {
                    await GetNoteDetailsAsync(note.NoteId, doGetParatextSendNoteInformation: true,
                        setIsBusy: false);
                }
            }
            finally
            {
                SetIsBusyBackground(false);
            }

        }

        /// <summary>
        /// Gets the unique note details for a collection of note IDs.
        /// </summary>
        /// <remarks>
        /// Each note ID will be included in the resulting collection only once, for example in the case where two entities are associated to the same note.
        /// </remarks>
        /// <param name="noteIds">A collection of note IDs for which to retrieve the notes details.</param>
        /// <param name="doGetParatextSendNoteInformation"></param>
        /// <returns>A <see cref="NoteViewModelCollection"/> containing the notes details.</returns>
        public async Task<NoteViewModelCollection> GetNotesDetailsAsync(IEnumerable<IId> noteIds, bool doGetParatextSendNoteInformation = true)
        {
            var result = new List<NoteViewModel>();

            foreach (var id in noteIds)
            {
                var noteId = id as NoteId;
                if (noteId != null)
                {
                    if (!result.Any(n => n.NoteId != null && n.NoteId.Id == noteId.Id))
                    {
                        result.Add(await GetNoteDetailsAsync(noteId, doGetParatextSendNoteInformation, collabUpdate:false, setIsBusy:true));
                    }
                }
                else
                {
                    Logger?.LogError($"GetNoteDetails: ID {id} is not a NoteId");
                }
            }

            var orderedList = result.OrderByDescending(n => n.CreatedLocalTime).ToArray();

            var index = 0;
            foreach (var item in orderedList)
            {
                item.TabHeader = $"Jot{++index}";
            }

            return new NoteViewModelCollection(orderedList);
        }

        //public async Task SetCurrentNoteIds(NoteIdCollection noteIds)
        //{
        //    CurrentNotes = await GetNoteDetailsAsync(noteIds);
        //}

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
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await note.Entity.CreateOrUpdate(Mediator);

                stopwatch.Stop();
                Logger?.LogInformation(
                    $"Added note \"{note.Text}\" ({note.NoteId?.Id}) in {stopwatch.ElapsedMilliseconds} ms");

                // clear the existing domain entities so we can properly add them to the database.
                note.Entity.ClearDomainEntityIds();
                foreach (var entityId in entityIds)
                {
                    stopwatch.Restart();

                    await note.Entity.AssociateDomainEntity(Mediator, entityId);

                    stopwatch.Stop();
                    Logger?.LogInformation(
                        $"Associated note \"{note.Text}\" ({note.NoteId?.Id}) with entity {entityId} in {stopwatch.ElapsedMilliseconds} ms");
                }

                if (note.Labels.Any())
                {
                    stopwatch.Restart();

                    foreach (var label in note.Labels)
                    {
                        if (label.LabelId == null)
                        {
                            await label.CreateOrUpdate(Mediator);
                        }

                        await note.Entity.AssociateLabel(Mediator, label);
                    }

                    stopwatch.Stop();
                    Logger?.LogInformation(
                        $"Associated labels with note \"{note.Text}\" ({note.NoteId?.Id}) in {stopwatch.ElapsedMilliseconds} ms");
                }

                var newNoteDetail = await GetNoteDetailsAsync(note.NoteId!);
                newNoteDetail.TabHeader = $"Jot{CurrentNotes.Count + 1}";
                CurrentNotes.Add(newNoteDetail);
                SelectedNote = CurrentNotes.Last();
                NotifyOfPropertyChange(nameof(CurrentNotes));
                await EventAggregator.PublishOnUIThreadAsync(new NoteAddedMessage(note, entityIds));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
               await SetIsBusy(false);
            }
        }

        private async Task UpdateNoteAsync(Note note)
        {
            try
            {
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await EventAggregator.PublishOnUIThreadAsync(new NoteUpdatingMessage(note.NoteId!));

                await note.CreateOrUpdate(Mediator);

                stopwatch.Stop();
                Logger?.LogInformation(
                    $"Updated note \"{note.Text}\" ({note.NoteId?.Id}) in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new NoteUpdatedMessage(note, true));

            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                await EventAggregator.PublishOnUIThreadAsync(new NoteUpdatedMessage(note, false));
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Updates a note.
        /// </summary>
        /// <param name="noteViewModel">The <see cref="NoteViewModel"/> to update.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task UpdateNoteAsync(NoteViewModel noteViewModel)
        {
            await UpdateNoteAsync(noteViewModel.Entity);
            NotesCache[noteViewModel.NoteId!.Id] = noteViewModel;
        }

        public async Task AddReplyToNoteAsync(NoteViewModel parentNote, string replyText)
        {
            var replyNote = new Note(parentNote.Entity) { Text = replyText, NoteStatus = NoteStatus.Open.ToString()};
            await UpdateNoteAsync(replyNote);

            var replyNoteViewModel = new NoteViewModel(replyNote);

            replyNoteViewModel.ParatextSendNoteInformation = parentNote.ParatextSendNoteInformation;
            parentNote.Replies.Add(replyNoteViewModel);
            NotesCache[parentNote.NoteId!.Id] = parentNote;
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
                await SetIsBusy(true);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await note.Entity.Delete(Mediator);

                stopwatch.Stop();
                Logger?.LogInformation(
                    $"Deleted note \"{note.Text}\" ({note.NoteId?.Id}) in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new NoteDeletedMessage(note, entityIds));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Sends a note to Paratext.
        /// </summary>
        /// <param name="note">The note to send to Paratext.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task SendToParatextAsync(NoteViewModel note)
        {
            await ExternalNoteManager.SendToExternalAsync(Mediator, note, Logger);
        }

        /// <summary>
        /// Publishes a message indicating that a note was hovered (that is, the mouse was moved into its editor).
        /// </summary>
        /// <param name="note">The <see cref="Note"/> that was hovered.</param>
        /// <param name="entityIds">The entity IDs associated with the hovered note.</param>
        /// <param name="isNewNote">Is this for a new note?</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task NoteMouseEnterAsync(NoteViewModel note, EntityIdCollection entityIds, bool isNewNote = false)
        {
            await EventAggregator.PublishOnUIThreadAsync(new NoteMouseEnterMessage(note, entityIds, isNewNote));
        }

        /// <summary>
        /// Publishes a message indicating that the mouse was moved our of a note's editor.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> that was left.</param>
        /// <param name="entityIds">The entity IDs associated with the note that was left.</param>
        /// <param name="isNewNote">Is this for a new note?</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task NoteMouseLeaveAsync(NoteViewModel note, EntityIdCollection entityIds, bool isNewNote = false)
        {
            await EventAggregator.PublishOnUIThreadAsync(new NoteMouseLeaveMessage(note, entityIds, isNewNote));
        }

        /// <summary>
        /// Creates a new <see cref="Label"/> and associates it with a <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="labelText">The text of the new <see cref="Label"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task<Label?> CreateAssociateNoteLabelAsync(NoteViewModel note, string labelText)
        {
            try
            {
                await SetIsBusy(true);
                var matchingLabel = note.Labels.GetMatchingLabel(labelText);
                if (matchingLabel == null)
                {
#if DEBUG
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
#endif
                    Label? newLabel = null;

                    // For thread safety, because Note.CreateAssociateLabel() modifies an observable collection, we need to invoke the operation on the dispatcher.
                    async void CreateAssociateLabel()
                    {
                        newLabel = await note.Entity.CreateAssociateLabel(Mediator, labelText, null);
                        LabelSuggestions.Add(newLabel);
                        LabelSuggestions.Sort();
                    }

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(CreateAssociateLabel);
#if DEBUG
                    stopwatch.Stop();
                    Logger?.LogInformation(
                        $"Created label {labelText} and associated it with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");

#endif
                    await Task.Delay(100);
                    await EventAggregator.PublishOnUIThreadAsync(new NoteLabelAttachedMessage(note.NoteId!, newLabel!));
                    await EventAggregator.PublishOnUIThreadAsync(new NoteUpdatedMessage(note.Entity, true));
                    return newLabel;
                }

                return matchingLabel;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Associates a <see cref="Label"/> with a <see cref="Note"/>, if it isn't already.
        /// </summary>
        /// <param name="note">The <see cref="NoteViewModel"/> to which to associate the label.</param>
        /// <param name="label">The <see cref="Label"/> to associate.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AssociateNoteLabelAsync(NoteViewModel note, Label label)
        {
            try
            {
                await SetIsBusy(true);
                if (!note.Labels.ContainsMatchingLabel(label.Text))
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
                    Logger?.LogInformation(
                        $"Associated label {label.Text} with note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");
#endif
                    await Task.Delay(100);
                    await EventAggregator.PublishOnUIThreadAsync(new NoteLabelAttachedMessage(note.NoteId!, label));
                    await EventAggregator.PublishOnUIThreadAsync(new NoteUpdatedMessage(note.Entity, true));
                }
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
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
                await SetIsBusy(true);
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
                Logger?.LogInformation(
                    $"Detached label {label.Text} from note {note.NoteId?.Id} in {stopwatch.ElapsedMilliseconds} ms");

                await EventAggregator.PublishOnUIThreadAsync(new NoteLabelDetachedMessage(note.NoteId!, label));
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Deletes an existing <see cref="Label"/>.
        /// </summary>
        /// <param name="label">The <see cref="Label"/> to delete.</param>
        public void DeleteLabel(Label label)
        {
            try
            {
                SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                LabelSuggestions.RemoveIfExists(label);
                label.Delete(Mediator);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Deleted label {label.Text} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                SetIsBusy(false);
            }
        }

        /// <summary>
        /// Updates an existing <see cref="Label"/>.
        /// </summary>
        /// <param name="label">The <see cref="Label"/> to update.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        public async Task UpdateLabelAsync(Label label, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                async void UpdateLabel()
                {
                    await label.CreateOrUpdate(Mediator, cancellationToken);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(UpdateLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Updated label {label.Text} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Creates a new <see cref="LabelGroup"/> and optionally initialize it with a collection of <see cref="Label"/>s from another label group.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> containing the new label group.</param>
        /// <param name="sourceLabelGroup">The source label group from which to populate labels in the new group.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateLabelGroupAsync(LabelGroupViewModel labelGroup, LabelGroupViewModel? sourceLabelGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, because LabelGroup.CreateOrUpdate() modifies an observable collection, we need to invoke the operation on the dispatcher.
                async void CreateLabelGroupWithLabelsAsync()
                {
                    var newLabelGroup =
                        new LabelGroupViewModel(await labelGroup.Entity.CreateOrUpdate(Mediator, cancellationToken));

                    if (sourceLabelGroup != null)
                    {
                        foreach (var sourceLabel in sourceLabelGroup.Labels)
                        {
                            await newLabelGroup.Entity.AssociateLabel(Mediator, sourceLabel, cancellationToken);
                            newLabelGroup.Labels.Add(sourceLabel);
                        }
                    }

                    LabelGroups.Add(newLabelGroup);
                    LabelGroups = new LabelGroupViewModelCollection(LabelGroups.OrderBy(l => l.Name));

                    await EventAggregator.PublishOnUIThreadAsync(new LabelGroupAddedMessage(newLabelGroup),
                        cancellationToken);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(CreateLabelGroupWithLabelsAsync);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation(sourceLabelGroup != null
                    ? $"Created label group {labelGroup.Name} and initialized it with {sourceLabelGroup.Labels.Count} in {stopwatch.ElapsedMilliseconds} ms"
                    : $"Created label group {labelGroup.Name} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Associates a <see cref="Label"/> to a <see cref="LabelGroup"/>.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> to which to add the label.</param>
        /// <param name="label">The label to add.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task AssociateLabelToLabelGroupAsync(LabelGroupViewModel labelGroup, Label label, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, we need to invoke the operation on the dispatcher.
                async void AssociateLabel()
                {
                    await labelGroup.Entity.AssociateLabel(Mediator, label, cancellationToken);

                    labelGroup.Labels.Add(label);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(AssociateLabel);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation(
                    $"Associated label '{label.Text}' ({label.LabelId}) to label group '{labelGroup.Name}' ({labelGroup.LabelGroupId}) in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Detaches a <see cref="Label"/> from a <see cref="LabelGroup"/>.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> from which to disassociate the label.</param>
        /// <param name="label">The label to disassociate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DetachLabelFromLabelGroupAsync(LabelGroupViewModel labelGroup, Label label, CancellationToken cancellationToken = default)
        {
            try
            {


                //TODO:  should we bail here?
                if (labelGroup.Name == LocalizationService["All"])
                {
                    return;
                }

                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, we need to invoke the operation on the dispatcher.
                async void DetachLabelAsync()
                {
                    labelGroup.Labels.RemoveIfExists(label);
                    await labelGroup.Entity.DetachLabel(Mediator, label, cancellationToken);
                }
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(DetachLabelAsync);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Detached label '{label.Text}' ({label.LabelId}) from label group '{labelGroup.Name}' ({labelGroup.LabelGroupId}) in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Removes an existing <see cref="LabelGroup"/>.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> containing the new label group.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task RemoveLabelGroupAsync(LabelGroupViewModel labelGroup, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                // For thread safety, because LabelGroup.CreateOrUpdate() modifies an observable collection, we need to invoke the operation on the dispatcher.
                async void RemoveLabelGroupViewModelAsync()
                {
                    labelGroup.Entity.Delete(Mediator, cancellationToken);

                    LabelGroups.Remove(labelGroup);

                    //await EventAggregator.PublishOnUIThreadAsync(new LabelGroupAddedMessage(newLabelGroup), cancellationToken);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(RemoveLabelGroupViewModelAsync);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Removed label group {labelGroup.Name} in {stopwatch.ElapsedMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Saves a specified <see cref="LabelGroup"/> as the default for a user.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> to set as default.</param>
        /// <param name="userId">The <see cref="UserId"/> for the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        private void SaveLabelGroupDefault(LabelGroupViewModel labelGroup, UserId userId, CancellationToken cancellationToken = default)
        {
            try
            {
               SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                labelGroup.Entity.PutAsUserDefault(Mediator, userId, cancellationToken);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Saved label group {labelGroup.Name} for user {userId.DisplayName}");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                SetIsBusy(false);
            }

        }

        /// <summary>
        /// Saves a specified <see cref="LabelGroup"/> as the default for the current user.
        /// </summary>
        /// <param name="labelGroup">The <see cref="LabelGroupViewModel"/> to set as default.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public void SaveLabelGroupDefault(LabelGroupViewModel labelGroup, CancellationToken cancellationToken = default)
        {
            SaveLabelGroupDefault(labelGroup, CurrentUserId!, cancellationToken);
            DefaultLabelGroup = labelGroup;
        }        
        
        /// <summary>
        /// Clears the <see cref="LabelGroup"/> default for a user.
        /// </summary>
        /// <param name="userId">The <see cref="UserId"/> for the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        private async Task ClearLabelGroupDefault(UserId userId, CancellationToken cancellationToken = default)
        {
            try
            {
                await SetIsBusy(true);
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif
                await LabelGroup.PutUserDefault(Mediator, null, userId, cancellationToken);
#if DEBUG
                stopwatch.Stop();
                Logger?.LogInformation($"Cleared label group default for user {userId.DisplayName}");
#endif
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
            finally
            {
                await SetIsBusy(false);
            }
        }

        /// <summary>
        /// Clears the <see cref="LabelGroup"/> for the current user.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the asynchronous operation.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task ClearLabelGroupDefault(CancellationToken cancellationToken = default)
        {
            await ClearLabelGroupDefault(CurrentUserId!, cancellationToken);
            DefaultLabelGroup = NoneLabelGroup;
        }

        public async Task PopulateLabelsAsync()
        {
            LabelSuggestions = await GetLabelSuggestionsAsync();
            AllLabelGroup.Labels = LabelSuggestions;
            LabelGroups = await GetLabelGroupsAsync();
            NoneLabelGroup.Labels = GetNoneLabelGroupLabels();

            DefaultLabelGroup = await GetUserDefaultLabelGroupAsync();
        }

        private LabelCollection GetNoneLabelGroupLabels()
        {
            var allGroupedLabels = DomainLabelGroups.SelectMany(lg => lg.Labels).Distinct().ToList();
            return new LabelCollection(AllLabelGroup.Labels.Except(allGroupedLabels));
        }

        public async Task InitializeAsync()
        {
            try 
            {
                await PopulateLabelsAsync();

            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task HandleAsync(LabelsUpdatedMessage message, CancellationToken cancellationToken = default)
        {
            await PopulateLabelsAsync();
        }

        public async Task HandleAsync(SelectionUpdatedMessage message, CancellationToken cancellationToken)
        {

            // Jots refactor
            //CurrentNotes = await GetNoteDetailsAsync(message.SelectedTokens.NoteIds);
        }


        public async Task CreateNewNote()
        {
            try
            {
               
                await Execute.OnUIThreadAsync(async () =>
                {
                    
                    var newNote = new NoteViewModel();

                    newNote.Entity.SetDomainEntityIds( SelectedEntityIds);
                    
                    var domainEntityContexts =
                        new EntityContextDictionary(
                            await Note.GetDomainEntityContexts(Mediator,  SelectedEntityIds));
                    newNote.Associations =
                        GetNoteAssociations(SelectedEntityIds, domainEntityContexts);

                    NewNote = newNote;
                });

            }
            catch (Exception e)
            {
                var s = e.Message;
                throw;
            }
          
        }
    }
}
