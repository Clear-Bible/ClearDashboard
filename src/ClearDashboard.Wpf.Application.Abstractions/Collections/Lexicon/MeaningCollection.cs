using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    public class MeaningCollection : BindableCollection<Meaning>
    {
        public MeaningCollection()
        {
        }

        public MeaningCollection(IEnumerable<Meaning> meanings) : base(meanings)
        {
        }
    }
}
