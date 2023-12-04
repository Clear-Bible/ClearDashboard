using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Aligner.Translation;
using SIL.Machine.Translation;
using SIL.Machine.Utils;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public enum SmtModelType
    {
        FastAlign,
        IBM4,
        Hmm
    }

    public interface ITranslationCommandable
    {
        

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
        /// <param name="fileGetSyntaxTreeWordAlignerHyperparametersLocation"></param>
        /// <param name="syntaxTreesPath"></param>
        /// <returns></returns>
        Task<SyntaxTreeWordAlignmentModel> TrainSyntaxTreeModel(
            EngineParallelTextCorpus parallelCorpus,
            IWordAlignmentModel smtTrainedWordAlignmentModel,
            SyntaxTreeWordAlignerHyperparameters hyperparameters,
            IProgress<ProgressStatus>? progress = null,
            string? syntaxTreesPath = null);

        /// <summary>
        /// Used to predict the alignments for all engine parallel verses.
        /// 
        /// Implementation Node: use ISyntaxTreeWordAligner.GetBestAlignmentAlignedWordPairs() if is ISyntaxTreeWordAligner else IWordAligner.GetBestAlignment()
        /// </summary>
        /// <param name="wordAligner"></param>
        /// <param name="parallelCorpus"></param>
        /// <returns></returns>
        IEnumerable<AlignedTokenPairs> PredictAllAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextCorpus parallelCorpus);

        /// <summary>
        /// Used to predict the alignments for a specific engine paralel verses (i.e. a single pair of verses or grouping of verses). 
        /// </summary>
        /// <param name="wordAligner"></param>
        /// <param name="engineParallelVerses"></param>
        /// <returns></returns>
        IEnumerable<AlignedTokenPairs> PredictParallelMappedVersesAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextRow parallelMappedVerses);
    }
}
