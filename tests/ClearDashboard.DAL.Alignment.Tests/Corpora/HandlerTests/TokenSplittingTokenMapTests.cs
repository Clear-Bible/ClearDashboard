using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.Wpf.Application.Collections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SIL.Machine.Corpora;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class SplitTokensTest
{

    private ITestOutputHelper Output { get; }
    public SplitTokensTest(ITestOutputHelper output) 
    {
        Output = output;
    }


    [Fact]
    public void IndexesNotInAscendingOrder()
    {
        var result = Assert.Throws<SplitInstructionException>(() => SplitInstructions.CreateSplits(
            "mputughup",
            [1, 10, 6, 8],
            [null, "to", null, "give", "her"]));
       
        Assert.Equal("The split indexes must be in ascending order.", result.Message);
        Assert.NotNull(result.Details);

        Output.WriteLine(result.Message);
        Output.WriteLine(result.Details);
    }

    [Fact]
    public void TrainingTextsListMustBeOneGreaterThanSplitIndexesList()
    {
        var result = Assert.Throws<SplitInstructionException>(() => SplitInstructions.CreateSplits(
            "mputughup",
            [1, 3, 6, 8],
            [null, "to", null, "give"]));

        Assert.Equal("The number of split indexes must be one less than the number of training texts.", result.Message);
        Output.WriteLine(result.Message);
        Output.WriteLine(result.Details);
    }

    [Fact]
    public  async Task CreateSplitInstructions()
    {
        try {
            var splitInstructions = SplitInstructions.CreateSplits(
                "mputughup",
                [1, 3, 6, 8],
                [null, "to", null, "give", "her"]
            );

            TestSplitInstructions(splitInstructions);


            

            var json = JsonConvert.SerializeObject(splitInstructions);
            Output.WriteLine(json);
        }
        finally
        {
            
        }

    }

    private void TestSplitInstructions(SplitInstructions splitInstructions)
    {
        Assert.True(splitInstructions.Validate());

        Assert.Equal(5, splitInstructions.Count);

        LogSplitInstruction(splitInstructions[0]);
        Assert.Equal(0, splitInstructions[0].Index);
        Assert.Equal(1, splitInstructions[0].Length);
        Assert.Equal("m", splitInstructions[0].TokenText);
        Assert.Null(splitInstructions[0].TrainingText);

        LogSplitInstruction(splitInstructions[1]);
        Assert.Equal(1, splitInstructions[1].Index);
        Assert.Equal(2, splitInstructions[1].Length);
        Assert.Equal("pu", splitInstructions[1].TokenText);
        Assert.Equal("to",splitInstructions[1].TrainingText);

        LogSplitInstruction(splitInstructions[2]);
        Assert.Equal(3, splitInstructions[2].Index);
        Assert.Equal(3, splitInstructions[2].Length);
        Assert.Equal("tug", splitInstructions[2].TokenText);
        Assert.Null(splitInstructions[0].TrainingText);

        LogSplitInstruction(splitInstructions[3]);
        Assert.Equal(6, splitInstructions[3].Index);
        Assert.Equal(2, splitInstructions[3].Length);
        Assert.Equal("hu", splitInstructions[3].TokenText);
        Assert.Equal("give", splitInstructions[3].TrainingText);

        LogSplitInstruction(splitInstructions[4]);
        Assert.Equal(8, splitInstructions[4].Index);
        Assert.Equal(1, splitInstructions[4].Length);
        Assert.Equal("p", splitInstructions[4].TokenText);
        Assert.Equal("her", splitInstructions[4].TrainingText);
    }


    [Fact]
    public void DeserializedJsonIsValid()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonConvert.DeserializeObject<SplitInstructions>(json);

        TestSplitInstructions(splitInstructions);

    }

    [Fact]
    public void DeserializedJsonInstructionsLengthDoNotMatchTokenTextLengthThrowsException()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":10,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonConvert.DeserializeObject<SplitInstructions>(json);

        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal(@"The 'Length' of each split instruction must equal to the actual length of the instruction's 'TokenText'.", result.Message);
        Output.WriteLine(result.Message);
        Output.WriteLine(result.Details);

    }

    [Fact]
    public void DeserializedJsonInstructionAggregatedTokenTextsDoNotMatchSurfaceTextThrowsException()
    {
        var json =
            @"{""SurfaceText"":""*mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonConvert.DeserializeObject<SplitInstructions>(json);

        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal(@"The aggregated 'TokenText' properties from the 'Instructions' list must be equal to the 'SurfaceText' property of the 'SplitInstructions'.", result.Message);
        Output.WriteLine(result.Message);
        Output.WriteLine(result.Details);

        LogSplitInstruction(splitInstructions[0]);
        LogSplitInstruction(splitInstructions[1]);
        LogSplitInstruction(splitInstructions[2]);
        LogSplitInstruction(splitInstructions[3]);
        LogSplitInstruction(splitInstructions[4]);

    }

    [Fact]
    public void DeserializedJsonWithNonSequentialIndexesThrowsException()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":10,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonConvert.DeserializeObject<SplitInstructions>(json);
        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal("The split indexes must be in ascending order.", result.Message);
        Assert.NotNull(result.Details);

        Output.WriteLine(result.Message);
        Output.WriteLine(result.Details);

    }
    private void LogSplitInstruction(SplitInstruction splitInstruction)
    {
        Output.WriteLine($"Index: {splitInstruction.Index}, Length: {splitInstruction.Length}, Token Text: {splitInstruction.TokenText}, \t Training Text: {splitInstruction.TrainingText}");
    }

    [Fact]
    public void Test()
    {
        var tokens = new List<Token>
        {
            new(new TokenId(1, 1, 1, 1, 1), "m", "m"),
            new(new TokenId(1, 1, 1, 1, 2), "p", "p"),
            new(new TokenId(1, 1, 1, 1, 3), "u", "u"),
            new(new TokenId(1, 1, 1, 1, 4), "t", "t"),
            new(new TokenId(1, 1, 1, 1, 5), "u", "u"),
            new(new TokenId(1, 1, 1, 1, 6), "g", "g"),
            new(new TokenId(1, 1, 1, 1, 7), "h", "h"),
            new(new TokenId(1, 1, 1, 1, 8), "u", "u"),
            new(new TokenId(1, 1, 1, 1, 9), "p", "p")

        };
        var tokenCollection = new TokenCollection(tokens);


    }
}


