﻿using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS.Features;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public record GetTokenVerseContextQuery(ParallelCorpusId? ParallelCorpusId, Token Token) : 
        ProjectRequestQuery<(IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)>;
}