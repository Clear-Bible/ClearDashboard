﻿using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record GetLabelGroupNamesLabelsByNamesQuery(IEnumerable<string>? LabelGroupNames) : ProjectRequestQuery<IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>;
}