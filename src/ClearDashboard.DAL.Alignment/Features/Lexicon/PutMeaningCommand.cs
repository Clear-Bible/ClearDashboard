﻿using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public record PutMeaningCommand(
        LexemeId LexemeId,
        Meaning Meaning) : ProjectRequestCommand<MeaningId>;
}