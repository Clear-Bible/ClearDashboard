using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon;

public class EditableLexemeViewModel : PropertyChangedBase
{
    private Lexeme _lexeme;

    public EditableLexemeViewModel(Lexeme lexeme)
    {
        _lexeme = lexeme;
    }

    public string? Text { get => _lexeme.Lemma; set => _lexeme.Lemma = value; }

    public string? Type { get => _lexeme.Type; set => _lexeme.Type = value; }

    public string? Meanings => _lexeme.Meanings.Select(m => m.Translations.Select(t=>t.Text).ToDelimitedString()).ToDelimitedString();

    public string? Forms => _lexeme.Forms.Select(f => f.Text).ToDelimitedString();
}