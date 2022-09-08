using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class PaddedTokenTranslation
    {
        public Token Token { get; set; }
        public string PaddingBefore { get; set; }
        public string PaddingAfter { get; set; }
        public Translation Translation { get; set; }
    }
}
