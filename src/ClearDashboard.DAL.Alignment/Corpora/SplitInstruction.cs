using ClearDashboard.DataAccessLayer.Models;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora;

public class SplitInstruction
{
    public SplitInstruction()
    {
    }

    public SplitInstruction(int index, string tokenText)
    {
        Index = index;
        TokenText = tokenText;
    }

    public string? TokenText
    {
        get => _tokenText;
        set => _tokenText = value;
    }


    [JsonIgnore]
    public int Length => TokenText.Length;


    public int Index { get; set; }
    private string? _trainingText;
    private string? _circumfixGroup;
    private string? _gloss;
    private Guid? _grammarId;
    private string? _tokenText;
    private string? _tokenType;


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

    public string? TokenType
    {
        get => _tokenType;
        set => _tokenType = value;
    }
};