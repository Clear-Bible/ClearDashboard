using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DataAccessLayer.Models;
using Token = ClearBible.Engine.Corpora.Token;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    public class AddNoteTests: TestBase
    {
        public AddNoteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async void AddNoteToSingleToken()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var textCorpus = (await ParatextProjectTextCorpus.Get(Mediator!, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", cancellationToken))
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();


                var tokensTextRow = textCorpus.Cast<TokensTextRow>().Skip(1).First();

                //display verse info
                var verseRefStr = tokensTextRow.Ref.ToString();
                Output.WriteLine(verseRefStr);

                //display tokenIds
                var tokenIds = string.Join(" ", tokensTextRow.Tokens.GetPositionalSortedBaseTokens().Select(t => t.TokenId.ToString()));
                Output.WriteLine($"tokenIds: {tokenIds}");

                //display tokens tokenized
                var tokensText = string.Join(" ", tokensTextRow.Tokens.GetPositionalSortedBaseTokens().Select(t => t.SurfaceText));
                Output.WriteLine($"tokensText: {tokensText}");

                //display tokens detokenized
                var detokenizer = new LatinWordDetokenizer();
                var tokensTextDetokenized =
                    detokenizer.Detokenize(tokensTextRow.Tokens.GetPositionalSortedBaseTokens().Select(t => t.SurfaceText).ToList());
                Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");

                AddNoteCommandParam addNoteCommandParam = new AddNoteCommandParam();
                addNoteCommandParam.SetProperties(
                    "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    tokensTextRow.Tokens, 
                    tokensTextRow.Tokens.Where((t,i) => i == 7), 
                    new EngineStringDetokenizer(new WhitespaceDetokenizer()), 
                    "howdy!",
                    tokensTextRow.Tokens.First().TokenId.BookNumber,
                    tokensTextRow.Tokens.First().TokenId.ChapterNumber,
                    tokensTextRow.Tokens.First().TokenId.VerseNumber
                    );

                var result = await Mediator!.Send(new AddNoteCommand(addNoteCommandParam));
                if (result.Success && result.Data != null)
                {
                    var data = result.Data;
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }
    }
}
