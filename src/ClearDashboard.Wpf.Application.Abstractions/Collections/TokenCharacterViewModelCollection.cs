using Caliburn.Micro;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class TokenCharacterViewModelCollection : BindableCollection<TokenCharacterViewModel>
    {
        public TokenCharacterViewModelCollection(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                Add(new TokenCharacterViewModel {Character = text[i], Index = i });
            }
        }
    }
}
