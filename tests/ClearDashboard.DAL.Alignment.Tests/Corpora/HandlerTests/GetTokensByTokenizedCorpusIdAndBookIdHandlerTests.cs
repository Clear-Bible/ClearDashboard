using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
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
    #nullable disable
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
            var corpus = await Corpus.Create(Mediator!, false, "Greek NT", "grc", "Resource");
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId,
                "Unit Test",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()",
                ScrVers.Original);
            var createResult = await Mediator!.Send(command);
            Assert.True(createResult.Success);
            Assert.NotNull(createResult.Data);
            var tokenizedTextCorpus = createResult.Data!;

            ProjectDbContext?.ChangeTracker.Clear();

            // Retrieve Tokens
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(tokenizedTextCorpus.TokenizedTextCorpusId, "MAT");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);


            // Validate Matt 1:1
            var matthewCh1V1 = result.Data.First();
            Assert.Equal("1", matthewCh1V1.Chapter);
            Assert.Equal("1", matthewCh1V1.Verse);
            Assert.Equal(9, matthewCh1V1.Tokens.Count());
            Assert.Equal("Βίβλος", matthewCh1V1.Tokens.First().SurfaceText);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                String.Join(" ", matthewCh1V1.Tokens.Select(t => t.SurfaceText)));

            // Validate Matt 5:9
            var matthewCh5V9 = result.Data.Single(datum => datum.Chapter == "5" && datum.Verse == "9");
            var matthewCh5V9Text = String.Join(" ", matthewCh5V9.Tokens.Select(t => t.SurfaceText));
            Assert.Equal("μακάριοι οἱ εἰρηνοποιοί , ὅτι αὐτοὶ υἱοὶ Θεοῦ κληθήσονται .", matthewCh5V9Text);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void GetDataAsync__HandlesInvalidBookAbbreviation()
    {
        try
        {
            // Load data
            var textCorpus = TestDataHelpers.GetFullGreekNTCorpus();
            var corpus = await Corpus.Create(Mediator!, false, "Greek NT", "grc", "Resource");
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId,
                "Unit Test",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()",
                ScrVers.Original);
            await Mediator.Send(command);


            ProjectDbContext.ChangeTracker.Clear();

            // Retrieve Tokens
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedTextCorpusId(ProjectDbContext.TokenizedCorpora.First().Id), "BARF");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.StartsWith(
                "System.Exception: Unable to map book abbreviation: BARF to book number.",
                result.Message
            );
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }


    [Fact]
    [Trait("Category", "Handlers")]
    public async void GetDataAsync__HandlesMissingTokenizedCorpus()
    {
        try
        {
            ProjectDbContext.ChangeTracker.Clear();

            // Retrieve Tokens
            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                new Alignment.Corpora.TokenizedTextCorpusId(new Guid("00000000-0000-0000-0000-000000000000")), "MRK");
            var result = await Mediator.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.StartsWith(
                "System.Exception: Tokenized Corpus 00000000-0000-0000-0000-000000000000 does not exist.",
                result.Message);
            Assert.Null(result.Data);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}