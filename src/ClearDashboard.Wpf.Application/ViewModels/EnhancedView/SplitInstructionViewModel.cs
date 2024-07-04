using System;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class SplitInstructionViewModel : PropertyChangedBase
{
   
    private LexiconDialogViewModel? _lexiconDialogViewModel;
    
    public SplitInstructionViewModel(): this(new SplitInstruction())
    {
        
    }

    public SplitInstructionViewModel(SplitInstruction entity)
    {
        Entity = entity;
      
    }

    public SplitInstruction Entity { get; set; }
   
    public LexiconDialogViewModel? LexiconDialogViewModel
    {
        get => _lexiconDialogViewModel;
        set => Set(ref _lexiconDialogViewModel, value);
    }

    public string? TrainingText
    {
        get => Entity.TrainingText;
        set
        {
            Entity.TrainingText = value;
            NotifyOfPropertyChange(() => TrainingText);
           
        }
    }

    public int Index
    {
        get => Entity.Index;
        set
        {
            Entity.Index = value;
            NotifyOfPropertyChange(() => Index);
        }
    }

    public string? TokenText
    {
        get => Entity.TokenText;
        set
        {
            Entity.TokenText = value;
            NotifyOfPropertyChange(() => TokenText);
            
        }
    }


    public string? TokenType
    {
        get => Entity.TokenType;
        set
        {
            Entity.TokenType = value;
            NotifyOfPropertyChange(() => TokenType);
        }
    }

    public string? CircumfixGroup
    {
        get => Entity.CircumfixGroup;
        set
        {
            Entity.CircumfixGroup = value;
            NotifyOfPropertyChange(() => CircumfixGroup);
        }
    }

    public string? Gloss
    {
        get => Entity.Gloss;
        set
        {
            Entity.Gloss = value;
            NotifyOfPropertyChange(() => Gloss);
        }
    }

    private Grammar? _grammar;
    public Grammar? Grammar
    {
        get => _grammar;
        set
        {
            if (value != null)
            {
                Set(ref _grammar, value);
                GrammarId = value?.Id;
            }
           
        }
    }

    public Guid? GrammarId
    {
        get => Entity.GrammarId;
        set
        {
            // if (value != null)
            //{
                Entity.GrammarId = value;
                NotifyOfPropertyChange(() => GrammarId);
            //}
            //else
            //{
            //    if (Entity.GrammarId != null)
            //    {
            //        Entity.GrammarId = value;
            //        NotifyOfPropertyChange(() => GrammarId);
            //    }
            //}
           
        }
    }

    public bool HasGloss => !string.IsNullOrEmpty(Gloss);
}