

namespace ClearDashboard.Wpf.Application.Models.EnhancedView
{
    public class PivotWord
    {
        public string? Word { get; set; }
        public int Count { get; set; }
    }

    public static class BulkAlignmentReviewTags
    {
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
