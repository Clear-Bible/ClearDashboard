using Autofac;
using Caliburn.Micro;
using ClearApi.Command.CQRS.Commands;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public static class ParallelCorpusExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="engineParallelTextCorpus"></param>
        /// <param name="mediator"></param>
        /// <param name="sourceCorpusId"></param>
        /// <param name="targetCorpusId"></param>
        /// <param name="parallelCorpusId"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidTypeEngineException"></exception>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<ParallelCorpus> CreateAsync(
            this EngineParallelTextCorpus engineParallelTextCorpus,
            string displayName,
            IComponentContext context,
            CancellationToken token = default,
            bool useCache = false)
        {
            if (engineParallelTextCorpus.GetType() == typeof(ParallelCorpus))
            {
                throw new InvalidTypeEngineException(
                    name: "engineParallelTextCorpus",
                    value: "ParallelCorpus",
                    message: "ParallelCorpus already created");
            }

            if (
                engineParallelTextCorpus.SourceCorpus.GetType() != typeof(TokenizedTextCorpus)
                ||
                engineParallelTextCorpus.TargetCorpus.GetType() != typeof(TokenizedTextCorpus))
            {
                throw new InvalidTypeEngineException(
                    name: "sourceOrTargetCorpus",
                    value: "Not TokenizedTextCorpus",
                    message: "both SourceCorpus and TargetCorpus of engineParallelTextCorpus must be from the database (of type TokenizedTextCorpus");
            }

            var createParallelCorpusCommandResult = await new CreateParallelCorpusCommand(
                ((TokenizedTextCorpus)engineParallelTextCorpus.SourceCorpus).TokenizedTextCorpusId,
                ((TokenizedTextCorpus)engineParallelTextCorpus.TargetCorpus).TokenizedTextCorpusId,
                engineParallelTextCorpus.SourceTextIdToVerseMappings?.GetVerseMappings() ?? throw new InvalidParameterEngineException(name: "engineParallelTextCorpus.VerseMappingList", value: "null"),
                displayName)
                .ExecuteAsProjectCommandAsync(context, token);

            return await ParallelCorpus.GetAsync(context, createParallelCorpusCommandResult, token, useCache);
        }
    }
}
