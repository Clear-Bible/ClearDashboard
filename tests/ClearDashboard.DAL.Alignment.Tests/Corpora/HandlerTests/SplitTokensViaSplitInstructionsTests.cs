using System;
using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class SplitTokensViaSplitInstructionsTests : TestBase
{
    public SplitTokensViaSplitInstructionsTests(ITestOutputHelper output) : base(output)
    {
           

    }

    [Fact]
    [Trait("Category", "Handlers")]
    public async void SplitTokens1()
    {
        try
        {
            var textCorpus = CreateFakeZZSurTextCorpusWithCompositie(false);

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
                .Where(e => e.SurfaceText == "mputughup")
                .ToDictionary(e => ModelHelper.BuildTokenId(e), e => e.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId).ToList());

            var splitInstructions = SplitInstructions.CreateSplits(
                "mputughup",
                [1, 3, 6, 8],
                [null, "to", null, "give", "her"]
            );

            var split1 = await tokenizedTextCorpus.SplitTokensViaSplitInstructions(
                Mediator!,
                tokenIdsWithCommonSurfaceText.Keys,
                splitInstructions,
                false,
                SplitTokenPropagationScope.None
            );

            Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitChildTokensByIncomingTokenId.Count());
            Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitCompositeTokensByIncomingTokenId.Count());

            foreach (var t in split1.SplitChildTokensByIncomingTokenId)
            {
                var children = t.Value.ToArray();
                Assert.Equal(5, children.Length);

                Assert.True(children[0].SurfaceText == "m");
                Assert.True(children[0].TrainingText == "m");

                Assert.True(children[1].SurfaceText == "pu");
                Assert.True(children[1].TrainingText == "to");

                Assert.True(children[2].SurfaceText == "tug");
                Assert.True(children[2].TrainingText == "tug");

                Assert.True(children[3].SurfaceText == "hu");
                Assert.True(children[3].TrainingText == "give");

                Assert.True(children[4].SurfaceText == "p");
                Assert.True(children[4].TrainingText == "her");
            }

            var tokenWithExistingComposite = tokenIdsWithCommonSurfaceText
                .Where(e => e.Value.Any())
                .Select(e => (e.Key, e.Value))
                .ToList();

            //Assert.Single(tokenWithExistingComposite);
            //Assert.Single(tokenWithExistingComposite.First().Value);

            var tokenIdWithExistingComposite = tokenWithExistingComposite.First().Key.Id;
            var existingCompositeId = tokenWithExistingComposite.First().Item2.First();

            foreach (var tc in split1.SplitCompositeTokensByIncomingTokenId)
            {
                Assert.Single(tc.Value);
                var composite = tc.Value.First();

                // TODO:  Code review with Chris - when would the composite token not be the same as the existing composite token?
                if (tc.Key.Id == tokenIdWithExistingComposite)
                {
                    Assert.Equal(composite.TokenId.Id, existingCompositeId);
                    Assert.Equal(5, composite.Tokens.Count());
                    Assert.Equal("m_pu_tug_hu_p", composite.SurfaceText);
                    Assert.Equal("m_to_tug_give_her", composite.TrainingText);
                }
                else
                {
                    Assert.NotEqual(composite.TokenId.Id, existingCompositeId);
                    Assert.Equal(5, composite.Tokens.Count());
                    Assert.Equal("m_pu_tug_hu_p", composite.SurfaceText);
                    Assert.Equal("m_to_tug_give_her", composite.TrainingText);
                }
            }
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    public static ITextCorpus CreateFakeZZSurTextCorpusWithCompositie(bool includeBadCompositeToken)
    {
        var textCorpus = new DictionaryTextCorpus(
                new MemoryText("GEN", new[]
                {
                    new TextRow(new VerseRef(1, 43, 30)) { Segment = "Ɓe ni le an nJosep mputughup zam, kaa a yitmwaan nsham wuri si mpee wal ɗi wuri kɨ ni mpee ɗyemnɨghɨn fɨri, ɓe wuri yaghal kɨlak ku wuri tang pee ɗi wuri nwal ɗi, Wuri ɗel nlu fɨri, ɓe wuri wal.".Split(), IsSentenceStart = true,  IsEmpty = false },
                    new TextRow(new VerseRef(1, 45, 27)) { Segment = "Amma kaaɗi mo sat nwuri po ɗi Josep sat mo nwuri jir, kɨ kaaɗi wuri naa keke bɨring ɗi Josep lop mpee mang wuri ɓe ni waa cin ɓal mputughup nwuri.".Split(), IsSentenceStart = true,  IsEmpty = false },
                    new TextRow(new VerseRef(1, 49, 9)) { Segment = "Ba mee gurum ɗi kɨ ɓal mputughup ɗi nso tung wuri kas.".Split(), IsSentenceStart = true,  IsEmpty = false },
                    //""
                }))
            .Tokenize<LatinWordTokenizer>()
            .Transform<IntoFakeCompositeTokensTextRowProcessor>()
            .Transform<SetTrainingBySurfaceLowercase>();

        return textCorpus;
    }

    private class IntoFakeCompositeTokensTextRowProcessor : IRowProcessor<TextRow>
    {
        public TextRow Process(TextRow textRow)
        {
            if (textRow.Text.Contains("mputughup"))
            {
                return GenerateTokensTextRow(textRow);
            }

            return new TokensTextRow(textRow);
        }
    }

    private static TokensTextRow GenerateTokensTextRow(TextRow textRow)
    {
        var tr = new TokensTextRow(textRow);

        var tokens = tr.Tokens;

        var tokenIds = tokens
            .Select(t => t.TokenId)
            .ToList();

        var compositeTokens = new List<Token>() { tokens.First(t=>t.SurfaceText == "mputughup") };
        var tokensWithComposite = new List<Token>()
        {
            new CompositeToken(compositeTokens),
        };

        tr.Tokens = tokensWithComposite;
        return tr;
    }
}