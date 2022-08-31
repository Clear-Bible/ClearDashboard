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
using Microsoft.EntityFrameworkCore;
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

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, tokenizationFunction);

            var result = await Mediator!.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);

            var tokenizedTextCorpus = result.Data;

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.CorpusId.Id);

            Assert.NotNull(corpusDB);
            Assert.True(corpusDB.IsRtl);
            Assert.Equal("NameX", corpusDB.Name);
            Assert.Equal("LanguageX", corpusDB.Language);
            Assert.Equal("Standard", corpusDB.CorpusType.ToString());
            Assert.Equal(tokenizationFunction, corpusDB.TokenizedCorpora.First().TokenizationFunction);

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
    [Trait("Category", "Handlers")]
    public async void TokenizedCorpus__CreateCompositeTokens()
    {
        try
        {
            var textCorpus = CreateFakeTextCorpusWithComposite(false);

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, tokenizationFunction);

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.CorpusId.Id);

            Assert.NotNull(corpusDB);
            Assert.True(corpusDB.IsRtl);
            Assert.Equal("NameX", corpusDB.Name);
            Assert.Equal("LanguageX", corpusDB.Language);
            Assert.Equal("Standard", corpusDB.CorpusType.ToString());
            Assert.Equal(tokenizationFunction, corpusDB.TokenizedCorpora.First().TokenizationFunction);

            var tokenizedTextCorpusDB = await TokenizedTextCorpus.Get(Mediator!, new TokenizedTextCorpusId(corpusDB.TokenizedCorpora.First().Id));
            var ct = 0;

            foreach (var tokensTextRow in tokenizedTextCorpusDB?.Cast<TokensTextRow>()!)
            {
                // FIXME:  Is it ok if Tokens is empty for a tokensTextRow?
                //                Assert.NotEmpty(tokensTextRow.Tokens);
                Assert.All(tokensTextRow.Tokens, t => Assert.NotNull(t.TokenId));

                // Verse info
                Output.WriteLine($"Ref                    : {tokensTextRow.Ref.ToString()}");

                // Legacy segment
                Output.WriteLine($"SegmentText            : {string.Join(" ", tokensTextRow.Segment)}");

                // TokenIds
                var tokenIds = tokensTextRow.Tokens.Select(t => t.TokenId);
                ct = tokenIds.Count();
                Output.WriteLine($"TokenIds               : {string.Join(" ", tokenIds)} (count: {ct})");

                // Component token ids:
                var componentTokenIds = tokensTextRow.Tokens
                    .GetPositionalSortedBaseTokens() //pull out the tokens from composite tokens
                    .Select(t => t.TokenId);
                ct = componentTokenIds.Count();
                Output.WriteLine($"Component tokenIds     : {string.Join(" ", componentTokenIds)} (count: {ct})");

                // Component surface texts:
                var surfaceTexts = tokensTextRow.Tokens
                    .GetPositionalSortedBaseTokens()
                    .Select(t => t.SurfaceText);
                Output.WriteLine($"SurfaceTexts           : {string.Join(" ", surfaceTexts)}");

                // Tokens detokenized:
                var tokensWithPadding = new EngineStringDetokenizer(new LatinWordDetokenizer()).Detokenize(tokensTextRow.Tokens);
                Output.WriteLine($"Detokenized surfaceText: {tokensWithPadding.Aggregate(string.Empty, (constructedString, tokenWithPadding) => $"{constructedString}{tokenWithPadding.paddingBefore}{tokenWithPadding.token}{tokenWithPadding.paddingAfter}")}");

                Output.WriteLine("");
            }
        }
        catch (Exception ex)
        {
            Output.WriteLine(ex.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void TokenizedCorpus__CreateCompositeTokensInvalid()
    {
        try
        {
            var textCorpus = CreateFakeTextCorpusWithComposite(true);

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, tokenizationFunction);

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

    [Fact]
    public async void TokenizedCorpus__CreateMultiple()
    {
        try
        {
            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction1 = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus1 = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpus.CorpusId, tokenizationFunction1);

            Assert.NotNull(tokenizedTextCorpus1);

            var tokenizationFunction2 = ".Tokenize<ZwspWordTokenizer>()";
            var tokenizedTextCorpus2 = await tokenizedTextCorpus1
                .Tokenize<ZwspWordTokenizer>()
                .Create(Mediator!, corpus.CorpusId, tokenizationFunction1 + tokenizationFunction2);

            Assert.NotNull(tokenizedTextCorpus2);

            var tokenizationFunction3 = ".Tokenize<LineSegmentTokenizer>()";
            var tokenizedTextCorpus3 = await tokenizedTextCorpus1
                .Tokenize<LineSegmentTokenizer>()
                .Create(Mediator!, corpus.CorpusId, tokenizationFunction1 + tokenizationFunction3);

            Assert.NotNull(tokenizedTextCorpus3);

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .FirstOrDefault(c => c.Id == corpus.CorpusId.Id);

            Assert.NotNull(corpusDB);
            Assert.True(corpusDB!.IsRtl);
            Assert.Equal("NameX", corpusDB.Name);
            Assert.Equal("LanguageX", corpusDB.Language);
            Assert.Equal("Standard", corpusDB.CorpusType.ToString());
            Assert.True(corpusDB!.TokenizedCorpora.Count == 3);

            var tokenizedCorpora = corpusDB.TokenizedCorpora.OrderBy(tc => tc.Created).ToList();

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
    public async void TokenizedCorpus__CreateGetByBookChapterVerse()
    {
        try
        {
            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpus.CorpusId, tokenizationFunction);

            Assert.NotNull(tokenizedTextCorpus);

            var bookIds = tokenizedTextCorpus.Texts.Select(t => t.Id).ToList();
            Assert.True(bookIds.Count > 0);

            Output.WriteLine("");
            foreach (var bookId in bookIds)
            {
                Output.WriteLine($"Book ID in tokenized text corpus: {bookId}");
            }

            Output.WriteLine("");
            foreach (var bookId in bookIds)
            {
                Output.WriteLine($"Book ID: {bookId}");

                var chapterVerseGroups = tokenizedTextCorpus.GetRows(new List<string>() {  bookId }).Cast<TokensTextRow>()
                    .SelectMany(ttr => ttr.Tokens).OrderBy(t => t.TokenId.ChapterNumber).GroupBy(t => t.TokenId.ChapterNumber)
                    .Select(g => new
                    {
                        ChapterNumber = g.Key,
                        Verses = g.OrderBy(v => v.TokenId.VerseNumber).GroupBy(v => v.TokenId.VerseNumber)
                                .Select(vg => new
                                {
                                    VerseNumber = vg.Key,
                                    Tokens = vg.ToList()
                                })
                    });

                Assert.True(chapterVerseGroups.Count() > 0);

                foreach (var chapter in chapterVerseGroups)
                {
                    Assert.True(chapter.Verses.Count() > 0);

                    Output.WriteLine($"\tChapter number: {chapter.ChapterNumber}");
                    foreach (var verse in chapter.Verses)
                    {
                        Output.WriteLine($"\t\tVerse number {verse.VerseNumber}: [" + String.Join(" ", verse.Tokens.Select(t => t.SurfaceText)) + "]");
                    }
                }
            }

            Output.WriteLine("");
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

            var corpus = await Corpus.Create(Mediator!, false, "New Testament", "grc", "Resource");
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, tokenizationFunction);

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

            var corpora = ProjectDbContext?.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.Tokens)
                .ToList();

            Assert.Single(corpora);
            var corpusDB = corpora.First();
            Assert.Single(corpusDB.TokenizedCorpora);
            var tokenizedCorpus = corpusDB.TokenizedCorpora.First();
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

            var corpus1 = await Corpus.Create(Mediator!, false, "New Testament 1", "grc", "Resource");
            var command1 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus1.CorpusId,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");


            var corpus2 = await Corpus.Create(Mediator!, false, "New Testament 2", "grc", "Resource");
            var command2 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus2.CorpusId,
                ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");


            var corpus3 = await Corpus.Create(Mediator!, false, "New Testament 3", "grc", "Resource");
            var command3 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus3.CorpusId,
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

            Assert.True(ProjectDbContext.Corpa.Count() > 0);
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

    private static ITextCorpus CreateFakeTextCorpusWithComposite(bool includeBadCompositeToken)
    {
        var textCorpus = new DictionaryTextCorpus(
            new MemoryText("GEN", new[]
            {
                    //new VerseRef(1,1,1), "Source segment Jacob 1", isSentenceStart: true
                    new TextRow(new VerseRef(1, 1, 1)) { Segment = "Source segment Test 1". Split(), IsSentenceStart = true,  IsEmpty = false },
                    new TextRow(new VerseRef(1, 1, 2)) { Segment = "Source segment Test 2.".Split(), IsSentenceStart = false, IsEmpty = false },
                    new TextRow(new VerseRef(1, 1, 3)) { Segment = "Source segment Test 3.".Split(), IsSentenceStart = true,  IsEmpty = false }
            }))
            .Tokenize<LatinWordTokenizer>();

        if (includeBadCompositeToken)
        {
            textCorpus = textCorpus.Transform<IntoFakeBadCompositeTokensTextRowProcessor>();
        }
        else
        {
            textCorpus = textCorpus.Transform<IntoFakeCompositeTokensTextRowProcessor>();
        }
        
        return textCorpus.Transform<SetTrainingBySurfaceTokensTextRowProcessor>();
    }

    private class IntoFakeCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("Test 1"))
            {
                return GenerateTokensTextRow(textRow, false);
            }

            return new TokensTextRow(textRow);
        }
    }
    private class IntoFakeBadCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("Test 1") || textRow.Text.Contains("Test 2"))
            {
                return GenerateTokensTextRow(textRow, true);
            }

            return new TokensTextRow(textRow);
        }
    }

    private static TokensTextRow GenerateTokensTextRow(TextRow textRow, bool includeBadCompositeToken)
    {
        var tr = new TokensTextRow(textRow);

        var tokens = tr.Tokens;

        var tokenIds = tokens
            .Select(t => t.TokenId)
            .ToList();

        var compositeTokens = new List<Token>() { tokens[0], tokens[1], tokens[3] };
        if (includeBadCompositeToken)
        {
            compositeTokens.Insert(1, new Token(new TokenId(1, 1, 5, 1, 1), "Bad boy", "Really Bad Boy"));
        }

        var tokensWithComposite = new List<Token>()
                 {
                     new CompositeToken(compositeTokens),
                     tokens[2]
                 };

        tr.Tokens = tokensWithComposite;
        return tr;
    }
}