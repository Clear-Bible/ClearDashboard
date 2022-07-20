using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class GetTokensByTokenizedCorpusIdAndBookIdHandlerTests : TestBase
{
    public GetTokensByTokenizedCorpusIdAndBookIdHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task GetDataAsync__ValidateResults()
    {
        try
        {
            // Load data
            var textCorpus = TestDataHelpers.GetSampleGreekCorpus();
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, false, "Greek NT", "grc",
                "Resource",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
            await Mediator.Send(command);
            var factory = ServiceProvider.GetService<ProjectDbContextFactory>();
            var context = await factory.GetDatabaseContext(ProjectName);
            //Assert.Equal(context, ProjectDbContext);

            // Retrieve Tokens
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedCorpusId(context.TokenizedCorpora.First().Id), "40");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Validate Matt 1:1
            var matthewCh1V1 = result.Data.First();
            Assert.Equal("1", matthewCh1V1.Chapter);
            Assert.Equal("1", matthewCh1V1.Verse);
            Assert.Equal(9, matthewCh1V1.Tokens.Count());
            Assert.Equal("Βίβλος", matthewCh1V1.Tokens.First().Text);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                String.Join(" ", matthewCh1V1.Tokens.Select(t => t.Text)));

            // Validate Matt 5:9
            var matthewCh5V9 = result.Data.Single(datum => datum.Chapter == "5" && datum.Verse == "9");
            var matthewCh5V9Text = String.Join(" ", matthewCh5V9.Tokens.Select(t => t.Text));
            Assert.Equal("μακάριοι οἱ εἰρηνοποιοί , ὅτι αὐτοὶ υἱοὶ Θεοῦ κληθήσονται .", matthewCh5V9Text);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void GetDataAsync__HandlesError()
    {
        try
        {
            // Retrieve Tokens for a TokenizedCorpus that does not exist
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedCorpusId(new Guid("00000000-0000-0000-0000-000000000000")), "40");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.True(result.Message.StartsWith(
                "System.NullReferenceException: Object reference not set to an instance of an object."));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}