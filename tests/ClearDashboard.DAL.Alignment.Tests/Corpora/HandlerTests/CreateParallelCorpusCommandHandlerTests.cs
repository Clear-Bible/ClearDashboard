using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;
using Verse = ClearBible.Engine.Corpora.Verse;
using SIL.Extensions;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateParallelCorpusCommandHandlerTests : TestBase
{
    public CreateParallelCorpusCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__Create()
    {
        try
        {
            var sourceTextCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceCommand = new CreateTokenizedCorpusFromTextCorpusCommand(sourceTextCorpus, sourceCorpus.CorpusId,
                "Greek Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", ScrVers.Original);

            var sourceCommandResult = await Mediator.Send(sourceCommand);

            var targetTextCorpus = TestDataHelpers.GetSampleGreekCorpus();

            var targetCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 2",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var targetCommand = new CreateTokenizedCorpusFromTextCorpusCommand(targetTextCorpus, targetCorpus.CorpusId,
                "Greek Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()", ScrVers.Original);

            var targetCommandResult = await Mediator.Send(targetCommand);

            var sourceTokenizedCorpusId =
                ProjectDbContext!.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 1").Id;
            var targetTokenizedCorpusId =
                ProjectDbContext!.TokenizedCorpora.First(tc => tc.Corpus.Name == "New Testament 2").Id;

            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("JAS", 1, 1) },
                    new List<Verse>() { new Verse("JAS", 1, 1) }
                )
            };

            var command =
                new CreateParallelCorpusCommand(new TokenizedTextCorpusId(sourceTokenizedCorpusId),
                    new TokenizedTextCorpusId(targetTokenizedCorpusId), verseMappings, "awesome parallel corpus");
            var createResult = await Mediator.Send(command);

            ProjectDbContext.ChangeTracker.Clear();

            // General assertions
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);
            Assert.Equal("Success", createResult.Message);
            Assert.NotNull(createResult.Data);

            var returnedParallelCorpus = await ParallelCorpus.Get(Mediator, createResult.Data);
            Assert.NotNull(returnedParallelCorpus);

            // Validate persisted ParallelCorpus data

            Assert.Equal(1, ProjectDbContext.ParallelCorpa.Count());
            var theParallelCorporaEntry = ProjectDbContext.ParallelCorpa
                .Include(pc => pc.VerseMappings)
                .Include(pc => pc.SourceTokenizedCorpus)
                .ThenInclude(stc => stc.Corpus)
                .Include(pc => pc.TargetTokenizedCorpus)
                .ThenInclude(ttc => ttc.Corpus)
                .First();
            Assert.Equal(1, theParallelCorporaEntry.VerseMappings.Count);

            Assert.Equal(sourceTokenizedCorpusId, theParallelCorporaEntry.SourceTokenizedCorpusId);
            Assert.Equal(targetTokenizedCorpusId, theParallelCorporaEntry.TargetTokenizedCorpusId);

            Assert.Equal("New Testament 1", theParallelCorporaEntry.SourceTokenizedCorpus?.Corpus?.Name);
            Assert.Equal("New Testament 2", theParallelCorporaEntry.TargetTokenizedCorpus?.Corpus?.Name);

            // Validate returned ParallelCorpus Data
            Assert.NotNull(returnedParallelCorpus);
            Assert.NotNull(returnedParallelCorpus!.SourceCorpus);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                returnedParallelCorpus.SourceCorpus.Texts.First().GetRows().First().Text);
            Assert.NotNull(returnedParallelCorpus.TargetCorpus);
            Assert.Equal("Βίβλος γενέσεως Ἰησοῦ Χριστοῦ υἱοῦ Δαυεὶδ υἱοῦ Ἀβραάμ .",
                returnedParallelCorpus.TargetCorpus.Texts.First().GetRows().First().Text);
            Assert.Single(returnedParallelCorpus.VerseMappingList);

            Assert.Equal(1, returnedParallelCorpus!.VerseMappingList!.First().SourceVerses.First().ChapterNum);
            Assert.Equal(1, returnedParallelCorpus!.VerseMappingList!.First().SourceVerses.First().VerseNum);
            Assert.Equal(1, returnedParallelCorpus!.VerseMappingList!.First().TargetVerses.First().ChapterNum);
            Assert.Equal(1, returnedParallelCorpus!.VerseMappingList!.First().TargetVerses.First().VerseNum);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

