using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class GetBookIdsByTokenizedCorpusIdQueryHandlerTests : TestBase
{
    public GetBookIdsByTokenizedCorpusIdQueryHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds__CreateTokenizedCorpusThenQuery()
    {
        try
        {
            var textCorpus = TestDataHelpers.GetSampleTextCorpus();

            // Create the corpus in the database:
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "BackTranslation", Guid.NewGuid().ToString());

            // Create the TokenizedCorpus + Tokens in the database:
            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpus.CorpusId, string.Empty, string.Empty, ScrVers.Original);
            var commandResult = await Mediator!.Send(command);

            ProjectDbContext!.ChangeTracker.Clear();

            var query = new GetBookIdsByTokenizedCorpusIdQuery(commandResult.Data?.TokenizedTextCorpusId!);
            var queryResult = await Mediator.Send(query);

            Assert.NotNull(queryResult);
            Assert.True(queryResult.Success);

            Output.WriteLine(queryResult.Data.corpusId.ToString());
            foreach (var bookId in queryResult.Data.bookIds)
            {
                Output.WriteLine($"\t{bookId}");
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void BookIds__InvalidTokenizedCorpusId()
    {
        try
        {
            var query = new GetBookIdsByTokenizedCorpusIdQuery(new TokenizedTextCorpusId(Guid.NewGuid()));

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
    public async void BookIds__InvalidTokenBookNumber()
    {
        try
        {
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());
            var tokenizedTextCorpus = await TestDataHelpers.GetSampleTextCorpus()
                .Create(Mediator!, corpus.CorpusId, "Unit Test", ".a.function()");

            var tokenizedCorpus = ProjectDbContext!.TokenizedCorpora.FirstOrDefault(tc => tc.Id == tokenizedTextCorpus.TokenizedTextCorpusId.Id);
            Assert.NotNull(tokenizedCorpus);

            var verseRow = new Models.VerseRow()
            {
                BookChapterVerse = "999001001",
                OriginalText = "booboo",
                IsSentenceStart = true,
                IsInRange = true,
                IsRangeStart = true,
                IsEmpty = false
            };

            // Add token with bogus book number:
            var token = new Models.Token
            {
                BookNumber = 999,
                ChapterNumber = 1,
                VerseNumber = 1,
                WordNumber = 1,
                SubwordNumber = 1
            };

            verseRow.TokenComponents.Add(token);
            tokenizedCorpus!.VerseRows.Add(verseRow);
            tokenizedCorpus!.TokenComponents.Add(token);

            // Commit to database:
            await ProjectDbContext.SaveChangesAsync();

            // Run the query:
            var query = new GetBookIdsByTokenizedCorpusIdQuery(tokenizedTextCorpus.TokenizedTextCorpusId);

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
    public async void BookIds__QueryLargeCorpus_MeasurePerformance()
    {
        try
        {
            //Import
            var textCorpus = new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();

            //ITextCorpus.Create() extension requires that ITextCorpus source and target corpus have been transformed
            // into TokensTextRow, puts them into the DB, and returns a TokensTextRow.
            var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "LanguageType", Guid.NewGuid().ToString());
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            var tokenizedTextCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId;

            ProjectDbContext!.ChangeTracker.Clear();

            Process proc = Process.GetCurrentProcess();
            proc.Refresh();

            Output.WriteLine($"Private memory usage (BEFORE): {proc.PrivateMemorySize64}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            _ = await TokenizedTextCorpus.Get(Mediator!, tokenizedTextCorpusId);

            sw.Stop();

            proc.Refresh();
            Output.WriteLine($"Private memory usage (AFTER):  {proc.PrivateMemorySize64}");

            Output.WriteLine("Elapsed={0}", sw.Elapsed);
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }
}
