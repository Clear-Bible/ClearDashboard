using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class SplitInstructionsViewModel : PropertyChangedBase
{

    public SplitInstructionsViewModel(): this(new SplitInstructions())
    {
        
    }

    public SplitInstructionsViewModel(SplitInstructions entity)
    {
        Entity = entity;

        foreach (var splitInstruction in entity.Instructions)
        {
            Instructions.Add(new SplitInstructionViewModel(splitInstruction));
        }

        Instructions.CollectionChanged += (sender, args) =>
        {
            args.NewItems?.OfType<SplitInstructionViewModel>().ToList().ForEach(i => entity.Instructions.Add(i.Entity));
            args.OldItems?.OfType<SplitInstructionViewModel>().ToList().ForEach(i => entity.Instructions.Remove(i.Entity));
        };
    }
    public SplitInstructions Entity { get; set; }
    public List<int> SplitIndexes
    {
        get => Entity.SplitIndexes;
        set
        {
            Entity.SplitIndexes = value;
            NotifyOfPropertyChange(()=> SplitIndexes);
        }
    }

    public int Count => Instructions.Count;

    public int? SurfaceTextLength => SurfaceText?.Length;

    public string? ErrorMessage
    {
        get=>Entity.ErrorMessage;
         set
        {
            Entity.ErrorMessage = value;
            NotifyOfPropertyChange(() => ErrorMessage);
        }
    }

    public string? SurfaceText
    {
        get=> Entity.SurfaceText;
        set
        {
            Entity.SurfaceText = value;
            NotifyOfPropertyChange(() => SurfaceText);
        }
    }

    public BindableCollection<SplitInstructionViewModel> Instructions { get; set; } = [];


    public SplitInstructionViewModel this[int index]
    {
        get => Instructions[index];
        set => Instructions[index] = value;
    }

    public void Add(SplitInstructionViewModel instruction)
    {
        Instructions.Add(instruction);
    }

    public void Clear()
    {
        Instructions.Clear();
    }

    public void UpdateInstructions(int index, bool selected)
    {
        var currentSplitIndexes = Instructions.Select(i => i.Index).ToList();
        if (selected)
        {
            if (!currentSplitIndexes.Contains(index))
            {
                var last = currentSplitIndexes.Count > 0 ? currentSplitIndexes.Last() == index : true;
                var first = currentSplitIndexes.Count > 0 ? currentSplitIndexes.First() == index : true;

                string tokenText;

                if (first)
                {
                    tokenText = SurfaceText[..(index+1)];
                    Instructions.Add(new SplitInstructionViewModel { Index = 0, TokenText = tokenText, TrainingText = tokenText });
                    index++;
                }

                int? length = last ? SurfaceTextLength - index : currentSplitIndexes[index] - index;
                tokenText = SurfaceText.Substring(index, length.HasValue ? length.Value: 0);
                Instructions.Add(new SplitInstructionViewModel { Index = index, TokenText = tokenText, TrainingText = tokenText });



               
            }
        }
        else
        {
            if (currentSplitIndexes.Contains(index))
            {
                Instructions.RemoveAt(index);
            }
        }
    }

}