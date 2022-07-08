using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers;
using ClearDashboard.DAL.Alignment.Tests.Tokenization;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    public class CorpusTests : TestBase
    {
        public CorpusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__ImportFromUsfm_SaveToDb()
        {
            try
            {
                //Import
                var corpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
                // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
                var tokenizedTextCorpus = await corpus.Create(Mediator, true, "NameX", "LanguageX", "LanguageType",
                    ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                foreach (var tokensTextRow in tokenizedTextCorpus.Cast<TokensTextRow>())
                {
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
                //await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetAllCorpusVersionIds()
        {
            var corpusVersionIds = await TokenizedTextCorpus.GetAllCorpusIds(Mediator);
            Assert.True(corpusVersionIds.Count() > 0);
        }

        [Fact]
        [Trait("Category", "Example")]
        // Mike notices that this test is broken.
        public async void Corpus__GetAllTokenizedCorpusIds()
        {
            var corpusIds = await TokenizedTextCorpus.GetAllCorpusIds(Mediator);
            Assert.True(corpusIds.Count() > 0);

            var tokenizedCorpusIds = await TokenizedTextCorpus.GetAllTokenizedCorpusIds(Mediator, corpusIds.First());
            Assert.True(tokenizedCorpusIds.Count() > 0);
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedTextCorpusFromDb()
        {
            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedCorpusId(new Guid()));

            foreach (var tokensTextRow in tokenizedTextCorpus.Cast<TokensTextRow>())
            {
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
                var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
                Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedTextCorpusFromDB_Change_SaveToDb()
        {
            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedCorpusId(new Guid()));

            Assert.Equal(16, tokenizedTextCorpus.Count());

            OnlyUpToCountTextRowProcessor.Train(2);

            var tokenizedTextCorpusWithOnlyTwoTokens = tokenizedTextCorpus
                .Filter<OnlyUpToCountTextRowProcessor>();

            Assert.Equal(2, tokenizedTextCorpusWithOnlyTwoTokens.Count());
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedCorpus_byBook()
        {
            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedCorpusId(new Guid()));

            Assert.Equal(16, tokenizedTextCorpus.Count());
            Assert.Equal(4, tokenizedTextCorpus.GetRows(new List<string>() { "MRK" }).Count());

            foreach (var tokensTextRow in tokenizedTextCorpus.GetRows(new List<string>() { "MRK" })
                         .Cast<TokensTextRow>())
            {
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
                var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.Text).ToList());
                Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
            }
        }
    }
}