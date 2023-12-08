using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon;

public class EditableLexemeViewModel : PropertyChangedBase
{
    private readonly Lexeme _lexeme;
    private bool _isEditing;

    public EditableLexemeViewModel(Lexeme lexeme)
    {
        _lexeme = lexeme;
    }

    public Lexeme Lexeme => _lexeme;

    public string? Text
    {
        get => _lexeme.Lemma;
        set
        {
            _lexeme.Lemma = value;
            NotifyOfPropertyChange(nameof(Text));
        }
    }

    public string? Type
    {
        get => _lexeme.Type;
        set
        {
            _lexeme.Type = value;
            NotifyOfPropertyChange(nameof(Type));
        }
    }

    public string? SourceLanguage { get; set; }

    public string? TargetLanguage { get; set; }

    public bool IsEditing
    {
        get => _isEditing;
        set => Set(ref _isEditing, value);
    }

    public string EditLabel
    {
        get => _editLabel;
        set => Set(ref _editLabel, value);
    }

    public string DoneLabel
    {
        get => _doneLabel;
        set => Set(ref _doneLabel, value);
    }

    /// <summary>
    /// Builds a string of the form "Meaning [Translation]" for each meaning of the lexeme.
    /// </summary>
    /// <remarks>
    /// Note:  Meanings imported from Paratext have String.Empty as their Text property, which can result in a string of the form " [Translation1, Translation2]".
    /// </remarks>
    public string? Meanings
    {
        get => _lexeme.Meanings.Select(m => $"{m.Text} [{m.Translations.Select(t => t.Text).ToDelimitedString()}]")
            .ToDelimitedString(";");
        set
        {
            var meanings = value?.Unjoin(";") ?? new List<string>();

            //if (meanings.Count == 0)
            //{
            //    return;
            //}

            foreach (var meaningWithTranslations in meanings)
            {
                if (meaningWithTranslations.StartsWith("["))
                {
                    var translations = meaningWithTranslations.TrimStart('[').TrimEnd(']').Unjoin();
                    var defaultMeaning = _lexeme.Meanings.FirstOrDefault();
                    ProcessTranslations(defaultMeaning, translations);
                }
                else
                {
                    var parts = meaningWithTranslations.Split('[');
                    var meaning = _lexeme.Meanings.FirstOrDefault(m => m.Text == parts[0].TrimEnd());
                    var translations = parts[1].TrimEnd(']').Unjoin();
                    if (meaning == null)
                    {
                        meaning = new Meaning { Text = parts[0].TrimEnd(), Language = TargetLanguage };
                        ProcessTranslations(meaning, translations);
                    }
                    else
                    {
                        ProcessTranslations(meaning, translations);
                    }
                }

            }

            NotifyOfPropertyChange(nameof(Meanings));
        }
    }

    private string _editButtonLabel;
    private string _editLabel;
    private string _doneLabel;
    private bool _isDirty;

    public string EditButtonLabel
    {
        get => _editButtonLabel;
        set => Set(ref _editButtonLabel, value);
    }

    public async Task OnEditButtonClicked()
    {
        IsEditing = !IsEditing;
        if(IsEditing)
        {
           EditButtonLabel = DoneLabel;
        }
        else
        {
            EditButtonLabel = EditLabel;
        }
        await Task.CompletedTask;
    }


    private void ProcessTranslations(Meaning defaultMeaning, List<string> translations)
    {
        var translationsToDelete = defaultMeaning.Translations.Where(t => !translations.Contains(t.Text)).ToList();

        foreach (var translation in translationsToDelete)
        {
            var translationToDelete = defaultMeaning.Translations.FirstOrDefault(t => t.Text == translation.Text);
            var index = defaultMeaning.Translations.IndexOf(translationToDelete);
            if (index > -1)
            {
                defaultMeaning.Translations.RemoveAt(index);
                if (IsDirty == false)
                {
                    IsDirty = true;
                }
            }
        }

        var translationsToAdd =
            translations.Where(translation => defaultMeaning.Translations.All(t => t.Text != translation)).ToList();
        foreach (var translation in translationsToAdd)
        {
            defaultMeaning.Translations.Add(new Translation { Text = translation });
            if (IsDirty == false)
            {
                IsDirty = true;
            }
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        set => Set(ref _isDirty, value);
    }

    public string? Forms
    {
        get => _lexeme.Forms.Select(f => f.Text).ToDelimitedString();
        set
        {
            var forms = value?.Unjoin() ?? new List<string>();

            //if (forms.Count == 0)
            //{
            //    return;
            //}


            var formsToDelete = _lexeme.Forms.Where(f => !forms.Contains(f.Text)).ToList();
            foreach (var form in formsToDelete)
            {
                var index = _lexeme.Forms.IndexOf(form);
                _lexeme.Forms.RemoveAt(index);

                if (IsDirty == false)
                {
                    IsDirty = true;
                }
            }

            var formsToAdd = forms.Where(form => _lexeme.Forms.All(f => f.Text != form)).ToList();
            foreach (var form in formsToAdd)
            {
                _lexeme.Forms.Add(new Form { Text = form });
                if (IsDirty == false)
                {
                    IsDirty = true;
                }

            }

            NotifyOfPropertyChange(nameof(Forms));
        }
    }


    /// <summary>
    /// Adds a new form to the lexeme.
    /// </summary>
    /// <param name="form">The form to add.</param>
    /// <returns>The StartIndex and Length of the newly added form in the first Forms string so it can be highlighted in the UI.</returns>
    public (int StartIndex, int Length) AddNewForm(string? form)
    {
        if (form == null)
        {
            return (-1, -1);
        }

        if (_lexeme.Forms.All(f => f.Text != form))
        {
            _lexeme.Forms.Add(new Form { Text = form });
            var startIndex = Forms?.IndexOf(form) ?? -1;
            if (IsDirty == false)
            {
                IsDirty = true;
            }
            NotifyOfPropertyChange(nameof(Forms));
            return (startIndex, form?.Length ?? -1);
        }

        return (-1, -1);

    }

    /// <summary>
    /// Adds a translation to the first meaning of the lexeme.
    /// </summary>
    /// <param name="translation">The translation to add.</param>
    /// <returns>The StartIndex and Length of the newly added translation in the first meaning string so it can be highlighted in the UI.</returns>
    public (int StartIndex, int Length) AddTranslationToFirstMeaning(string? translation)
    {
        var defaultMeaning = _lexeme.Meanings.FirstOrDefault();
        if (defaultMeaning == null)
        {
            return (-1, -1);
        }

        if (defaultMeaning.Translations.All(t => t.Text != translation))
        {
            defaultMeaning.Translations.Add(new Translation { Text = translation });
            var startIndex = Meanings?.IndexOf(translation) ?? -1;
            if (IsDirty == false)
            {
                IsDirty = true;
            }
            NotifyOfPropertyChange(nameof(Meanings));
            return (startIndex, translation?.Length ?? -1);
        }

        return (-1, -1);
    }
}