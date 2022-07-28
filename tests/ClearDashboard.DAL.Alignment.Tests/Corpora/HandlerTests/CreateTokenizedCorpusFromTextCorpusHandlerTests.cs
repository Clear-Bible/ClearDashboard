using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateTokenizedCorpusFromTextCorpusHandlerTests : TestBase
{
    public CreateTokenizedCorpusFromTextCorpusHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void TokenizedCorpus__Create()
    {
        try
        {
            //Import
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            // Create the corpus in the database:
            var corpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId, tokenizationFunction);

            var result = await Mediator!.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);

            var tokenizedTextCorpus = result.Data;

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var corpus = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.CorpusId.Id);

            Assert.NotNull(corpus);
            Assert.True(corpus.IsRtl);
            Assert.Equal("NameX", corpus.Name);
            Assert.Equal("LanguageX", corpus.Language);
            Assert.Equal("Standard", corpus.CorpusType.ToString());
            Assert.Equal(tokenizationFunction, corpus.TokenizedCorpora.First().TokenizationFunction);

            foreach (var tokensTextRow in tokenizedTextCorpus?.Cast<TokensTextRow>()!)
            {
                // FIXME:  Is it ok if Tokens is empty for a tokensTextRow?
//                Assert.NotEmpty(tokensTextRow.Tokens);
                Assert.All(tokensTextRow.Tokens, t => Assert.NotNull(t.TokenId));

                //display verse info
                var verseRefStr = tokensTextRow.Ref.ToString();
                Output.WriteLine(verseRefStr);

                //display legacy segment
                var segmentText = string.Join(" ", tokensTextRow.Segment);
                Output.WriteLine($"segmentText: {segmentText}");

                //display tokenIds
                var tokenIds = string.Join(" ", tokensTextRow.Tokens.Select(t => t.TokenId.ToString()));
                Output.WriteLine($"tokenIds: {tokenIds}");

                //display tokens tokenized
                var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.Text));
                Output.WriteLine($"tokensText: {tokensText}");

                //display tokens detokenized
                var detokenizer = new LatinWordDetokenizer();
                var tokensTextDetokenized =
                    detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
                Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    public async void TokenizedCorpus__CreateMultiple()
    {
        try
        {
            // Create the corpus in the database:
            var corpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction1 = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus1 = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpusId, tokenizationFunction1);

            Assert.NotNull(tokenizedTextCorpus1);

            var tokenizationFunction2 = ".Tokenize<ZwspWordTokenizer>()";
            var tokenizedTextCorpus2 = await tokenizedTextCorpus1
                .Tokenize<ZwspWordTokenizer>()
                .Create(Mediator!, corpusId, tokenizationFunction1 + tokenizationFunction2);

            Assert.NotNull(tokenizedTextCorpus2);

            var tokenizationFunction3 = ".Tokenize<LineSegmentTokenizer>()";
            var tokenizedTextCorpus3 = await tokenizedTextCorpus1
                .Tokenize<LineSegmentTokenizer>()
                .Create(Mediator!, corpusId, tokenizationFunction1 + tokenizationFunction3);

            Assert.NotNull(tokenizedTextCorpus3);

            ProjectDbContext!.ChangeTracker.Clear();

            var corpus = ProjectDbContext!.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .FirstOrDefault(c => c.Id == corpusId.Id);

            Assert.NotNull(corpus);
            Assert.True(corpus!.IsRtl);
            Assert.Equal("NameX", corpus.Name);
            Assert.Equal("LanguageX", corpus.Language);
            Assert.Equal("Standard", corpus.CorpusType.ToString());
            Assert.True(corpus!.TokenizedCorpora.Count == 3);

            var tokenizedCorpora = corpus.TokenizedCorpora.OrderBy(tc => tc.Created).ToList();

            Assert.NotNull(tokenizedCorpora);
            Assert.Equal(tokenizationFunction1, tokenizedCorpora[0].TokenizationFunction);
            Assert.Equal(tokenizationFunction1 + tokenizationFunction2, tokenizedCorpora[1].TokenizationFunction);
            Assert.Equal(tokenizationFunction1 + tokenizationFunction3, tokenizedCorpora[2].TokenizationFunction);

            tokenizedCorpora.ForEach(tc =>
                {
                    Assert.True(tc.Tokens.Count > 0);
                    Output.WriteLine($"Token count for {tc.TokenizationFunction}: {tc.Tokens.Count}");
                }
            );
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Validate Persistence")]
    public async void TokenizedCorpus__Validate()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleGreekCorpus();
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";

            var corpusId = await TokenizedTextCorpus.CreateCorpus(Mediator!, false, "New Testament", "grc", "Resource");
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId, tokenizationFunction);

            var result = await Mediator.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Like getting a new context.
            // Imitates a fresh runtime process fetching data.
            ProjectDbContext!.ChangeTracker.Clear();

            // Validate correctness of data
            Assert.Equal(1, ProjectDbContext.Corpa.Count());
            Assert.False(ProjectDbContext.Corpa.First().IsRtl);
            Assert.Equal("grc", ProjectDbContext.Corpa.First().Language);
            Assert.Equal("New Testament", ProjectDbContext.Corpa.First().Name);

            var corpora = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .ToList();

            Assert.Single(corpora);
            var corpus = corpora.First();
            Assert.Single(corpus.TokenizedCorpora);
            var tokenizedCorpus = corpus.TokenizedCorpora.First();
            Assert.Equal(tokenizationFunction, tokenizedCorpus.TokenizationFunction);
            Assert.Equal(20723, tokenizedCorpus.Tokens.Count);
            var matthewCh1V1Tokens = tokenizedCorpus.Tokens
                .Where(t => t.BookNumber == 40 && t.ChapterNumber == 1 && t.VerseNumber == 1)
                .OrderBy(t => t.WordNumber);
            Assert.Equal(9, matthewCh1V1Tokens.Count());
            Assert.Equal("Βίβλος", matthewCh1V1Tokens.First().SurfaceText);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Load")]
    public async void TokenizedCorpus__Create__FullNT_x3()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetFullGreekNTCorpus();

            var corpusId1 = await TokenizedTextCorpus.CreateCorpus(Mediator!, false, "New Testament 1", "grc", "Resource");
            var command1 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId1,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");


            var corpusId2 = await TokenizedTextCorpus.CreateCorpus(Mediator!, false, "New Testament 2", "grc", "Resource");
            var command2 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId2,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");


            var corpusId3 = await TokenizedTextCorpus.CreateCorpus(Mediator!, false, "New Testament 3", "grc", "Resource");
            var command3 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId3,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var result1 = await Mediator.Send(command1);
            var result2 = await Mediator.Send(command2);
            var result3 = await Mediator.Send(command3);

            ProjectDbContext.ChangeTracker.Clear();

            var corpusNT1 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .FirstOrDefault(c => c.Name == "New Testament 1");

            var corpusNT2 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .FirstOrDefault(c => c.Name == "New Testament 2");

            var corpusNT3 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .FirstOrDefault(c => c.Name == "New Testament 3");

            Assert.Equal(3, ProjectDbContext.Corpa.Count());
            Assert.Equal(1, corpusNT1?.TokenizedCorpora.Count);
            Assert.Equal(157590, corpusNT1?.TokenizedCorpora.First().Tokens.Count);
            Assert.Equal(157590, corpusNT2?.TokenizedCorpora.First().Tokens.Count);
            Assert.Equal(157590, corpusNT3?.TokenizedCorpora.First().Tokens.Count);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handler")]
    public async void TokenizedCorpus__InvalidCorpusId()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleGreekCorpus();
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, new CorpusId(new Guid()), "bogus");

            var result = await Mediator!.Send(command);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Output.WriteLine(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}