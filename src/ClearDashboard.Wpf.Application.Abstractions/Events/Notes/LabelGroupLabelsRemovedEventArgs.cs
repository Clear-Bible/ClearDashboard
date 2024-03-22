using System.Collections.Generic;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;

namespace ClearDashboard.Wpf.Application.Events.Notes;

public class LabelGroupLabelsRemovedEventArgs : LabelGroupEventArgs
{
    public List<Label>? Labels { get; set; }

    /// <summary>
    /// The <see cref="LabelGroupViewModel"/> to which the event pertains.
    /// </summary>
    public LabelGroupViewModel? NoneLabelGroup { get; set; }
}