using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

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

    public bool IsEditing
    {
        get => _isEditing;
        set => Set(ref _isEditing, value);
    }

    /// <summary>
    /// Builds a string of the form "Meaning [Translation]" for each meaning of the lexeme.
    /// </summary>
    /// <remarks>
    /// Note:  Meanings imported from Paratext have String.Empty as their Text property, which can result in a string of the form " [Translation1, Translation2]".
    /// </remarks>
    public string? Meanings
    {
        get => _lexeme.Meanings.Select(m => $"{m.Text} [{m.Translations.Select(t => t.Text).ToDelimitedString()}]").ToDelimitedString(";");
        set
        {
             var meanings = value?.Split(';').Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList() ?? new List<string>();
            foreach (var meaningWithTranslations in meanings)
            {
                var parts = meaningWithTranslations.Split('[');
                var meaning = _lexeme.Meanings.FirstOrDefault(m => m.Text == parts[0].TrimEnd());
                var translations = parts[1].TrimEnd(']').Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                if (meaning == null)
                {
                    meaning = new Meaning { Text = parts[0].TrimEnd(), Language = Lexeme.Language };
                    AddNewTranslations(translations, meaning);
                    _lexeme.Meanings.Add(meaning);
                }
                else
                {
                    AddNewTranslations(translations, meaning);
                }

            }
            NotifyOfPropertyChange(nameof(Meanings));
        }
    }

    private void AddNewTranslations(IEnumerable<string> translations, Meaning meaning)
    {
       
        foreach (var translation in translations.Where(translation => meaning.Translations.All(t => t.Text != translation)))
        {
            meaning.Translations.Add(new Translation { Text = translation });
        }
        
    }

    public string? Forms
    {
        get => _lexeme.Forms.Select(f => f.Text).ToDelimitedString();
        set
        {
            var forms = value?.Split(',').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList() ?? new List<string>();
            foreach (var form in forms.Where(form => _lexeme.Forms.All(f => f.Text != form)))
            {
                _lexeme.Forms.Add(new Form { Text = form });
            }

            NotifyOfPropertyChange(nameof(Forms));
        }
    }

    public void AddNewForm(string? form)
    {
        if (_lexeme.Forms.All(f => f.Text != form))
        {
            _lexeme.Forms.Add(new Form { Text = form });
        }
        NotifyOfPropertyChange(nameof(Forms));
    }

    public void AddNewMeaning(string? meaningText)
    {
        if (_lexeme.Meanings.All(m => m.Text != meaningText))
        {
            _lexeme.Meanings.Add(new Meaning { Text = meaningText });
        }
        NotifyOfPropertyChange(nameof(Meanings));
    }
}