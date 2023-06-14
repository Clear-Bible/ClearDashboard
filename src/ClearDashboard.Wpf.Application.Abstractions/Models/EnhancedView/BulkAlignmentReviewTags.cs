
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.Models.EnhancedView
{


    public class BulkAlignment
    {
        public bool IsSelected { get; set; }
        public Alignment? Alignment { get; set; }

        public string? Type { get; set; }

        public string? SourceRef =>
            $"{Alignment.AlignmentId.SourceTokenId.BookNumber} {Alignment.AlignmentId.SourceTokenId.ChapterNumber}:{Alignment.AlignmentId.SourceTokenId.VerseNumber}";

        public string? Text { get; set; }

        public AlignedTokenPairs? AlignedTokenPair { get; set; }

    }
    public class PivotWord
    {
        public string? Word { get; set; }
        public int Count { get; set; }
    }

    public class AlignedWord
    {
        public string? Source { get; set; }

        public string? Target { get; set; }
    }

    public class CountedAlignedWord : AlignedWord
    {
        public int Count { get; set; }

    }

    public static class BulkAlignmentReviewTags
    {
        public const string CountsByTrainingText = "CountByTrainingText";
        public const string CountBySurfaceText = "CountBySurfaceText";
        public const string Source = "Source";
        public const string Target = "Target";
        public const string Machine = "Machine";
        public const string NeedsReview = "NeedsReview";
        public const string Disapproved = "Disapproved";
        public const string Approved = "Approved";
        public const string ApproveSelected = "ApproveSelected";
        public const string DisapproveSelected = "DisapproveSelected";
        public const string MarkSelectedAsNeedsReview = "MarkSelectedAsNeedsReview";
    }
}
