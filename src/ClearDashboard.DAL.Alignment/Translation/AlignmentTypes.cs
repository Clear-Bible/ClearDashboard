using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Translation;

[Flags]
public enum AlignmentTypes
{
    None = 0,
    FromAlignmentModel_Unverified_Not_Otherwise_Included = 1,
    FromAlignmentModel_Unverified_All = 2,
    Assigned_Unverified = 4,
    Assigned_Verified = 8,
    Assigned_Invalid = 16,

    AssignedAndUnverifiedNotOtherwiseIncluded =
        FromAlignmentModel_Unverified_Not_Otherwise_Included |
        Assigned_Verified |
        Assigned_Unverified |
        Assigned_Invalid,

    AllAlignmentTypes =
        FromAlignmentModel_Unverified_All |
        Assigned_Verified |
        Assigned_Unverified |
        Assigned_Invalid,

    AssignedAlignmentTypes =
        Assigned_Verified |
        Assigned_Unverified |
        Assigned_Invalid
}
