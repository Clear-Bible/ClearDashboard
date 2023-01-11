using System;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView;

public abstract class EnhancedViewItemMetadatum
{
    public bool? IsNewWindow { get; set; }

    public virtual Type GetEnhancedViewItemMetadatumType()
    {
        return GetType();
    }
}