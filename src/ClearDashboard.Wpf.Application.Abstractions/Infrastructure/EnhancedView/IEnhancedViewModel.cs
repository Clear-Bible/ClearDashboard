

using ClearDashboard.DAL.ViewModels;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Infrastructure.EnhancedView
{
    public interface IEnhancedViewModel
    {
        string? Title { get; }
        Dictionary<string, string> BcvDictionary { get; set; }
        BookChapterVerseViewModel CurrentBcv { get; set; }
        int VerseOffsetRange { get; set; }
        bool ParagraphMode { get; set; }
        bool ShowExternalNotes { get; set; }
    }
}
