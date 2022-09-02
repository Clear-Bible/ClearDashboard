using System;
using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Aligner.Persistence;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Translation;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using Xunit;
using Xunit.Abstractions;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;
using Verse = ClearBible.Engine.Corpora.Verse;
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
            Assert.Equal(translationSet.TranslationModel.Count, translationSetDb!.TranslationModel.Count);
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