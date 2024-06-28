using System;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class SplitInstructionViewModel : PropertyChangedBase
{
    private string? _trainingText;
    private int _index;
    private string? _tokenText;

    private string? _circumfixGroup;
    private string? _gloss;
    private Grammar? _grammar;
    private Guid? _grammarId;
    private string? _tokenType;
    private LexiconDialogViewModel? _lexiconDialogViewModel;

   
    public LexiconDialogViewModel? LexiconDialogViewModel
    {
        get => _lexiconDialogViewModel;
        set => Set(ref _lexiconDialogViewModel, value);
    }

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

    public Grammar? Grammar
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