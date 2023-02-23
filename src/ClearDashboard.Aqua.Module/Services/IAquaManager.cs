using ClearDashboard.DAL.Alignment.Corpora;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Aqua.Module.Services
{
    public interface IAquaManager
    {
        public const string AquaDialogMenuId = "AquaDialogMenuId";

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
            int? id,
            string? name,
            string? isoLanguage,
            string? isoScript,
            string? abbreviation,
            string? rights = null,
            int? forwardTranslationToVersionId = null,
            int? backTranslationToVersionId = null,
            bool machineTranslation = false,
            string? language = null
        );

        public Task<Version?> GetVersion(
            int id,
            CancellationToken cancellationToken = default);
        public Task<Version?> AddVersion(
            Version version,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Version>?> ListVersions(
            CancellationToken cancellationToken = default);
        
        public record Language(string iso693, string name);
        public Task<IEnumerable<Language>?> ListLanguages(
            CancellationToken cancellationToken = default);

        public record Script(string iso15924, string name);
        public Task<IEnumerable<Script>?> ListScripts(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionId">version.abbreviation</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteVersion(
            string abbreviation,
            CancellationToken cancellationToken = default);

        public record Revision(
            int? id, 
            string? version_abbreviation,
            string? name,
            bool published = false)

        {
            [JsonPropertyName("Revision ID")]
            public int? RevisionId { get; set; }
        };

        public Task<Revision?> AddRevision(
            TokenizedTextCorpusId tokenizedTextCorpusId,
            Revision revision, 
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
            int? id, 
            int? revision, 
            int? reference,
            string? type, 
            string? requested_time,
            string? start_time,
            string? end_time,
            //string? metric,
            string? status //used in Create
            ); // additional properties provided in List
        //{
        //    public static class Type
        //    {
        //        public const string WordAlignment = "word-alignment";
        //        public const string SentenceLength = "sentence-length";
        //        public const string SemanticSimilarity = "semantic-similarity";
        //        public const string Dummy = "dummy";
        //    }
        //}
        public Task<Assessment> AddAssessment(
            Assessment assessment,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Assessment>?> ListAssessments(
            int revisionId, 
            CancellationToken cancellationToken = default);
        public Task DeleteAssessment(
            int assessmentId,
            CancellationToken cancellationToken = default);


        public record Result(string revision, string Book, string Chapter, IEnumerable<Verse> verses);
        public record Verse (string vref,string text, IEnumerable<AssessmentResult> assessment_results);
        public record AssessmentResult(string id, string? type, string? reference, string? score, string? flag, string? note);
        public Task<Result?> GetResult(
            int assessmentId,
            string bookAbbreviation,
            int chapterNumber,
            CancellationToken cancellationToken = default );

    }
}
