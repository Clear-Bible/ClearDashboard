using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Collaboration.Services;

public enum MergeMode
{
    RemoteOverridesLocal,
    LocalOverridesRemote,
    RequiresConflictResolution
}
