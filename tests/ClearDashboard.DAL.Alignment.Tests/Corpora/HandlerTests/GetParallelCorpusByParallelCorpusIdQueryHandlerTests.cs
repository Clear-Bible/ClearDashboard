using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.Data.Sqlite;
using SIL.Scripture;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class GetParallelCorpusByParallelCorpusIdQueryHandlerTests : TestBase
{
    public GetParallelCorpusByParallelCorpusIdQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__GetHandlerWithTokens()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, 
                new SourceTextIdToVerseMappingsFromVerseMappings(TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

            var sourceTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.TokenComponents)
                .First(tc => tc.Id == sourceTokenizedTextCorpus.TokenizedTextCorpusId.Id);
            var targetTokenizedCorpus = ProjectDbContext!.TokenizedCorpora
                .Include(tc => tc.TokenComponents)
                .First(tc => tc.Id == targetTokenizedTextCorpus.TokenizedTextCorpusId.Id);

            var sourceTokenGuidIds = sourceTokenizedCorpus.Tokens.Take(10)
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber) { Id = t.Id });
            var sourceTokenIdValuesSortedDescending = sourceTokenGuidIds.Values
                .OrderByDescending(t => t.BookNumber)
                .ThenByDescending(t => t.ChapterNumber)
                .ThenByDescending(t => t.VerseNumber);
            var targetTokenGuidIds = targetTokenizedCorpus.Tokens.Skip(3).Take(10)
                .ToDictionary(t => t.Id, t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber) { Id = t.Id });
            var targetTokenIdValuesSortedAscending = targetTokenGuidIds.Values
                .OrderBy(t => t.VerseNumber)
                .ThenBy(t => t.ChapterNumber)
                .ThenBy(t => t.VerseNumber);

            Assert.True(sourceTokenGuidIds.Keys.Count == 10);
            Assert.True(targetTokenGuidIds.Keys.Count == 10);
            Assert.Empty(sourceTokenGuidIds.Keys.Intersect(targetTokenGuidIds.Keys));

            Assert.NotNull(parallelTextCorpus.SourceTextIdToVerseMappings);

            var verseMappingList = parallelTextCorpus.SourceTextIdToVerseMappings.GetVerseMappings().ToList();
            Assert.True(verseMappingList.Count >= 5);

            // Take the verse mapping list and recreate it but with TokenIds added to each set of verses
            verseMappingList =
                verseMappingList.Take(5).Select(vm =>
                {
                    return new VerseMapping(
                        vm.SourceVerses.Select(v =>
                        {
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, sourceTokenIdValuesSortedDescending);
                        }).ToList(),
                        vm.TargetVerses.Select(v =>
                        {
                            return new Verse(v.Book, v.ChapterNum, v.VerseNum, targetTokenIdValuesSortedAscending);
                        }).ToList()
                    ); ;
                }
                ).ToList();

            parallelTextCorpus.SourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingList);

            // Save:
            var parallelTokenizedCorpus = await parallelTextCorpus.CreateAsync("test pc", Container!);

            var sourceTokensForComposite = ProjectDbContext.Tokens
                .Where(t => t.TokenizedCorpusId == sourceTokenizedCorpus.Id)
                .Take(10)
                .ToList()
                .GroupBy(t => t.VerseRowId)
                .First();
            var targetTokensForComposite = ProjectDbContext.Tokens
                .Where(t => t.TokenizedCorpusId == targetTokenizedCorpus.Id)
                .Take(10)
                .ToList()
                .GroupBy(t => t.VerseRowId)
                .First();

            var sourceTokenComposite = new Models.TokenComposite()
            {
                Id = Guid.NewGuid(),
                EngineTokenId = "0101010101-999",
                VerseRowId = sourceTokensForComposite.Key,
                TokenizedCorpusId = sourceTokenizedCorpus.Id,
                ParallelCorpusId = parallelTokenizedCorpus.ParallelCorpusId.Id
            };
            var targetTokenComposite = new Models.TokenComposite()
            {
                Id = Guid.NewGuid(),
                EngineTokenId = "0101010101-9999",
                VerseRowId = targetTokensForComposite.Key,
                TokenizedCorpusId = targetTokenizedCorpus.Id,
                ParallelCorpusId = parallelTokenizedCorpus.ParallelCorpusId.Id
            };
            ProjectDbContext.TokenComponents.Add(targetTokenComposite);
            ProjectDbContext.TokenComponents.Add(sourceTokenComposite);

            foreach (var t in sourceTokensForComposite.Select(t => t).Take(3))
            {
                t.TokenCompositeTokenAssociations.Add(new Models.TokenCompositeTokenAssociation { 
                    TokenId = t.Id,
                    TokenCompositeId = sourceTokenComposite.Id
                });
            }
            foreach (var t in targetTokensForComposite.Select(t => t).Take(2))
            {
                t.TokenCompositeTokenAssociations.Add(new Models.TokenCompositeTokenAssociation
                {
                    TokenId = t.Id,
                    TokenCompositeId = targetTokenComposite.Id
                });
            }

            _ = await ProjectDbContext.SaveChangesAsync();

            // Clear ProjectDbContext:
            ProjectDbContext.ChangeTracker.Clear();

            // Get:
            var query = new GetParallelCorpusByParallelCorpusIdQuery(parallelTokenizedCorpus.ParallelCorpusId);
            var queryResult = await Mediator!.Send(query);

            // Get verse mappings:
            var query2 = new GetVerseMappingsByParallelCorpusIdAndBookIdQuery(parallelTokenizedCorpus.ParallelCorpusId, null);
            var queryResult2 = await Mediator!.Send(query2);

            // Validate:
            Assert.NotNull(queryResult);
            Assert.True(queryResult.Success);
            Assert.Equal(sourceTokenizedTextCorpus.TokenizedTextCorpusId, queryResult.Data.sourceTokenizedCorpusId);
            Assert.Equal(targetTokenizedTextCorpus.TokenizedTextCorpusId, queryResult.Data.targetTokenizedCorpusId);

            Assert.NotNull(queryResult2);
            Assert.True(queryResult2.Success);
            Assert.Equal(5, queryResult2.Data!.Count());

            Assert.Empty(queryResult2.Data!
                .SelectMany(vm => vm.SourceVerses)
                .Select(v => (v.Book, v.ChapterNum, v.VerseNum ))
                .Except(verseMappingList
                .SelectMany(vm => vm.SourceVerses)
                .Select(v => (v.Book, v.ChapterNum, v.VerseNum))));

            // Check tokenId values and order between first VerseMapping/first Verse from the database and 
            // the tokenIds we added before calling 'parallelTextCorpus.Create':
            var sourceTokenIdsFirstMappingFirstVerse = queryResult2.Data!.Take(1)
                .SelectMany(vm => vm.SourceVerses.Take(1)
                .SelectMany(sv => sv.TokenIds));
            var targetTokenIdsFirstMappingFirstVerse = queryResult2.Data!.Take(1)
                .SelectMany(vm => vm.TargetVerses.Take(1)
                .SelectMany(tva => tva.TokenIds));

            Assert.NotEqual(sourceTokenIdsFirstMappingFirstVerse, targetTokenIdsFirstMappingFirstVerse);
            Assert.Equal(sourceTokenIdsFirstMappingFirstVerse, sourceTokenIdValuesSortedDescending);
            Assert.Equal(targetTokenIdsFirstMappingFirstVerse, targetTokenIdValuesSortedAscending);

            foreach (var tokenId in sourceTokenIdsFirstMappingFirstVerse)
            {
                Output.WriteLine($"TokenVerseAssocation (source) book {tokenId.BookNumber}, chapter {tokenId.ChapterNumber}, verse: {tokenId.VerseNumber}");
            }

            //var sct = queryResult.Data.verseMappings
            //    .SelectMany(vm => vm.SourceVerses
            //        .SelectMany(sv => sv.TokenIds))
            //    .Where(t => t.Id == sourceTokenComposite.Id);
            //Assert.Single(sct);
            //Assert.True(sct.First() is CompositeTokenId);
            //Output.WriteLine($"\nComposite (source): {sct.First()}");

            Output.WriteLine("");

            foreach (var tokenId in targetTokenIdsFirstMappingFirstVerse)
            {
                Output.WriteLine($"TokenVerseAssocation (target) book {tokenId.BookNumber}, chapter {tokenId.ChapterNumber}, verse: {tokenId.VerseNumber}");
            }

            //var tct = queryResult.Data.verseMappings
            //    .SelectMany(vm => vm.TargetVerses
            //        .SelectMany(sv => sv.TokenIds))
            //    .Where(t => t.Id == targetTokenComposite.Id);
            //Assert.Single(tct);
            //Assert.True(tct.First() is CompositeTokenId);
            //Output.WriteLine($"\nComposite (target): {tct.First()}");

            //// Get tokenized corpus tokens to make sure these parallel corpus composites are not included:
            //var sourceBookId = Canon.BookNumberToId(sourceTokenComposite.Tokens.First().BookNumber);
            //var targetBookId = Canon.BookNumberToId(targetTokenComposite.Tokens.First().BookNumber);

            //var tcsQuery = new GetTokensByTokenizedCorpusIdAndBookIdQuery(sourceTokenizedTextCorpus.TokenizedTextCorpusId, sourceBookId);
            //var tcsQueryResult = await Mediator!.Send(tcsQuery);

            //var tctQuery = new GetTokensByTokenizedCorpusIdAndBookIdQuery(targetTokenizedTextCorpus.TokenizedTextCorpusId, targetBookId);
            //var tctQueryResult = await Mediator!.Send(tctQuery);

            //Assert.NotEmpty(tcsQueryResult.Data!.SelectMany(vt => vt.Tokens));
            //Assert.NotEmpty(tctQueryResult.Data!.SelectMany(vt => vt.Tokens));
            //Assert.Empty(tcsQueryResult.Data!.SelectMany(vt => vt.Tokens.Where(t => t.TokenId.Id == sourceTokenComposite.Id)));
            //Assert.Empty(tctQueryResult.Data!.SelectMany(vt => vt.Tokens.Where(t => t.TokenId.Id == targetTokenComposite.Id)));
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__InvalidParallelCorpusId()
    {
        try
        {
            var query = new GetParallelCorpusByParallelCorpusIdQuery(new ParallelCorpusId(Guid.NewGuid()));

            var result = await Mediator!.Send(query);
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Message);
            Output.WriteLine(result.Message);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void ParallelCorpus__ForeignKeyRequiredForTokenizedCorpus()
    {
        try
        {
            var sourceCorpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var sourceTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, sourceCorpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var targetCorpus = await Corpus.Create(Mediator!, true, "NameY", "LanguageY", "StudyBible", Guid.NewGuid().ToString());
            var targetTokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, targetCorpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var parallelTextCorpus = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, 
                new SourceTextIdToVerseMappingsFromVerseMappings(TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
                    sourceTokenizedTextCorpus.Versification,
                    targetTokenizedTextCorpus.Versification)));

            var parallelTokenizedCorpus = await parallelTextCorpus.CreateAsync("test pc", Container!);

            var parallelCorpusDB = ProjectDbContext!.ParallelCorpa.Include(pc => pc.SourceTokenizedCorpus).FirstOrDefault(pc => pc.Id == parallelTokenizedCorpus.ParallelCorpusId.Id);
            Assert.NotNull(parallelCorpusDB);

            // Remove the source tokenized corpus:
            parallelCorpusDB!.SourceTokenizedCorpusId = Guid.Empty;

            // Commit to database.  Should throw a DbUpdateException
            // containing a SqliteException 19 "FOREIGN KEY constraint failed"
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => ProjectDbContext.SaveChangesAsync());
            Assert.IsType<SqliteException>(ex.InnerException);
            Assert.True((ex.InnerException as SqliteException)!.SqliteErrorCode == 19);

            parallelCorpusDB = ProjectDbContext!.ParallelCorpa.Include(pc => pc.TargetTokenizedCorpus).FirstOrDefault(pc => pc.Id == parallelTokenizedCorpus.ParallelCorpusId.Id);
            Assert.NotNull(parallelCorpusDB);

            // Remove the target tokenized corpus:
            parallelCorpusDB!.TargetTokenizedCorpusId = Guid.Empty;

            // Commit to database.  Should throw a DbUpdateException
            // containing a SqliteException 19 "FOREIGN KEY constraint failed"
            ex = await Assert.ThrowsAsync<DbUpdateException>(() => ProjectDbContext.SaveChangesAsync());
            Assert.IsType<Microsoft.Data.Sqlite.SqliteException>(ex.InnerException);
            Assert.True((ex.InnerException as SqliteException)!.SqliteErrorCode == 19);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}
