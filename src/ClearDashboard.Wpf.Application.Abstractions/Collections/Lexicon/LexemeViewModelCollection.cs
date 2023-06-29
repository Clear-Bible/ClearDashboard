using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    public sealed class LexemeViewModelCollection : BindableCollection<LexemeViewModel>
    {
        public LexemeViewModelCollection()
        {
        }

        public LexemeViewModelCollection(IEnumerable<Lexeme> lexemes)
        {
            AddRange(lexemes.Select(l => new LexemeViewModel(l)));
        }

        public LexemeViewModelCollection(IEnumerable<LexemeViewModel> lexemeViewModels) : base(lexemeViewModels)
        {
        }

        public LexemeViewModel? GetLexemeWithTranslation(TranslationId translationId)
        {
            foreach (var lexeme in this)
            {
                foreach (var meaning in lexeme.Meanings)
                {
                    var match = meaning.Translations.FirstOrDefault(t => t.TranslationId == translationId);
                    if (match != null)
                    {
                        return lexeme;
                    }
                }
            }

            return null;
        }
    }
}
