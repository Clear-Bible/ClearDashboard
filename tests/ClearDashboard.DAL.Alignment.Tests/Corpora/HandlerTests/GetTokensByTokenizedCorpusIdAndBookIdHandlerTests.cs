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
    public async void GetDataAsync__ValidateResults()
    {
        try
        {
            // Load data
            var textCorpus = TestDataHelpers.GetSampleGreekCorpus();
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, false, "Greek NT", "grc",
                "Resource",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
            await Mediator.Send(command);

            // Retrieve Tokens
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedCorpusId(ProjectDbContext.TokenizedCorpora.First().Id), "40");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Validate Matt 1:1
            var matthewCh1V1 = result.Data.First();
            Assert.Equal("1", matthewCh1V1.chapter);
            Assert.Equal("1", matthewCh1V1.verse);
            Assert.Equal(9, matthewCh1V1.tokens.Count());
            Assert.Equal("Βίβλος", matthewCh1V1.tokens.First().Text);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                String.Join(" ", matthewCh1V1.tokens.Select(t => t.Text)));

            // Validate Matt 5:9
            var matthewCh5V9 = result.Data.Single(datum => datum.chapter == "5" && datum.verse == "9");
            var matthewCh5V9Text = String.Join(" ", matthewCh5V9.tokens.Select(t => t.Text));
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