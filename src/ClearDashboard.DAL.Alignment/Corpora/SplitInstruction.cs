using ClearDashboard.DataAccessLayer.Models;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora;

public record SplitInstruction(int Index, string TokenText)
{
   
    [JsonIgnore]
    public int Length => TokenText.Length;

    private string? _trainingText;
    private string? _circumfixGroup;
    private string? _gloss;
    private Guid? _grammarId;

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

    public string? CircumfixGroup
    {
        get => _circumfixGroup;
        set => _circumfixGroup = value;
    }

    public string? Gloss
    {
        get => _gloss;
        set => _gloss = value;
    }

    public Guid? GrammarId
    {
        get => _grammarId;
        set => _grammarId = value;
    }
};