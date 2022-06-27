using ClearBible.Engine.Corpora;
using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Engine.SyntaxTree.Aligner.Translation;

using SIL.Machine.Translation;
using SIL.Machine.Utils;

namespace ClearBible.Alignment.DataServices.Translation
{
    public interface ITranslationCommandable
    {
        public enum SmtModelType
        {
            FastAlign,
            IBM4,
            Hmm
        }

        /// <summary>
        /// Used to obtain a trained SMT model from which either alignments can be obtained, or subsequently used to build a syntax tree model from which 
        /// alignments can be obtained
        /// when the source corpus is a syntax trees corpus (e.g. manuscript syntax trees).
        /// </summary>
        /// <param name="smtModeltype"></param>
        /// <param name="parallelCorpus"></param>
        /// <param name="progress"></param>
        /// <param name="symmetrizationHeuristic">If null, don't symmetrize.</param>
        /// <returns></returns>
        Task<IWordAlignmentModel> TrainSmtModel(
            SmtModelType smtModelType,
            EngineParallelTextCorpus parallelCorpus,
            IProgress<ProgressStatus>? progress = null,
            SymmetrizationHeuristic? symmetrizationHeuristic = null);

        /// <summary>
        /// Used to obtain a trained syntax tree model from which alignments can be obtained when the source corpus is the same syntax tree corpus as identified in syntaxTreePath
        /// parameter and the smtTrainedWordAlignmentModel provided was trained on the same parallel corpus.
        /// 
        /// Implementation node: check that the source corpus is SyntaxTreeFileTextCorpus.
        /// </summary>
        /// <param name="parallelCorpus"></param>
        /// <param name="smtTrainedWordAlignmentModel"></param>
        /// <param name="progress"></param>
        /// <param name="syntaxTreesPath"></param>
        /// <param name="fileGetSyntaxTreeWordAlignerHyperparametersLocation"></param>
        /// <returns></returns>
        Task<SyntaxTreeWordAlignmentModel> TrainSyntaxTreeModel(
            EngineParallelTextCorpus parallelCorpus,
            IWordAlignmentModel smtTrainedWordAlignmentModel,
            SyntaxTreeWordAlignerHyperparameters hyperparameters,
            string syntaxTreesPath,
            IProgress<ProgressStatus>? progress = null);

        /// <summary>
        /// Used to predict the alignments for all engine parallel verses.
        /// 
        /// Implementation Node: use ISyntaxTreeWordAligner.GetBestAlignmentAlignedWordPairs() if is ISyntaxTreeWordAligner else IWordAligner.GetBestAlignment()
        /// </summary>
        /// <param name="wordAligner"></param>
        /// <param name="parallelCorpus"></param>
        /// <returns></returns>
        IEnumerable<(Token sourceToken, Token targetToken, double score)> PredictAllAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextCorpus parallelCorpus);

        /// <summary>
        /// Used to predict the alignments for a specific engine paralel verses (i.e. a single pair of verses or grouping of verses). 
        /// </summary>
        /// <param name="wordAligner"></param>
        /// <param name="engineParallelVerses"></param>
        /// <returns></returns>
        IEnumerable<(Token sourceToken, Token targetToken, double score)> PredictParallelMappedVersesAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextRow parallelMappedVerses);

        /* IMPLEMENTER'S NOTES:
         * mediator's result.Data is ignored. Marked as object in Command to accommodate compilation needs of RequestResult only.
         * 
         */
        /// <summary>
        /// Puts alignments into the DB
        /// </summary>
        /// <param name="engineParallelTextCorpusId"></param>
        /// <param name="alignments"></param>
        Task PutAlignments(ParallelCorpusId parallelCorpusId, IEnumerable<(Token, Token, double)> alignments);
    }
}
