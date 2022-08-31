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

                if (iteration > 5)
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
            Assert.Equal(5, translationSetDb!.Translations.Count);

            var tokenDb = translationSetDb!.Translations.First().Token;
            var tokenIdDb = new TokenId(
                tokenDb!.BookNumber, 
                tokenDb!.ChapterNumber, 
                tokenDb!.VerseNumber, 
                tokenDb!.WordNumber, 
                tokenDb!.SubwordNumber);
            Assert.Equal(exampleTranslations[0].SourceToken.TokenId, tokenIdDb);
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
            .Create(Mediator!, sourceCorpus.CorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

        var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible");
        var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
            .Create(Mediator!, targetCorpus.CorpusId, ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

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
                new DelegateProgress(status =>
                    Output.WriteLine($"Training symmetrized Fastalign model: {status.PercentCompleted:P}")),
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