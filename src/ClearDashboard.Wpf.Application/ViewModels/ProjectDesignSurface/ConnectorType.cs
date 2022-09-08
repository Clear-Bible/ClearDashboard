using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    /// <summary>
    /// Defines the type of a connector (aka connection point).
    /// </summary>
    public enum ConnectorType
    {
        Undefined,
        Input,
        Output,
    }

    public enum ParatextProjectType
    {
        Standard,
        BackTranslation,
        Reference,
        Other
    }
}
