using ClearDashboard.DAL.Alignment.Corpora;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Sample.Module.Services
{
    public interface ISampleManager
    {
        public const string SampleDialogMenuId = "SampleDialogMenuId";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isoLanguage"></param>
        /// <param name="isoScript"></param>
        /// <param name="abbreviation"></param>
        /// <param name="rights"></param>
        /// <param name="forwardTranslationToVersionId">links a version to another 
        /// version in our database, where the current version is a forwardTranslation 
        /// of another version (e.g., a Biblica translation that is a forward translation of the NIV could link to the NIV). 
        /// Optional.</param>
        /// <param name="backTranslationToVersionId">identifies this version as a 
        /// backtranslation of another version in our database.</param>
        public record Version(
            string name,
            string isoLanguage,
            string isoScript,
            string abbreviation,
            string? rights = null,
            int? forwardTranslationToVersionId = null,
            int? backTranslationToVersionId = null,
            bool machineTranslation = false
        );
        public Task<string> AddVersion(
            TokenizedTextCorpusId tokenizedTextCorpusId,
            Version versionInfo,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Version>?> ListVersions(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionId">version.abbreviation</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteVersion(
            string versionId,
            CancellationToken cancellationToken = default);

        public record Revision(string info);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizedTextCorpusId"></param>
        /// <param name="versionId">version.abbreviation</param>
        /// <param name="cancellationToken"></param>
        /// <param name="progressReporter"></param>
        /// <returns></returns>
        public Task<string> AddRevision(
            TokenizedTextCorpusId tokenizedTextCorpusId,
            string versionId,
            CancellationToken cancellationToken = default,
            IProgress<ProgressStatus>? progressReporter = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionId">version.abbreviation</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IEnumerable<Revision>?> ListRevisions(
            string versionId,
            CancellationToken cancellationToken = default);
        public Task DeleteRevision(
            int revisionId,
            CancellationToken cancellationToken = default);

        public record Assessment(
            string id, string? type, string? Revision, string? Version, string? status, //used in Create
            string? reference, string? metric) // additional properties provided in List
        {
            public static class Type
            {
                public const string WordAlignment = "word-alignment";
                public const string SentenceLength = "sentence-length";
                public const string SemanticSimilarity = "semantic-similarity";
                public const string Dummy = "dummy";
            }
        }
        public Task<string> AddAssessment(
            Assessment assessment,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Assessment>?> ListAssessments(
            //string revisionId, 
            CancellationToken cancellationToken = default);
        public Task DeleteAssessment(
            int assessmentId,
            CancellationToken cancellationToken = default);


        public record Result(string revision, string Book, string Chapter, IEnumerable<Verse> verses);
        public record Verse(string vref, string text, IEnumerable<AssessmentResult> assessment_results);
        public record AssessmentResult(string id, string? type, string? reference, string? score, string? flag, string? note);
        public Task<Result?> GetResult(
            int assessmentId,
            string bookAbbreviation,
            int chapterNumber,
            CancellationToken cancellationToken = default);

    }
}