//    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__CreateLarge_MeasurePerformance()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 1",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Create(Mediator!, sourceCorpus.CorpusId, "Big Source", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            Output.WriteLine("Created source tokenized corpus");

            var targetCorpus = await Corpus.Create(Mediator!, false,
                "New Testament 2",
                "grc",
                "Resource", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Create(Mediator!, targetCorpus.CorpusId, "Big Target", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            Output.WriteLine("Created target tokenized corpus");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            Output.WriteLine("Created engine parallel text corpus");

            var someSourceTokens = ProjectDbContext!.Tokens
                .Where(tc => tc.TokenizationId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(50)
                .ToList();

            var someTargetTokens = ProjectDbContext!.Tokens
                .Where(tc => tc.TokenizationId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Skip(50)
                .Take(50)
                .ToList();

            int vmi = 0;
            parallelTextCorpus.VerseMappingList =
                parallelTextCorpus.VerseMappingList!.Take(5).Select(vm =>
                {
                    vmi++;
                    int svi = 0, tvi = 0;
                    return new VerseMapping(
                        vm.SourceVerses.Select(v =>
                        {
                            svi++;
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, someSourceTokens.Skip(vmi * svi * 3).Take(vmi)
                                .Select(t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber)
                                    {
                                        Id = t.Id
                                    }
                                ));
                        }).ToList(),
                        vm.TargetVerses.Select(v =>
                        {
                            tvi++;
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, someTargetTokens.Skip(vmi * tvi * 4).Take(vmi)
                                .Select(t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber)
                                {
                                    Id = t.Id
                                }
                                ));
                        }).ToList()
                    ); ;
                }).ToList();

            Output.WriteLine("Added tokens to verse mapping list");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Output.WriteLine($"Private memory usage (BEFORE): {proc.PrivateMemorySize64}");

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            proc.Refresh();
            Output.WriteLine($"Private memory usage (AFTER):  {proc.PrivateMemorySize64}");

            sw.Stop();
            Output.WriteLine("Elapsed={0}", sw.Elapsed);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__CreateUsingHandler()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            Assert.Equal(15, parallelTokenizedCorpus.VerseMappingList?.Count() ?? 0);
            Assert.True(parallelTokenizedCorpus.SourceCorpus.Count() == 15);
            Assert.True(parallelTokenizedCorpus.TargetCorpus.Count() == 15);

        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__CreateUsingHandlerWithTokens()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var sourceTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.TokenComponents)
                .First(tc => tc.Id == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id);
            var targetTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.TokenComponents)
                .First(tc => tc.Id == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id);

            var sourceTokenGuidIds = sourceTokenizedCorpus.Tokens
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber) { Id = t.Id });
            var targetTokenGuidIds = targetTokenizedCorpus.Tokens
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber) { Id = t.Id });

            Assert.True(sourceTokenGuidIds.Keys.Count > 50);
            Assert.True(targetTokenGuidIds.Keys.Count > 50);
            Assert.Empty(sourceTokenGuidIds.Keys.Intersect(targetTokenGuidIds.Keys));

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, new());

            Assert.NotNull(parallelTextCorpus.VerseMappingList);
            Assert.True(parallelTextCorpus.VerseMappingList!.Count >= 5);

            // Take the verse mapping list and recreate it but with TokenIds added to each set of verses
            int vmi = 0;
            parallelTextCorpus.VerseMappingList =
                parallelTextCorpus.VerseMappingList!.Take(5).Select(vm =>
                    {
                        vmi++;
                        int svi = 0, tvi = 0;
                        return new VerseMapping(
                            vm.SourceVerses.Select(v =>
                            {
                                svi++;
                                return new Verse(v.Book, v.ChapterNum, v.VerseNum, sourceTokenGuidIds.Values.Skip(vmi * svi * 3).Take(vmi));
                            }).ToList(),
                            vm.TargetVerses.Select(v =>
                            {
                                tvi++;
                                return new Verse(v.Book, v.ChapterNum, v.VerseNum, targetTokenGuidIds.Values.Skip(vmi * tvi * 4).Take(vmi));
                            }).ToList()
                        ); ;
                    }
                ).ToList();

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            ProjectDbContext.ChangeTracker.Clear();

