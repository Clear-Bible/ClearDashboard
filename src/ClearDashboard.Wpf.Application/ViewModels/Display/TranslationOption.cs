using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class TranslationOption
    {
        public string Word { get; set; }
        public double Probability { get; set; }

        public string ProbabilityPercentage => (Probability / 100).ToString("P");
    }
}
