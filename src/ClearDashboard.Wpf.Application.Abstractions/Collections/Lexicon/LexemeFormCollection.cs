using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Lexicon;

namespace ClearDashboard.Wpf.Application.Collections.Lexicon
{
    public class LexemeFormCollection : BindableCollection<Form>
    {
        public LexemeFormCollection()
        {
        }

        public LexemeFormCollection(IEnumerable<Form> forms) : base(forms)
        {
        }
    }
}
