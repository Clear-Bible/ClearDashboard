using ClearBible.Engine.Corpora;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Translation;

public class Alignment
{
    public const AlignmentTypes AssignedAndUnverifiedNotOtherwiseIncluded =
        AlignmentTypes.FromAlignmentModel_Unverified_Not_Otherwise_Included |
        AlignmentTypes.Assigned_Verified |
        AlignmentTypes.Assigned_Unverified |
        AlignmentTypes.Assigned_Invalid;

    public const AlignmentTypes AllAlignmentTypes =
        AlignmentTypes.FromAlignmentModel_Unverified_All |
        AlignmentTypes.Assigned_Verified |
        AlignmentTypes.Assigned_Unverified |
        AlignmentTypes.Assigned_Invalid;

    public const AlignmentTypes AssignedAlignmentTypes =
        AlignmentTypes.Assigned_Verified |
        AlignmentTypes.Assigned_Unverified |
        AlignmentTypes.Assigned_Invalid;

    public AlignmentId? AlignmentId { get; internal set; }
    public AlignedTokenPairs AlignedTokenPair { get; }
    /// <summary>
    /// Valid values are:  "Unverified", "Verified", "Question" only
    /// </summary>
    public string Verification { get; set; }
    /// <summary>
    /// Valid values are:  "FromAlignmentModel", "Assigned" only
    /// </summary>
    public string OriginatedFrom { get; }

    /// <summary>
    /// For domain model use only!
    /// </summary>
    /// <param name="alignmentId"></param>
    /// <param name="alignedTokenPair"></param>
    /// <param name="verification">Valid values are:  "Unverified", "Verified", "Question" only</param>
    /// <param name="originatedFrom">Valid values are:  "FromAlignmentModel", "Assigned" only</param>
    internal Alignment(AlignmentId alignmentId, AlignedTokenPairs alignedTokenPair, string verification, string originatedFrom)
    {
        AlignmentId = alignmentId;
        AlignedTokenPair = alignedTokenPair;
        Verification = verification;
        OriginatedFrom = originatedFrom;
    }

    /// <summary>
    /// For domain model use only!
    /// </summary>
    /// <param name="alignedTokenPair"></param>
    /// <param name="verification">Valid values are:  "Unverified", "Verified", "Question" only</param>
    /// <param name="originatedFrom">Valid values are:  "FromAlignmentModel", "Assigned" only</param>
    internal Alignment(AlignedTokenPairs alignedTokenPair, string verification, string originatedFrom)
    {
        AlignedTokenPair = alignedTokenPair;
        Verification = verification;
        OriginatedFrom = originatedFrom;
    }

    /// <summary>
    /// Use this one to create manual alignments
    /// </summary>
    /// <param name="alignedTokenPair"></param>
    /// <param name="alignmentVerification">Valid values are:  "Unverified", "Verified", "Question" only</param>
    public Alignment(AlignedTokenPairs alignedTokenPair, string verification)
    {
        AlignedTokenPair = alignedTokenPair;
        Verification = verification;
        OriginatedFrom = AlignmentOriginatedFrom.Assigned.ToString();
    }
}