using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Machine.Tokenization;
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
                    false
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
                    false
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
                Assert.Empty(await tokenizedTextCorpus.FindTokensBySurfaceText(Mediator!, "sO", WordPart.First, false));
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
    }
}
