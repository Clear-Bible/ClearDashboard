using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Aqua.Module.Services
{
    public interface IAquaManager
    {
        public const string AquaDialogMenuId = "AquaDialogMenuId";

        //public const string AquaAddToCurrentEnhancedView = "AquaAddToCurrentEnhancedView";
        public Task AddCorpusAnalysisToEnhancedView();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paratextProjectId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns>VersionId</returns>
        public Task<string> AddVersion(
            string paratextProjectId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress);
        public Task<string> AddRevision(
            string versionId, 
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress);

        public Task<IEnumerable<string>> GetRevisions(
            string versionId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress);


        public Task<IEnumerable<string>> GetAssessmentStatuses(
            string revisionId, 
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress);

        public Task<string> GetAssessmentResult(
            string assessmentId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress);

    }
}
