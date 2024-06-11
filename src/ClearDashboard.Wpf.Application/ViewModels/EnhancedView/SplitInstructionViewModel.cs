using System;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class SplitInstructionViewModel : PropertyChangedBase
{
    private string? _trainingText;
    private int _index;
    private string? _tokenText;

    private string? _circumfixGroup;
    private string? _gloss;
    private string? _grammar;
    private Guid? _grammarId;

    public string? TrainingText
    {
        get => _trainingText;
        set => Set(ref _trainingText, value);
    }

    public int Index
    {
        get => _index;
        set => Set(ref _index, value);
    }

    public string? TokenText
    {
        get => _tokenText;
        set => Set(ref _tokenText, value);
    }

    private string? _tokenType;

    public string? TokenType
    {
        get => _tokenType;
        set => Set(ref _tokenType, value);
    }

    public string? CircumfixGroup
    {
        get => _circumfixGroup;
        set => Set(ref _circumfixGroup, value);
    }

    public string? Gloss
    {
        get => _gloss;
        set => Set(ref _gloss, value);
    }

    public string? Grammar
    {
        get => _grammar;
        set => Set(ref _grammar, value);
    }

    public Guid? GrammarId
    {
        get => _grammarId;
        set => Set(ref _grammarId, value);
    }
}