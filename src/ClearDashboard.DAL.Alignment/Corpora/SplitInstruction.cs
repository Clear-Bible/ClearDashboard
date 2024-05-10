using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora;

public record SplitInstruction(
    int Index,
    string TokenText
)
{
   

    [JsonIgnore]
    public int Length => TokenText.Length;

    private string? _trainingText;
    /// <summary>
    /// The training text for the split instruction. If not set, the TokenText is used.
    /// </summary>
    public string? TrainingText
    {
        get
        {
            if (string.IsNullOrEmpty(_trainingText))
            {
                _trainingText = TokenText;
            }
            return _trainingText;
        }
        set => _trainingText = value;
    }
};