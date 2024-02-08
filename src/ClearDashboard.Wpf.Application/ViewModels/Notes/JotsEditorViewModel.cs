using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using CefSharp;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Linq;


namespace ClearDashboard.Wpf.Application.ViewModels.Notes;



public class JotsEditorViewModel : ApplicationScreen
{
    #region Member Variables

    protected ILogger Logger { get; }
    public NoteManager NoteManager { get; }

    public SelectionManager SelectionManager { get; }
     
    protected DashboardProjectManager? ProjectManager { get;  }
    protected ILocalizationService? LocalizationService { get; }

    protected IEventAggregator? EventAggregator { get; }
    protected IMediator Mediator { get; }
    protected ILifetimeScope? LifetimeScope { get; }
    #endregion

    #region Constructor
    public JotsEditorViewModel(ILogger<JotsEditorViewModel> logger,
        DashboardProjectManager? projectManager,
        NoteManager noteManager,
        SelectionManager selectionManager,
        INavigationService navigationService,
        IEventAggregator? eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService): base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        NoteManager = noteManager;
        SelectionManager = selectionManager;
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
    public string? Message
    {
        get => _message;
        set => Set(ref _message, value);
    }
    #endregion

    public async void Close()
    {
        await this.DeactivateAsync(true);
    }

    #region NoteControl


    public async void CloseNotePaneRequested(object sender, RoutedEventArgs args)
    {
        // TODO:  Jots dialog  
        //NoteControlVisibility = Visibility.Collapsed;
        await this.DeactivateAsync(true);
    }


    public void NoteAdded(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteAddedAsync(e).GetAwaiter());
    }

    public async Task NoteAddedAsync(NoteEventArgs e)
    {
        await Execute.OnUIThreadAsync(async () =>
        {
            //TODO This is a TEMPORARY FIX just for the hotfix, this needs to be resolved by ANDY in the longterm
            e.Note.Labels.Clear();
            await NoteManager.AddNoteAsync(e.Note, e.EntityIds);

            // NB:  What to do here?
            //NotifyOfPropertyChange(() => Items);
        });

        Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";

        Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NoteCreationCount, 1);
    }

    public void NoteAssociationClicked(object sender, NoteEventArgs e)
    {
        Task.Run(() => NoteAssociationClickedAsync(e).GetAwaiter());
    }

    public async Task NoteAssociationClickedAsync(NoteEventArgs e)
    {
       

        // No-op for now
       
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

                // TODO:  Jots
                //NotifyOfPropertyChange(() => Items);
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
        await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds);
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
    }

    public async Task NoteSendToParatextAsync(NoteEventArgs e)
    {
        try
        {
            await NoteManager.SendToParatextAsync(e.Note);
            Message = $"Note '{e.Note.Text}' sent to Paratext.";
            Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NotePushCount, 1);
        }
        catch (Exception ex)
        {
            Message = $"Could not send note to Paratext: {ex.Message}";
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
        if (labelGroup.LabelGroupId != null)
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