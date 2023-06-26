using System;
using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using SIL.Machine.Translation;
using Xunit;
using Xunit.Abstractions;
using static ClearDashboard.DAL.Alignment.Translation.ITranslationCommandable;
using System.Threading.Tasks;
using System.Text;
using SIL.Machine.Tokenization;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;
using OriginatedFromValues = ClearDashboard.DAL.Alignment.Translation.Translation.OriginatedFromValues;
using Token = ClearBible.Engine.Corpora.Token;
using Corpus = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using System.Threading;
using Autofac;
using SIL.Machine.Utils;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Exceptions;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateTranslationSetCommandHandlerTests : TestBase
{
    public CreateTranslationSetCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__CreateGetAllIds()
    {
        try
        {
            var parallelTextCorpus1 = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus1 = await parallelTextCorpus1.Create("pc1", Mediator!);

            var alignmentModel1 = await BuildSampleAlignmentModel(parallelTextCorpus1);
            var alignmentSet1 = await alignmentModel1.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus1.ParallelCorpusId,
                    Mediator!);
            var translationSet1 = await TranslationSet.Create(null, alignmentSet1.AlignmentSetId, "display name 1", new(), parallelCorpus1.ParallelCorpusId, Mediator!);


            //var translationModel1 = await BuildSampleTranslationModel(parallelTextCorpus1);
            //var translationSet1 = await translationModel1.Create("display name 1", "smt model 1", new(), parallelCorpus1.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet1);


            var parallelTextCorpus2 = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus2 = await parallelTextCorpus2.Create("pc2", Mediator!);

            //var translationModel2 = await BuildSampleTranslationModel(parallelTextCorpus2);

            //var translationSet2 = await translationModel2.Create("display name 2", "smt model 2", new(), parallelCorpus2.ParallelCorpusId, Mediator!);

            var alignmentModel2 = await BuildSampleAlignmentModel(parallelTextCorpus2);
            var alignmentSet2 = await alignmentModel2.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus2.ParallelCorpusId,
                    Mediator!);
            var translationSet2 = await TranslationSet.Create(null, alignmentSet2.AlignmentSetId, "display name 1", new(), parallelCorpus2.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet2);

            ProjectDbContext!.ChangeTracker.Clear();
            var user = ProjectDbContext!.Users.First();

            var allTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!);
            Assert.Equal(2, allTranslationSetIds.Count());

            var someTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!, parallelCorpus2.ParallelCorpusId);
            Assert.Single(someTranslationSetIds);

            Assert.Equal(translationSet2.TranslationSetId, someTranslationSetIds.First());
            Assert.Equal(parallelCorpus2.ParallelCorpusId, someTranslationSetIds.First().ParallelCorpusId);

            var allTranslationSetIdsForUser = await TranslationSet.GetAllTranslationSetIds(Mediator!, null, new UserId(user.Id, user.FullName ?? string.Empty));
            Assert.Equal(2, allTranslationSetIdsForUser.Count());

            var allTranslationSetIdsForBogusUser = await TranslationSet.GetAllTranslationSetIds(Mediator!, null, new UserId(new Guid(), "User Boo"));
            Assert.Empty(allTranslationSetIdsForBogusUser);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    public async Task AlignmentSet_GetAlignmentTrainingTextCounts()
    {
        Stopwatch sw = new();
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);

            Assert.NotNull(alignmentSet);

            var count = 0;
            List<EngineParallelTextRow> someRows = new();
            foreach (var e in parallelCorpus)
            {
                someRows.Add((EngineParallelTextRow)e);
                if (count++ > 10) break;
            }

            var someAlignments = await alignmentSet.GetAlignments(someRows, AlignmentTypeGroups.AllAlignmentTypes);
            Assert.True(someAlignments.Any());

            var atp1 = someAlignments.Skip(1).Take(1).Select(a => a.AlignedTokenPair).First();
            var atp2 = someAlignments.Skip(3).Take(1).Select(a => a.AlignedTokenPair).First();

            var alignment1 = new Alignment.Translation.Alignment(atp1, "Unverified");
            await alignmentSet.PutAlignment(alignment1);
            await alignmentSet.PutAlignment(new Alignment.Translation.Alignment(atp2, "Verified"));

            sw.Start();

            var alignmentTrainingTextCounts = await alignmentSet.GetAlignmentCounts(true, true, false, AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken.None);
            Assert.Equal(13, alignmentTrainingTextCounts.Count);

            var one = alignmentTrainingTextCounts.Skip(4).First();
            Assert.Equal(1, one.Value.Count);

            Output.WriteLine("");
            Output.WriteLine($"Source training text:  {one.Key}");
            foreach (var one2 in one.Value)
            {
                Assert.Equal(2, one2.Value.StatusCounts.Count);
                Output.WriteLine($"\tTarget training text: {one2.Key}");
                foreach (var one3 in one2.Value.StatusCounts)
                {
                    Output.WriteLine($"\t\tStatus:  {one3.Key}    Count:  {one3.Value}");
                }
            }

            Output.WriteLine("");

            var alignmentTrainingTextCounts2 = await alignmentSet.GetAlignmentCounts(false, true, false, AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken.None);
            Assert.Equal(12, alignmentTrainingTextCounts2.Count);

            var verse = alignmentTrainingTextCounts2.Skip(1).First();
            Assert.Equal(3, verse.Value.Count);

            Output.WriteLine("");
            Output.WriteLine($"Target training text:  {verse.Key}");
            foreach (var verse2 in verse.Value)
            {
                Output.WriteLine($"\tSource training text: {verse2.Key}");
                foreach (var verse3 in verse2.Value.StatusCounts)
                {
                    Output.WriteLine($"\t\tStatus:  {verse3.Key}    Count:  {verse3.Value}");
                }
            }

            alignment1.Verification = "Verified";
            await alignmentSet.PutAlignment(alignment1);
            Output.WriteLine("");

        }
        finally
        {
            await DeleteDatabaseContext();

            sw.Stop();
            Output.WriteLine("Elapsed={0} - Overall", sw.Elapsed);
        }
    }
    
    [Fact]
    public async Task TokenizedCorpus_GetTokenVerseRowContext()
    {
        Stopwatch sw = new();

        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            sw.Start();

            var token1 = ProjectDbContext!.Tokens
                .Skip(10)
                .Select(e => ModelHelper.BuildToken(e))
                .FirstOrDefault();
            Assert.NotNull(token1);

            var (tokens, index) = TokenVerseContextFinder.GetTokenVerseRowContext(token1, ProjectDbContext!, Logger);

            Output.WriteLine($"Token Id: {token1.TokenId}");
            Output.WriteLine($"Index: {index}");
            foreach (var t in tokens)
            {
                Output.WriteLine($"\t{t.TokenId}");
            }

            var token2 = ProjectDbContext!.TokenComposites
                .Include(e => e.Tokens)
                .Select(e => ModelHelper.BuildToken(e))
                .FirstOrDefault();
            Assert.NotNull(token2);

            var (tokens2, index2) = TokenVerseContextFinder.GetTokenVerseRowContext(token2, ProjectDbContext!, Logger);

            Output.WriteLine($"Token Id: {token2.TokenId}");
            Output.WriteLine($"Index: {index2}");
            foreach (var t in tokens2)
            {
                Output.WriteLine($"\t{t.TokenId}");
            }

            var token3 = ((CompositeToken)token2).Tokens.First();
            var (tokens3, index3) = TokenVerseContextFinder.GetTokenVerseRowContext(token3, ProjectDbContext!, Logger);

            Output.WriteLine($"Token Id: {token3.TokenId}");
            Output.WriteLine($"Index: {index3}");
            foreach (var t in tokens3)
            {
                Output.WriteLine($"\t{t.TokenId}");
            }
        }
        finally
        {
            await DeleteDatabaseContext();

            sw.Stop();
            Output.WriteLine("Elapsed={0} - Overall", sw.Elapsed);

        }
    }

    //[Fact]
    //public async Task AlignmentSet_GetAlignmentVerseContexts_Project2969()
    //{
    //    Stopwatch sw = new();

    //    try
    //    {
    //        sw.Start();

    //        var alignmentSet = await AlignmentSet.Get(new AlignmentSetId(Guid.Parse("1EAA0CF4-D16E-4BFC-89CF-F82FF93CD7B8")), Mediator!);

    //        var alignmentTrainingTextCounts = await alignmentSet.GetAlignmentCounts(true, true, true, AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, CancellationToken.None);
    //        Assert.Equal(13939, alignmentTrainingTextCounts.Count);

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - GetAlignmentCounts()", sw.Elapsed);
    //        sw.Restart();

    //        var alignmentBookNumbers = await alignmentSet.GetBookNumbers(
    //            "וְ",
    //            "kɨ",
    //            true,
    //            AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
    //            CancellationToken.None);
    //        Assert.Equal(39, alignmentBookNumbers.Count());

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - GetBookNumbers()", sw.Elapsed);
    //        sw.Restart();

    //        var hasNextPage = true;
    //        string? cursor = null;
    //        var sum = 0;
    //        var alignmentIds = new List<Guid>();
    //        while (hasNextPage)
    //        {
    //            Stopwatch swInner = new();
    //            swInner.Start();

    //            var alignmentSetVerseContextsInner = await alignmentSet.GetAlignmentVerseContexts(
    //                "וְ",
    //                "kɨ",
    //                true,
    //                null,
    //                AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
    //                2000,
    //                cursor,
    //                CancellationToken.None);

    //            swInner.Stop();

    //            cursor = alignmentSetVerseContextsInner.Cursor;
    //            hasNextPage = alignmentSetVerseContextsInner.HasNextPage;
    //            if (alignmentSetVerseContextsInner.VerseContexts.Any())
    //            {
    //                Output.WriteLine($"\t{alignmentSetVerseContextsInner.VerseContexts.First().alignment.AlignedTokenPair.SourceToken.TokenId} to {alignmentSetVerseContextsInner.VerseContexts.Last().alignment.AlignedTokenPair.SourceToken.TokenId}: {alignmentSetVerseContextsInner.VerseContexts.Count()}  {swInner.Elapsed}");
    //                sum += alignmentSetVerseContextsInner.VerseContexts.Count();
    //            }
    //            else
    //            {
    //                Output.WriteLine("\tNo more alignments");
    //            }

    //            alignmentIds.AddRange(alignmentSetVerseContextsInner.VerseContexts.Select(e => e.alignment!.AlignmentId!.Id));
    //        }

    //        Output.WriteLine($"\t SUM: {sum}");
    //        Assert.Equal(15588, sum);

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - GetAlignmentVerseContexts(paging) all", sw.Elapsed);
    //        sw.Restart();

    //        var alignmentSetVerseContexts = await alignmentSet.GetAlignmentVerseContexts(
    //            "וְ",
    //            "kɨ",
    //            true,
    //            null,
    //            AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
    //            CancellationToken.None);
    //        Assert.Equal(15588, alignmentSetVerseContexts.Count());

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - GetAlignmentVerseContexts(null booknumber) {1}", sw.Elapsed, alignmentSetVerseContexts.Count());
    //        sw.Restart();

    //        var alignmentSetVerseContexts2 = await alignmentSet.GetAlignmentVerseContexts(
    //            "וְ",
    //            "kɨ",
    //            true,
    //            1,
    //            AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
    //            CancellationToken.None);
    //        Assert.Equal(1041, alignmentSetVerseContexts2.Count());

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - GetAlignmentVerseContexts(1 booknumber)", sw.Elapsed);
    //        sw.Restart();

    //    }
    //    finally
    //    {
    //        //            await DeleteDatabaseContext();

    //        sw.Stop();
    //        Output.WriteLine("Elapsed={0} - Overall", sw.Elapsed);
    //    }
    //}

    [Fact]
    public async Task AlignmentSet_GetAlignmentVerseContexts()
    {
        Stopwatch sw = new();

        try
        { 
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);

            Assert.NotNull(alignmentSet);

            sw.Start();

            var alignmentSetVerseContexts = await alignmentSet.GetAlignmentVerseContexts(
                "verse", 
                "verse",
                true,
                null,
                AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, 
                CancellationToken.None);
            Assert.Equal(12, alignmentSetVerseContexts.Count());

            var alignmentSetVerseContexts2 = await alignmentSet.GetAlignmentVerseContexts(
                "verse",
                "verse",
                true,
                40,
                AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
                CancellationToken.None);
            Assert.Equal(9, alignmentSetVerseContexts2.Count());

            var one = alignmentSetVerseContexts.FirstOrDefault();
            Assert.True(one != default);

            Assert.Equal(6, one.sourceVerseTokens.Count());
            Assert.Equal(6, one.targetVerseTokens.Count());
            Assert.Equal((uint)3, one.sourceVerseTokensIndex);
            Assert.Equal((uint)3, one.targetVerseTokensIndex);

            Output.WriteLine($"Alignment source: {one.alignment.AlignedTokenPair.SourceToken.TokenId}");
            Output.WriteLine($"Verse context index (source): {one.sourceVerseTokensIndex}");
            foreach (var vc in one.sourceVerseTokens)
            {
                Output.WriteLine($"\t{vc.TokenId}");
            }
            Output.WriteLine($"Alignment target: {one.alignment.AlignedTokenPair.TargetToken.TokenId}");
            Output.WriteLine($"Verse context index (target): {one.targetVerseTokensIndex}");
            foreach (var vc in one.targetVerseTokens)
            {
                Output.WriteLine($"\t{vc.TokenId}");
            }

            Output.WriteLine("");

            var alignmentSetVerseContexts3 = await alignmentSet.GetAlignmentVerseContexts(
                "one_verse_three", 
                "three",
                true,
                null,
                AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded, 
                CancellationToken.None);
            Assert.Equal(2, alignmentSetVerseContexts3.Count());

            var alignmentSetVerseContexts4 = await alignmentSet.GetAlignmentVerseContexts(
                "one_verse_three",
                "three",
                true,
                40,
                AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded,
                CancellationToken.None);
            Assert.Single(alignmentSetVerseContexts4);

            var two = alignmentSetVerseContexts3.FirstOrDefault();
            Assert.True(two != default);

            Assert.Equal(4, two.sourceVerseTokens.Count());
            Assert.Equal(6, two.targetVerseTokens.Count());
            Assert.Equal((uint)1, two.sourceVerseTokensIndex);
            Assert.Equal((uint)4, two.targetVerseTokensIndex);

            Output.WriteLine($"Alignment source: {two.alignment.AlignedTokenPair.SourceToken.TokenId}");
            Output.WriteLine($"Verse context index (source): {two.sourceVerseTokensIndex}");
            foreach (var vc in two.sourceVerseTokens)
            {
                Output.WriteLine($"\t{vc.TokenId}");
            }
            Output.WriteLine($"Alignment target: {two.alignment.AlignedTokenPair.TargetToken.TokenId}");
            Output.WriteLine($"Verse context index (target): {two.targetVerseTokensIndex}");
            foreach (var vc in two.targetVerseTokens)
            {
                Output.WriteLine($"\t{vc.TokenId}");
            }
        }
        finally
        {
            await DeleteDatabaseContext();

            sw.Stop();
            Output.WriteLine("Elapsed={0} - Overall", sw.Elapsed);
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__Create()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            //var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            //var translationSet = await translationModel.Create("display name", "smt model", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet);

            ProjectDbContext!.ChangeTracker.Clear();

            var translationSetDb = ProjectDbContext.TranslationSets
                .Include(ts => ts.TranslationModel)
                .Include(ts => ts.Translations)
                .FirstOrDefault(ts => ts.Id == translationSet.TranslationSetId.Id);
            Assert.NotNull(translationSetDb);

            Assert.Equal(translationSet.ParallelCorpusId.Id, translationSetDb!.ParallelCorpusId);
            Assert.Empty(translationSetDb!.Translations);
            /*
            Assert.True(translationModel.Keys.Count > 3);
            var tm = await translationSet.GetTranslationModelEntryForToken(new Token(new TokenId(1, 1, 1, 1, 1), "surface", translationModel.Keys.Skip(3).First()));
            Assert.NotNull(tm);

            Output.WriteLine($"Translation model entry for {translationModel.Keys.Skip(3).First()}");
            foreach (var kvp in tm)
            {
                Output.WriteLine($"\tTarget text: {kvp.Key} / score: {kvp.Value}");
            }
            */
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__CreateTranslations()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            //var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            //var translationSet = await translationModel.Create("display name", "smt model", new() { { "boo", "baa"} }, parallelCorpus.ParallelCorpusId, Mediator!);
            

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet);

            var bookId = parallelCorpus.SourceCorpus.Texts.Select(t => t.Id).ToList().First();
            var sourceTokens = parallelCorpus.SourceCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().First().Tokens;

            var iteration = 0;
            var exampleTranslations = new List<Alignment.Translation.Translation>();
            foreach (var sourceToken in sourceTokens)
            {
                iteration++;

                exampleTranslations.Add(new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", Alignment.Translation.Translation.OriginatedFromValues.Assigned));
                Output.WriteLine($"Token for adding Translation: {sourceToken.TokenId}");

                if (iteration >= 5)
                {
                    break;
                }
            }

            foreach (var exampleTranslation in exampleTranslations)
            {
                await translationSet.PutTranslation(
                    exampleTranslation,
                    TranslationActionTypes.PutNoPropagate);
            }

            ProjectDbContext!.ChangeTracker.Clear();

            var translationSetDb = ProjectDbContext.TranslationSets
                .Include(ts => ts.Translations)
                    .ThenInclude(t => t.SourceTokenComponent)
                .FirstOrDefault(ts => ts.Id == translationSet.TranslationSetId.Id);
            Assert.NotNull(translationSetDb);
            Assert.Equal(exampleTranslations.Count, translationSetDb!.Translations.Count);

            var tokensInDb = translationSetDb!.Translations.Select(t => t.SourceTokenComponent!.EngineTokenId!);

            foreach (var exampleTranslation in exampleTranslations)
            {
                Assert.Contains<string>(exampleTranslation.SourceToken.TokenId.ToString(), tokensInDb);
            }

            // FIXME:  quick test of PutPropagate.  Need better tests of this
            await translationSet.PutTranslation(
                    new Alignment.Translation.Translation(exampleTranslations[1].SourceToken!, $"toobedoo", Alignment.Translation.Translation.OriginatedFromValues.Assigned),
                    TranslationActionTypes.PutNoPropagate);

            await translationSet.PutTranslation(
                new Alignment.Translation.Translation(exampleTranslations[2].SourceToken!, $"goobedoo", Alignment.Translation.Translation.OriginatedFromValues.Assigned),
                TranslationActionTypes.PutNoPropagate);

            var to = new Token(
                new TokenId(
                    exampleTranslations[1].SourceToken!.TokenId.BookNumber,
                    exampleTranslations[1].SourceToken!.TokenId.ChapterNumber,
                    exampleTranslations[1].SourceToken!.TokenId.VerseNumber,
                    5,
                    exampleTranslations[1].SourceToken!.TokenId.SubWordNumber)
                {
                    Id = exampleTranslations[1].SourceToken.TokenId.Id
                },
                exampleTranslations[1].SourceToken!.SurfaceText,
                exampleTranslations[1].SourceToken!.TrainingText);

            await translationSet.PutTranslation(
                    new Alignment.Translation.Translation(to, $"shoobedoo", Alignment.Translation.Translation.OriginatedFromValues.Assigned),
                    TranslationActionTypes.PutPropagate);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Single(ProjectDbContext.Translations.Where(t => t.TargetText == "goobedoo"));
            Assert.Equal(10, ProjectDbContext.Translations
                .Where(t => t.TargetText == "shoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.FromOther)
                .Count());
            Assert.Equal(1, ProjectDbContext.Translations
                .Where(t => t.TargetText == "shoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.Assigned)
                .Count());

            await translationSet.PutTranslation(
                    new Alignment.Translation.Translation(to, $"hoobedoo", "Assigned"),
                    TranslationActionTypes.PutPropagate);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Single(ProjectDbContext.Translations.Where(t => t.TargetText == "goobedoo"));
            Assert.Equal(0, ProjectDbContext.Translations
                .Where(t => t.TargetText == "shoobedoo")
                .Count());
            Assert.Equal(10, ProjectDbContext.Translations
                .Where(t => t.TargetText == "hoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.FromOther)
                .Count());
            Assert.Equal(1, ProjectDbContext.Translations
                .Where(t => t.TargetText == "hoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.Assigned)
                .Count());

            var to2 = ModelHelper.BuildToken(ProjectDbContext.Translations
                .Include(t => t.SourceTokenComponent)
                .Where(t => t.TargetText == "hoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.FromOther)
                .Skip(3)
                .First()
                .SourceTokenComponent!);

            await translationSet.PutTranslation(
                    new Alignment.Translation.Translation(to2, $"coobedoo", Alignment.Translation.Translation.OriginatedFromValues.Assigned),
                    TranslationActionTypes.PutPropagate);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Single(ProjectDbContext.Translations.Where(t => t.TargetText == "goobedoo"));
            Assert.Equal(0, ProjectDbContext.Translations
                .Where(t => t.TargetText == "shoobedoo")
                .Count());
            Assert.Equal(0, ProjectDbContext.Translations
                .Where(t => t.TargetText == "hoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.FromOther)
                .Count());
            Assert.Equal(1, ProjectDbContext.Translations
                .Where(t => t.TargetText == "hoobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.Assigned)
                .Count());
            Assert.Equal(9, ProjectDbContext.Translations
                .Where(t => t.TargetText == "coobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.FromOther)
                .Count());
            Assert.Equal(1, ProjectDbContext.Translations
                .Where(t => t.TargetText == "coobedoo" && t.TranslationState == Models.TranslationOriginatedFrom.Assigned)
                .Count());


            /*
            translationSet.PutTranslationModelEntry("one", new Dictionary<string, double>()
            {
                { "boo", 0.98 },
                { "baa", 0.53 },
                { "doo", 0.11 },
                { "daa", 0.001 }
            });

            translationSet.PutTranslationModelEntry("newone", new Dictionary<string, double>()
            {
                { "noo", 0.98 },
                { "naa", 0.53 },
                { "moo", 0.11 },
                { "maa", 0.001 }
            });
            */

            var translationSetGuid = translationSet.TranslationSetId.Id;
            var alignmentSetGuid = alignmentSet.AlignmentSetId.Id;

            Output.WriteLine($"Deleting TranslationSet: '{translationSetGuid}'");
            await translationSet.Delete(Mediator!);
            Assert.False(ProjectDbContext.TranslationSets.Where(e => e.Id == translationSetGuid).Any());
            Assert.True(ProjectDbContext.AlignmentSets.Where(e => e.Id == alignmentSetGuid).Any());

            Output.WriteLine($"Deleting AlignmentSet: '{alignmentSetGuid}'");
            await alignmentSet.Delete(Mediator!);
            Assert.False(ProjectDbContext.AlignmentSets.Where(e => e.Id == alignmentSetGuid).Any());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    //[Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationAlignmentSets__ManuscriptZZSur()
    {
        try
        {
            var engineParallelTextCorpus = await BuildSampleManuscriptToZZSurEngineParallelTextCorpus();
            var parallelCorpus = await engineParallelTextCorpus.Create("test pc", Mediator!);

            var translationCommandable = Container!.Resolve<TranslationCommands>();

            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                parallelCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);

            /*
            var translationModel = smtWordAlignmentModel.GetTranslationTable();
            var translationSet = await translationModel.Create(
                "manuscript to zz_sur",
                "fastalign",
                new() { { "size", "large" }, { "how large", "huge" } }, 
                parallelCorpus.ParallelCorpusId, 
                Mediator!);
            */
           
            var alignmentModel = translationCommandable.PredictAllAlignedTokenIdPairs(smtWordAlignmentModel, parallelCorpus).ToList();
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);

            Assert.NotNull(alignmentSet);

            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet);

        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

//    [Fact]
    [Trait("Category", "Handlers")]
    public async Task AlignmentSet__ManuscriptZZSur()
    {
        try
        {
            var engineParallelTextCorpus = await BuildSampleManuscriptToZZSurEngineParallelTextCorpus();
            var parallelCorpus = await engineParallelTextCorpus.Create("test pc", Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);

            Assert.NotNull(alignmentSet);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task AlignmentSet__SmallSample()
    {
        try
        {
            var engineParallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await engineParallelTextCorpus.Create("test pc", Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    true,
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);

            var count = 0;
            List<EngineParallelTextRow> someRows = new();
            foreach (var e in parallelCorpus)
            {
                someRows.Add((EngineParallelTextRow)e);
                if (count++ > 10) break;
            }

            var someAlignments = await alignmentSet.GetAlignments(someRows, AlignmentTypeGroups.AllAlignmentTypes);
            Assert.True(someAlignments.Any());

            var atp1 = someAlignments.Skip(1).Take(1).Select(a => a.AlignedTokenPair).First();
            var atp2 = someAlignments.Skip(3).Take(1).Select(a => a.AlignedTokenPair).First();
            var a3 = someAlignments.Skip(5).Take(1).First();
            var a4 = someAlignments.Skip(7).Take(1).First();
            var a5 = someAlignments.Skip(8).Take(1).First();

            await alignmentSet.DeleteAlignment(a3.AlignmentId!);

            var alignmentToUpdateById = new Alignment.Translation.Alignment(atp1, "Unverified");
            var alignmentToUpdateBySourceTargetToken = new Alignment.Translation.Alignment(new AlignedTokenPairs(atp2.SourceToken, a3.AlignedTokenPair.TargetToken, 101), "Unverified");
            await alignmentSet.PutAlignment(alignmentToUpdateById);
            await alignmentSet.PutAlignment(alignmentToUpdateBySourceTargetToken);

            alignmentToUpdateById.Verification = "Verified";
            alignmentToUpdateBySourceTargetToken = new Alignment.Translation.Alignment(new AlignedTokenPairs(atp2.SourceToken, a3.AlignedTokenPair.TargetToken, 101), "Verified");

            var alignmentsToPut = new List<Alignment.Translation.Alignment>
            {
                alignmentToUpdateById,
                alignmentToUpdateBySourceTargetToken,
                new Alignment.Translation.Alignment(new AlignedTokenPairs(a3.AlignedTokenPair.SourceToken, atp2.TargetToken, 102), "Unverified"),
                new Alignment.Translation.Alignment(new AlignedTokenPairs(a4.AlignedTokenPair.SourceToken, a5.AlignedTokenPair.TargetToken, 102), "Unverified")
            };

            await alignmentSet.PutAlignments(alignmentsToPut);

            var manualOnly = await alignmentSet.GetAlignments(someRows, AlignmentTypeGroups.AssignedAlignmentTypes);
            Assert.Equal(4, manualOnly.Count());

            var manualVerifiedOnly = await alignmentSet.GetAlignments(someRows, AlignmentTypes.Assigned_Verified);
            Assert.Equal(2, manualVerifiedOnly.Count());

            var manualOnlyNonManualAuto = await alignmentSet.GetAlignments(someRows, AlignmentTypeGroups.AssignedAndUnverifiedNotOtherwiseIncluded);

            // DeleteAlignment above reduces someAlignments count by 1 (79 -> 78), and the
            // subsequent four PutAlignments should effectively 'hide' four auto alignments
            // (by including auto alignment tokens: atp1, atp2, a4 and a5 in new manual
            // alignments).  So that takes the number of auto alignments returned when using
            // "ManualAndOnlyNonManualAuto" from 78 -> 74.  

            Assert.Equal(someAlignments.Count() - 5, manualOnlyNonManualAuto.Where(a => a.OriginatedFrom == Models.AlignmentOriginatedFrom.FromAlignmentModel.ToString()).Count());
            Assert.Equal(4, manualOnlyNonManualAuto.Where(a => a.OriginatedFrom == Models.AlignmentOriginatedFrom.Assigned.ToString()).Count());

            var alignment = someAlignments.First();
            Assert.NotNull(alignment);
            Assert.NotNull(alignment.AlignmentId);
            Assert.Equal(Models.AlignmentVerification.Unverified.ToString(), alignment.Verification);
            Assert.Equal(Models.AlignmentOriginatedFrom.FromAlignmentModel.ToString(), alignment.OriginatedFrom);

            var alignmentDbBefore = ProjectDbContext!.Alignments
                .Where(e => e.Id == alignment.AlignmentId.Id)
                .FirstOrDefault();

            Assert.NotNull(alignmentDbBefore);
            Assert.Equal(Models.AlignmentVerification.Unverified, alignmentDbBefore.AlignmentVerification);
            Assert.Equal(Models.AlignmentOriginatedFrom.FromAlignmentModel, alignmentDbBefore.AlignmentOriginatedFrom);
            Assert.Null(alignmentDbBefore.Deleted);

            ProjectDbContext.ChangeTracker.Clear();

            await alignmentSet.DeleteAlignment(alignment.AlignmentId);

            var alignmentDbAfter = ProjectDbContext!.Alignments
                .Where(e => e.Id == alignment.AlignmentId.Id)
                .FirstOrDefault();

            Assert.NotNull(alignmentDbAfter);
            Assert.NotNull(alignmentDbAfter.Deleted);

            var alignmentSetCopy = await AlignmentSet.Get(alignmentSet.AlignmentSetId, Mediator!);
            Assert.NotNull(alignmentSetCopy);
            Assert.Equal(alignmentSet.AlignmentSetId.DisplayName, alignmentSetCopy.AlignmentSetId.DisplayName);
            Assert.Equal(alignmentSet.AlignmentSetId.SmtModel, alignmentSetCopy.AlignmentSetId.SmtModel);
            Assert.Equal(alignmentSet.AlignmentSetId.IsSyntaxTreeAlignerRefined, alignmentSetCopy.AlignmentSetId.IsSyntaxTreeAlignerRefined);
            Assert.Equal(alignmentSet.AlignmentSetId.IsSymmetrized, alignmentSetCopy.AlignmentSetId.IsSymmetrized);
            Assert.Equal(alignmentSet.AlignmentSetId.ParallelCorpusId, alignmentSetCopy.AlignmentSetId.ParallelCorpusId);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__GetTranslationRange()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            //var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            //var translationSet = await translationModel.Create("display name", "smt model", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            Assert.NotNull(translationSet);

            var initialFilteredEngineParallelTextRows = parallelCorpus.Take(5).Cast<EngineParallelTextRow>();
            var initialTranslations = await translationSet.GetTranslations(initialFilteredEngineParallelTextRows);

            Output.WriteLine($"Translations from model count: {initialTranslations.Count()} (from first five engine parallel text rows)");
            //foreach (var translation in initialTranslations)
            //{
            //    Output.WriteLine($"\tTokenId: {translation.SourceToken.TokenId}, TrainingText: {translation.SourceToken.TrainingText}, TargetTranslationText: {translation.TargetTranslationText}, TranslationState: {translation.OriginatedFrom}");
            //}

            Output.WriteLine("");

            var iteration = 1;
            var putTranslationTokenIds = new List<TokenId>();
            foreach (var bookId in parallelCorpus.SourceCorpus.Texts.Select(t => t.Id))
            {
                Output.WriteLine($"Book: {bookId}");
                var row = 1;
                foreach (var ttr in parallelCorpus.SourceCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>())
                {
                    Output.WriteLine($"\tVerse (row): {row++}");
                    foreach (var sourceToken in ttr.Tokens)
                    {
                        if (sourceToken.TokenId.ToString()[..15].CompareTo("040001004004001") >= 0 && sourceToken.TokenId.ToString()[..15].CompareTo("040002002004001") <= 0)
                        {
                            Output.WriteLine($"\t\tTokenId: {sourceToken.TokenId}");
                            await translationSet.PutTranslation(
                                new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", Alignment.Translation.Translation.OriginatedFromValues.Assigned),
                                TranslationActionTypes.PutNoPropagate);

                            iteration++;
                            putTranslationTokenIds.Add(sourceToken.TokenId);
                        }
                    }
                }
            }

            Output.WriteLine($"\nPutTranslations (no propagate) for {putTranslationTokenIds.Count} token ids: ");
            foreach (var tokenId in putTranslationTokenIds)
            {
                Output.WriteLine($"\t{tokenId}");
            }

            ProjectDbContext!.ChangeTracker.Clear();

            Output.WriteLine("");

            var filteredEngineParallelTextRows = parallelTextCorpus.Cast<EngineParallelTextRow>()
                .Where(e => e.SourceTokens!
                    .Any(t => t.TokenId.ToString()[..15].CompareTo("040001002004001") >= 0 && t.TokenId.ToString()[..15].CompareTo("040001005005001") <= 0))
                .ToList();

            var translations = await translationSet.GetTranslations(filteredEngineParallelTextRows);
            Assert.Equal(22, translations.Count());

            Output.WriteLine($"Translation count: {translations.Count()}");
            Output.WriteLine("");
            foreach (var translation in translations)
            {
                if (translation.OriginatedFrom != OriginatedFromValues.FromTranslationModel && translation.OriginatedFrom != OriginatedFromValues.FromAlignmentModel)
                {
                    Assert.InRange<TokenId>(translation.SourceToken.TokenId, new TokenId("040001003001001"), new TokenId("040001005006001"), Comparer<TokenId>.Create((t1, t2) => t1.CompareTo(t2)));
                }
                Output.WriteLine($"TokenId: {translation.SourceToken.TokenId}, TrainingText: {translation.SourceToken.TrainingText}, TargetTranslationText: {translation.TargetTranslationText}, TranslationState: {translation.OriginatedFrom}");
            }
            Output.WriteLine("DONE");
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__NoTranslationFound()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            //var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            //var translationSet = await translationModel.Create("display name", "smt model", new(), parallelCorpus.ParallelCorpusId, Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            Output.WriteLine("Denormalizing alignment set data");
            await RunAlignmentSetDenormalizationAsync(Guid.Empty);

            var tokenToCopy = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == parallelCorpus.ParallelCorpusId.SourceTokenizedCorpusId!.Id)
                .FirstOrDefault();

            Assert.NotNull(tokenToCopy);

            var newModelToken = new Models.Token()
            {
                Id = Guid.NewGuid(),
                BookNumber = tokenToCopy.BookNumber,
                ChapterNumber = tokenToCopy.ChapterNumber,
                VerseNumber = tokenToCopy.VerseNumber,
                WordNumber = tokenToCopy.WordNumber,
                SubwordNumber = tokenToCopy.SubwordNumber,
                SurfaceText = "boobooboo",
                EngineTokenId = tokenToCopy.EngineTokenId,
                TrainingText ="booboobooboo",
                VerseRowId = tokenToCopy.VerseRowId,
                TokenizedCorpusId = tokenToCopy.TokenizedCorpusId
            };

            ProjectDbContext!.TokenComponents.Add(newModelToken);
            await ProjectDbContext!.SaveChangesAsync();

            var newToken = ModelHelper.BuildToken(newModelToken);

            var translationsForNewToken = await translationSet.GetTranslations(new List<TokenId>() { newToken.TokenId });
            Assert.Single(translationsForNewToken);
            Assert.Equal(newToken.TokenId, translationsForNewToken.First().SourceToken.TokenId);
            Assert.Equal(OriginatedFromValues.FromAlignmentModel, translationsForNewToken.First().OriginatedFrom);
            Assert.Empty(translationsForNewToken.First().TargetTranslationText);
            Assert.Null(translationsForNewToken.First().TranslationId);  // The Translation returned should be a default 'empty' translation - not from the DB

            var tme = await translationSet.GetTranslationModelEntryForToken(newToken);
            Assert.NotNull(tme);
            Assert.Empty(tme);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__Lexicon()
    {
        try
        {
            // SETUP
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpusWithComposite();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            Output.WriteLine("Denormalizing alignment set data");
            await RunAlignmentSetDenormalizationAsync(Guid.Empty);

            var tokensToQuery = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == parallelCorpus.ParallelCorpusId.SourceTokenizedCorpusId!.Id)
                .Take(4)
                .ToArray();

            Assert.NotNull(tokensToQuery);
            Assert.Equal(4, tokensToQuery.Length);

            // LEXICON
            // lexeme1:
            var lexeme1 = await new Lexeme { 
                Lemma = tokensToQuery[0].TrainingText,
                Language = parallelCorpus.ParallelCorpusId?.SourceTokenizedCorpusId?.CorpusId?.Language,
                Type = "some type"
            }.Create(Mediator!);

            await lexeme1.PutForm(Mediator!, new Form { Text = tokensToQuery[0].TrainingText + "_form1" });
            await lexeme1.PutForm(Mediator!, new Form { Text = tokensToQuery[0].TrainingText + "_form2" });

            var lexeme1Meaning1 = new Meaning
            {
                Text = "l1_meaning1",
                Language = parallelCorpus.ParallelCorpusId?.TargetTokenizedCorpusId?.CorpusId?.Language
            };
            var lexeme1Meaning2 = new Meaning { Text = "l1_meaning2" /* no language */ };
            await lexeme1.PutMeaning(Mediator!, lexeme1Meaning1);
            await lexeme1.PutMeaning(Mediator!, lexeme1Meaning2);

            await lexeme1Meaning2.PutTranslation(Mediator!, new Lexicon.Translation { Text = "l1_meaning2_t1" });
            await lexeme1Meaning2.PutTranslation(Mediator!, new Lexicon.Translation { Text = "l1_meaning2_t2" });

            // lexeme2:
            var lexeme2 = await new Lexeme { Lemma = tokensToQuery[1].TrainingText /* no language */  }.Create(Mediator!);
            await lexeme2.PutMeaning(Mediator!, new Meaning { Text = "l2_meaning1" /* no language */ });
            var lexeme2Meaning2 = new Meaning { Text = "l2_meaning2", Language = "bogus" };
            var lexeme2Meaning3 = new Meaning { Text = "l2_meaning3", Language = parallelCorpus.ParallelCorpusId?.TargetTokenizedCorpusId?.CorpusId?.Language };
            await lexeme2.PutMeaning(Mediator!, lexeme2Meaning2);
            await lexeme2.PutMeaning(Mediator!, lexeme2Meaning3);
            var s1 = await lexeme2Meaning2.CreateAssociateSenanticDomain(Mediator!, "sem1");
            var s2 = await lexeme2Meaning3.CreateAssociateSenanticDomain(Mediator!, "sem2");

            // lexeme3:
            var lexeme3 = await new Lexeme { Lemma = tokensToQuery[3].TrainingText, Language = "bogus"  }.Create(Mediator!);
            await lexeme3.PutMeaning(Mediator!, new Meaning { Text = "l3_meaning1" /* no language */ });
            await lexeme3.Meanings.First().AssociateSemanticDomain(Mediator!, s2);

            ProjectDbContext!.ChangeTracker.Clear();

            Assert.Null(await Lexeme.Get(Mediator!, "bogusLemma", null, null));

            var l1Db1 = await Lexeme.Get(Mediator!, lexeme1.Lemma!, lexeme1.Language!, null);
            Assert.NotNull(l1Db1);
            Assert.Equal(2, l1Db1.Forms.Count);
            Assert.Equal(2, l1Db1.Meanings.Count);
            Assert.Equal(lexeme1.Lemma, l1Db1.Lemma);
            Assert.Equal(lexeme1.Language, l1Db1.Language);
            Assert.Equal(lexeme1.Type, l1Db1.Type);
            Assert.Empty(l1Db1.Meanings.Where(s => s.Text == "l1_meaning1").First().Translations);
            Assert.Equal(2, l1Db1.Meanings.Where(s => s.Text == "l1_meaning2").First().Translations.Count);

            ProjectDbContext!.ChangeTracker.Clear();

            var l1Db2 = await Lexeme.Get(
                Mediator!,
                lexeme1.Lemma!,
                lexeme1.Language!,
                parallelCorpus.ParallelCorpusId?.TargetTokenizedCorpusId?.CorpusId?.Language);
            Assert.NotNull(l1Db2);

            ProjectDbContext!.ChangeTracker.Clear();

            var l2Db1 = await Lexeme.Get(Mediator!, lexeme2.Lemma!, null, null);

            Assert.NotNull(l2Db1);
            Assert.Equal(3, l2Db1.Meanings.Count);
            Assert.Contains(lexeme2Meaning2.Text, l2Db1.Meanings.Select(s => s.Text));
            Assert.Contains(lexeme2Meaning3.Text, l2Db1.Meanings.Select(s => s.Text));
            Assert.Single(l2Db1.Meanings.Where(s => s.Text == lexeme2Meaning2.Text).First().SemanticDomains);
            Assert.Equal(s1.Text, l2Db1.Meanings.Where(s => s.Text == lexeme2Meaning2.Text).First().SemanticDomains.First().Text);
            Assert.Single(l2Db1.Meanings.Where(s => s.Text == lexeme2Meaning3.Text).First().SemanticDomains);
            Assert.Equal(s2.Text, l2Db1.Meanings.Where(s => s.Text == lexeme2Meaning3.Text).First().SemanticDomains.First().Text);

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => new Lexeme
            {
                Lemma = l1Db1!.Lemma,
                Language = l1Db1!.Language
            }.Create(Mediator!));

            var l3Db1 = await Lexeme.Get(
                Mediator!,
                lexeme3.Lemma!,
                null,
                null);

            Assert.NotNull(l3Db1);
            Assert.Single(l3Db1.Meanings);
            Assert.Single(l3Db1.Meanings.First().SemanticDomains);
            Assert.Equal(s2.Text, l3Db1.Meanings.First().SemanticDomains.First().Text);

            // Test GetTranslations:
            var tokenIds = tokensToQuery.Where(t => t.TrainingText != ",").Select(t => ModelHelper.BuildTokenId(t));

            var translations = await translationSet.GetTranslations(tokenIds);
            Assert.Equal(3, translations.Count());

            var translationsFromLexicon = translations.Where(t => t.OriginatedFrom == OriginatedFromValues.FromLexicon);
            Assert.Equal(2, translationsFromLexicon.Count());

            var translation1 = translationsFromLexicon.Where(t => t.SourceToken.TrainingText == lexeme1.Lemma).FirstOrDefault();
            Assert.NotNull(translation1);
            Assert.Null(translation1.TranslationId);
            Assert.Equal(tokensToQuery[0].Id, translation1.SourceToken.TokenId.Id);
            Assert.Equal("l1_meaning1/l1_meaning2", translation1.TargetTranslationText);

            var translation2 = translationsFromLexicon.Where(t => t.SourceToken.TrainingText == lexeme2.Lemma).FirstOrDefault();
            Assert.NotNull(translation2);
            Assert.Null(translation2.TranslationId);
            Assert.Equal(tokensToQuery[1].Id, translation2.SourceToken.TokenId.Id);
            Assert.Equal("l2_meaning1/l2_meaning3", translation2.TargetTranslationText);

            ProjectDbContext!.ChangeTracker.Clear();

            await l2Db1!.Delete(Mediator!);
            Assert.Null(await Lexeme.Get(Mediator!, lexeme2.Lemma!, null, null));

            await lexeme3.Meanings.First().DetachSemanticDomain(Mediator!, s2);
            Assert.Empty(lexeme3.Meanings.First().SemanticDomains);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__CorpusCascadeDelete()
    {
        try
        {
            var parallelCorpusId = await BuildSampleCorpusAlignmentTranslationData();

            ProjectDbContext!.ChangeTracker.Clear();

            var leftoverCorpusId = parallelCorpusId.TargetTokenizedCorpusId!.CorpusId!.Id;
            var leftoverTokenizedCorpusId = parallelCorpusId.TargetTokenizedCorpusId!.Id;

            await Corpus.Delete(Mediator!, parallelCorpusId.SourceTokenizedCorpusId!.CorpusId!);

            Assert.Empty(ProjectDbContext!.ParallelCorpa);
            Assert.Empty(ProjectDbContext!.TranslationSets);
            Assert.Empty(ProjectDbContext!.Translations);
            Assert.Empty(ProjectDbContext!.TranslationModelEntries);
            Assert.Empty(ProjectDbContext!.Set<Models.TranslationModelTargetTextScore>());
            Assert.Empty(ProjectDbContext!.AlignmentSets);
            Assert.Empty(ProjectDbContext!.Alignments);

            Assert.Single(ProjectDbContext!.Corpa);
            Assert.Single(ProjectDbContext!.TokenizedCorpora);

            Assert.NotEmpty(ProjectDbContext!.TokenComponents.Where(tc => tc.TokenizedCorpusId == leftoverTokenizedCorpusId));
            Assert.Empty(ProjectDbContext!.TokenComponents.Where(tc => tc.TokenizedCorpusId != leftoverTokenizedCorpusId));

            Assert.NotNull(ProjectDbContext!.Corpa.Where(c => c.Id == leftoverCorpusId).FirstOrDefault());
            Assert.NotNull(ProjectDbContext!.TokenizedCorpora.Where(c => c.Id == leftoverTokenizedCorpusId).FirstOrDefault());
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__ParallelCorpusCascadeDelete()
    {
        try
        {
            var parallelCorpusId = await BuildSampleCorpusAlignmentTranslationData();

            ProjectDbContext!.ChangeTracker.Clear();

            var leftoverCorpusIds = new List<Guid>() { parallelCorpusId.SourceTokenizedCorpusId!.CorpusId!.Id, parallelCorpusId.TargetTokenizedCorpusId!.CorpusId!.Id };
            var leftoverTokenizedCorpusIds = new List<Guid>() { parallelCorpusId.SourceTokenizedCorpusId!.Id, parallelCorpusId.TargetTokenizedCorpusId!.Id };

            await ParallelCorpus.Delete(Mediator!, parallelCorpusId);

            Assert.Empty(ProjectDbContext!.ParallelCorpa);
            Assert.Empty(ProjectDbContext!.TranslationSets);
            Assert.Empty(ProjectDbContext!.Translations);
            Assert.Empty(ProjectDbContext!.TranslationModelEntries);
            Assert.Empty(ProjectDbContext!.Set<Models.TranslationModelTargetTextScore>());
            Assert.Empty(ProjectDbContext!.AlignmentSets);
            Assert.Empty(ProjectDbContext!.Alignments);

            Assert.True(ProjectDbContext!.Corpa.Count() == 2);
            Assert.True(ProjectDbContext!.TokenizedCorpora.Count() == 2);

            Assert.NotEmpty(ProjectDbContext!.TokenComponents.Where(tc => leftoverTokenizedCorpusIds.Contains(tc.TokenizedCorpusId)));
            Assert.Empty(ProjectDbContext!.TokenComponents.Where(tc => !leftoverTokenizedCorpusIds.Contains(tc.TokenizedCorpusId)));

            Assert.Empty(ProjectDbContext!.Corpa.Where(c => !leftoverCorpusIds.Contains(c.Id)));
            Assert.Empty(ProjectDbContext!.TokenizedCorpora.Where(c => !leftoverTokenizedCorpusIds.Contains(c.Id)));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__InvalidParallelCorpusId()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await parallelTextCorpus.Create("pc1", Mediator!);

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            // Should throw an exception because of the bogus ParallelCorpusId:
            //await Assert.ThrowsAnyAsync<Exception>(() => translationModel.Create(null, string.Empty, new(), new ParallelCorpusId(new Guid()), Mediator!));

            var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
            var alignmentSet = await alignmentModel.Create(
                    "manuscript to zz_sur",
                    "fastalign",
                    false,
                    false,
                    new Dictionary<string, object>(), //metadata
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
            await Assert.ThrowsAnyAsync<Exception>(() => TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), new ParallelCorpusId(new Guid()), Mediator!));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private async Task<ParallelCorpusId> BuildSampleCorpusAlignmentTranslationData()
    {
        var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
        var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

        var alignmentModel = await BuildSampleAlignmentModel(parallelTextCorpus);
        var alignmentSet = await alignmentModel.Create(
                "manuscript to zz_sur",
                "fastalign",
                false,
                false,
                new Dictionary<string, object>(), //metadata
                parallelCorpus.ParallelCorpusId,
                Mediator!);
        var translationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId, "display name 1", new(), parallelCorpus.ParallelCorpusId, Mediator!);

        var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);
        var translationSet0 = await TranslationSet.Create(translationModel, alignmentSet.AlignmentSetId, "display name 2", new(), parallelCorpus.ParallelCorpusId, Mediator!);

        Assert.NotNull(translationSet);
        Assert.NotNull(translationSet0);

        var bookId = parallelCorpus.SourceCorpus.Texts.Select(t => t.Id).ToList().First();
        var sourceTokens = parallelCorpus.SourceCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().First().Tokens;

        var iteration = 0;
        var exampleTranslations = new List<Alignment.Translation.Translation>();
        foreach (var sourceToken in sourceTokens)
        {
            iteration++;

            exampleTranslations.Add(new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", Alignment.Translation.Translation.OriginatedFromValues.Assigned));
            Output.WriteLine($"Token for adding Translation: {sourceToken.TokenId}");

            if (iteration >= 5)
            {
                break;
            }
        }

        foreach (var exampleTranslation in exampleTranslations)
        {
            await translationSet.PutTranslation(
                exampleTranslation,
                TranslationActionTypes.PutNoPropagate);
        }

        return parallelCorpus.ParallelCorpusId;
    }

    private async Task<EngineParallelTextCorpus> BuildSampleManuscriptToZZSurEngineParallelTextCorpus()
    {
        var sourceCorpus = await Corpus.Create(Mediator!, true, "Manuscript", "LanguageX", "Standard", Guid.NewGuid().ToString());
        var sourceTokenizedTextCorpus = await TestDataHelpers.GetManuscript()
            .Create(Mediator!, sourceCorpus.CorpusId, "Source TC", "");

        var targetCorpus = await Corpus.Create(Mediator!, true, "zz_SUR", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
        var targetTokenizedTextCorpus = await TestDataHelpers.GetZZSurCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, "Target TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var engineParallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

        return engineParallelTextCorpus;
    }

    private async Task<EngineParallelTextCorpus> BuildSampleEngineParallelTextCorpus()
    {
        var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
        var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, sourceCorpus.CorpusId, "Source TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
        var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, "Target TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var engineParallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

        return engineParallelTextCorpus;
    }

    private async Task<EngineParallelTextCorpus> BuildSampleEngineParallelTextCorpusWithComposite()
    {
        var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
        var sourceTextCorpus = new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, TestDataHelpers.UsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoFakeCompositeTokensTextRowProcessor>();
        var sourceTokenizedTextCorpus = await sourceTextCorpus
            .Create(Mediator!, sourceCorpus.CorpusId, "Source TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoFakeCompositeTokensTextRowProcessor>()");

        var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible",Guid.NewGuid().ToString());
        var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, "Target TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

        return parallelTextCorpus;
    }

    private async Task<Dictionary<string, Dictionary<string, double>>> BuildSampleTranslationModel(EngineParallelTextCorpus parallelTextCorpus)
    {
        try
        {
            var translationCommandable = Container!.Resolve<TranslationCommands>();

            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                parallelTextCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);

            return smtWordAlignmentModel.GetTranslationTable();
        }
        catch (EngineException eex)
        {
            Output.WriteLine(eex.ToString());
            throw eex;
        }
    }

    private async Task<IEnumerable<AlignedTokenPairs>> BuildSampleAlignmentModel(EngineParallelTextCorpus parallelTextCorpus)
    {
        try
        {
            var translationCommandable = Container!.Resolve<TranslationCommands>();

            using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                SmtModelType.FastAlign,
                parallelTextCorpus,
                null,
                SymmetrizationHeuristic.GrowDiagFinalAnd);

            return translationCommandable.PredictAllAlignedTokenIdPairs(smtWordAlignmentModel, parallelTextCorpus).ToList();
        }
        catch (EngineException eex)
        {
            Output.WriteLine(eex.ToString());
            throw eex;
        }
    }

    private async Task RunAlignmentSetDenormalizationAsync(Guid alignmentSetId, CancellationToken cancellationToken = default)
    {
        var longRunningProgress = new TestLongRunningProgress(Output);
        await Mediator!.Send(new DenormalizeAlignmentTopTargetsCommand(alignmentSetId, longRunningProgress), cancellationToken);
    }

    private class IntoFakeCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("verse three"))
            {
                var tr = new TokensTextRow(textRow);

                var tokens = tr.Tokens;

                var tokenIds = tokens
                    .Select(t => t.TokenId)
                    .ToList();

                var compositeTokens = new List<Token>() { tokens[1], tokens[3], tokens[4] };
                var tokensWithComposite = new List<Token>()
                 {
                     tokens[0],
                     tokens[2],
                     new CompositeToken(compositeTokens),
                     tokens[5]
                 };

                tr.Tokens = tokensWithComposite;
                return tr;
            }

            return new TokensTextRow(textRow);
        }
    }

    private class TestLongRunningProgress : ILongRunningProgress<ProgressStatus>
    {
        protected ITestOutputHelper Output { get; private set; }
        public TestLongRunningProgress(ITestOutputHelper output)
        {
            Output = output;
        }

        public void Report(ProgressStatus value)
        {
            Output.WriteLine($"Long running progress message: {value.Message}");
        }

        public void ReportCancelRequestReceived(string? description = null)
        {
            Output.WriteLine($"Long running progress cancel request received: {description}");
        }

        public void ReportCompleted(string? description = null)
        {
            Output.WriteLine($"Long running progress completed: {description}");
        }

        public void ReportException(Exception exception)
        {
            Output.WriteLine($"Long running progress exception: {exception.Message}");
        }
    }
}