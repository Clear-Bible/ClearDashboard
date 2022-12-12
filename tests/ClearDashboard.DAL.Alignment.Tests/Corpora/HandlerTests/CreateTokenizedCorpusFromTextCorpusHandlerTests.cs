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
using System.Text;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearBible.Engine.Persistence;
using System.Collections;
using ClearDashboard.DAL.Alignment.Translation;
using SIL.Machine.Translation;
using Models = ClearDashboard.DataAccessLayer.Models;

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
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.Original);

            var result = await Mediator!.Send(command);
            Assert.NotNull(result);
            Assert.True(result.Success);

            var tokenizedTextCorpus = result.Data;

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.TokenizedTextCorpusId.CorpusId.Id);

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
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa.Include(c => c.TokenizedCorpora).FirstOrDefault(c => c.Id == tokenizedTextCorpus!.TokenizedTextCorpusId.CorpusId.Id);

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
    public async void TokenizedCorpus__CreateCompositeTokensApi()
    {
        try
        {
            var textCorpus = CreateFakeTextCorpusWithComposite(false);

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var verseRows = ProjectDbContext!.VerseRows
                .Include(vr => vr.TokenComponents)
                .Take(2)
                .ToArray();

            var tokensForComposite1 = verseRows[0].Tokens.Take(4).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var composite1 = new CompositeToken(tokensForComposite1);
            composite1.TokenId.Id = Guid.NewGuid();

            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite1, null);

            ProjectDbContext!.ChangeTracker.Clear();

            var tokenComposite1 = ProjectDbContext.TokenComposites.Include(tc => tc.Tokens)
                .Where(tc => tc.Id == composite1.TokenId.Id)
                .FirstOrDefault();

            Assert.NotNull(tokenComposite1);
            Assert.Equal(composite1.Tokens.Count(), tokenComposite1.Tokens.Count);
            Assert.Empty(composite1.Tokens.Select(t => t.TokenId.Id).Except(tokenComposite1.Tokens.Select(tc => tc.Id)));

            var tokensForComposite2 = verseRows[0].Tokens.Skip(1).Take(4).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var composite2 = new CompositeToken(tokensForComposite2);
            composite2.TokenId.Id = composite1.TokenId.Id;

            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite2, null);

            ProjectDbContext!.ChangeTracker.Clear();

            var tokenComposite2 = ProjectDbContext.TokenComposites.Include(tc => tc.Tokens)
                .Where(tc => tc.Id == composite2.TokenId.Id)
                .FirstOrDefault();

            Assert.NotNull(tokenComposite2);
            Assert.Equal(composite2.Tokens.Count(), tokenComposite2.Tokens.Count);
            Assert.Empty(composite2.Tokens.Select(t => t.TokenId.Id).Except(tokenComposite2.Tokens.Select(tc => tc.Id)));

            // Exception case:  multiple VerseRow tokens in non-ParallelCorpus Composite
            var tokensForComposite3 = verseRows[0].Tokens.Skip(2).Take(3).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            tokensForComposite3.AddRange(verseRows[1].Tokens.Take(2).Select(tc => ModelHelper.BuildToken(tc)));
            var composite3 = new CompositeToken(tokensForComposite3);
            composite3.TokenId.Id = composite1.TokenId.Id;

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => TokenizedTextCorpus.PutCompositeToken(Mediator!, composite3, null));

            // Full round trip.  Use the API to retrieve the CompositeToken:
            var ttc = await TokenizedTextCorpus.Get(Mediator!, tokenizedTextCorpus.TokenizedTextCorpusId);
            var bookNumber = int.Parse(verseRows[0].BookChapterVerse[..3]);
            var bookId = FileGetBookIds.BookIds.Where(b => int.Parse(b.silCannonBookNum) == bookNumber).Select(b => b.silCannonBookAbbrev).First();
            var ttrs = ttc.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().ToList();

            foreach (var ttr in ttrs)
            {
                var (b, c, v) = (((VerseRef)ttr.Ref).BookNum, ((VerseRef)ttr.Ref).ChapterNum, ((VerseRef)ttr.Ref).VerseNum);
                var bookChapterVerse = $"{b:000}{c:000}{v:000}";

                if (bookChapterVerse == verseRows[0].BookChapterVerse)
                {
                    var t2 = ttr.Tokens.Where(t => t.TokenId.Id == composite2.TokenId.Id).FirstOrDefault();
                    Assert.NotNull(t2);
                    Assert.IsType<CompositeToken>(t2);
                    var c2 = (CompositeToken)t2;
                    Assert.Equal(composite2.Tokens.Count(), c2.Tokens.Count());
                    Assert.Empty(composite2.Tokens.Select(t => t.TokenId.Id).Except(c2.Tokens.Select(tc => tc.TokenId.Id)));
                }
            }

            // This should delete composite1, but leave its child tokens:
            composite1.Tokens = new List<Token>();
            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite1, null);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Null(ProjectDbContext.TokenComposites.Where(tc => tc.Id == composite1.TokenId.Id).FirstOrDefault());
            Assert.Equal(4, ProjectDbContext.Tokens.Where(t => tokensForComposite1.Select(t => t.TokenId.Id).Contains(t.Id)).Count());
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
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var nonTokenizedTextCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath);

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction1 = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus1 = await nonTokenizedTextCorpus
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Create(Mediator!, corpus.CorpusId, "Unit Test, a", tokenizationFunction1);

            Assert.NotNull(tokenizedTextCorpus1);

            var tokenizationFunction2 = ".Tokenize<ZwspWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus2 = await nonTokenizedTextCorpus
                .Tokenize<ZwspWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Create(Mediator!, corpus.CorpusId, "Unit Test, b", tokenizationFunction2);

            Assert.NotNull(tokenizedTextCorpus2);

            ProjectDbContext!.ChangeTracker.Clear();

            var corpusDB = ProjectDbContext!.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.TokenComponents)
                .FirstOrDefault(c => c.Id == corpus.CorpusId.Id);

            Assert.NotNull(corpusDB);
            Assert.True(corpusDB!.IsRtl);
            Assert.Equal("NameX", corpusDB.Name);
            Assert.Equal("LanguageX", corpusDB.Language);
            Assert.Equal("Standard", corpusDB.CorpusType.ToString());
            Assert.True(corpusDB!.TokenizedCorpora.Count == 2);

            var tokenizedCorpora = corpusDB.TokenizedCorpora.OrderBy(tc => tc.Created).ToList();

            Assert.NotNull(tokenizedCorpora);
            Assert.Equal(tokenizationFunction1, tokenizedCorpora[0].TokenizationFunction);
            Assert.Equal(tokenizationFunction2, tokenizedCorpora[1].TokenizationFunction);

            tokenizedCorpora.ForEach(tc =>
                {
                    Assert.True(tc.Tokens.Any());
                    Output.WriteLine($"Token count for {tc.TokenizationFunction}: {tc.Tokens.Count()}");
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
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);

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

            var corpus = await Corpus.Create(Mediator!, false, "New Testament", "grc", "Resource", Guid.NewGuid().ToString());
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.Original);

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
                .ThenInclude(tc => tc.TokenComponents)
                .ToList();

            Assert.Single(corpora);
            var corpusDB = corpora.First();
            Assert.Single(corpusDB.TokenizedCorpora);
            var tokenizedCorpus = corpusDB.TokenizedCorpora.First();
            Assert.Equal(tokenizationFunction, tokenizedCorpus.TokenizationFunction);
            Assert.Equal(20723, tokenizedCorpus.TokenComponents.Count());
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

            var corpus1 = await Corpus.Create(Mediator!, false, "New Testament 1", "grc", "Resource", Guid.NewGuid().ToString());
            var command1 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus1.CorpusId,
                "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", ScrVers.Original);


            var corpus2 = await Corpus.Create(Mediator!, false, "New Testament 2", "grc", "Resource", Guid.NewGuid().ToString());
            var command2 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus2.CorpusId,
                "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", ScrVers.Original);


            var corpus3 = await Corpus.Create(Mediator!, false, "New Testament 3", "grc", "Resource", Guid.NewGuid().ToString());
            var command3 = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus3.CorpusId,
                "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", ScrVers.Original);

            var result1 = await Mediator.Send(command1);
            var result2 = await Mediator.Send(command2);
            var result3 = await Mediator.Send(command3);

            ProjectDbContext.ChangeTracker.Clear();

            var corpusNT1 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.TokenComponents)
                .FirstOrDefault(c => c.Name == "New Testament 1");

            var corpusNT2 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.TokenComponents)
                .FirstOrDefault(c => c.Name == "New Testament 2");

            var corpusNT3 = ProjectDbContext.Corpa
                .Include(c => c.TokenizedCorpora)
                .ThenInclude(tc => tc.TokenComponents)
                .FirstOrDefault(c => c.Name == "New Testament 3");

            Assert.True(ProjectDbContext.Corpa.Count() > 0);
            Assert.Equal(1, corpusNT1?.TokenizedCorpora.Count);
            Assert.Equal(157590, corpusNT1?.TokenizedCorpora.First().TokenComponents.Count());
            Assert.Equal(157590, corpusNT2?.TokenizedCorpora.First().TokenComponents.Count());
            Assert.Equal(157590, corpusNT3?.TokenizedCorpora.First().TokenComponents.Count());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handler")]
    public async void TokenizedCorpus__PartialImports()
    {
        try
        {
            var initialBookIds = new List<string>() { "EZR", "LUK", "GEN" };

            // Create the corpus in the database:
            var textCorpus = new TextCorpusDecorator(TestDataHelpers.GetManuscript(), initialBookIds);
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", "func", default);

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            var bookIds = tokenizedTextCorpus.Texts.Select(t => t.Id).ToList();
            Assert.Equal(initialBookIds.Count, bookIds.Count);
            Assert.Equal(initialBookIds.Count, bookIds.Intersect(initialBookIds).Count());

            var targetTextCorpus = new TextCorpusDecorator(TestDataHelpers.GetZZSurCorpus(), initialBookIds);
            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "Standard", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await targetTextCorpus.Create(Mediator!, targetCorpus.CorpusId, "Unit Test", "func", default);

            var engineParallelTextCorpus = tokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());
            var parallelCorpus = await engineParallelTextCorpus.Create("test pc", Mediator!);

            var translationCommandable = new TranslationCommands();
            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                engineParallelTextCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);
            var alignmentModel = translationCommandable.PredictAllAlignedTokenIdPairs(smtWordAlignmentModel, engineParallelTextCorpus).ToList();
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var sourceTokens = ProjectDbContext.Tokens.Include(t => t.VerseRow)
                .Where(t => t.VerseRow.BookChapterVerse == "001001001")
                .Where(t => t.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(3)
                .Select(t => ModelHelper.BuildToken(t)).ToArray();
            var targetTokens = ProjectDbContext.Tokens.Include(t => t.VerseRow)
                .Where(t => t.VerseRow.BookChapterVerse == "001001001")
                .Where(t => t.TokenizedCorpusId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(3)
                .Select(t => ModelHelper.BuildToken(t)).ToArray();
            var sourceVerses = ProjectDbContext.Verses.Include(v => v.VerseMapping)
                .Where(v => v.BookNumber == 1 && v.ChapterNumber == 1)
                .Where(v => v.VerseMapping.ParallelCorpusId == parallelCorpus.ParallelCorpusId.Id)
                .Where(v => v.CorpusId == corpus.CorpusId.Id);
            var targetVerses = ProjectDbContext.Verses.Include(v => v.VerseMapping)
                .Where(v => v.BookNumber == 1 && v.ChapterNumber == 1)
                .Where(v => v.VerseMapping.ParallelCorpusId == parallelCorpus.ParallelCorpusId.Id)
                .Where(v => v.CorpusId == targetCorpus.CorpusId.Id);
            var verseMapping1And3 = ProjectDbContext.VerseMappings
                .Include(vm => vm.Verses)
                .Where(vm => vm.ParallelCorpusId == parallelCorpus.ParallelCorpusId.Id)
                .Where(vm => vm.Verses.Any(v => v.BookNumber == 1 && v.ChapterNumber == 1 && v.VerseNumber == 1))
                .First();
            verseMapping1And3.Verses.Add(sourceVerses.Where(v => v.VerseNumber == 3).First());
            await ProjectDbContext.SaveChangesAsync();
            var tokenFromVerse3 = ProjectDbContext.Tokens.Include(t => t.VerseRow)
                .Where(t => t.VerseRow.BookChapterVerse == "001001003")
                .Where(t => t.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Select(t => ModelHelper.BuildToken(t))
                .First();

            // Add Translations and Alignments for tokens in VerseRow 001001001,
            // so that they have to get soft deleted during UpdateOrAddVerses: 
            await translationSet.PutTranslation(new Alignment.Translation.Translation(sourceTokens[0], "duh", "Assigned"), "PutNoPropagate");
            await translationSet.PutTranslation(new Alignment.Translation.Translation(sourceTokens[1], "doh", "Assigned"), "PutNoPropagate");
            alignmentSet.PutAlignment(new Alignment.Translation.Alignment(new AlignedTokenPairs(sourceTokens[1], targetTokens[0], 42), "Verified"));
            alignmentSet.PutAlignment(new Alignment.Translation.Alignment(new AlignedTokenPairs(sourceTokens[2], targetTokens[1], 1), "Verified"));
            await TokenizedTextCorpus.PutCompositeToken(Mediator!, new CompositeToken(new List<Token>() { sourceTokens[0], sourceTokens[1] }), null);

            // Composite using tokens from source Verse 1 and 3 (won't get soft deleted by VerseRow)
            // to make sure UpdateOrAddVerses deletes it (since it is connected to one of the VerseRow's
            // tokens:
            await TokenizedTextCorpus.PutCompositeToken(Mediator!, new CompositeToken(new List<Token>() { sourceTokens[2], tokenFromVerse3 }), parallelCorpus.ParallelCorpusId);

            // TokenVerseAssociation connected to one of the VerseRow's source tokens:
            ProjectDbContext.Add(new Models.TokenVerseAssociation() 
            { 
                Id = Guid.NewGuid(), 
                TokenComponentId = sourceTokens[1].TokenId.Id, 
                VerseId = sourceVerses.Where(v => v.VerseNumber == 1).First().Id 
            });
            await ProjectDbContext.SaveChangesAsync();

            var updateBookIds = new List<string>() { "GEN", "MRK", "ROM" };
            textCorpus.AddToBookIdFilter(updateBookIds);
            textCorpus.AddOriginalTextAlteration("GEN", 1, 1, "bubba was here");
            await tokenizedTextCorpus.UpdateOrAddVerses(Mediator!, textCorpus);

            var updatedBookIds = tokenizedTextCorpus.Texts.Select(t => t.Id).ToList();
            var combinedBookIds = initialBookIds.Union(updateBookIds);
            Assert.Equal(combinedBookIds.Count(), updatedBookIds.Count);
            Assert.Equal(combinedBookIds.Count(), updatedBookIds.Intersect(combinedBookIds).Count());

            var verseRow1 = ProjectDbContext.VerseRows
                .Include(t => t.TokenComponents)
                    .ThenInclude(tc => tc.SourceAlignments)
                .Include(t => t.TokenComponents)
                    .ThenInclude(tc => tc.Translations)
                .Include(t => t.TokenComponents)
                    .ThenInclude(tc => tc.TokenVerseAssociations)
                .Where(vr => vr.BookChapterVerse == "001001001")
                .Where(vr => vr.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .First();
            var vr1TokenCount = verseRow1.Tokens.Count();
            var vr1SourceAlignmentCount = verseRow1.Tokens.SelectMany(t => t.SourceAlignments).Count();
            var vr1TranslationCount = verseRow1.Tokens.SelectMany(t => t.Translations).Count();
            var vr1TvaCount = verseRow1.Tokens.SelectMany(t => t.TokenVerseAssociations).Count();

            Assert.Equal(vr1TokenCount, ProjectDbContext.Tokens.Where(t => t.Deleted != null).Count());
            Assert.Equal(vr1SourceAlignmentCount, ProjectDbContext.Alignments.Where(a => a.Deleted != null).Count());
            Assert.Equal(vr1TranslationCount, ProjectDbContext.Translations.Where(t => t.Deleted != null).Count());
            Assert.Equal(vr1TvaCount, ProjectDbContext.Set<Models.TokenVerseAssociation>().Where(a => a.Deleted != null).Count());
            Assert.Equal(2, ProjectDbContext.Set<Models.TokenComposite>().Where(a => a.Deleted != null).Count());
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
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, new CorpusId(new Guid()), "bogus name", "bogus tokenization", ScrVers.Original);

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
            .Tokenize<LatinWordTokenizer>()
            .Transform<IntoFakeCompositeTokensTextRowProcessor>()
            .Transform<SetTrainingBySurfaceLowercase>();

        return textCorpus;
    }

    private class IntoFakeCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("Test 1"))
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

        var compositeTokens = new List<Token>() { tokens[0], tokens[1], tokens[3] };
        var tokensWithComposite = new List<Token>()
                 {
                     new CompositeToken(compositeTokens),
                     tokens[2]
                 };

        tr.Tokens = tokensWithComposite;
        return tr;
    }

    private class TextCorpusDecorator : ITextCorpus
    {
        private readonly ITextCorpus _innerTextCorpus;
        private readonly HashSet<string> _bookIdFilter;
        private readonly Dictionary<string, string> _originalTextAlterations;
        private readonly HashSet<string> _bookIdsInOriginalTextAlterations;

        public TextCorpusDecorator(ITextCorpus innerTextCorpus, IEnumerable<string> bookIdFilter)
        {
            _innerTextCorpus = innerTextCorpus;
            _bookIdFilter = new (bookIdFilter);
            _originalTextAlterations = new();
            _bookIdsInOriginalTextAlterations = new();
        }

        public void AddToBookIdFilter(IEnumerable<string> bookIds)
        {
            _bookIdFilter.UnionWith(bookIds);
        }

        public void AddOriginalTextAlteration(string bookId, int chapterNumber, int verseNumber, string originalText)
        {
            var bookNumber = int.Parse(FileGetBookIds.BookIds
                .Where(bi => bi.silCannonBookAbbrev == bookId)
                .Select(bi => bi.silCannonBookNum).First());
            
            var bookChapterVerse = $"{bookNumber:000}{chapterNumber:000}{verseNumber:000}";

            _originalTextAlterations.Add(bookChapterVerse, originalText);
            _bookIdsInOriginalTextAlterations.Add(bookId);
        }

        public IEnumerable<TextRow> GetRows(IEnumerable<string> textIds = null)
        {
            if (_bookIdFilter.Any())
            {
                if (textIds is not null)
                {
                    textIds = textIds.Intersect(_bookIdFilter);
                    if (!textIds.Any())
                    {
                        return Enumerable.Empty<TextRow>();
                    }
                }
                else
                {
                    textIds = _bookIdFilter;
                }
            }

            var rows = _innerTextCorpus.GetRows(textIds);

            if (_bookIdsInOriginalTextAlterations.Any() && 
                (textIds is null || _bookIdsInOriginalTextAlterations.Intersect(textIds).Any()))
            {
                rows = rows.ToList();
                ((List<TextRow>)rows).ForEach(row =>
                {
                    var bookChapterVerse = $"{((VerseRef)row.Ref).BookNum:000}{((VerseRef)row.Ref).ChapterNum:000}{((VerseRef)row.Ref).VerseNum:000}";

                    if (_originalTextAlterations.TryGetValue(bookChapterVerse, out var originalText))
                    {
                        row.OriginalText = originalText;
                    }
                });
            }

            return rows;
        }

        public IEnumerable<IText> Texts
        {
            get
            {
                if (_bookIdFilter.Any())
                {
                    return _innerTextCorpus.Texts.Where(tx => _bookIdFilter.Contains(tx.Id));
                }
                return _innerTextCorpus.Texts;
            }
        }

        public IEnumerator<TextRow> GetEnumerator()
        {
            return _innerTextCorpus.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerTextCorpus.GetEnumerator();
        }
    }
}