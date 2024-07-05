using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Extensions;
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
using Models = ClearDashboard.DataAccessLayer.Models;
using SIL.Extensions;
using System.Text.Json;
using System.IO;
using Paratext.PluginInterfaces;

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
    public async void TokenizedCorpus__CreateWithVersification()
    {
        try
        {
            // Create a customized versification:
            Versification.Table.Implementation.RemoveAllUnknownVersifications();
            string customVersificationAddition = "&MAT 1:2 = MAT 1:1\nMAT 1:3 = MAT 1:2\nMAT 1:1 = MAT 1:3\n";
            ScrVers versification = ScrVers.RussianOrthodox;
            using (var reader = new StringReader(customVersificationAddition))
            {
                Versification.Table.Implementation.RemoveAllUnknownVersifications();
                versification = Versification.Table.Implementation.Load(reader, "not a file", versification, "custom");
            }

            var v = versification;
            var newVerse = v.ChangeVersification(040001002, ScrVers.Original);
            Assert.Equal(40001003, newVerse);

            using var writer = new StringWriter();
            versification.Save(writer);

            Assert.Equal(ScrVersType.Unknown, versification.Type);
            Assert.Equal(ScrVers.RussianOrthodox, versification.BaseVersification);
            Assert.True(versification.IsCustomized);

            //Import
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction, versification);

            Assert.NotNull(tokenizedTextCorpus);

            var tokenizedTextCorpus2 = await TokenizedTextCorpus.Get(Mediator!, tokenizedTextCorpus.TokenizedTextCorpusId, false);
            Assert.NotNull(tokenizedTextCorpus2);

            var versification2 = tokenizedTextCorpus2.Versification;

            Assert.Equal(versification.Type, versification2.Type);
            Assert.Equal(versification.BaseVersification, versification2.BaseVersification);
            Assert.True(versification2.IsCustomized);

            using var writer2 = new StringWriter();
            versification2.Save(writer2);

            Assert.Equal(writer.ToString(), writer2.ToString());
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
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.RussianProtestant);

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
            Assert.Equal(ScrVers.RussianProtestant, tokenizedTextCorpusDB.Versification);
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
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);

            Assert.NotNull(tokenizedTextCorpus);
            Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            ProjectDbContext!.ChangeTracker.Clear();

            var verseRows = ProjectDbContext!.VerseRows
                .Include(vr => vr.TokenComponents.Where(tc => tc.GetType() == typeof(Models.Token)).AsQueryable())
                    .ThenInclude(tc => ((Models.Token)tc).TokenCompositeTokenAssociations)
                        .ThenInclude(ta => ta.TokenComposite)
                .Where(vr => vr.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(3)
                .ToArray();

            var tokensForComposite1 = verseRows[0].Tokens.Take(4).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var composite1 = new CompositeToken(tokensForComposite1)
            {
                TokenId =
                {
                    Id = Guid.NewGuid()
                }
            };

            // First this should succeed since its the first one:
            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite1, null);

            ProjectDbContext!.ChangeTracker.Clear();

            var tokenComposite1 = ProjectDbContext.TokenComposites.Include(tc => tc.Tokens)
                .Where(tc => tc.Id == composite1.TokenId.Id)
                .FirstOrDefault();

            Assert.NotNull(tokenComposite1);
            Assert.Equal(composite1.Tokens.Count(), tokenComposite1.Tokens.Count);
            Assert.Empty(composite1.Tokens.Select(t => t.TokenId.Id).Except(tokenComposite1.Tokens.Select(tc => tc.Id)));

            var tokensForComposite2 = verseRows[0].Tokens.Skip(1).Take(4).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var composite2 = new CompositeToken(tokensForComposite2)
            {
                TokenId =
                {
                    Id = Guid.NewGuid()
                }
            };

            // Should fail since its a new composite Id and some of the same tokens in composite1:
            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => TokenizedTextCorpus.PutCompositeToken(Mediator!, composite2, null));

            // Should succeed:  multiple VerseRow tokens in non-ParallelCorpus Composite
            // (same compositeId as in composite1)
            var tokensForComposite3 = verseRows[0].Tokens.Skip(2).Take(3).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var otherTokensForComposite3 = verseRows[1].Tokens.Take(2).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var composite3 = new CompositeToken(tokensForComposite3, otherTokensForComposite3)
            {
                TokenId =
                {
                    Id = composite1.TokenId.Id
                }
            };

            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite3, null);

            // Should fail:  multiple TokenizedCorpus tokens:
            var tokenizedTextCorpusOther = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction);
            var verseRowsOther = ProjectDbContext!.VerseRows
                .Include(vr => vr.TokenComponents.Where(tc => tc.GetType() == typeof(Models.Token)).AsQueryable())
                    .ThenInclude(tc => ((Models.Token)tc).TokenCompositeTokenAssociations)
                        .ThenInclude(ta => ta.TokenComposite)
                .Where(vr => vr.TokenizedCorpusId == tokenizedTextCorpusOther.TokenizedTextCorpusId.Id)
                .Take(1)
                .ToArray();

            var tokensForComposite4 = verseRows[0].Tokens.Skip(2).Take(3).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            var otherTokensForComposite4 = verseRows[1].Tokens.Take(2).Select(tc => ModelHelper.BuildToken(tc)).ToList();
            otherTokensForComposite4.AddRange(verseRowsOther[0].Tokens.Take(2).Select(tc => ModelHelper.BuildToken(tc)).ToList());
            var composite4 = new CompositeToken(tokensForComposite4, otherTokensForComposite4)
            {
                TokenId =
                {
                    Id = composite1.TokenId.Id
                }
            };

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => TokenizedTextCorpus.PutCompositeToken(Mediator!, composite4, null));

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
                    var t4 = ttr.Tokens.Where(t => t.TokenId.Id == composite4.TokenId.Id).FirstOrDefault();
                    Assert.NotNull(t4);
                    Assert.IsType<CompositeToken>(t4);
                    var c4 = (CompositeToken)t4;
                    Assert.Equal(composite4.Tokens.Count(), c4.Tokens.Count());
                    Assert.Empty(composite4.Tokens.Select(t => t.TokenId.Id).Except(c4.Tokens.Select(tc => tc.TokenId.Id)));
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
    [Trait("Category", "Handlers")]
    public async void TokenizedCorpus__Update()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var tokenizedTextCorpus = await textCorpus.Create(
                Mediator!,
                corpus.CorpusId,
                "DisplayNameV1",
                "Func1");

            // Check initial save values:
            Assert.NotNull(tokenizedTextCorpus);
            Assert.Equal("DisplayNameV1", tokenizedTextCorpus.TokenizedTextCorpusId.DisplayName);
            Assert.Equal(new Dictionary<string, object>(), tokenizedTextCorpus.TokenizedTextCorpusId.Metadata);

            var currentTime = DateTimeOffset.UtcNow;
            var m2 = new MetadataObject
            {
                ValueString = "Hi there!",
                ValueBool = true,
                ValueInt = 14,
                ValueDateTimeOffset = currentTime
            };

            // Change values to "V2" and save:
            tokenizedTextCorpus.TokenizedTextCorpusId.DisplayName = "DisplayNameV2";
            tokenizedTextCorpus.TokenizedTextCorpusId.Metadata.Add("m2", m2);

            await tokenizedTextCorpus.Update(Mediator!);
            ProjectDbContext!.ChangeTracker.Clear();

            // Check "V2" values via API:
            var tokenizedTextCorpus2 = await TokenizedTextCorpus.Get(
                Mediator!,
                tokenizedTextCorpus.TokenizedTextCorpusId,
                false);

            Assert.NotNull(tokenizedTextCorpus2);
            Assert.Equal("DisplayNameV2", tokenizedTextCorpus2.TokenizedTextCorpusId.DisplayName);
            Assert.True(tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.ContainsKey("m2"));
            Assert.True(tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata["m2"] is JsonElement);

            var m2Deserialized = tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.DeserializeValue<MetadataObject>("m2");

            Assert.Equal(m2, m2Deserialized);

            tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.Add("ValueInt", 14);
            tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.Add("ValueBool", true);
            tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.Add("ValueString", "hi!");
            tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.Add("ValueDateTimeOffset", currentTime);
            tokenizedTextCorpus2.TokenizedTextCorpusId.Metadata.Add("ValueEnum", Models.CorpusType.SourceLanguage);

            await tokenizedTextCorpus2.Update(Mediator!);
            ProjectDbContext!.ChangeTracker.Clear();

            // Check "V2" values via API:
            var tokenizedTextCorpus3 = await TokenizedTextCorpus.Get(
                Mediator!,
                tokenizedTextCorpus2.TokenizedTextCorpusId,
                false);

            var metadata3 = tokenizedTextCorpus3.TokenizedTextCorpusId.Metadata;
            Assert.NotNull(tokenizedTextCorpus3);
            Assert.Equal(14, metadata3.DeserializeValue<int>("ValueInt"));
            Assert.True(metadata3.DeserializeValue<bool>("ValueBool"));
            Assert.Equal("hi!", metadata3.DeserializeValue<string>("ValueString"));
            Assert.Equal(currentTime, metadata3.DeserializeValue<DateTimeOffset>("ValueDateTimeOffset"));
            Assert.Equal(Models.CorpusType.SourceLanguage, metadata3.DeserializeValue<Models.CorpusType>("ValueEnum"));

            // Set Metadata back to default and save:
            tokenizedTextCorpus3.TokenizedTextCorpusId.Metadata = new Dictionary<string, object>();

            await tokenizedTextCorpus3.Update(Mediator!);
            ProjectDbContext!.ChangeTracker.Clear();

            // Check "V3" values via API:
            var tokenizedTextCorpus4 = await TokenizedTextCorpus.Get(
                Mediator!,
                tokenizedTextCorpus3.TokenizedTextCorpusId,
                false);

            Assert.NotNull(tokenizedTextCorpus4);
            Assert.Equal("DisplayNameV2", tokenizedTextCorpus4.TokenizedTextCorpusId.DisplayName);
            Assert.Equal(new Dictionary<string, object>(), tokenizedTextCorpus4.TokenizedTextCorpusId.Metadata);
        }

        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private struct MetadataObject
    {
        public string ValueString { get; set; }
        public bool ValueBool { get; set; }
        public int ValueInt { get; set; }
        public DateTimeOffset ValueDateTimeOffset { get; set; }
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

    public static ITextCorpus CreateFakeTextCorpusWithComposite(bool includeBadCompositeToken)
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
}