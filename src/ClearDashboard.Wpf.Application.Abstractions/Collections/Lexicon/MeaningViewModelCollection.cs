using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    public sealed class MeaningViewModelCollection : BindableCollection<MeaningViewModel>
    {
        public MeaningViewModelCollection()
        {
        }

        public MeaningViewModelCollection(IEnumerable<Meaning> meanings)
        {
            AddRange(meanings.Select(m => new MeaningViewModel(m)));
        }

        public MeaningViewModelCollection(IEnumerable<MeaningViewModel> meanings) : base(meanings)
        {
        }
    }
}
