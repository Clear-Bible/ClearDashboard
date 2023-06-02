﻿using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetAlignmentVerseContextsQuery(AlignmentSetId AlignmentSetId, string SourceTokenTrainingText, string TargetTokenTrainingText, AlignmentTypes AlignmentTypesToInclude) : 
        ProjectRequestQuery<IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )>>;
}
