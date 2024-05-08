using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using Xunit;
using Xunit.Abstractions;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    public class CorpusTests : TestBase
    {
        #nullable disable
        public CorpusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Corpus_SyntaxTrees_SyntaxTreeTokenExtendedProperties()
        {
            var syntaxTree = new SyntaxTrees();
            var corpus = new SyntaxTreeFileTextCorpus(syntaxTree)
                .Transform<AddPronominalReferencesToTokens>();

            var tokensTextRow = corpus.GetRows(new List<string> { "GEN" }).Cast<TokensTextRow>().Take(10).ToList();
            var token = tokensTextRow[4].Tokens[9];
            var extendedProperties = token.ExtendedProperties;

            if (extendedProperties != null)
            {
                var rootElement = XDocument.Parse(extendedProperties).Root;
                Assert.Equal("he called", rootElement?.XPathSelectElement("/ExtendedProperties/SyntaxTreeToken/English")?.Value ?? "");   //token's english
                Assert.Equal("God", rootElement?.XPathSelectElement("/ExtendedProperties/PronominalReferences/VerbSubjectReferences/English")?.Value ?? ""); //token's verb's subject 
            }
        }

        [Fact]
        public void SyntaxTrees_Read()
        {
            try
            {
                var syntaxTree = new SyntaxTrees();
                var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree);

                // now get the first 5 verses
                foreach (var tokensTextRow in sourceCorpus.Cast<TokensTextRow>().Take(5))
                {
                    var sourceVerseText = string.Join(" ", tokensTextRow.Segment);
                    Output.WriteLine($"Source: {sourceVerseText}");
                }
            }
            catch (EngineException eex)
            {
                Output.WriteLine(eex.ToString());
                throw eex;
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__ImportFromUsfm_SaveToDb()
        {
            try
            {
                //Import
                var textCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
                // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "LanguageType", Guid.NewGuid().ToString());
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "boo", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

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
                    var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.SurfaceText));
                    Output.WriteLine($"tokensText: {tokensText}");

                    //display tokens detokenized
                    var detokenizer = new LatinWordDetokenizer();
                    var tokensTextDetokenized =
                        detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.SurfaceText).ToList());
                    Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__ImportZZ_SURFromParatext_SaveToDb()
        {
            try
            {
                //Import
//                var textCorpus = (await ParatextProjectTextCorpus.Get(Mediator!, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", System.Threading.CancellationToken.None))
                var textCorpus = new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
                // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "LanguageType", Guid.NewGuid().ToString());
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                foreach (var tokensTextRow in tokenizedTextCorpus.Cast<TokensTextRow>().Take(10))
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
                    var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.SurfaceText));
                    Output.WriteLine($"tokensText: {tokensText}");

                    //display tokens detokenized
                    var detokenizer = new LatinWordDetokenizer();
                    var tokensTextDetokenized =
                        detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.SurfaceText).ToList());
                    Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetAllCorpusIds()
        {
            try
            {
                var corpus1 = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
                var corpus2 = await Corpus.Create(Mediator!, false, "NameY", "LanguageY", "BackTranslation", Guid.NewGuid().ToString());

                var expectedCorpusIds = new List<CorpusId>() { corpus1.CorpusId, corpus2.CorpusId };
                expectedCorpusIds.Sort((a, b) => a.Id.CompareTo(b.Id));

                var actualCorpusIds = (await Corpus.GetAllCorpusIds(Mediator!)).ToList();
                actualCorpusIds.Sort((a, b) => a.Id.CompareTo(b.Id));

                Assert.True(actualCorpusIds.Count() == 2);
                Assert.Equal(expectedCorpusIds, actualCorpusIds);

            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        // Mike notices that this test is broken.
        public async void Corpus__GetAllTokenizedCorpusIds()
        {
            try
            {
                var textCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                var corpusIds = await Corpus.GetAllCorpusIds(Mediator!);
                Assert.Single(corpusIds);
                Assert.Equal(corpus.CorpusId, corpusIds.First());

                var tokenizedCorpusIds = await TokenizedTextCorpus.GetAllTokenizedCorpusIdsAsync(Container!, corpusIds.First(), CancellationToken.None);
                Assert.Single(tokenizedCorpusIds);
                Assert.Equal(tokenizedTextCorpus.TokenizedTextCorpusId.CorpusId, corpusIds.First());
                Assert.Equal(tokenizedTextCorpus.TokenizedTextCorpusId, tokenizedCorpusIds.First());
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public void Corpus__BookTest()
        {
            Output.WriteLine(BookIds
                .Where(b => b.silCannonBookAbbrev.Equals("1JN"))
                .Select(b => b.silCannonBookNum)
                .First());
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedTextCorpusFromDb()
        {
            try
            {
                var textCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
                var createdTokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, createdTokenizedTextCorpus.TokenizedTextCorpusId);

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
                    var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.SurfaceText));
                    Output.WriteLine($"tokensText: {tokensText}");

                    //display tokens detokenized
                    var detokenizer = new LatinWordDetokenizer();
                    var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.SurfaceText).ToList());
                    Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedTextCorpusFromDB_Change_SaveToDb()
        {
            try
            {
                var textCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
                var createdTokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, createdTokenizedTextCorpus.TokenizedTextCorpusId);

                Assert.True(tokenizedTextCorpus.Count() > 0);
            } 
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Example")]
        public async void Corpus__GetTokenizedCorpus_byBook()
        {
            try
            {
                var textCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
                var createdTokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

                var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, createdTokenizedTextCorpus.TokenizedTextCorpusId);

                Assert.True(tokenizedTextCorpus.Count() > 0);
                Assert.True(tokenizedTextCorpus.GetRows(new List<string>() { "MRK" }).Count() > 0);

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
                    var tokensText = string.Join(" ", tokensTextRow.Tokens.Select(t => t.SurfaceText));
                    Output.WriteLine($"tokensText: {tokensText}");

                    //display tokens detokenized
                    var detokenizer = new LatinWordDetokenizer();
                    var tokensTextDetokenized = detokenizer.Detokenize(tokensTextRow.Tokens.Select(t => t.SurfaceText).ToList());
                    Output.WriteLine($"tokensTextDetokenized: {tokensTextDetokenized}");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Test")]
        public async void Corpus__GetAll()
        {
            await Corpus.Create(Mediator!, true, "First Corpus", "english", "Standard", Guid.NewGuid().ToString());
            await Corpus.Create(Mediator!, true, "Second Corpus", "english", "BackTranslation", Guid.NewGuid().ToString());

            ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(2, ProjectDbContext.Corpa.Count());

            var allCorpora = await Corpus.GetAll(Mediator!);

            Assert.Equal(2, allCorpora.Count());

            var firstCorpus = allCorpora.FirstOrDefault(c => c.CorpusId.Name == "First Corpus");
            var secondCorpus = allCorpora.FirstOrDefault(c => c.CorpusId.Name == "Second Corpus");

            Assert.Equal("First Corpus", firstCorpus.CorpusId.Name);
            Assert.Equal("english", firstCorpus.CorpusId.Language);
            Assert.Equal(Models.CorpusType.Standard.ToString(), firstCorpus.CorpusId.CorpusType);
            Assert.NotNull(firstCorpus.CorpusId.Metadata);

            Assert.Equal("Second Corpus", secondCorpus.CorpusId.Name);
            Assert.Equal("english", secondCorpus.CorpusId.Language);
            Assert.Equal(Models.CorpusType.BackTranslation.ToString(), secondCorpus.CorpusId.CorpusType);
            Assert.NotNull(secondCorpus.CorpusId.Metadata);
        }
    }
}