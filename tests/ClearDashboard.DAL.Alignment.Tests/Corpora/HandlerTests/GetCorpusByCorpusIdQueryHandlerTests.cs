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

public class GetCorpusByCorpusIdQueryHandlerTests : TestBase
{
    public GetCorpusByCorpusIdQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Corpus__CreateCorpusThenQuery()
    {
        try
        {
            var command = new CreateCorpusCommand(true, "ff", "a name", "a language", "StudyBible", Guid.NewGuid().ToString());
            var createResult = await Mediator!.Send(command);
            Assert.True(createResult.Success);
            Assert.NotNull(createResult.Data);

            var corpusId = createResult.Data!;

            // Clear ProjectDbContext:
            ProjectDbContext!.ChangeTracker.Clear();

            var query = new GetCorpusByCorpusIdQuery(corpusId);
            var queryResult = await Mediator!.Send(query);

            Assert.True(queryResult.Success);
            Assert.NotNull(queryResult.Data);

            var corpus = queryResult.Data!;

            Assert.True(corpus.CorpusId.IsRtl);
            Assert.Equal("a name", corpus.CorpusId.Name);
            Assert.Equal("a language", corpus.CorpusId.Language);
            Assert.NotNull(corpus.CorpusId.ParatextGuid);
            Assert.Equal(Models.CorpusType.StudyBible.ToString(), corpus.CorpusId.CorpusType);
            Assert.Empty(corpus.CorpusId.Metadata);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void Corpus__InvalidCorpusId()
    {
        try
        {
            var query = new GetCorpusByCorpusIdQuery(new CorpusId(Guid.NewGuid()));

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
