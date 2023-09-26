using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon
{
    public interface ILexiconObtainable
    {
        DataAccessLayer.Models.Lexicon_Lexicon GetLexicon(string projectId);
    }
}
