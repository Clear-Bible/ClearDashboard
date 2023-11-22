using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon;

public class EditableLexemeViewModel : PropertyChangedBase
{
    private readonly Lexeme _lexeme;

    public EditableLexemeViewModel(Lexeme lexeme)
    {
        _lexeme = lexeme;
    }

    public string? Text { get => _lexeme.Lemma; set => _lexeme.Lemma = value; }

    public string? Type { get => _lexeme.Type; set => _lexeme.Type = value; }

    /// <summary>
    /// Builds a string of the form "Meaning [Translation]" for each meaning of the lexeme.
    /// </summary>
    /// <remarks>
    /// Note:  Meanings imported from Paratext have String.Empty as their Text property, which can result in a string of the form " [Translation1, Translation2]".
    /// </remarks>
    public string? Meanings => _lexeme.Meanings.Select(m => $"{m.Text} [{m.Translations.Select(t => t.Text).ToDelimitedString()}]" ).ToDelimitedString();

    public string? Forms => _lexeme.Forms.Select(f => f.Text).ToDelimitedString();
}