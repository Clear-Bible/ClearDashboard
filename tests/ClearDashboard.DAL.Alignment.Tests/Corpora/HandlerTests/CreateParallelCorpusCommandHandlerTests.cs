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

            var sourceCommand = new CreateTokenizedCorpusFromTextCorpusCommand(sourceTextCorpus, false,
                "New Testament 1",
                "grc",
                "Resource",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var sourceCommandResult = await Mediator.Send(sourceCommand);

            var targetTextCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var targetCommand = new CreateTokenizedCorpusFromTextCorpusCommand(targetTextCorpus, false,
                "New Testament 2",
                "grc",
                "Resource",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCommandResult = await Mediator.Send(targetCommand);

            var sourceTokenizedCorpusId =
                ProjectDbContext.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 1").Id;
            var targetTokenizedCorpusId =
                ProjectDbContext.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 2").Id;

            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("Jms", 1, 1) },
                    new List<Verse>() { new Verse("Jms", 1, 1) }
                )
            };

            var command =
                new CreateParallelCorpusCommand(new TokenizedCorpusId(sourceTokenizedCorpusId),
                    new TokenizedCorpusId(targetTokenizedCorpusId), verseMappings);
            var result = await Mediator.Send(command);
            Output.WriteLine(result.Message);
            Assert.Equal("That was a pain.", result.Message);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}