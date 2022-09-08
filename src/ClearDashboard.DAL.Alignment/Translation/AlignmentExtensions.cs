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
            this IEnumerable<Alignment> alignments, 
                string? displayName,
                string smtModel,
                bool isSyntaxTreeAlignerRefined,
                Dictionary<string, object> metadata,
                ParallelCorpusId parallelCorpusId, 
                IMediator mediator)
        {
            var createTranslationSetCommandResult = await mediator.Send(new CreateAlignmentSetCommand(
                alignments,
                displayName,
                smtModel,
                isSyntaxTreeAlignerRefined,
                metadata,
                parallelCorpusId));

            if (createTranslationSetCommandResult.Success && createTranslationSetCommandResult.Data != null)
            {
                return createTranslationSetCommandResult.Data;
            }
            else
            {
                throw new MediatorErrorEngineException(createTranslationSetCommandResult.Message);
            }
        }
    }
}
