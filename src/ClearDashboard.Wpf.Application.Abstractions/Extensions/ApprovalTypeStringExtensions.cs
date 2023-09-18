using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Models.EnhancedView;

namespace ClearDashboard.Wpf.Application.Extensions;

public static class ApprovalTypeStringExtensions 
{
    public static string DetermineAlignmentVerificationStatus(this string? approvalType)
    {
        switch (approvalType)
        {
            case BulkAlignmentReviewTags.MarkSelectedAsValid:
                return AlignmentVerificationStatus.Verified;
            case BulkAlignmentReviewTags.MarkSelectedAsInvalid:
                return AlignmentVerificationStatus.Invalid;
            case BulkAlignmentReviewTags.MarkSelectedAsNeedsReview:
                return AlignmentVerificationStatus.Unverified;
            default:
                return AlignmentVerificationStatus.Unverified;
        }
    }
}