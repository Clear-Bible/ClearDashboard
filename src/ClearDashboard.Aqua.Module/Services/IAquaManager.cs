using ClearDashboard.DAL.Alignment.Corpora;
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
        public const string Status_Finished = "finished";

        //Version endpoint

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
            bool machineTranslation = false
        );

        public Task<Version?> GetVersion(
            int id,
            CancellationToken cancellationToken = default);
        public Task<Version?> AddVersion(
            Version version,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Version>?> ListVersions(
            CancellationToken cancellationToken = default);
        
        // to obtain valid values for isoLanguage and isoString
        public record Language(string iso693, string name);
        public Task<IEnumerable<Language>?> ListLanguages(
            CancellationToken cancellationToken = default);

        public record Script(string iso15924, string name);
        public Task<IEnumerable<Script>?> ListScripts(
            CancellationToken cancellationToken = default);

        public Task DeleteVersion(
            int id,
            CancellationToken cancellationToken = default);



        // Revision endpoint
        public record Revision(
            int? id, 
            int? version_id,
            string? name,
            string? date,
            bool published = false);

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
            int versionId,
            CancellationToken cancellationToken = default);
        public Task DeleteRevision(
            int revisionId,
            CancellationToken cancellationToken = default);



        // Assessment endpoint


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision_id"></param>
        /// <param name="reference_id"></param>
        /// <param name="type">Valid types: "word-alignment", "sentence-length", "semantic-similarity", "missing-words", and "dummy"</param>
        /// <param name="modal_suffix"></param>
        /// <param name="requested_time"></param>
        /// <param name="start_time"></param>
        /// <param name="end_time"></param>
        /// <param name="status"></param>
        public record Assessment(
            int? id, 
            int? revision_id, 
            int? reference_id,
            string? type, 
            string? modal_suffix,
            string? requested_time,
            string? start_time,
            string? end_time,
            string? status //used in Create
            ); 

        public Task<Assessment?> AddAssessment(
            Assessment assessment,
            CancellationToken cancellationToken = default);
        public Task<IEnumerable<Assessment>?> ListAssessments(
            int revisionId, 
            CancellationToken cancellationToken = default);

        public Task<IEnumerable<Assessment>?> GetAssessment(
            int assessmentId,
        CancellationToken cancellationToken = default);

        public Task DeleteAssessment(
            int assessmentId,
            CancellationToken cancellationToken = default);


        // Result endpoint
        public record Result(
            int? id,
            int? assessment_id,
            string? vref,
            string? source,
            string? target,
            double? score,
            bool? flag,
            string? type,
            string? note);

        public Task<IEnumerable<Result>?> ListResults(
            int assessmentId,
            CancellationToken cancellationToken = default );

    }
}
