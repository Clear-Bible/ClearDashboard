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
            Output.WriteLine(ProjectDbContext?.TokenizedCorpora.First().Id.ToString());
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedCorpusId(ProjectDbContext.TokenizedCorpora.First().Id), "40");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("1", result.Data.First().chapter);
            Assert.Equal("1", result.Data.First().verse);
            Assert.Equal(9, result.Data.First().tokens.Count());
            Assert.Equal("Βίβλος", result.Data.First().tokens.First().Text);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                String.Join(" ", result.Data.First().tokens.Select(t => t.Text)));
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
            Assert.Equal("Object reference not set to an instance of an object.", result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}