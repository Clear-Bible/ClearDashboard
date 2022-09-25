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
using System.Diagnostics;
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
            var corpus = await Corpus.Create(Mediator!, false, "Greek NT", "grc", "Resource", Guid.NewGuid().ToString());
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
            var corpus = await Corpus.Create(Mediator!, false, "Greek NT", "grc", "Resource", Guid.NewGuid().ToString());
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

    [Fact]
    [Trait("Category", "Handlers")]
    public async void GetDataAsync__LargeCorpusWithComposites_MeasurePerformance()
    {
        try
        {
            var textCorpus = CreateFakeTextCorpusWithComposite(false);

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Output.WriteLine($"Private memory usage (BEFORE): {proc.PrivateMemorySize64}");

            var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(
                tokenizedTextCorpus.TokenizedTextCorpusId, "GEN");
            var result = await Mediator.Send(query);

            proc.Refresh();
            Output.WriteLine($"Private memory usage (AFTER):  {proc.PrivateMemorySize64}");

            sw.Stop();
            Output.WriteLine("Elapsed={0}", sw.Elapsed);

            //foreach (var verseTokens in result.Data)
            //{
            //    Output.WriteLine($"Chapter {verseTokens.Chapter}, verse {verseTokens.Verse}");
            //    foreach (var token in verseTokens.Tokens)
            //    {
            //        Output.WriteLine($"{token.TokenId}");
            //    }
            //}
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private static ITextCorpus CreateFakeTextCorpusWithComposite(bool includeBadCompositeToken)
    {
        var textCorpus = new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
            .Tokenize<LatinWordTokenizer>()
            .Transform<IntoFakeCompositeTokensTextRowProcessor>()
            .Transform<SetTrainingBySurfaceLowercase>();

        return textCorpus;
    }

    private class IntoFakeCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("wuri ɓàk am ɗi") || textRow.Text.Contains("ɗi moo put"))
            {
                return GenerateTokensTextRow(textRow);
            }

            return new TokensTextRow(textRow);
        }
    }

    private static TokensTextRow GenerateTokensTextRow(TextRow textRow)
    {
        var tr = new TokensTextRow(textRow);

        var tokens = tr.Tokens;

        var tokenIds = tokens
            .Select(t => t.TokenId)
            .ToList();

        var compositeTokens = new List<Token>() { tokens[0], tokens[1], tokens[3], tokens[7] };
        var tokensWithComposite = new List<Token>()
                 {
                     new CompositeToken(compositeTokens),
                     tokens[2],
                     tokens[4],
                     tokens[5],
                     tokens[6]
                 };

        tokensWithComposite.AddRange(tokens.GetRange(8, tokens.Count - 8));

        tr.Tokens = tokensWithComposite;
        return tr;
    }
}