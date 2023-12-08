using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Notes;

public class ExternalLabel
{
    public int ExternalLabelId { get; set; }
    public string ExternalProjectId { get; set; }
    public string ExternalProjectName { get; set; }
    public string ExternalText { get; set; }

    public string ExternalTemplate { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ExternalLabel label &&
               ExternalLabelId == label.ExternalLabelId &&
               ExternalProjectId == label.ExternalProjectId;
    }

    public override int GetHashCode()
    {
        int hashCode = -2028607509;
        hashCode = hashCode * -1521134295 + ExternalLabelId.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExternalProjectId);
        return hashCode;
    }
}

