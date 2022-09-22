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

            var translationModel1 = await BuildSampleTranslationModel(parallelTextCorpus1);

            var translationSet1 = await translationModel1.Create("display name 1", "smt model 1", new(), parallelCorpus1.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet1);

            var parallelTextCorpus2 = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus2 = await parallelTextCorpus2.Create("pc2", Mediator!);

            var translationModel2 = await BuildSampleTranslationModel(parallelTextCorpus2);

            var translationSet2 = await translationModel2.Create("display name 2", "smt model 2", new(), parallelCorpus2.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet2);

            ProjectDbContext!.ChangeTracker.Clear();
            var user = ProjectDbContext!.Users.First();

            var allTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!);
            Assert.Equal(2, allTranslationSetIds.Count());

            var someTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!, parallelCorpus2.ParallelCorpusId);
            Assert.Single(someTranslationSetIds);

            Assert.Equal(translationSet2.TranslationSetId, someTranslationSetIds.First().translationSetId);
            Assert.Equal(parallelCorpus2.ParallelCorpusId, someTranslationSetIds.First().parallelCorpusId);

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
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__Create()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create("display name", "smt model", new(), parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            ProjectDbContext!.ChangeTracker.Clear();

            var translationSetDb = ProjectDbContext.TranslationSets
                .Include(ts => ts.TranslationModel)
                .Include(ts => ts.Translations)
                .FirstOrDefault(ts => ts.Id == translationSet.TranslationSetId.Id);
            Assert.NotNull(translationSetDb);

            Assert.Equal(translationSet.ParallelCorpusId.Id, translationSetDb!.ParallelCorpusId);
            Assert.Empty(translationSetDb!.Translations);
            Assert.True(translationModel.Keys.Count > 3);
            var tm = await translationSet.GetTranslationModelEntryForToken(new Token(new TokenId(1, 1, 1, 1, 1), "surface", translationModel.Keys.Skip(3).First()));
            Assert.NotNull(tm);

            Output.WriteLine($"Translation model entry for {translationModel.Keys.Skip(3).First()}");
            foreach (var kvp in tm)
            {
                Output.WriteLine($"\tTarget text: {kvp.Key} / score: {kvp.Value}");
            }
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

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create("display name", "smt model", new() { { "boo", "baa"} }, parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            var bookId = parallelCorpus.SourceCorpus.Texts.Select(t => t.Id).ToList().First();
            var sourceTokens = parallelCorpus.SourceCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().First().Tokens;

            var iteration = 0;
            var exampleTranslations = new List<Alignment.Translation.Translation>();
            foreach (var sourceToken in sourceTokens)
            {
                iteration++;

                exampleTranslations.Add(new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", "Assigned"));
                Output.WriteLine($"Token for adding Translation: {sourceToken.TokenId}");

                if (iteration >= 5)
                {
                    break;
                }
            }

            foreach (var exampleTranslation in exampleTranslations)
            {
                translationSet.PutTranslation(
                    exampleTranslation,
                    TranslationActionType.PutNoPropagate.ToString());
            }

            ProjectDbContext!.ChangeTracker.Clear();

            var translationSetDb = ProjectDbContext.TranslationSets
                .Include(ts => ts.Translations)
                .FirstOrDefault(ts => ts.Id == translationSet.TranslationSetId.Id);
            Assert.NotNull(translationSetDb);
            Assert.Equal(exampleTranslations.Count, translationSetDb!.Translations.Count);

            var tokensInDb = translationSetDb!.Translations.Select(t => t.SourceTokenComponent!.EngineTokenId!);

            foreach (var exampleTranslation in exampleTranslations)
            {
                Assert.Contains<string>(exampleTranslation.SourceToken.TokenId.ToString(), tokensInDb);
            }

            // FIXME:  quick test of PutPropagate.  Need better tests of this
            translationSet.PutTranslation(
                    new Alignment.Translation.Translation(exampleTranslations[1].SourceToken!, $"toobedoo", "Assigned"),
                    TranslationActionType.PutNoPropagate.ToString());

            var to = new Token(
                new TokenId(
                    exampleTranslations[1].SourceToken!.TokenId.BookNumber,
                    exampleTranslations[1].SourceToken!.TokenId.ChapterNumber,
                    exampleTranslations[1].SourceToken!.TokenId.VerseNumber,
                    5,
                    exampleTranslations[1].SourceToken!.TokenId.SubWordNumber),
                exampleTranslations[1].SourceToken!.SurfaceText,
                exampleTranslations[1].SourceToken!.TrainingText);

            translationSet.PutTranslation(
                    new Alignment.Translation.Translation(to, $"shoobedoo", "Assigned"),
                    TranslationActionType.PutPropagate.ToString());

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
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
        }
        finally
        {
//            await DeleteDatabaseContext();
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
                    new Dictionary<string, object>(),
                    parallelCorpus.ParallelCorpusId,
                    Mediator!);
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

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create("display name", "smt model", new(), parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            var initialFilteredEngineParallelTextRows = parallelCorpus.Take(5).Cast<EngineParallelTextRow>();
            var initialTranslations = await translationSet.GetTranslations(initialFilteredEngineParallelTextRows);

            Output.WriteLine($"Translations from model count: {initialTranslations.Count()} (from first five engine parallel text rows)");
            //foreach (var translation in initialTranslations)
            //{
            //    Output.WriteLine($"\tTokenId: {translation.SourceToken.TokenId}, TrainingText: {translation.SourceToken.TrainingText}, TargetTranslationText: {translation.TargetTranslationText}, TranslationState: {translation.TranslationOriginatedFrom}");
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
                            translationSet.PutTranslation(
                                new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", "Assigned"),
                                TranslationActionType.PutNoPropagate.ToString());

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
                if (translation.TranslationOriginatedFrom != "FromTranslationModel")
                {
                    Assert.InRange<TokenId>(translation.SourceToken.TokenId, new TokenId("040001003001001"), new TokenId("040001005006001"), Comparer<TokenId>.Create((t1, t2) => t1.CompareTo(t2)));
                }
                Output.WriteLine($"TokenId: {translation.SourceToken.TokenId}, TrainingText: {translation.SourceToken.TrainingText}, TargetTranslationText: {translation.TargetTranslationText}, TranslationState: {translation.TranslationOriginatedFrom}");
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
    public async Task TranslationSet__InvalidParallelCorpusId()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            // Should throw an exception because of the bogus ParallelCorpusId:
            await Assert.ThrowsAnyAsync<Exception>(() => translationModel.Create(null, string.Empty, new(), new ParallelCorpusId(new Guid()), Mediator!));
       }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private async Task<EngineParallelTextCorpus> BuildSampleManuscriptToZZSurEngineParallelTextCorpus()
    {
        var sourceCorpus = await Corpus.Create(Mediator!, true, "Manuscript", "LanguageX", "Standard", Guid.NewGuid().ToString());
        var sourceTokenizedTextCorpus = await TestDataHelpers.GetManuscript()
            .Create(Mediator!, sourceCorpus.CorpusId, "Source TC", "");

        var targetCorpus = await Corpus.Create(Mediator!, true, "zz_SUR", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
        var targetTokenizedTextCorpus = await TestDataHelpers.GetZZSurCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, "Target TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var engineParallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

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

        var engineParallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

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

        var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

        return parallelTextCorpus;
    }

    private async Task<Dictionary<string, Dictionary<string, double>>> BuildSampleTranslationModel(EngineParallelTextCorpus parallelTextCorpus)
    {
        try
        {
            var translationCommandable = new TranslationCommands();

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
            var translationCommandable = new TranslationCommands();

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
}