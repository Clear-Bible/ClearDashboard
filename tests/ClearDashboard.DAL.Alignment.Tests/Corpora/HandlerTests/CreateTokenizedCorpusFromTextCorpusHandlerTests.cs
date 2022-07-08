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

            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, true, "NameX", "LanguageX", "Standard",
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
}
