using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.Wpf.Application.Collections;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class SplitInstructionsTests
{

    private ITestOutputHelper Output { get; }
    public SplitInstructionsTests(ITestOutputHelper output) 
    {
        Output = output;
    }


    [Fact]
    public void IndexesNotInAscendingOrderThrowsException()
    {
        var result = Assert.Throws<SplitInstructionException>(() => SplitInstructions.CreateSplits(
            "mputughup",
            [1, 10, 6, 8],
            [null, "to", null, "give", "her"]));
       
        Assert.Equal("The split indexes must be in ascending order.", result.Message);
        Assert.NotNull(result.Details);

        LogException(result);
    }

    [Fact]
    public void IndexesNotInRangeThrowsException()
    {
        var result = Assert.Throws<SplitInstructionException>(() => SplitInstructions.CreateSplits(
            "mputughup",
            [0, 3, 6, 10],
            [null, "to", null, "give", "her"]));

        Assert.Equal("The first and last split indexes must be within the range of the 'SurfaceText' length: '9'.",  result.Message);
        Assert.NotNull(result.Details);

        LogException(result);
    }

    [Fact]
    public void TrainingTextsListMustBeOneGreaterThanSplitIndexesListThrowException()
    {
        var result = Assert.Throws<SplitInstructionException>(() => SplitInstructions.CreateSplits(
            "mputughup",
            [1, 3, 6, 8],
            [null, "to", null, "give"]));

        Assert.Equal("The number of split indexes must be one less than the number of training texts.", result.Message);
        LogException(result);
    }

    [Fact]
    public void SplitInstructionNullTrainingText()
    {
        var splitInstruction = new SplitInstruction(1, "pu");

        Assert.Equal("pu", splitInstruction.TrainingText);
    }

    [Fact]
    public void CreateSplitInstructions()
    {
        var splitInstructions = SplitInstructions.CreateSplits(
            "mputughup",
            [1, 3, 6, 8],
            [string.Empty, "to", null, "give", "her"]
        );

        TestSplitInstructions(splitInstructions);

        var json = JsonSerializer.Serialize(splitInstructions);
     
        Output.WriteLine(string.Empty);
        Output.WriteLine("JSON:");
        Output.WriteLine(json);

    }


    private void TestSplitInstructions(SplitInstructions splitInstructions)
    {
        Assert.True(splitInstructions.Validate());

        Assert.Equal(5, splitInstructions.Count);

        LogSplitInstructions(splitInstructions);
        
        Assert.Equal(0, splitInstructions[0].Index);
        Assert.Equal(1, splitInstructions[0].Length);
        Assert.Equal("m", splitInstructions[0].TokenText);
        Assert.Equal("m", splitInstructions[0].TrainingText);
      

        Assert.Equal(1, splitInstructions[1].Index);
        Assert.Equal(2, splitInstructions[1].Length);
        Assert.Equal("pu", splitInstructions[1].TokenText);
        Assert.Equal("to",splitInstructions[1].TrainingText);

        Assert.Equal(3, splitInstructions[2].Index);
        Assert.Equal(3, splitInstructions[2].Length);
        Assert.Equal("tug", splitInstructions[2].TokenText);
        Assert.Equal("tug", splitInstructions[2].TrainingText);
        

        Assert.Equal(6, splitInstructions[3].Index);
        Assert.Equal(2, splitInstructions[3].Length);
        Assert.Equal("hu", splitInstructions[3].TokenText);
        Assert.Equal("give", splitInstructions[3].TrainingText);

        Assert.Equal(8, splitInstructions[4].Index);
        Assert.Equal(1, splitInstructions[4].Length);
        Assert.Equal("p", splitInstructions[4].TokenText);
        Assert.Equal("her", splitInstructions[4].TrainingText);
    }


    [Fact]
    public void DesrializedJosnIndexesNotInRangeThrowsException()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":11,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonSerializer.Deserialize<SplitInstructions>(json);

        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal("The first and last split indexes must be within the range of the 'SurfaceText' length: '9'.", result.Message);
        Assert.NotNull(result.Details);

        LogException(result);
    }


    [Fact]
    public void DeserializedJsonIsValid()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonSerializer.Deserialize<SplitInstructions>(json);

        TestSplitInstructions(splitInstructions);

    }

    [Fact]
    public void DeserializedJsonInstructionsLengthDoNotMatchTokenTextLengthDoesNotThrowException()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":10,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonSerializer.Deserialize<SplitInstructions>(json);

        var exception = Record.Exception(() => splitInstructions.Validate());

        Assert.Null(exception);

        LogSplitInstructions(splitInstructions);

    }

    private void LogException(SplitInstructionException result)
    {
        Output.WriteLine($"Error message: {result.Message}");
        Output.WriteLine($"Error details: {result.Details}");
    }

    [Fact]
    public void DeserializedJsonInstructionAggregatedTokenTextsDoNotMatchSurfaceTextThrowsException()
    {
        var json =
            @"{""SurfaceText"":""*mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":3,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonSerializer.Deserialize<SplitInstructions>(json);

        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal(@"The aggregated 'TokenText' properties from the 'Instructions' list must be equal to the 'SurfaceText' property of the 'SplitInstructions'.", result.Message);
        LogException(result);
        Output.WriteLine(string.Empty);
        LogSplitInstructions(splitInstructions);
    }

    private void LogSplitInstructions(SplitInstructions? splitInstructions)
    {
        Output.WriteLine("Split Instructions:");
        foreach (var splitInstruction in splitInstructions.Instructions)
        {
            LogSplitInstruction(splitInstruction);
        }
    }

    [Fact]
    public void DeserializedJsonWithNonSequentialIndexesThrowsException()
    {
        var json =
            @"{""SurfaceText"":""mputughup"",""Instructions"":[{""Index"":0,""Length"":1,""TokenText"":""m"",""TrainingText"":null},{""Index"":1,""Length"":2,""TokenText"":""pu"",""TrainingText"":""to""},{""Index"":10,""Length"":3,""TokenText"":""tug"",""TrainingText"":null},{""Index"":6,""Length"":2,""TokenText"":""hu"",""TrainingText"":""give""},{""Index"":8,""Length"":1,""TokenText"":""p"",""TrainingText"":""her""}]}";

        var splitInstructions = JsonSerializer.Deserialize<SplitInstructions>(json);
        var result = Assert.Throws<SplitInstructionException>(() => splitInstructions.Validate());

        Assert.Equal("The split indexes must be in ascending order.", result.Message);
        Assert.NotNull(result.Details);

        LogException(result);

    }
    private void LogSplitInstruction(SplitInstruction splitInstruction)
    {
        Output.WriteLine($"Index: {splitInstruction.Index}, Length: {splitInstruction.Length}, Token Text: {splitInstruction.TokenText}, \t Training Text: {splitInstruction.TrainingText}");
    }

    [Fact(Skip="Skipping for now")]
  
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