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


namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests
{
    public class SplitTokensTests : TestBase
    {
#nullable disable
        public SplitTokensTests(ITestOutputHelper output) : base(output)
        {
        }

		[Fact]
		[Trait("Category", "Handlers")]
		public async void WordAnalysesImportFromTokenizedCorpus()
		{
			try
			{
				var textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN" });

				// Create the corpus in the database:
				var corpus = await Corpus.Create(Mediator!, false, "SUR", "SUR", "Standard", "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f" /* zzSUR */);

				// Create the TokenizedCorpus + Tokens in the database:
				var tokenizedTextCorpus = await textCorpus.Create(
					Mediator!,
					corpus.CorpusId,
					"SURWordAnalysesTest",
					"LatinWordTokenizer");

				// Check initial save values:
				Assert.NotNull(tokenizedTextCorpus);

                await tokenizedTextCorpus.ImportWordAnalyses(Mediator!, CancellationToken.None);

				var externalWordAnalysesCommand = new GetExternalWordAnalysesQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f" /* zzSUR */);
				var externalWordAnalysesResult = await Mediator.Send(externalWordAnalysesCommand);

                var wordAnalyses = externalWordAnalysesResult.Data!.ToList();

                // Change some of these to force a re-split
                var waChanged = 0;
                foreach (var d in wordAnalyses.Where(w => w.Lexemes.Count() == 2).Where(w => w.Lexemes.Last().Lemma.Length > 4))
                {
                    // take first two characters of 2nd lexeme and put at end of first lexeme
                    var lexeme1 = d.Lexemes.First();
                    var lexeme2 = d.Lexemes.Last();
                    var charactersToMove = lexeme2.Lemma.Substring(0, 2);
                    lexeme1.Lemma = $"{lexeme1.Lemma}{charactersToMove}";
                    lexeme2.Lemma = lexeme2.Lemma.Substring(2);

                    if (waChanged++ >= 10)
                        break;
                }

				var importWordAnalysesCommand = new ImportWordAnalysesCommand(wordAnalyses, tokenizedTextCorpus.TokenizedTextCorpusId);
				await Mediator.Send(importWordAnalysesCommand);
			}
			finally
			{
				//				await DeleteDatabaseContext();
			}
		}

		[Fact]
		[Trait("Category", "Handlers")]
		public async void WordAnalysesImport()
        {
            try
            {
			    var externalWordAnalysesCommand = new GetExternalWordAnalysesQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f" /* zzSUR */);
			    var externalWordAnalysesResult = await Mediator.Send(externalWordAnalysesCommand);

			    var textCorpus = TestDataHelpers.GetZZSurCorpus(new string[] { "GEN", "EXO" });

			    // Create the corpus in the database:
			    var corpus = await Corpus.Create(Mediator!, false, "SUR", "SUR", "Standard", Guid.NewGuid().ToString());

			    // Create the TokenizedCorpus + Tokens in the database:
			    var tokenizedTextCorpus = await textCorpus.Create(
				    Mediator!,
				    corpus.CorpusId,
				    "SURWordAnalysesTest",
				    "LatinWordTokenizer");

			    // Check initial save values:
			    Assert.NotNull(tokenizedTextCorpus);

                var importWordAnalysesCommand = new ImportWordAnalysesCommand(externalWordAnalysesResult.Data!, tokenizedTextCorpus.TokenizedTextCorpusId);
                var importWordAnalysesResult = await Mediator.Send(importWordAnalysesCommand);
			}
			finally
			{
//				await DeleteDatabaseContext();
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
