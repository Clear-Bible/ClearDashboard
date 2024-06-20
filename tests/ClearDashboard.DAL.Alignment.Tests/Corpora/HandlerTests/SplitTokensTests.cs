using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DAL.Alignment.Features.Common;
using System.Threading;
using ClearDashboard.DataAccessLayer.Data;
using Autofac;
using ClearDashboard.DAL.Interfaces;


namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests
{
    public class SplitTokensTests : TestBase
    {
        public const string PARATEXT_PROJECT_ID_ZZ_SUR = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f";
#nullable disable
		public SplitTokensTests(ITestOutputHelper output) : base(output)
        {
        }

		[Fact]
		[Trait("Category", "Handlers")]
		public async void WordAnalysesImportSecondTimeNoChange()
		{
			// TODO:  make sure Paratext is started/stopped
			try
			{
				var textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN" });

				// Create the corpus in the database:
				var corpus = await Corpus.Create(Mediator!, false, "SUR", "SUR", "Standard", PARATEXT_PROJECT_ID_ZZ_SUR);

				// Create the TokenizedCorpus + Tokens in the database:
				Output.WriteLine($"Creating tokenized corpus with book GEN");
				var tokenizedTextCorpus = await textCorpus.Create(
					Mediator!,
					corpus.CorpusId,
					"SURWordAnalysesTest",
					"LatinWordTokenizer");

				// Check initial save values:
				Assert.NotNull(tokenizedTextCorpus);

				var initialTokenCount = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Assert.True(initialTokenCount > 0);
				Assert.Empty(ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id));

				Output.WriteLine($"Initial token count: {initialTokenCount}");

				ProjectDbContext!.ChangeTracker.Clear();

				await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

                var postImportTokenCount1 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
                var postImportTokenCompositeCount1 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

                Assert.True(postImportTokenCount1 > 0);
				Assert.True(postImportTokenCompositeCount1 > 0);

				Output.WriteLine($"Post import word analyses (first round) token count: {postImportTokenCount1}, token composite count: {postImportTokenCompositeCount1}");

				ProjectDbContext!.ChangeTracker.Clear();

				await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

				var postImportTokenCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
				var postImportTokenCompositeCount2 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Output.WriteLine($"Post import word analyses (second round) token count: {postImportTokenCount2}, token composite count: {postImportTokenCompositeCount2}");

				Assert.True(postImportTokenCount2 > 0);
				Assert.True(postImportTokenCompositeCount2 > 0);

				Assert.Equal(postImportTokenCount1, postImportTokenCount2);
				Assert.Equal(postImportTokenCompositeCount1, postImportTokenCompositeCount2);
			}
			finally
			{
				await DeleteDatabaseContext();
			}
		}

		[Fact]
		[Trait("Category", "Handlers")]
		public async void WordAnalysesImportSecondTimeAddBook()
		{
			// TODO:  make sure Paratext is started/stopped
			try
			{
				var textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN" });

				// Create the corpus in the database:
				var corpus = await Corpus.Create(Mediator!, false, "SUR", "SUR", "Standard", PARATEXT_PROJECT_ID_ZZ_SUR);

				// Create the TokenizedCorpus + Tokens in the database:
				Output.WriteLine($"Creating tokenized corpus with book GEN");
				var tokenizedTextCorpus = await textCorpus.Create(
					Mediator!,
					corpus.CorpusId,
					"SURWordAnalysesTest",
					"LatinWordTokenizer");

				// Check initial save values:
				Assert.NotNull(tokenizedTextCorpus);

				var initialTokenCount = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Assert.True(initialTokenCount > 0);
				Assert.Empty(ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id));

				Output.WriteLine($"Initial token count: {initialTokenCount}");

				ProjectDbContext!.ChangeTracker.Clear();

				await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

				var postImportTokenCount1 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
				var postImportTokenCompositeCount1 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Assert.True(postImportTokenCount1 > 0);
				Assert.True(postImportTokenCompositeCount1 > 0);

				Output.WriteLine($"Post import word analyses (first round) token count: {postImportTokenCount1}, token composite count: {postImportTokenCompositeCount1}");

				ProjectDbContext!.ChangeTracker.Clear();

				Output.WriteLine($"Updating tokenized corpus to have books GEN, EXO");
				textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN", "EXO" });
                await tokenizedTextCorpus.UpdateOrAddVerses(Mediator!, textCorpus, CancellationToken.None);

				var postAddBookTokenCount = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
				var postAddBookTokenCompositeCount = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Assert.True(postAddBookTokenCount > postImportTokenCount1);
				Assert.True(postAddBookTokenCompositeCount == postImportTokenCompositeCount1);

				Output.WriteLine($"Post add book (EXO) token count: {postAddBookTokenCount}, token composite count: {postAddBookTokenCompositeCount}");

				ProjectDbContext!.ChangeTracker.Clear();

				await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

				var postImportTokenCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
				var postImportTokenCompositeCount2 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

				Output.WriteLine($"Post import word analyses (second round) token count: {postImportTokenCount2}, token composite count: {postImportTokenCompositeCount2}");

				Assert.True(postImportTokenCount2 > postAddBookTokenCount);  // Number of deleted tokens should have increased
				Assert.True(postImportTokenCompositeCount2 > postAddBookTokenCompositeCount);

				var postImportGENTokenCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Where(e => e.BookNumber == 1).Count();
				var postImportGENTokenCompositeCount2 = ProjectDbContext.TokenComposites
					.Include(e => e.TokenCompositeTokenAssociations)
						.ThenInclude(e => e.Token)
					.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
					.Where(e => e.TokenCompositeTokenAssociations.Any(t => t.Token.BookNumber == 1))
					.Count();

				var postImportEXOTokenCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Where(e => e.BookNumber == 2).Count();
				var postImportEXOTokenCompositeCount2 = ProjectDbContext.TokenComposites
					.Include(e => e.TokenCompositeTokenAssociations)
						.ThenInclude(e => e.Token)
					.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
					.Where(e => e.TokenCompositeTokenAssociations.Any(t => t.Token.BookNumber == 2))
					.Count();

                Assert.Equal(postImportTokenCount1, postImportGENTokenCount2);
				Assert.Equal(postImportTokenCompositeCount1, postImportGENTokenCompositeCount2);
                Assert.True(postImportEXOTokenCount2 > 0);
				Assert.True(postImportEXOTokenCompositeCount2 > 0);

				Output.WriteLine($"Post import word analyses (second round) token count GEN: {postImportGENTokenCount2}, token composite count GEN: {postImportGENTokenCompositeCount2}");
				Output.WriteLine($"Post import word analyses (second round) token count EXO: {postImportEXOTokenCount2}, token composite count EXO: {postImportEXOTokenCompositeCount2}");
			}
			finally
			{
				await DeleteDatabaseContext();
			}
		}

		[Fact]
		[Trait("Category", "Handlers")]
		public async void WordAnalysesImportSecondTimeChangeAnalyses()
		{
            // TODO:  make sure Paratext is started/stopped
            try
            {
                var userProvider = Container!.Resolve<IUserProvider>();

                var textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN" });

                // Create the corpus in the database:
                var corpus = await Corpus.Create(Mediator!, false, "SUR", "SUR", "Standard", PARATEXT_PROJECT_ID_ZZ_SUR);

                // Create the TokenizedCorpus + Tokens in the database:
                Output.WriteLine($"Creating tokenized corpus with book GEN");
                var tokenizedTextCorpus = await textCorpus.Create(
                    Mediator!,
                    corpus.CorpusId,
                    "SURWordAnalysesTest",
                    "LatinWordTokenizer");

                // Check initial save values:
                Assert.NotNull(tokenizedTextCorpus);

                var initialTokenCount = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();

                Assert.True(initialTokenCount > 0);
                Assert.Empty(ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id));

                Output.WriteLine($"Initial token count: {initialTokenCount}");

                ProjectDbContext!.ChangeTracker.Clear();

                await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

                var postImportTokenCount1 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
                var postImportTokenCompositeCount1 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
                var postImportTokenSoftDeletedCount1 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Where(e => e.Deleted != null).Count();

                Assert.True(postImportTokenCount1 > 0);
                Assert.True(postImportTokenCompositeCount1 > 0);
                Assert.True(postImportTokenSoftDeletedCount1 > 0);
                Assert.Equal(postImportTokenCompositeCount1, postImportTokenSoftDeletedCount1);

                Output.WriteLine($"Post import word analyses (first round) token count: {postImportTokenCount1}, token composite count: {postImportTokenCompositeCount1}");

                ProjectDbContext!.ChangeTracker.Clear();

                var externalWordAnalysesCommand = new GetExternalWordAnalysesQuery(PARATEXT_PROJECT_ID_ZZ_SUR);
                var externalWordAnalysesResult = await Mediator.Send(externalWordAnalysesCommand);
                var wordAnalyses = externalWordAnalysesResult.Data!.ToList();

                // Change some of these to force a re-split
                var waChanged = 0;

                var exampleWord1 = string.Empty;
                var exampleLexemeInfoBefore1 = new (string surfaceText, string trainingText, string type)[] { };
                var exampleLemmasAfter1 = (string.Empty, string.Empty, string.Empty);

                var exampleWord2 = string.Empty;
                var exampleLexemeInfoBefore2 = new (string surfaceText, string trainingText, string type)[] { };
				var exampleLemmasAfter2 = (string.Empty, string.Empty);

                foreach (var d in wordAnalyses.Where(w => w.Lexemes.Count() == 2).Where(w => w.Lexemes.Last().Lemma.Length > 4))
                {
                    var lexeme1 = d.Lexemes.First();
                    var lexeme2 = d.Lexemes.Last();

                    if (waChanged == 0)
                    {
                        exampleWord1 = d.Word;
                        exampleLexemeInfoBefore1 = d.Lexemes.Select(e => (e.Lemma, e.Lemma, e.Type)).ToArray();

                        // take first character of second lexeme and put at end of first lexeme lemma
                        var lemma1 = $"{lexeme1.Lemma}{lexeme2.Lemma.Substring(0, 1)}";
                        // take the second and third characters and use as second lexeme lemma
                        var lemma2 = $"{lexeme2.Lemma.Substring(1, lexeme2.Lemma.Length - 3)}";
                        // add a new lexeme lemma using the remaining characters of the second lexeme lemma 
                        var lemma3 = $"{lexeme2.Lemma.Substring(3)}";

                        lexeme1.Lemma = lemma1;
                        lexeme2.Lemma = lemma2;

                        var lexeme3 = new DataAccessLayer.Models.Lexicon_Lexeme
                        {
                            Id = Guid.NewGuid(),
                            Language = "SUR",
                            Lemma = lemma3,
                            Type = "FakeType",
                            User = userProvider.CurrentUser!,
                            UserId = userProvider.CurrentUser!.Id
                        };

                        d.Lexemes.Add(ModelHelper.BuildLexeme(lexeme3, null, false));

                        exampleLemmasAfter1 = (lemma1, lemma2, lemma3);
                    }
                    else
                    {
                        if (waChanged == 1)
                        {
                            exampleWord2 = d.Word;
                            exampleLexemeInfoBefore2 = d.Lexemes.Select(e => (e.Lemma, e.Lemma, e.Type)).ToArray();
						}

                        // take first two characters of 2nd lexeme and put at end of first lexeme lemma
                        var charactersToMove = lexeme2.Lemma.Substring(0, 2);
                        lexeme1.Lemma = $"{lexeme1.Lemma}{charactersToMove}";
                        // use the remaining characters as the second lexeme lemma
                        lexeme2.Lemma = lexeme2.Lemma.Substring(2);

                        if (waChanged == 1)
                        {
                            exampleLemmasAfter2 = (lexeme1.Lemma, lexeme2.Lemma);
                        }
                    }

                    if (waChanged++ >= 10)
                        break;
                }

                var importWordAnalysesCommand = new ImportWordAnalysesCommand(wordAnalyses, tokenizedTextCorpus.TokenizedTextCorpusId);
                await Mediator.Send(importWordAnalysesCommand);

                var postImportTokenCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
                var postImportTokenCompositeCount2 = ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Count();
                var postImportTokenSoftDeletedCount2 = ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id).Where(e => e.Deleted != null).Count();

                Output.WriteLine($"Post import word analyses (second round) token count: {postImportTokenCount2}, token composite count: {postImportTokenCompositeCount2}");

                var example1SourceCount = ProjectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                    .Where(e => e.Deleted != null)
                    .Where(e => e.SurfaceText == exampleWord1)
                    .Count();

                Assert.Equal(postImportTokenCount2, postImportTokenCount1 + example1SourceCount);  // We added an extra lexeme to each one of these
                Assert.Equal(postImportTokenCompositeCount2, postImportTokenCompositeCount1);
                Assert.Equal(postImportTokenSoftDeletedCount2, postImportTokenSoftDeletedCount1);
                Assert.Equal(postImportTokenCompositeCount2, postImportTokenSoftDeletedCount2);

                // Check that example one was handled correctly:
                var matchingSources1 = ProjectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                    .Where(e => e.Deleted != null)
                    .Where(e => e.SurfaceText == exampleWord1)
                    .ToList();

                var exampleCompositeText1 = $"{exampleLemmasAfter1.Item1}_{exampleLemmasAfter1.Item2}_{exampleLemmasAfter1.Item3}";
                var matchingComposites1 = ProjectDbContext.TokenComposites
                    .Include(e => e.TokenCompositeTokenAssociations.OrderBy(e => e.Token.EngineTokenId))
                        .ThenInclude(e => e.Token)
                    .Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                    .Where(e => e.SurfaceText == exampleCompositeText1)
                    .ToList();

                Assert.NotEmpty(matchingComposites1);
                Assert.Equal(matchingComposites1.Count(), matchingSources1.Count());
                var matchingComposite1 = matchingComposites1.First();

                var sourceId1 = Guid.Parse(matchingComposite1.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenSourceId).Select(e => e.Value).First());
                var sourceSurfaceText1 = matchingComposite1.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenSourceSurfaceText).Select(e => e.Value).First();
				var sourceInitialChildren1 = matchingComposite1.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenInitialChildren).Select(e => e.Value).First();

				var matchingSource1 = matchingSources1.Where(e => e.Id == sourceId1).FirstOrDefault();
                Assert.NotNull(matchingSource1);
				var wasSplit1 = matchingSource1.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.WasSplit).Select(e => e.Value).First();

				Assert.Equal(sourceSurfaceText1, exampleWord1);
                Assert.Equal(sourceInitialChildren1, exampleLexemeInfoBefore1.GetSplitMatchInfoAsHash());
				Assert.Equal(wasSplit1, true.ToString());

				Assert.Equal(3, matchingComposite1.TokenCompositeTokenAssociations.Count());
                var surfaceTexts1 = matchingComposite1.TokenCompositeTokenAssociations.Select(e => e.Token.SurfaceText).ToList();
                Assert.Contains(exampleLemmasAfter1.Item1, surfaceTexts1);
                Assert.Contains(exampleLemmasAfter1.Item2, surfaceTexts1);
                Assert.Contains(exampleLemmasAfter1.Item3, surfaceTexts1);

				// Check that example two was handled correctly:
				var matchingSources2 = ProjectDbContext.Tokens
                    .Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                    .Where(e => e.Deleted != null)
                    .Where(e => e.SurfaceText == exampleWord2)
                    .ToList();

                var exampleCompositeText2 = $"{exampleLemmasAfter2.Item1}_{exampleLemmasAfter2.Item2}";
                var matchingComposites2 = ProjectDbContext.TokenComposites
                    .Include(e => e.TokenCompositeTokenAssociations.OrderBy(e => e.Token.EngineTokenId))
                        .ThenInclude(e => e.Token)
                    .Where(e => e.TokenizedCorpusId == tokenizedTextCorpus.TokenizedTextCorpusId.Id)
                    .Where(e => e.SurfaceText == exampleCompositeText2)
                    .ToList();

                Assert.NotEmpty(matchingComposites2);
                Assert.Equal(matchingComposites2.Count(), matchingSources2.Count());
                var matchingComposite2 = matchingComposites2.First();

                var sourceId2 = Guid.Parse(matchingComposite2.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenSourceId).Select(e => e.Value).First());
                var sourceSurfaceText2 = matchingComposite2.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenSourceSurfaceText).Select(e => e.Value).First();
				var sourceInitialChildren2 = matchingComposite2.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.SplitTokenInitialChildren).Select(e => e.Value).First();

				var matchingSource2 = matchingSources2.Where(e => e.Id == sourceId2).FirstOrDefault();
                Assert.NotNull(matchingSource2);
				var wasSplit2 = matchingSource2.Metadata.Where(e => e.Key == DataAccessLayer.Models.MetadatumKeys.WasSplit).Select(e => e.Value).First();
				
                Assert.Equal(sourceSurfaceText2, exampleWord2);
				Assert.Equal(sourceInitialChildren2, exampleLexemeInfoBefore2.GetSplitMatchInfoAsHash());
				Assert.Equal(wasSplit2, true.ToString());

				Assert.Equal(2, matchingComposite2.TokenCompositeTokenAssociations.Count);
                var surfaceTexts2 = matchingComposite2.TokenCompositeTokenAssociations.Select(e => e.Token.SurfaceText).ToList();
                Assert.Contains(exampleLemmasAfter2.Item1, surfaceTexts2);
                Assert.Contains(exampleLemmasAfter2.Item2, surfaceTexts2);
            }
            finally
            {
                await DeleteDatabaseContext();
            }
		}

		[Fact]
		[Trait("Category", "Handlers")]
		public async Task TestMetadataSaveUsingBulkInsertAsync()
        {
			ProjectDbContext.Database.OpenConnection();
			try
			{
				var textCorpus = TestDataHelpers.GetSampleTextCorpus();

				// Create the corpus in the database:
				var corpus = await Corpus.Create(Mediator!, false, "SAMPLE", "SAMPLE", "Standard", Guid.NewGuid().ToString());

				// Create the TokenizedCorpus + Tokens in the database:
				var tokenizedTextCorpus = await textCorpus.Create(
					Mediator!,
					corpus.CorpusId,
					"MetadataTest",
					"LatinWordTokenizer");

				// Check initial save values:
				Assert.NotNull(tokenizedTextCorpus);

                var testMetadatum = new List<DataAccessLayer.Models.Metadatum>
                {
                    new DataAccessLayer.Models.Metadatum { Key = "key1", Value = "value1" },
                    new DataAccessLayer.Models.Metadatum { Key = "key2", Value = "value2" }
                };

                var testToken = new DataAccessLayer.Models.Token
                {
                    Id = Guid.NewGuid(),
                    TokenizedCorpusId = tokenizedTextCorpus.TokenizedTextCorpusId.Id,
                    EngineTokenId = "001002003004005",
                    VerseRowId = ProjectDbContext.VerseRows.First().Id,
                    BookNumber = 1,
                    ChapterNumber = 2,
                    VerseNumber = 3,
                    WordNumber = 4,
                    SubwordNumber = 0,
                    TrainingText = "testToken1",
                    SurfaceText = "testToken1",
                    Metadata = testMetadatum
                };

                // Add a token having Metadata using DbContext: 
				ProjectDbContext.TokenComponents.Add(testToken);
                await ProjectDbContext.SaveChangesAsync();

                // Add a token having Metadata using DbCommand (i.e. Bulk Insert):
				using (var cmd = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(ProjectDbContext.Database.GetDbConnection()))
				{
                    testToken.Id = Guid.NewGuid();
                    testToken.Metadata.Add(new DataAccessLayer.Models.Metadatum { Key = "key3", Value = "value3" });
					await TokenizedCorpusDataBuilder.InsertTokenAsync(testToken, null, cmd, CancellationToken.None);
				}

                Assert.Equal(2, ProjectDbContext.Tokens
                    .Where(t => t.Deleted == null)
                    .Where(t => t.Metadata.Any(m => m.Key == "key1" && m.Value == "value1"))
                    .Count());

				Assert.Single(ProjectDbContext.Tokens
					.Where(t => t.Deleted == null)
					.Where(t => t.Metadata.Any(m => m.Key == "key3" && m.Value != null)));

				Assert.Empty(ProjectDbContext.Tokens
                    .Where(t => t.Deleted == null)
                    .Where(t => t.Metadata.Any(m => m.Key == "key2" && m.Value == "value1")));
			}
			finally
			{
				ProjectDbContext.Database.CloseConnection();
				await DeleteDatabaseContext();
			}
		}

		[Fact]
        [Trait("Category", "Handlers")]
        public async void SplitTokens1()
        {
            try
            {
                var textCorpus = CreateTokenizedCorpusFromTextCorpusHandlerTests.CreateFakeTextCorpusWithComposite(false);

                // Create the corpus in the database:
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

                // Create the TokenizedCorpus + Tokens in the database:
                var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.RussianProtestant);

                Assert.NotNull(tokenizedTextCorpus);
                Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

                ProjectDbContext!.ChangeTracker.Clear();

                var tokenIdsWithCommonSurfaceText = ProjectDbContext!.Tokens
                    .Include(e => e.TokenCompositeTokenAssociations)
                    .Where(e => e.SurfaceText == "Source")
                    .ToDictionary(e => ModelHelper.BuildTokenId(e), e => e.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId).ToList());

                var split1 = await tokenizedTextCorpus.SplitTokens(
                    Mediator!,
                    tokenIdsWithCommonSurfaceText.Keys,
                    2,
                    2,
                    "bob",
                    "joe",
                    "bo",
                    false,
                    SplitTokenPropagationScope.None
                    );

                Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitChildTokensByIncomingTokenId.Count());
                Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitCompositeTokensByIncomingTokenId.Count());

                foreach (var t in split1.SplitChildTokensByIncomingTokenId)
                {
                    var children = t.Value.ToArray();
                    Assert.Equal(3, children.Length);

                    Assert.True(children[0].SurfaceText == "So");
                    Assert.True(children[0].TrainingText == "bob");

                    Assert.True(children[1].SurfaceText == "ur");
                    Assert.True(children[1].TrainingText == "joe");

                    Assert.True(children[2].SurfaceText == "ce");
                    Assert.True(children[2].TrainingText == "bo");
                }

                var tokenWithExistingComposite = tokenIdsWithCommonSurfaceText
                    .Where(e => e.Value.Any())
                    .Select(e => (e.Key, e.Value))
                    .ToList();

                Assert.Single(tokenWithExistingComposite);
                Assert.Single(tokenWithExistingComposite.First().Value);

                var tokenIdWithExistingComposite = tokenWithExistingComposite.First().Key.Id;
                var existingCompositeId = tokenWithExistingComposite.First().Item2.First();

                foreach (var tc in split1.SplitCompositeTokensByIncomingTokenId)
                {
                    Assert.Single(tc.Value);
                    var composite = tc.Value.First();

                    if (tc.Key.Id == tokenIdWithExistingComposite)
                    {
                        Assert.Equal(composite.TokenId.Id, existingCompositeId);
                        Assert.Equal(5, composite.Tokens.Count());
                        Assert.Equal("So_ur_ce_segment_1", composite.SurfaceText);
                        Assert.Equal("bob_joe_bo_segment_1", composite.TrainingText);
                    }
                    else
                    {
                        Assert.NotEqual(composite.TokenId.Id, existingCompositeId);
                        Assert.Equal(3, composite.Tokens.Count());
                        Assert.Equal("So_ur_ce", composite.SurfaceText);
                        Assert.Equal("bob_joe_bo", composite.TrainingText);
                    }
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Handlers")]
        public async void SplitTokens2()
        {
            try
            {
                var textCorpus = CreateTokenizedCorpusFromTextCorpusHandlerTests.CreateFakeTextCorpusWithComposite(false);

                // Create the corpus in the database:
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

                // Create the TokenizedCorpus + Tokens in the database:
                var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.RussianProtestant);

                Assert.NotNull(tokenizedTextCorpus);
                Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

                ProjectDbContext!.ChangeTracker.Clear();

                var tokenIdsWithCommonSurfaceText = ProjectDbContext!.Tokens
                    .Include(e => e.TokenCompositeTokenAssociations)
                    .Where(e => e.SurfaceText == "Source")
                    .ToDictionary(e => ModelHelper.BuildTokenId(e), e => e.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId).ToList());

                var split1 = await tokenizedTextCorpus.SplitTokens(
                    Mediator!,
                    tokenIdsWithCommonSurfaceText.Keys,
                    0,
                    3,
                    "bobby",
                    "joey",
                    null,
                    false,
                    SplitTokenPropagationScope.None
                    );

                Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitChildTokensByIncomingTokenId.Count());
                Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitCompositeTokensByIncomingTokenId.Count());

                foreach (var t in split1.SplitChildTokensByIncomingTokenId)
                {
                    var children = t.Value.ToArray();
                    Assert.Equal(2, children.Length);

                    Assert.True(children[0].SurfaceText == "Sou");
                    Assert.True(children[0].TrainingText == "bobby");

                    Assert.True(children[1].SurfaceText == "rce");
                    Assert.True(children[1].TrainingText == "joey");
                }

                var tokenWithExistingComposite = tokenIdsWithCommonSurfaceText
                    .Where(e => e.Value.Any())
                    .Select(e => (e.Key, e.Value))
                    .ToList();

                Assert.Single(tokenWithExistingComposite);
                Assert.Single(tokenWithExistingComposite.First().Value);

                var tokenIdWithExistingComposite = tokenWithExistingComposite.First().Key.Id;
                var existingCompositeId = tokenWithExistingComposite.First().Item2.First();

                foreach (var tc in split1.SplitCompositeTokensByIncomingTokenId)
                {
                    Assert.Single(tc.Value);
                    var composite = tc.Value.First();

                    if (tc.Key.Id == tokenIdWithExistingComposite)
                    {
                        Assert.Equal(composite.TokenId.Id, existingCompositeId);
                        Assert.Equal(4, composite.Tokens.Count());
                        Assert.Equal("Sou_rce_segment_1", composite.SurfaceText);
                        Assert.Equal("bobby_joey_segment_1", composite.TrainingText);
                    }
                    else
                    {
                        Assert.NotEqual(composite.TokenId.Id, existingCompositeId);
                        Assert.Equal(2, composite.Tokens.Count());
                        Assert.Equal("Sou_rce", composite.SurfaceText);
                        Assert.Equal("bobby_joey", composite.TrainingText);
                    }
                }

                ProjectDbContext!.ChangeTracker.Clear();

                // "Source" was split, so the original token should be soft deleted:
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "Source", WordPart.Full, true));
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "Source", WordPart.Full, false));

                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Full, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "segment", WordPart.Full, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Full, false));

                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "sO", WordPart.First, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "So", WordPart.First, false)).Count());
                var sOTokens =
                    await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "sO", WordPart.First, false);
                Assert.Empty(sOTokens);
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.First, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "segment", WordPart.First, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.First, false));

                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "GME", WordPart.Middle, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "gme", WordPart.Middle, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "GME", WordPart.Middle, false));
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Middle, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "segment", WordPart.Middle, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Middle, false));

                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "CE", WordPart.Last, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "ce", WordPart.Last, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "CE", WordPart.Last, false));
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Last, true)).Count());
                Assert.Equal(3, (await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "segment", WordPart.Last, false)).Count());
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "SEGment", WordPart.Last, false));
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }

        [Fact]
        [Trait("Category", "Handlers")]
        public async void SplitTokens3()
        {
            try
            {
                var textCorpus = TestDataHelpers.GetSampleTextCorpus();

                // Create the corpus in the database:
                var corpus = await Corpus.Create(Mediator!, true, "NameX", "LanguageX", "Standard", Guid.NewGuid().ToString());

                // Create the TokenizedCorpus + Tokens in the database:
                var tokenizationFunction = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()";
                var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "Unit Test", tokenizationFunction, ScrVers.RussianProtestant);

                Assert.NotNull(tokenizedTextCorpus);
                Assert.All(tokenizedTextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

                ProjectDbContext!.ChangeTracker.Clear();

                var tokenIdsWithCommonSurfaceText = ProjectDbContext!.Tokens
                    .Include(e => e.TokenCompositeTokenAssociations)
                    .Where(e => e.SurfaceText == "Chapter")
                    .ToDictionary(e => ModelHelper.BuildTokenId(e), e => e.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId).ToList());

                var singleTokenId = tokenIdsWithCommonSurfaceText.Keys.Take(1);
                var split1 = await tokenizedTextCorpus.SplitTokens(
                    Mediator!,
                    singleTokenId,
                    0,
                    2,
                    "bob",
                    "joe",
                    null,
                    false,
                    SplitTokenPropagationScope.BookChapter
                    );

                var scopeMatchCount = ProjectDbContext!.Tokens
                    .Where(e => e.SurfaceText == "Chapter")
                    .Where(e => e.BookNumber == singleTokenId.First().BookNumber)
                    .Where(e => e.ChapterNumber == singleTokenId.First().ChapterNumber)
                    .Count();

                Assert.Equal(scopeMatchCount, split1.SplitChildTokensByIncomingTokenId.Count());
                Assert.Equal(scopeMatchCount, split1.SplitCompositeTokensByIncomingTokenId.Count());

                foreach (var t in split1.SplitChildTokensByIncomingTokenId)
                {
                    var children = t.Value.ToArray();
                    Assert.Equal(2, children.Length);

                    Assert.True(children[0].SurfaceText == "Ch");
                    Assert.True(children[0].TrainingText == "bob");

                    Assert.True(children[1].SurfaceText == "apter");
                    Assert.True(children[1].TrainingText == "joe");
                }
            }
            finally
            {
                await DeleteDatabaseContext();
            }
        }
    }
}
