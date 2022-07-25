using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;
using Verse = ClearBible.Engine.Corpora.Verse;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateParallelCorpusCommandHandlerTests : TestBase
{
    public CreateParallelCorpusCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__Create()
    {
        try
        {
            var sourceTextCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var sourceCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource");
            var sourceCommand = new CreateTokenizedCorpusFromTextCorpusCommand(sourceTextCorpus, sourceCorpusId,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var sourceCommandResult = await Mediator.Send(sourceCommand);

            var targetTextCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var targetCorpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, false,
                "New Testament 2",
                "grc",
                "Resource");
            var targetCommand = new CreateTokenizedCorpusFromTextCorpusCommand(targetTextCorpus, targetCorpusId,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCommandResult = await Mediator.Send(targetCommand);

            var sourceTokenizedCorpusId =
                ProjectDbContext.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 1").Id;
            var targetTokenizedCorpusId =
                ProjectDbContext.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 2").Id;

            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("Jas", 1, 1) },
                    new List<Verse>() { new Verse("Jas", 1, 1) }
                )
            };

            var command =
                new CreateParallelCorpusCommand(new TokenizedCorpusId(sourceTokenizedCorpusId),
                    new TokenizedCorpusId(targetTokenizedCorpusId), verseMappings);
            var result = await Mediator.Send(command);

            ProjectDbContext.ChangeTracker.Clear();

            // General assertions
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Success", result.Message);
            Assert.NotNull(result.Data);

            // Validate persisted ParallelCorpus data

            Assert.Equal(1, ProjectDbContext.ParallelCorpa.Count());
            var theParallelCorporaEntry = ProjectDbContext.ParallelCorpa
                .Include(pc => pc.VerseMappings)
                .Include(pc => pc.SourceTokenizedCorpus)
                .ThenInclude(stc => stc.Corpus)
                .Include(pc => pc.TargetTokenizedCorpus)
                .ThenInclude(ttc => ttc.Corpus)
                .First();
            Assert.Equal(1, theParallelCorporaEntry.VerseMappings.Count);

            Assert.Equal(sourceTokenizedCorpusId, theParallelCorporaEntry.SourceTokenizedCorpusId);
            Assert.Equal(targetTokenizedCorpusId, theParallelCorporaEntry.TargetTokenizedCorpusId);

            Assert.Equal("New Testament 1", theParallelCorporaEntry.SourceTokenizedCorpus?.Corpus?.Name);
            Assert.Equal("New Testament 2", theParallelCorporaEntry.TargetTokenizedCorpus?.Corpus?.Name);

            // Validate returned ParallelCorpus Data
            var returnedParallelCorpus = result.Data;
            Assert.NotNull(returnedParallelCorpus);
            Assert.NotNull(returnedParallelCorpus.SourceCorpus);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                returnedParallelCorpus.SourceCorpus.Texts.First().GetRows().First().Text);
            Assert.NotNull(returnedParallelCorpus.TargetCorpus);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                returnedParallelCorpus.TargetCorpus.Texts.First().GetRows().First().Text);
            Assert.Equal(1, returnedParallelCorpus.VerseMappingList.Count);

            Assert.Equal(1, returnedParallelCorpus.VerseMappingList.First().SourceVerses.First().ChapterNum);
            Assert.Equal(1, returnedParallelCorpus.VerseMappingList.First().SourceVerses.First().VerseNum);
            Assert.Equal(1, returnedParallelCorpus.VerseMappingList.First().TargetVerses.First().ChapterNum);
            Assert.Equal(1, returnedParallelCorpus.VerseMappingList.First().TargetVerses.First().VerseNum);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__CreateUsingHandler()
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

            Assert.Equal(15, parallelTokenizedCorpus.VerseMappingList?.Count() ?? 0);
            Assert.True(parallelTokenizedCorpus.SourceCorpus.Count() == 15);
            Assert.True(parallelTokenizedCorpus.TargetCorpus.Count() == 15);

        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__Error_Case()
    {
        try
        {
            var sourceTokenizedCorpusId = new TokenizedCorpusId(new Guid());
            var targetTokenizedCorpusId = new TokenizedCorpusId(new Guid());

            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("Jas", 1, 1) },
                    new List<Verse>() { new Verse("Jas", 1, 1) }
                )
            };

            var command =
                new CreateParallelCorpusCommand(new TokenizedCorpusId(sourceTokenizedCorpusId.Id),
                    new TokenizedCorpusId(targetTokenizedCorpusId.Id), verseMappings);
            var result = await Mediator.Send(command);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("SourceTokenizedCorpus not found for TokenizedCorpusId", result.Message);
            Assert.Null(result.Data);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}