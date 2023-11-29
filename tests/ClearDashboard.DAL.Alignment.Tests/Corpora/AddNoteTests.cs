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
        [Trait("Requires", "Paratext ZZ_SUR on test machine, paratextprojectid 2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f")]
        public async void AddNoteToSelectionAndVerse()
        {
            try
            {
                CancellationTokenSource cancellationSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationSource.Token;

                var textCorpus = (await ParatextProjectTextCorpus.Get(Mediator!, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", null, cancellationToken))
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

                //verify in paratext that a note shows up for the entire verse Gen1:2.
                addNoteCommandParam.SetProperties(
                    "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    tokensTextRow.Tokens, 
                    tokensTextRow.Tokens.Where((t,i) => false), 
                    new EngineStringDetokenizer(new WhitespaceDetokenizer()), 
                    "Whole verse note",
                    new List<Label>(),
                    tokensTextRow.Tokens.First().TokenId.BookNumber,
                    tokensTextRow.Tokens.First().TokenId.ChapterNumber,
                    tokensTextRow.Tokens.First().TokenId.VerseNumber
                    );

                var result = await Mediator!.Send(new AddNoteCommand(addNoteCommandParam));
                if (result.Success)
                {
                    var data = result.Data;
                    Assert.NotNull(data);
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }

                //verify in paratext that a note shows up for the third occurance of the word 'ni' in Gen1:2.
                addNoteCommandParam.SetProperties(
                    "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    tokensTextRow.Tokens,
                    tokensTextRow.Tokens.Where((t, i) => i == 15),
                    new EngineStringDetokenizer(new WhitespaceDetokenizer()),
                    "third occurance of the word 'ni'",
                    new List<Label>(),
                    tokensTextRow.Tokens.First().TokenId.BookNumber,
                    tokensTextRow.Tokens.First().TokenId.ChapterNumber,
                    tokensTextRow.Tokens.First().TokenId.VerseNumber
                    );

                result = await Mediator!.Send(new AddNoteCommand(addNoteCommandParam));
                if (result.Success)
                {
                    var data = result.Data;
                    Assert.NotNull(data);
                }
                else
                {
                    throw new MediatorErrorEngineException(result.Message);
                }

                //verify in paratext that no note shows up in Gen 1:2 since the selected text is non-contiguous.
                addNoteCommandParam.SetProperties(
                    "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    tokensTextRow.Tokens,
                    tokensTextRow.Tokens.Where((t, i) => i == 15 || i == 16 || i == 18),
                    new EngineStringDetokenizer(new WhitespaceDetokenizer()),
                    "shouldn't show up as note",
                    new List<Label>(),
                    tokensTextRow.Tokens.First().TokenId.BookNumber,
                    tokensTextRow.Tokens.First().TokenId.ChapterNumber,
                    tokensTextRow.Tokens.First().TokenId.VerseNumber
                    );

                result = await Mediator!.Send(new AddNoteCommand(addNoteCommandParam));
                Assert.False(result.Success);

                //verify in paratext that a note shows up for the 15th, 16th, and 17th words 'ni a nkoo' of Gen1:2.
                addNoteCommandParam.SetProperties(
                    "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",
                    tokensTextRow.Tokens,
                    tokensTextRow.Tokens.Where((t, i) => i == 15 || i == 16 || i == 17),
                    new EngineStringDetokenizer(new WhitespaceDetokenizer()),
                    "ni a nkoo",
                    new List<Label>(),
                    tokensTextRow.Tokens.First().TokenId.BookNumber,
                    tokensTextRow.Tokens.First().TokenId.ChapterNumber,
                    tokensTextRow.Tokens.First().TokenId.VerseNumber
                    );

                result = await Mediator!.Send(new AddNoteCommand(addNoteCommandParam));
                if (result.Success)
                {
                    var data = result.Data;
                    Assert.NotNull(data);
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
