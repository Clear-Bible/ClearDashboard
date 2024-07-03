using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class LanguageMapping
    {
        public string SourceLanguage;
        public string TargetLanguage;

        public LanguageMapping(string sourceLanguage, string targetLanguage)
        {
            SourceLanguage = sourceLanguage;
            TargetLanguage = targetLanguage;
        }

        public override string ToString()
        {
            return SourceLanguage + "-->" + TargetLanguage;
        }
    }
}
