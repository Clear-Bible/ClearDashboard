
namespace ClearDashboard.DAL.Alignment.Translation;

[Flags]
public enum AlignmentTypes
{
    None = 0,
    /// <summary>
    /// Machine-generated, unverified alignments that do not reference source or target
    /// tokens also referenced by any of the AlignmentOriginatedFrom: 'Assigned' alignments returned.
    /// (AlignmentOriginatedFrom: FromAlignmentModel, AlignmentVerification: Unverified)
    /// </summary>
    FromAlignmentModel_Unverified_Not_Otherwise_Included = 1,
    /// <summary>
    /// All machine-generated, unverified alignments
    /// (AlignmentOriginatedFrom: FromAlignmentModel, AlignmentVerification: Unverified)
    /// </summary>
    FromAlignmentModel_Unverified_All = 2,
    /// <summary>
    /// Manual alignments that need approval
    /// (AlignmentOriginatedFrom: Assigned and AlignmentVerification: Unverifed)
    /// </summary>
    Assigned_Unverified = 4,
    /// <summary>
    /// Manual alignments that have been approved
    /// (AlignmentOriginatedFrom: Assigned and AlignmentVerification: Verifed)
    /// </summary>
    Assigned_Verified = 8,
    /// <summary>
    /// Manual alignments that have been disapproved
    /// (AlignmentOriginatedFrom: Assigned and AlignmentVerification: Invalid)
    /// </summary>
    Assigned_Invalid = 16
}

public class AlignmentTypeGroups
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
}
