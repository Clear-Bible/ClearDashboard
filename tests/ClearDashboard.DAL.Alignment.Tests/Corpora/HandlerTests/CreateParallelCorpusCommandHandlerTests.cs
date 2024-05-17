using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Models = ClearDashboard.DataAccessLayer.Models;
using Verse = ClearBible.Engine.Corpora.Verse;
using VerseMapping = ClearBible.Engine.Corpora.VerseMapping;

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

            Assert.NotNull(returnedParallelCorpus.SourceTextIdToVerseMappings);
            var verseMappingsReturned = returnedParallelCorpus.SourceTextIdToVerseMappings!["JAS"];
            Assert.Single(verseMappingsReturned);

            Assert.Equal(1, verseMappingsReturned.First().SourceVerses.First().ChapterNum);
            Assert.Equal(1, verseMappingsReturned.First().SourceVerses.First().VerseNum);
            Assert.Equal(1, verseMappingsReturned.First().TargetVerses.First().ChapterNum);
            Assert.Equal(1, verseMappingsReturned.First().TargetVerses.First().VerseNum);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__QueryVerseMappings()
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

            var sourceTokensByGuidMat = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Where(t => t.BookNumber == 40)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));
            var targetTokensByGuidMat = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Where(t => t.BookNumber == 40)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));

            var sourceTokensByGuidMrk = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Where(t => t.BookNumber == 41)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));
            var targetTokensByGuidMrk = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Where(t => t.BookNumber == 41)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));

            Assert.True(sourceTokensByGuidMat.Keys.Count > 50);
            Assert.True(targetTokensByGuidMat.Keys.Count > 50);
            Assert.True(sourceTokensByGuidMrk.Keys.Count > 20);
            Assert.True(targetTokensByGuidMrk.Keys.Count > 20);

            int mat = 0;
            int mrk = 0;
            int verseMappingsSourceMatCount = 0;

            var verseMappingsForAllVerses = TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification).Select(vm =>
                {
                    IEnumerable<TokenId>? sourceTokenIds = null;
                    IEnumerable<TokenId>? targetTokenIds = null;

                    if (vm.SourceVerses.Any(v => v.BookNum.Equals(40)))
                    {
                        mat++;

                        if (mat == 1)
                        {
                            sourceTokenIds = sourceTokensByGuidMat.Values.Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMat.Values.Take(1).Select(t => t.TokenId);
                            verseMappingsSourceMatCount++;
                        }
                        else if (mat == 2)
                        {
                            sourceTokenIds = sourceTokensByGuidMrk.Values.Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMat.Values.Skip(1).Take(2).Select(t => t.TokenId);
                        }
                        else if (mat == 3)
                        {
                            sourceTokenIds = sourceTokensByGuidMat.Values.Skip(2).Take(1).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMrk.Values.Take(1).Select(t => t.TokenId);
                            verseMappingsSourceMatCount++;
                        }
                        else if (mat == 4)
                        {
                            sourceTokenIds = sourceTokensByGuidMrk.Values.Skip(2).Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMrk.Values.Skip(1).Take(1).Select(t => t.TokenId);
                        }
                        else
                        {
                            verseMappingsSourceMatCount++;
                        }
                    }
                    else
                    {
                        mrk++;

                        if (mrk == 1)
                        {
                            sourceTokenIds = sourceTokensByGuidMat.Values.Skip(10).Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMat.Values.Skip(10).Take(1).Select(t => t.TokenId);
                            verseMappingsSourceMatCount++;
                        }
                        else if (mrk == 2)
                        {
                            sourceTokenIds = sourceTokensByGuidMrk.Values.Skip(10).Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMat.Values.Skip(11).Take(2).Select(t => t.TokenId);
                        }
                        else if (mrk == 3)
                        {
                            sourceTokenIds = sourceTokensByGuidMat.Values.Skip(12).Take(1).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMrk.Values.Skip(10).Take(1).Select(t => t.TokenId);
                            verseMappingsSourceMatCount++;
                        }
                        else if (mrk == 4)
                        {
                            sourceTokenIds = sourceTokensByGuidMrk.Values.Skip(12).Take(2).Select(t => t.TokenId);
                            targetTokenIds = targetTokensByGuidMrk.Values.Skip(11).Take(1).Select(t => t.TokenId);
                        }
                    }

                    return new VerseMapping(
                        vm.SourceVerses.Select(v => new Verse(v.Book, v.ChapterNum, v.VerseNum, sourceTokenIds)).ToList(),
                        vm.TargetVerses.Select(v => new Verse(v.Book, v.ChapterNum, v.VerseNum, targetTokenIds)).ToList()
                    );
                });

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsForAllVerses));

            Assert.NotNull(parallelTextCorpus.SourceTextIdToVerseMappings);
            var verseMappingsReturned = parallelTextCorpus.SourceTextIdToVerseMappings.GetVerseMappings();
            Assert.NotNull(verseMappingsReturned);
            Assert.True(verseMappingsReturned.Count() >= 5);

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            var sourceTextId = "MAT";
            var sourceBookNumber = ModelHelper.GetBookNumberForSILAbbreviation(sourceTextId);

            var parallelCorpusGuid = parallelTokenizedCorpus.ParallelCorpusId.Id;
            var sourceTokenizedCorpusGuid = parallelTokenizedCorpus.ParallelCorpusId!.SourceTokenizedCorpusId!.Id;

            var verseMappings = ProjectDbContext!.VerseMappings
                .Include(verseMapping => verseMapping.Verses)
                    .ThenInclude(verse => verse.TokenVerseAssociations)
                        .ThenInclude(tva => tva.TokenComponent)
                .Where(verseMapping => verseMapping.ParallelCorpusId == parallelCorpusGuid)
                .Where(verseMapping =>
                    verseMapping.Verses
                        .Where(verse => !verse.TokenVerseAssociations.Any())
                        .Select(verse => verse.BookNumber)
                        .Any(bookNumber => bookNumber.Equals(sourceBookNumber))
                    ||
                    verseMapping.Verses
                        .Where(verse => verse.TokenVerseAssociations.Any())
                        .SelectMany(verse => verse.TokenVerseAssociations
                            .Select(tva => new { tva.TokenComponent!.TokenizedCorpusId, ((Models.Token)tva.TokenComponent!).BookNumber }))
                        .Any(tcb => 
                            tcb.TokenizedCorpusId == sourceTokenizedCorpusGuid && 
                            tcb.BookNumber.Equals(sourceBookNumber)))
                .ToList();

            Assert.Equal(verseMappingsSourceMatCount, verseMappings.Count);
            Assert.Equal("test pc", parallelTokenizedCorpus.ParallelCorpusId.DisplayName);

            parallelTokenizedCorpus.ParallelCorpusId.DisplayName = "some other name!";

            var newVerseMappings = new List<VerseMapping>()
            {
                new VerseMapping(
                    new List<Verse>() { new Verse("JAS", 1, 1) },
                    new List<Verse>() { new Verse("JAS", 1, 1) }
                )
            };

            parallelTokenizedCorpus.SourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromVerseMappings(newVerseMappings);
            await parallelTokenizedCorpus.Update(Mediator!);

            var updatedParallelCorpus = await ParallelCorpus.Get(Mediator!, parallelTokenizedCorpus.ParallelCorpusId);
            Assert.Equal("some other name!", updatedParallelCorpus.ParallelCorpusId.DisplayName);
            Assert.Single(updatedParallelCorpus.SourceTextIdToVerseMappings!.GetVerseMappings());
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

            var someSourceTokens = ProjectDbContext!.Tokens
                .Where(tc => tc.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Take(50)
                .ToList();

            var someTargetTokens = ProjectDbContext!.Tokens
                .Where(tc => tc.TokenizedCorpusId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Skip(50)
                .Take(50)
                .ToList();

            Output.WriteLine("Added tokens to verse mapping list");

            int vmi = 0;
            var verseMappingsForAllVerses =
                EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification).Take(5).Select(vm =>
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

            Output.WriteLine("Created engine parallel text corpus");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsForAllVerses));

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

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(
                targetTokenizedTextCorpus, 
                sourceTargetParallelVersesList: TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification, 
                    targetTokenizedTextCorpus.Versification));

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            Assert.NotNull(parallelTokenizedCorpus.SourceTextIdToVerseMappings);
            var verseMappingsReturned = parallelTokenizedCorpus.SourceTextIdToVerseMappings.GetVerseMappings();

            Assert.Equal(16, verseMappingsReturned.Count());
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

            // Take the verse mapping list and recreate it but with TokenIds added to each set of verses
            int vmi = 0;
            var verseMappingsForAllVerses =
                EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification).Take(5).Select(vm =>
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

            Assert.True(verseMappingsForAllVerses.Count >= 5);

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsForAllVerses));

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
                .SelectMany(v => v.TokenVerseAssociations.Where(tva => tva.TokenComponent!.TokenizedCorpusId == sourceTokenizedCorpus.Id)
                .Select(tva => tva.TokenComponentId)));
            var targetTokenGuidsFromDB = parallelCorpusFromDB.VerseMappings
                .SelectMany(vm => vm.Verses!
                .SelectMany(v => v.TokenVerseAssociations.Where(tva => tva.TokenComponent!.TokenizedCorpusId == targetTokenizedCorpus.Id)
                .Select(tva => tva.TokenComponentId)));

            Assert.True(sourceTokenGuidsFromDB.Count() > 0);
            Assert.True(targetTokenGuidsFromDB.Count() > 0);
            Assert.Empty(sourceTokenGuidsFromDB.Intersect(targetTokenGuidsFromDB));
            Assert.Empty(sourceTokenGuidsFromDB.Except(sourceTokenGuidIds.Keys));
            Assert.Empty(targetTokenGuidsFromDB.Except(targetTokenGuidIds.Keys));

            Assert.NotNull(parallelTokenizedCorpus.SourceTextIdToVerseMappings);
            var verseMappingsReturned = parallelTokenizedCorpus.SourceTextIdToVerseMappings.GetVerseMappings();

            Assert.Equal(5, verseMappingsReturned.Count());
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

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__CreateCompositeTokensApi()
    {
        // The primary testing of the PutCompositeToken API is in 
        // CreateTokenizedCorpusFromTextCorpusHandlerTests.TokenizedCorpus__CreateCompositeTokensApi.
        // This test adds in non-null ParallelCorpusId testing, most of which involves testing
        // the validation rules around ALL composite token children being in every VerseMapping
        // that ANY of the child tokens match (because of either a direct Verse+TokenVerseAssociation
        // relationship or an implied Verse book/chapter/verse relationship).
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "Sample Latin", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var sourceTokensByGuid = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));
            var targetTokensByGuid = ProjectDbContext!.Tokens
                .Where(t => t.TokenizedCorpusId == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .ToDictionary(t => t.Id, t => ModelHelper.BuildToken(t));

            Assert.True(sourceTokensByGuid.Keys.Count > 50);
            Assert.True(targetTokensByGuid.Keys.Count > 50);
            Assert.Empty(sourceTokensByGuid.Keys.Intersect(targetTokensByGuid.Keys));

            var verseMappingsForAllVerses = TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)
                .ToList();

            Assert.True(verseMappingsForAllVerses.Count >= 5);

            var sourceVerses = verseMappingsForAllVerses.First().SourceVerses.ToList();
            var targetVerses = verseMappingsForAllVerses.First().TargetVerses.ToList();
            sourceVerses[0] = new Verse(
                sourceVerses[0].Book,
                sourceVerses[0].ChapterNum,
                sourceVerses[0].VerseNum,
                sourceTokensByGuid.Values.Take(2).Select(t => t.TokenId));
            verseMappingsForAllVerses[0] = new VerseMapping(sourceVerses, targetVerses);

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus,
                sourceTargetParallelVersesList: new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsForAllVerses));

            var parallelTokenizedCorpus = await parallelTextCorpus.Create("test pc", Mediator!);

            // Two tokens in TokenVerseAssociations, and two additional ones:
            var composite1 = new CompositeToken(sourceTokensByGuid.Values.Take(4))
            {
                TokenId =
                {
                    Id = Guid.NewGuid()
                }
            };
            composite1.Metadata["IsParallelCorpusToken"] = true;

            var sw = new Stopwatch();
            sw.Start();

            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite1, parallelTokenizedCorpus.ParallelCorpusId);

            sw.Stop();
            Output.WriteLine($"Elapsed={sw.Elapsed} - ParallelCorpus PutCompositeToken (good)");

            ProjectDbContext!.ChangeTracker.Clear();

            var tokenComposite1 = ProjectDbContext.TokenComposites.Include(tc => tc.Tokens)
                .Where(tc => tc.Id == composite1.TokenId.Id)
                .FirstOrDefault();

            Assert.NotNull(tokenComposite1);
            Assert.Equal(composite1.Tokens.Count(), tokenComposite1.Tokens.Count);
            Assert.Empty(composite1.Tokens.Select(t => t.TokenId.Id).Except(tokenComposite1.Tokens.Select(tc => tc.Id)));

            // Can't mix tokens from multiple tokenized corpora
            var composite2 = new CompositeToken(sourceTokensByGuid.Values.Skip(1).Take(3).Union(targetTokensByGuid.Values.Take(2)));
            composite2.TokenId.Id = Guid.NewGuid();

            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => TokenizedTextCorpus.PutCompositeToken(Mediator!, composite2, parallelTokenizedCorpus.ParallelCorpusId));

            // Can use group of tokens of which only some are in a given VerseMapping:
            var otherVerse = verseMappingsForAllVerses[7].SourceVerses.First();
            var otherVerseBcv = $"{otherVerse.BookNum:000}{otherVerse.ChapterNum:000}{otherVerse.VerseNum:000}";
            var otherVerseMappingTokens = ProjectDbContext.Tokens.Include(t => t.VerseRow)
                .Where(t => t.TokenizedCorpusId == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id)
                .Where(t => t.VerseRow!.BookChapterVerse == otherVerseBcv)
                .Take(3)
                .Select(t => ModelHelper.BuildToken(t));

            var composite3TestTokens = sourceTokensByGuid.Values.Skip(1).Take(3).ToList();
            composite3TestTokens.AddRange(otherVerseMappingTokens);
            var composite3 = new CompositeToken(composite3TestTokens);
            composite3.TokenId.Id = Guid.NewGuid();

            sw.Restart();

            // Tokens already in another composite, so error:
            await Assert.ThrowsAsync<MediatorErrorEngineException>(() => TokenizedTextCorpus.PutCompositeToken(Mediator!, composite3, parallelTokenizedCorpus.ParallelCorpusId));

            // Ok, since different parallel corpus id:
            var parallelTokenizedCorpus2 = await parallelTextCorpus.Create("test pc", Mediator!);
            await TokenizedTextCorpus.PutCompositeToken(Mediator!, composite3, parallelTokenizedCorpus2.ParallelCorpusId);

            sw.Stop();
            Output.WriteLine($"Elapsed={sw.Elapsed} - ParallelCorpus PutCompositeToken with multiple VerseMapping candidates (ok)");
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}