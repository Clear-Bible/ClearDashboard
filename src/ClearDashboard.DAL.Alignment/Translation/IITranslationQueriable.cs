using ClearBible.Engine.Corpora;
using ClearBible.Alignment.DataServices.Corpora;

namespace ClearBible.Alignment.DataServices.Translation
{
    public interface IITranslationQueriable
    {
        /// <summary>
        /// Gets alignments from the DB
        /// </summary>
        /// <param name="engineParallelTextCorpusId"></param>
        /// <returns></returns>
        Task<IEnumerable<(Token, Token, double)>?> GetAlignemnts(ParallelCorpusId parallelCorpusId);
    }
}
