using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
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
    #nullable disable
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

            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, true, "NameX", "LanguageX",
                "Standard",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var result = await Mediator.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.All(result.Data, tc => Assert.IsType<TokensTextRow>(tc));

            foreach (var tokensTextRow in result.Data?.Cast<TokensTextRow>()!)
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
    [Trait("Category", "Validate Persistence")]
    public async void Corpus__ValidateTokenizedCorpus()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, false, "New Testament", "grc",
                "Resource",
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var result = await Mediator.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            // Like getting a new context.
            // Imitates a fresh runtime process fetching data.
            ProjectDbContext?.ChangeTracker.Clear();

            // Validate correctness of data
            Assert.Equal(1, ProjectDbContext?.Corpa.Count());
            Assert.False(ProjectDbContext?.Corpa.First().IsRtl);
            Assert.Equal("grc", ProjectDbContext?.Corpa.First().Language);
            Assert.Equal("New Testament", ProjectDbContext?.Corpa.First().Name);
            Assert.Equal(".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()",
                ProjectDbContext?.Corpa.First().Metadata["TokenizationQueryString"].ToString());

            var corpora = ProjectDbContext?.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .ToList();

            Assert.Single(corpora);
            var corpus = corpora.First();
            Assert.Single(corpus.TokenizedCorpora);
            var tokenizedCorpus = corpus.TokenizedCorpora.First();
            Assert.Equal(20723, tokenizedCorpus.Tokens.Count);
            var matthewCh1V1Tokens = tokenizedCorpus.Tokens
                .Where(t => t.BookNumber == 40 && t.ChapterNumber == 1 && t.VerseNumber == 1)
                .OrderBy(t => t.WordNumber);
            Assert.Equal(9, matthewCh1V1Tokens.Count());
            Assert.Equal("Βίβλος", matthewCh1V1Tokens.First().Text);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}