using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora;

public record SplitInstruction(
    int Index,
    string TokenText,
    string? TrainingText
)
{
    [JsonIgnore]
    public int Length => TokenText.Length;
};