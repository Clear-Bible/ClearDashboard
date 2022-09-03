using System;
using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Translation;
using Xunit;
using Xunit.Abstractions;
using static ClearDashboard.DAL.Alignment.Translation.ITranslationCommandable;
using System.Threading.Tasks;

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
            var parallelCorpus1 = await parallelTextCorpus1.Create(Mediator!);

            var translationModel1 = await BuildSampleTranslationModel(parallelTextCorpus1);

            var translationSet1 = await translationModel1.Create(parallelCorpus1.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet1);

            var parallelTextCorpus2 = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus2 = await parallelTextCorpus2.Create(Mediator!);

            var translationModel2 = await BuildSampleTranslationModel(parallelTextCorpus2);

            var translationSet2 = await translationModel2.Create(parallelCorpus2.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet2);

            ProjectDbContext!.ChangeTracker.Clear();
            var user = ProjectDbContext!.Users.First();

            var allTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!);
            Assert.Equal(2, allTranslationSetIds.Count());

            var someTranslationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!, parallelCorpus2.ParallelCorpusId);
            Assert.Single(someTranslationSetIds);
            Assert.Equal(translationSet2.TranslationSetId, someTranslationSetIds.First().translationSetId);
            Assert.Equal(parallelCorpus2.ParallelCorpusId, someTranslationSetIds.First().parallelCorpusId);

            var allTranslationSetIdsForUser = await TranslationSet.GetAllTranslationSetIds(Mediator!, null, new UserId(user.Id));
            Assert.Equal(2, allTranslationSetIdsForUser.Count());

            var allTranslationSetIdsForBogusUser = await TranslationSet.GetAllTranslationSetIds(Mediator!, null, new UserId(new Guid()));
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
            var parallelCorpus = await parallelTextCorpus.Create(Mediator!);

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create(parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            ProjectDbContext!.ChangeTracker.Clear();

            var translationSetDb = ProjectDbContext.TranslationSets
                .Include(ts => ts.TranslationModel)
                .Include(ts => ts.Translations)
                .FirstOrDefault(ts => ts.Id == translationSet.TranslationSetId.Id);
            Assert.NotNull(translationSetDb);

            Assert.Equal(translationSet.ParallelCorpusId.Id, translationSetDb!.ParallelCorpusId);
            Assert.Equal(translationSet.GetTranslationModel().Count, translationSetDb!.TranslationModel.Count);
            Assert.Empty(translationSetDb!.Translations);
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
            var parallelCorpus = await parallelTextCorpus.Create(Mediator!);

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create(parallelCorpus.ParallelCorpusId, Mediator!);
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

            var tokensInDb = translationSetDb!.Translations.Select(t => new TokenId(
                t.SourceToken!.BookNumber,
                t.SourceToken!.ChapterNumber,
                t.SourceToken!.VerseNumber,
                t.SourceToken!.WordNumber,
                t.SourceToken!.SubwordNumber));

            foreach (var exampleTranslation in exampleTranslations)
            {
                Assert.Contains<TokenId>(exampleTranslation.SourceToken.TokenId, tokensInDb);
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

    [Fact]
    [Trait("Category", "Handlers")]
    public async Task TranslationSet__GetTranslationRange()
    {
        try
        {
            var parallelTextCorpus = await BuildSampleEngineParallelTextCorpus();
            var parallelCorpus = await parallelTextCorpus.Create(Mediator!);

            var translationModel = await BuildSampleTranslationModel(parallelTextCorpus);

            var translationSet = await translationModel.Create(parallelCorpus.ParallelCorpusId, Mediator!);
            Assert.NotNull(translationSet);

            Output.WriteLine("");

            var iteration = 1;
            foreach (var bookId in parallelCorpus.SourceCorpus.Texts.Select(t => t.Id))
            {
//                Output.WriteLine($"Book: {bookId}");
                var row = 1;
                foreach (var ttr in parallelCorpus.SourceCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>())
                {
//                    Output.WriteLine($"\tVerse (row): {row++}");
                    foreach (var sourceToken in ttr.Tokens)
                    {
//                        Output.WriteLine($"\t\tTokenId: {sourceToken.TokenId}");
                        translationSet.PutTranslation(
                            new Alignment.Translation.Translation(sourceToken, $"booboo_{iteration}", "Assigned"),
                            TranslationActionType.PutPropagate.ToString());

                        iteration++;
                    }
                }
            }

            ProjectDbContext!.ChangeTracker.Clear();
            // First:  040002006004001
            // Last:   041001001005001
            // Should yield about 15 translations...

            var firstTokenId = new TokenId(40, 2, 6, 4, 1);
            var lastTokenId = new TokenId(41, 1, 1, 5, 1);
            var translations = await translationSet.GetTranslations(firstTokenId, lastTokenId);
            Assert.Equal(15, translations.Count());

            Output.WriteLine($"Translation count: {translations.Count()}");
            Output.WriteLine("");
            foreach (var translation in translations)
            {
                Assert.InRange<TokenId>(translation.SourceToken.TokenId, firstTokenId, lastTokenId, Comparer<TokenId>.Create((t1, t2) => t1.CompareTo(t2)));
                Output.WriteLine($"TokenId: {translation.SourceToken.TokenId}, TrainingText: {translation.SourceToken.TrainingText}, TargetTranslationText: {translation.TargetTranslationText}, TranslationState: {translation.TranslationState}");
            }
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
            await Assert.ThrowsAnyAsync<Exception>(() => translationModel.Create(new ParallelCorpusId(new Guid()), Mediator!));
       }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private async Task<EngineParallelTextCorpus> BuildSampleEngineParallelTextCorpus()
    {
        var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard");
        var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, sourceCorpus.CorpusId, "Source TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible");
        var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, "Target TC", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

        return parallelTextCorpus;
    }

    private async Task<Dictionary<string, Dictionary<string, double>>> BuildSampleTranslationModel(EngineParallelTextCorpus parallelTextCorpus)
    {
        try
        {
            FunctionWordTextRowProcessor.Train(parallelTextCorpus);

            parallelTextCorpus.SourceCorpus = parallelTextCorpus.SourceCorpus
                .Filter<FunctionWordTextRowProcessor>();

            var translationCommandable = new TranslationCommands(Mediator!);

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
}