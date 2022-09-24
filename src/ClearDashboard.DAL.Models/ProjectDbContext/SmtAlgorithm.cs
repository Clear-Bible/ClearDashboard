using System.ComponentModel;

namespace ClearDashboard.DataAccessLayer.Models;

public enum SmtAlgorithm
{
    [Description("Fast Align")]
    FastAlign,
    [Description("IBM 4")]
    IBM4,
    [Description("IBM 1")]
    IBM1,
    [Description("HMM")]
    HMM,
}