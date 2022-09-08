using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public interface IITranslationQueriable
    {
        /// <summary>
        /// Gets alignments from the DB
        /// </summary>
        /// <param name="engineParallelTextCorpusId"></param>
        /// <returns></returns>
        Task<IEnumerable<(Token, Token, double)>?> GetAlignments(ParallelCorpusId parallelCorpusId);
    }
}
