using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class GetParallelCorpusByParallelCorpusIdQueryHandlerTests : TestBase
{
    public GetParallelCorpusByParallelCorpusIdQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__GetHandlerWithTokens()
    {
        try
        {
            var sourceCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameX", "LanguageX", "Standard");
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameY", "LanguageY", "StudyBible");
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            var sourceTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.Tokens)
                .First(tc => tc.Id == sourceTokenizedTextCorpus.TokenizedCorpusId.Id);
            var targetTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.Tokens)
                .First(tc => tc.Id == targetTokenizedTextCorpus.TokenizedCorpusId.Id);

            var sourceTokenGuidIds = sourceTokenizedCorpus.Tokens
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber));
            var targetTokenGuidIds = targetTokenizedCorpus.Tokens
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber));

            Assert.True(sourceTokenGuidIds.Keys.Count > 100);
            Assert.True(targetTokenGuidIds.Keys.Count > 100);
            Assert.Empty(sourceTokenGuidIds.Keys.Intersect(targetTokenGuidIds.Keys));

            Assert.NotNull(parallelTextCorpus.VerseMappingList);
            Assert.True(parallelTextCorpus.VerseMappingList!.Count >= 5);

            // Take the verse mapping list and recreate it but with TokenIds added to each set of verses
            int vmi = 0;
            parallelTextCorpus.VerseMappingList =
                parallelTextCorpus.VerseMappingList!.Take(5).Select(vm =>
                {
                    vmi++;
                    int svi = 0, tvi = 0;
                    return new VerseMapping(
                        vm.SourceVerses.Select(v =>
                        {
                            svi++;
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, sourceTokenGuidIds.Values.Skip(vmi * svi).Take(vmi));
                        }).ToList(),
                        vm.TargetVerses.Select(v =>
                        {
                            tvi++;
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, targetTokenGuidIds.Values.Skip(vmi * tvi * 4).Take(vmi));
                        }).ToList()
                    ); ;
                }
                ).ToList();

            // Save:
            var parallelTokenizedCorpus = await parallelTextCorpus.Create(Mediator!);

            // Clear ProjectDbContext:
            ProjectDbContext.ChangeTracker.Clear();

            // Get:
            var query = new GetParallelCorpusByParallelCorpusIdQuery(parallelTokenizedCorpus.ParallelCorpusId);
            var queryResult = await Mediator!.Send(query);

            // Validate:
            Assert.NotNull(queryResult);
            Assert.True(queryResult.Success);
            Assert.Equal(sourceTokenizedTextCorpus.TokenizedCorpusId, queryResult.Data.sourceTokenizedCorpusId);
            Assert.Equal(targetTokenizedTextCorpus.TokenizedCorpusId, queryResult.Data.targetTokenizedCorpusId);
            Assert.Equal(5, queryResult.Data.verseMappings.Count());

            Assert.Empty(queryResult.Data.verseMappings
                .SelectMany(vm => vm.SourceVerses)
                .Select(v => (v.Book, v.ChapterNum, v.VerseNum ))
                .Except(parallelTextCorpus.VerseMappingList
                .SelectMany(vm => vm.SourceVerses)
                .Select(v => (v.Book, v.ChapterNum, v.VerseNum))));

            var sourceTokenIds = queryResult.Data.verseMappings
                .SelectMany(vm => vm.SourceVerses
                .SelectMany(sv => sv.TokenIds));
            var targetTokenIds = queryResult.Data.verseMappings
                .SelectMany(vm => vm.TargetVerses
                .SelectMany(tva => tva.TokenIds));

            Assert.NotEqual(sourceTokenIds, targetTokenIds);
            Assert.Empty(sourceTokenIds.Except(sourceTokenGuidIds.Values));
            Assert.Empty(targetTokenIds.Except(targetTokenGuidIds.Values));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds__InvalidParallelCorpusId()
    {
        try
        {
            var query = new GetParallelCorpusByParallelCorpusIdQuery(new ParallelCorpusId(Guid.NewGuid()));

            var result = await Mediator!.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
            Output.WriteLine(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds__NullTokenizedCorpus()
    {
        try
        {
            var sourceCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameX", "LanguageX", "Standard");
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameY", "LanguageY", "StudyBible");
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            var parallelTokenizedCorpus = await parallelTextCorpus.Create(Mediator!);

            var parallelCorpusDB = ProjectDbContext!.ParallelCorpa.FirstOrDefault(pc => pc.Id == parallelTokenizedCorpus.ParallelCorpusId.Id);
            Assert.NotNull(parallelCorpusDB);

            // Remove the source tokenized corpus:
            parallelCorpusDB!.SourceTokenizedCorpus = null;

            // Commit to database:
            await ProjectDbContext.SaveChangesAsync();

            var query = new GetParallelCorpusByParallelCorpusIdQuery(parallelTokenizedCorpus.ParallelCorpusId);

            var result = await Mediator!.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
            Output.WriteLine(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}
