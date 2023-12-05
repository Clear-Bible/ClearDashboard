using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Converters;
using ClearDashboard.Wpf.Application.Services;

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
                        //AddNewTranslations(translations, meaning);
                        //_lexeme.Meanings.Add(meaning);

                        ProcessTranslations(meaning, translations);
                    }
                    else
                    {
                        //AddNewTranslations(translations, meaning);
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
            //var translationToDelete = defaultMeaning.Translations.FirstOrDefault(t => t.Text == translation.Text);
            var index = defaultMeaning.Translations.IndexOf(translation);
            if (index > -1)
            {
                defaultMeaning.Translations.RemoveAt(index);
                if (defaultMeaning.IsDirty == false)
                {
                    defaultMeaning.SetInternalProperty(nameof(Meaning.IsDirty), true);
                }
            }
        }

        var translationsToAdd =
            translations.Where(translation => defaultMeaning.Translations.All(t => t.Text != translation));
        foreach (var translation in translationsToAdd)
        {
            defaultMeaning.Translations.Add(new Translation { Text = translation });
            defaultMeaning.SetInternalProperty(nameof(Meaning.IsDirty), true);
        }
    }

    //private void AddNewTranslations(IEnumerable<string> translations, Meaning meaning)
    //{

    //    foreach (var translation in translations.Where(translation => meaning.Translations.All(t => t.Text != translation)))
    //    {
    //        meaning.Translations.Add(new Translation { Text = translation });
    //    }

    //}

   


public string? Forms
    {
        get => _lexeme.Forms.Select(f => f.Text).ToDelimitedString();
        set
        {
            var forms = value?.Unjoin() ?? new List<string>();

            var formsToDelete = _lexeme.Forms.Where(f => !forms.Contains(f.Text));
            foreach (var form in formsToDelete)
            {
                var index = _lexeme.Forms.IndexOf(form);
                _lexeme.Forms.RemoveAt(index);
            }

            var formsToAdd = forms.Where(form => _lexeme.Forms.All(f => f.Text != form));
            foreach (var form in formsToAdd)
            {
                _lexeme.Forms.Add(new Form { Text = form });
            }

            NotifyOfPropertyChange(nameof(Forms));
        }
    }

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
            NotifyOfPropertyChange(nameof(Forms));
            return (startIndex, form?.Length ?? -1);
        }

        return (-1, -1);

    }

    //public void AddNewMeaning(string? meaningText)
    //{

    //    if (_lexeme.Meanings.All(m => m.Text != meaningText))
    //    {
    //        _lexeme.Meanings.Add(new Meaning { Text = meaningText, Language = TargetLanguage});
    //    }
    //    NotifyOfPropertyChange(nameof(Meanings));
    //}

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
            NotifyOfPropertyChange(nameof(Meanings));
            return (startIndex, translation?.Length ?? -1);
        }

        return (-1, -1);
    }
}