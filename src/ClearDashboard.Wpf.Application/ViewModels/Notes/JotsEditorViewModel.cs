using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using CefSharp;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Utils;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Linq;


namespace ClearDashboard.Wpf.Application.ViewModels.Notes;



public class JotsEditorViewModel : ApplicationScreen
{
    #region Member Variables

    public new ILogger Logger { get; }
    public NoteManager NoteManager { get; }

    public SelectionManager SelectionManager { get; set; }
     
    protected DashboardProjectManager? ProjectManager { get;  }
    protected ILocalizationService? LocalizationService { get; }

    protected IEventAggregator? EventAggregator { get; }
    protected IMediator Mediator { get; }
    protected ILifetimeScope? LifetimeScope { get; }
    #endregion

    #region Constructor
    public JotsEditorViewModel(ILogger<JotsEditorViewModel> logger,
        DashboardProjectManager? projectManager,
        [KeyFilter("JotsNoteManager")] NoteManager noteManager,
        INavigationService navigationService,
        IEventAggregator? eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService): base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        NoteManager = noteManager;
        EventAggregator = eventAggregator;
        Mediator = mediator;
        LifetimeScope = lifetimeScope;
        LocalizationService = localizationService;
        Logger = logger;
        ProjectManager = projectManager;
        Title = "Jots";  // TODO: localize me.
    }

    #endregion

    #region public properties

    private string? _message;
    private bool _isAddJotOpen;
    private double _addJotHorizontalOffset;

    public string? Message
    {
        get => _message;
        set => Set(ref _message, value);
    }

    public bool ShowTabControl => NoteManager.CurrentNotes.Count > 0;
    #endregion

    


    public void Initialize(IEnumerable<IId>? noteIds = null, IEnumerable<IId>? selectedEntityIds = null)
    {
        Task.Run(async () =>
        {
            if (NoteManager.DefaultLabelGroup == null)
            {
                await NoteManager.PopulateLabelsAsync();
            }

            if (noteIds != null)
            {
                await NoteManager.GetNotes(noteIds);
            }
            else
            {
                await NoteManager.GetNotes(SelectionManager.SelectedNoteIds);
            }

            NotifyOfPropertyChange(nameof(ShowTabControl));

            if (NoteManager.CurrentNotes.Count > 0)
            {
                NoteManager.SelectedNote = NoteManager.CurrentNotes[0];
            }

            if (NoteManager.CurrentNotes.Count == 0)
            {
                IsAddJotOpen = true;
            }

            NoteManager.SelectedEntityIds = selectedEntityIds != null ? new EntityIdCollection(selectedEntityIds) : SelectionManager.SelectedEntityIds;
      
            // Create a placeholder jot for the add jot popup
            await NoteManager.CreateNewNote();

        });

    }

    public bool IsAddJotOpen
    {
        get => _isAddJotOpen;
        set => Set(ref _isAddJotOpen, value);
    }

    public double AddJotHorizontalOffset
    {
        get => _addJotHorizontalOffset;
        set => Set(ref _addJotHorizontalOffset, value);
    }

    public async void Close()
    {
        await this.DeactivateAsync(true);
    }

    #region NoteControl


    public async void CloseNotePaneRequested(object sender, RoutedEventArgs args)
    {
        await this.DeactivateAsync(true);
    }


    public void NoteAdded(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteAddedAsync(e)); //.GetAwaiter());
    }


    public async Task NoteAddedAsync(NoteEventArgs e)
    {
        await Execute.OnUIThreadAsync(async () =>
        {
            IsBusy = true;
            try
            {
                IsAddJotOpen = false;
                await NoteManager.AddNoteAsync(e.Note, e.EntityIds);
                await NoteManager.CreateNewNote();

                NotifyOfPropertyChange(nameof(ShowTabControl));
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                throw;
            }
            finally
            {
                IsBusy = false; 
            }

        });

        Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";

        Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NoteCreationCount, 1);
    }

 
    public void NoteDeleted(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteDeletedAsync(e).GetAwaiter());
    }

    public async Task NoteDeletedAsync(NoteEventArgs e)
    {
        if (e.Note.NoteId != null)
        {
            EntityIdCollection associationIds = new();
            e.Note.Associations.ForEach(a => associationIds.Add(a.AssociatedEntityId));

            await Execute.OnUIThreadAsync(async () =>
            {
                await NoteManager.DeleteNoteAsync(e.Note, associationIds);
            });
        }
        Message = $"Note '{e.Note.Text}' deleted from tokens ({string.Join(", ", e.EntityIds.Select(id => id.ToString()))})";
    }

    public void NoteEditorMouseEnter(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteEditorMouseEnterAsync(e).GetAwaiter());
    }

    public async Task NoteEditorMouseEnterAsync(NoteEventArgs e)
    {
        await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds, e.IsNewNote);
    }

    public void NoteEditorMouseLeave(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteEditorMouseLeaveAsync(e).GetAwaiter());
    }

    public async Task NoteEditorMouseLeaveAsync(NoteEventArgs e)
    {
        await NoteManager.NoteMouseLeaveAsync(e.Note, e.EntityIds);
    }

    public void NoteReplyAdded(object sender, NoteReplyAddEventArgs e)
    {
        Task.Run(() => NoteReplyAddedAsync(e).GetAwaiter());
    }

    public async Task NoteReplyAddedAsync(NoteReplyAddEventArgs args)
    {
        await NoteManager.AddReplyToNoteAsync(args.NoteViewModelWithReplies, args.Text);
    }

    public void NoteSeen(object sender, NoteSeenEventArgs e)
    {
        Task.Run(() => NoteSeenAsync(e).GetAwaiter());
    }

    public async Task NoteSeenAsync(NoteSeenEventArgs args)
    {
        var note = args.NoteViewModel;
        var seen = args.Seen;
        var userId = NoteManager.CurrentUserId;

        if (note != null && seen != null && userId != null)
        {
            var seenByUserIdsChanged = false;
            if (seen.Value && !note.SeenByUserIds.Contains(userId.Id))
            {
                note.AddSeenByUserId(userId.Id);
                seenByUserIdsChanged = true;
            }
            else if (!seen.Value && note.SeenByUserIds.Contains(userId.Id))
            {
                note.RemoveSeenByUserId(userId.Id);
                seenByUserIdsChanged = true;
            }

            if (seenByUserIdsChanged)
            {
                await NoteManager.UpdateNoteAsync(note);
            }
        }
    }

    public void NoteSendToParatext(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteSendToParatextAsync(e).GetAwaiter());
        EventAggregator.PublishOnUIThreadAsync(new ReloadExternalNotesDataMessage(ReloadType.Refresh), CancellationToken.None);
    }
    
    public async Task NoteSendToParatextAsync(NoteEventArgs e)
    {
        try
        {
            Message = $"Note '{e.Note.Text}' sent to Paratext.";
            await NoteManager.SendToParatextAsync(e.Note);
        }
        catch (Exception ex)
        {
            Message = $"Could not send note to Paratext: {ex.Message}";

            if (ex == null || ex.InnerException == null)
            {
                //TODO although notes make it to Paratext, the result returns a failure so I'm keeping this stuff in the catch for now
                Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NotePushCount, 1);
                await UpdateNoteStatus(e.Note, NoteStatus.Archived);
            }
        }
    }

    public async Task UpdateNoteStatus(NoteViewModel noteViewModel, DataAccessLayer.Models.NoteStatus noteStatus)
    {
        if (noteViewModel.NoteStatus != noteStatus.ToString())
        {
            noteViewModel.NoteStatus = noteStatus.ToString();

            if (noteStatus == NoteStatus.Archived)
            {
                // archive the whole note and replies
                NoteViewModel? parentNote;
                if (noteViewModel.ThreadId is not null)
                {
                    parentNote = NoteManager.CurrentNotes.FirstOrDefault(n => n.NoteId.Id == noteViewModel.ThreadId.Id);
                }
                else
                {
                    parentNote = NoteManager.CurrentNotes.FirstOrDefault(n => n.NoteId.Id == noteViewModel.NoteId.Id);
                }

                if (parentNote != null) 
                {
                    parentNote.NoteStatus = NoteStatus.Archived.ToString();

                    foreach (var reply in parentNote.Replies)
                    {
                        reply.NoteStatus = NoteStatus.Archived.ToString();
                        await NoteManager!.UpdateNoteAsync(reply);
                    }

                    await NoteManager!.UpdateNoteAsync(parentNote);
                }
                else
                {
                    await NoteManager!.UpdateNoteAsync(noteViewModel);
                }

            }
            else
            {
                await NoteManager!.UpdateNoteAsync(noteViewModel);
            }



            if (noteStatus == NoteStatus.Resolved)
            {
                Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NoteClosedCount, 1);
            }
        }
    }

    public void NoteUpdated(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteUpdatedAsync(e).GetAwaiter());
    }

    public async Task NoteUpdatedAsync(NoteEventArgs e)
    {
        await NoteManager.UpdateNoteAsync(e.Note);
        Message = $"Note '{e.Note.Text}' updated on tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
    }
    #endregion

    #region Labels
    public void LabelAdded(object sender, LabelEventArgs e)
    {
        Task.Run(() => LabelAddedAsync(e).GetAwaiter());
    }

    public async Task LabelAddedAsync(LabelEventArgs e)
    {
        if (e.Note.NoteId != null)
        {
            var newLabel = await NoteManager.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            Message = $"Label '{e.Label.Text}' added for note";

            if (newLabel != null && e.LabelGroup is { LabelGroupId: not null })
            {
                await NoteManager.AssociateLabelToLabelGroupAsync(e.LabelGroup, newLabel);
                Message += $" and associated to label group {e.LabelGroup.Name}";
            }
        }
    }
    public void LabelDeleted(object sender, LabelEventArgs e)
    {
        NoteManager.DeleteLabel(e.Label);
        Message = $"Label '{e.Label.Text}' deleted";
    }

    public void LabelDisassociated(object sender, LabelEventArgs e)
    {
        Task.Run(() => LabelDisassociatedAsync(e).GetAwaiter());
    }

    public async Task LabelDisassociatedAsync(LabelEventArgs e)
    {
        await NoteManager.DetachLabelFromLabelGroupAsync(e.LabelGroup, e.Label);
        Message = $"Label '{e.Label.Text}' detached from label group '{e.LabelGroup.Name}'";
    }

 
    public void LabelGroupAdded(object sender, LabelGroupAddedEventArgs e)
    {
        Task.Run(() => LabelGroupAddedAsync(e).GetAwaiter());
    }

    public async Task LabelGroupAddedAsync(LabelGroupAddedEventArgs e)
    {
        if (e.LabelGroup.LabelGroupId == null)
        {
            await NoteManager.CreateLabelGroupAsync(e.LabelGroup, e.SourceLabelGroup);
        }
        Message = $"Label group '{e.LabelGroup.Name}' added";
    }

    public void LabelGroupLabelAdded(object sender, LabelGroupLabelEventArgs e)
    {
        Task.Run(() => LabelGroupLabelAddedAsync(e).GetAwaiter());
    }

    public async Task LabelGroupLabelAddedAsync(LabelGroupLabelEventArgs e)
    {
        if (e.LabelGroup.LabelGroupId == null)
        {
        }
        Message = $"Label group '{e.LabelGroup.Name}' added";
    }

    public void LabelGroupLabelRemoved(object sender, LabelGroupLabelEventArgs e)
    {
        Task.Run(() => LabelGroupLabelRemovedAsync(e).GetAwaiter());
    }

    public async Task LabelGroupLabelRemovedAsync(LabelGroupLabelEventArgs e)
    {
        if (e.LabelGroup.LabelGroupId == null)
        {
        }
        Message = $"Label group '{e.LabelGroup.Name}' removed";
    }

    public void LabelGroupLabelsRemoved(object sender, LabelGroupLabelsRemovedEventArgs e)
    {
        Task.Run(() => LabelGroupLabelsRemovedAsync(e).GetAwaiter());
    }

    public async Task LabelGroupLabelsRemovedAsync(LabelGroupLabelsRemovedEventArgs e)
    {

        if (e.Labels == null)
        {
            return;
        }
        
        foreach (var label in e.Labels)
        {
            await NoteManager.DetachLabelFromLabelGroupAsync(e.LabelGroup, label);
        }

        //foreach (var label in e.Labels)
        //{
        //    await NoteManager.AssociateLabelToLabelGroupAsync(e.NoneLabelGroup, label);

        //}


    }

    public void LabelGroupRemoved(object sender, LabelGroupEventArgs e)
    {
        Task.Run(() => LabelGroupRemovedAsync(e).GetAwaiter());
    }

    public async Task LabelGroupRemovedAsync(LabelGroupEventArgs e)
    {
        if (e.LabelGroup.LabelGroupId != null)
        {
            await NoteManager.RemoveLabelGroupAsync(e.LabelGroup);
        }
        Message = $"Label group '{e.LabelGroup.Name}' removed";
    }

    public void LabelGroupSelected(object sender, LabelGroupEventArgs e)
    {
        Task.Run(() => LabelGroupSelectedAsync(e.LabelGroup).GetAwaiter());
    }

    public async Task LabelGroupSelectedAsync(LabelGroupViewModel labelGroup)
    {
        if (labelGroup != null && labelGroup.LabelGroupId != null)
        {
            NoteManager.SaveLabelGroupDefault(labelGroup);
            Message = $"Label group '{labelGroup.Name}' selected";
        }
        else
        {
            await NoteManager.ClearLabelGroupDefault();
        }
    }


    public void LabelRemoved(object sender, LabelEventArgs e)
    {
        Task.Run(() => LabelRemovedAsync(e).GetAwaiter());
    }

    public async Task LabelRemovedAsync(LabelEventArgs e)
    {
        if (e.Note.NoteId != null)
        {
            await NoteManager.DetachNoteLabel(e.Note, e.Label);
        }
        Message = $"Label '{e.Label.Text}' removed for note";
    }



    public void LabelSelected(object sender, LabelEventArgs e)
    {
        Task.Run(() => LabelSelectedAsync(e).GetAwaiter());
    }

    public async Task LabelSelectedAsync(LabelEventArgs e)
    {
        if (e.Note.NoteId != null)
        {
            await NoteManager.AssociateNoteLabelAsync(e.Note, e.Label);
            Message = $"Label '{e.Label.Text}' selected for note";
        }
        else
        {
            // JOTS refactor
            Execute.OnUIThread(()=> NoteManager.NewNote.Labels.AddDistinct(e.Label));
        }



        // JOTS refactor - need to capture label add to new jot here

        if (e.LabelGroup != null && !e.LabelGroup.Labels.ContainsMatchingLabel(e.Label.Text))
        {
            await NoteManager.AssociateLabelToLabelGroupAsync(e.LabelGroup, e.Label);
            Message += $" and associated to label group {e.LabelGroup.Name}";
        }
    }

    public void LabelUpdated(object sender, LabelEventArgs e)
    {
        Task.Run(() => LabelUpdatedAsync(e).GetAwaiter());
    }

    public async Task LabelUpdatedAsync(LabelEventArgs e)
    {
        await NoteManager.UpdateLabelAsync(e.Label);
        Message = $"Label '{e.Label.Text}' updated";
    }


    #endregion
}