﻿using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public record DeleteLabelAndAssociationsByLabelIdCommand(
        LabelId LabelId) : ProjectRequestCommand<object>;
}
