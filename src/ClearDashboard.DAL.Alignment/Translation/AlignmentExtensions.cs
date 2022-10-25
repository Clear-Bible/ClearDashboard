using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Translation;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public static class AlignmentExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alignmentTokenPairs"></param>
        /// <param name="smtModel"></param>
        /// <param name="isSyntaxTreeAlignerRefined"></param>
        /// <param name="parallelCorpusId"></param>
        /// <param name="mediator"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<AlignmentSet> Create(
            this IEnumerable<AlignedTokenPairs> alignTokenPairs, 
                string? displayName,
                string smtModel,
                bool isSyntaxTreeAlignerRefined,
                Dictionary<string, object> metadata,
                ParallelCorpusId parallelCorpusId, 
                IMediator mediator,
                CancellationToken token = default)
        {
            return await AlignmentSet.Create(alignTokenPairs, displayName, smtModel, isSyntaxTreeAlignerRefined, metadata, parallelCorpusId, mediator, token);
        }
    }
}
