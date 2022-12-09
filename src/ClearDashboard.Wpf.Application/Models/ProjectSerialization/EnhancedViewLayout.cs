using System.Collections.Generic;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization;

public class EnhancedViewLayout
{
    public EnhancedViewLayout()
    {
        EnhancedViewItems = new BindableCollection<EnhancedViewItemMetadatum>();
    }
    public string? Title { get; set; } = string.Empty;
    public bool ParatextSync { get; set; }
    public string? BBBCCCVVV { get; set; } = "001001001";
    public BindableCollection<EnhancedViewItemMetadatum> EnhancedViewItems { get; set; } 
    public int VerseOffset { get; set; }
}