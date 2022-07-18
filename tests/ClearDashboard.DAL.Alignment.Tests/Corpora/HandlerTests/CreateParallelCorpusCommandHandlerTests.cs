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
            var sourceTokenizedCorpusId = new TokenizedCorpusId(new Guid());
            var targetTokenizedCorpusId = new TokenizedCorpusId(new Guid());
            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("Jms", 1, 1) },
                    new List<Verse>() { new Verse("Jms", 1, 1) }
                )
            };

            var command =
                new CreateParallelCorpusCommand(sourceTokenizedCorpusId, targetTokenizedCorpusId, verseMappings);
            var result = await Mediator.Send(command);
            Assert.Equal("mikey", result.Message);
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