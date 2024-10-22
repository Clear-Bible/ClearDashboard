﻿using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentsByAlignmentSetIdAndTokenIdsQuery(
        AlignmentSetId AlignmentSetId, 
        IEnumerable<EngineParallelTextRow> EngineParallelTextRows, 
        AlignmentTypes AlignmentTypes) : 
        ProjectRequestQuery<IEnumerable<Alignment.Translation.Alignment>>;
}
