using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public interface IWordAnalysesObtainable
    {
        IEnumerable<DataAccessLayer.Models.Lexicon_WordAnalysis> GetWordAnalyses(string projectId);
    }
}