public class TokenSplittingTokenMapTests : TestBase
{
    public TokenSplittingTokenMapTests(ITestOutputHelper output) : base(output)
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

            var split1 = await tokenizedTextCorpus.SplitTokens(
                Mediator!,
                tokenIdsWithCommonSurfaceText.Keys,
                1,
                2,
                null,
                "to",
                null,
                false,
                SplitTokenPropagationScope.None
            );

            Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitChildTokensByIncomingTokenId.Count());
            Assert.Equal(tokenIdsWithCommonSurfaceText.Count, split1.SplitCompositeTokensByIncomingTokenId.Count());

            foreach (var t in split1.SplitChildTokensByIncomingTokenId)
            {
                var children = t.Value.ToArray();
                Assert.Equal(3, children.Length);

                Assert.True(children[0].SurfaceText == "m");
                Assert.True(children[0].TrainingText == "bob");

                Assert.True(children[1].SurfaceText == "pu");
                Assert.True(children[1].TrainingText == "joe");

                Assert.True(children[2].SurfaceText == "tughup");
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

    public static ITextCorpus CreateFakeZZSurTextCorpusWithCompositie(bool includeBadCompositeToken)
    {
        var textCorpus = new DictionaryTextCorpus(
                new MemoryText("GEN", new[]
                {
                    new TextRow(new VerseRef(1, 1, 1)) { Segment = "Ɓe ni le an nJosep mputughup zam, kaa a yitmwaan nsham wuri si mpee wal ɗi wuri kɨ ni mpee ɗyemnɨghɨn fɨri, ɓe wuri yaghal kɨlak ku wuri tang pee ɗi wuri nwal ɗi, Wuri ɗel nlu fɨri, ɓe wuri wal.". Split(), IsSentenceStart = true,  IsEmpty = false },

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

        var compositeTokens = new List<Token>() { tokens[5] };
        var tokensWithComposite = new List<Token>()
        {
            new CompositeToken(compositeTokens),
        };

        tr.Tokens = tokensWithComposite;
        return tr;
    }
}