#nullable disable
            var parallelCorpusFromDB = 
                ProjectDbContext!.ParallelCorpa
                    .Include(pc => pc.SourceTokenizedCorpus)
                    .Include(pc => pc.TargetTokenizedCorpus)
                    .Include(pc => pc.VerseMappings)
                    .ThenInclude(vm => vm.Verses)
                    .ThenInclude(v => v.TokenVerseAssociations)
                    .ThenInclude(tva => tva.TokenComponent)
                    .FirstOrDefault(pc => pc.Id == parallelTokenizedCorpus.ParallelCorpusId.Id);
#nullable restore

            Assert.NotNull(parallelCorpusFromDB);
            Assert.Equal(parallelCorpusFromDB!.SourceTokenizedCorpusId, sourceTokenizedCorpus.Id);
            Assert.Equal(parallelCorpusFromDB!.TargetTokenizedCorpusId, targetTokenizedCorpus.Id);
            Assert.Equal(5, parallelCorpusFromDB!.VerseMappings.Count);

            var sourceTokenGuidsFromDB = parallelCorpusFromDB.VerseMappings
                .SelectMany(vm => vm.Verses!
                .SelectMany(v => v.TokenVerseAssociations.Where(tva => tva.TokenComponent!.TokenizationId == sourceTokenizedCorpus.Id)
                .Select(tva => tva.TokenComponentId)));
            var targetTokenGuidsFromDB = parallelCorpusFromDB.VerseMappings
                .SelectMany(vm => vm.Verses!
                .SelectMany(v => v.TokenVerseAssociations.Where(tva => tva.TokenComponent!.TokenizationId == targetTokenizedCorpus.Id)
                .Select(tva => tva.TokenComponentId)));

            Assert.True(sourceTokenGuidsFromDB.Count() > 0);
            Assert.True(targetTokenGuidsFromDB.Count() > 0);
            Assert.Empty(sourceTokenGuidsFromDB.Intersect(targetTokenGuidsFromDB));
            Assert.Empty(sourceTokenGuidsFromDB.Except(sourceTokenGuidIds.Keys));
            Assert.Empty(targetTokenGuidsFromDB.Except(targetTokenGuidIds.Keys));

            Assert.Equal(5, parallelTokenizedCorpus.VerseMappingList?.Count() ?? 0);
            Assert.True(parallelTokenizedCorpus.SourceCorpus.Count() == 15);
            Assert.True(parallelTokenizedCorpus.TargetCorpus.Count() == 15);

        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__InvalidTokenizedCorpusId()
    {
        try
        {
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var tokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var validTokenizedCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId;
            var bogusTokenizedCorpusId = new TokenizedTextCorpusId(new Guid());

            var verseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("JAS", 1, 1) },
                    new List<Verse>() { new Verse("JAS", 1, 1) }
                )
            };

            // Bogus source id:
            var command = new CreateParallelCorpusCommand(
                bogusTokenizedCorpusId,
                validTokenizedCorpusId,
                verseMappings,
                "invalid source pc");
            var result = await Mediator!.Send(command);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("sourcetokenizedcorpus not found", result.Message.ToLower());
            Output.WriteLine(result.Message);

            // Bogus target id:
            command = new CreateParallelCorpusCommand(
                validTokenizedCorpusId,
                bogusTokenizedCorpusId,
                verseMappings,
                "invalid target pc");
            result = await Mediator!.Send(command);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("targettokenizedcorpus not found", result.Message.ToLower());
            Output.WriteLine(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

}