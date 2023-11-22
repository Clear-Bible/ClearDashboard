using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon;

public class LexiconEditDialogState : PropertyChangedBase
{
    private bool _lexemeChecked;
    private bool _formsChecked;
    private bool _translationChecked;
    private MatchOption _lexemeOption;
    private MatchOption _formsOption;
    private PredicateOption _predicateOption;
    private string? _formsMatch;
    private string? _translationMatch;

    public bool LexemeChecked
    {
        get => _lexemeChecked;
        set
        {
            Set(ref _lexemeChecked, value);
            NotifyBooleansChanged();
        }
    }

    public bool FormsChecked
    {
        get => _formsChecked;
        set
        {
            Set(ref _formsChecked, value);
            NotifyBooleansChanged();
        }
    }

    public bool LexemeAndFormsChecked => LexemeChecked & FormsChecked;

    public bool LexemeOrFormsChecked => LexemeChecked | FormsChecked;

    public bool TransitionAndLexemeOrFormsChecked => TranslationChecked && LexemeAndFormsChecked;

    private void NotifyBooleansChanged()
    {
        NotifyOfPropertyChange(()=> LexemeAndFormsChecked);
        NotifyOfPropertyChange(() => LexemeOrFormsChecked);
        NotifyOfPropertyChange(() => TransitionAndLexemeOrFormsChecked);
    }

    public bool TranslationChecked
    {
        get => _translationChecked;
        set
        {
            Set(ref _translationChecked, value);
            NotifyBooleansChanged();
        }
    }

    public MatchOption LexemeOption
    {
        get => _lexemeOption;
        set => Set(ref _lexemeOption, value);
    }

    public MatchOption FormsOption
    {
        get => _formsOption;
        set => Set(ref _formsOption, value);
    }

    public PredicateOption PredicateOption
    {
        get => _predicateOption;
        set => Set(ref _predicateOption, value);
    }

    public string? FormsMatch
    {
        get => _formsMatch;
        set => Set(ref _formsMatch, value);
    }

    public string? TranslationMatch
    {
        get => _translationMatch;
        set => Set(ref _translationMatch, value);
    }

    public void Configure(LexiconEditMode editMode, string? toMatch)
    {
        /*
             
              -when [PartialMatchOnLexemeOrForm] parameter: (C) checked, (D) set to
              partially, (F) checked, (G) partial, (I) filled in with [toMatch] parameter,
              (K) unchecked; (M) is "Edit adding [other] as translation to first meaning" and
              when pressed (M) changes line into in-place editing, adds [] to first default meaning 
              if no meaning, then adds [other] to first meaning's comma delimited [] list and selects 
              other in this list so user can see what has been added.
               
            
             -when [MatchOnTranslation] parameter: (C) and (F) unchecked, (K) checked; (L) set to 
               [toMatch].; (M) is "Edit adding [other] as form" and when pressed (M) changes line 
               into in-place editing, adds [other] to the comma delimited list of forms, and highlights 
               added form so user can see what was added..

            */

        switch (editMode)
        {
            case LexiconEditMode.MatchOnTranslation:
                LexemeChecked = false;
                FormsChecked = false;
                TranslationChecked = true;
                LexemeOption = MatchOption.Partially;
                FormsOption = MatchOption.Partially;
                PredicateOption = PredicateOption.And;
                FormsMatch = string.Empty;
                TranslationMatch = toMatch;
                break;
            case LexiconEditMode.PartialMatchOnLexemeOrForm:
                LexemeChecked = true;
                FormsChecked = true;
                TranslationChecked = false;
                LexemeOption = MatchOption.Partially;
                FormsOption = MatchOption.Partially;
                PredicateOption = PredicateOption.And;
                FormsMatch = toMatch;
                TranslationMatch = string.Empty;
                break;

        }
    }

      
}