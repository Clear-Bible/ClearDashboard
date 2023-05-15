using System;
using System.Threading;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using SIL.Extensions;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public static class ParallelCorpusDataBuilder
{
    public static Models.ParallelCorpus BuildParallelCorpus(
        Guid parallelCorpusId,
        Models.TokenizedCorpus sourceTokenizedCorpus,
        Models.TokenizedCorpus targetTokenizedCorpus,
        IEnumerable<VerseMapping> verseMappings,
        string? displayName,
        CancellationToken cancellationToken)
    {
        var parallelCorpusModel = new Models.ParallelCorpus
        {
            Id = parallelCorpusId,
            SourceTokenizedCorpusId = sourceTokenizedCorpus.Id,
            TargetTokenizedCorpusId = targetTokenizedCorpus.Id,
            DisplayName = displayName
        };

        parallelCorpusModel.VerseMappings = verseMappings
            .Select(vm =>
            {
                var verseMapping = new Models.VerseMapping
                {
                    ParallelCorpusId = parallelCorpusId
                };

                verseMapping.Verses.AddRange(BuildVerses(vm.SourceVerses, parallelCorpusId, sourceTokenizedCorpus.CorpusId, cancellationToken));
                verseMapping.Verses.AddRange(BuildVerses(vm.TargetVerses, parallelCorpusId, targetTokenizedCorpus.CorpusId, cancellationToken));

                return verseMapping;
            })
            .ToList();

        return parallelCorpusModel;
    }

    public static IEnumerable<Models.Verse> BuildVerses(IEnumerable<Verse> verses, Guid parallelCorpusId, Guid corpusId, CancellationToken cancellationToken)
    {
        return verses
            .Select(v =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!ModelHelper.BookAbbreviationsToNumbers.TryGetValue(v.Book, out int bookNumber))
                {
                    throw new NullReferenceException($"Invalid book '{v.Book}' found in engine verse. ");
                }
                var verse = new Models.Verse
                {
                    VerseNumber = v.VerseNum,
                    BookNumber = bookNumber,
                    ChapterNumber = v.ChapterNum,
                    CorpusId = corpusId,
                    ParallelCorpusId = parallelCorpusId,
                    BBBCCCVVV = $"{bookNumber:000}{v.ChapterNum:000}{v.VerseNum:000}"
                };
                if (v.TokenIds.Any())
                {
                    verse.TokenVerseAssociations.AddRange(v.TokenIds.Select((td, index) => new Models.TokenVerseAssociation
                    {
                        TokenComponentId = td.Id,
                        Position = index
                    }));
                }
                return verse;
            })
            .ToList();
    }
}
