using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public interface IAquaManager
    {
        public const string AquaRequestCorpusAnalysis = "AquaRequestCorpusAnalysis";
        public const string AquaGetCorpusAnalysis = "AquaGetCorpusAnalysis";
        public const string AquaAddLatestCorpusAnalysisToCurrentEnhancedView = "AquaAddLatestCorpusAnalysisToCurrentEnhancedView";
        public Task AddCorpusAnalysisToEnhancedView();
        public Task RequestCorpusAnalysis(string paratextProjectId, CancellationToken cancellationToken);

        public Task GetCorpusAnalysis(string paratextProjectId, CancellationToken cancellationToken);
    }
}
