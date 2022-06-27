using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearBible.Alignment.DataServices.Corpora
{
    public static class ParallelTextCorpusExtensions
    {
        /*
        public static async Task<ParallelTokenizedCorpus> Update(
            this ParallelTokenizedCorpus parallelTokenizedCorpus, 
            IMediator mediator)
        {
            var sourceCorpusIdVersionId = (CorpusVersionId) ((TextCorpus < GetTokensByTokenizedCorpusIdAndBookIdQuery> ) parallelTokenizedCorpus.SourceCorpus).Id;
            var targetCorpusIdVersionId = (CorpusVersionId) ((TextCorpus < GetTokensByTokenizedCorpusIdAndBookIdQuery > ) parallelTokenizedCorpus.TargetCorpus).Id;
            var engineVerseMappingList = parallelTokenizedCorpus.EngineVerseMappingList ?? throw new InvalidStateEngineException(name:"EngineVerseMappingList", value: "null");

            var command = new UpdateParallelCorpusInfoCommand(sourceCorpusIdVersionId, targetCorpusIdVersionId, engineVerseMappingList, parallelTokenizedCorpus.ParallelCorpusIdVersionId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var info = result.Data;
                return new ParallelCorpusFromDb(
                    await TokenizedTextCorpus.Get(mediator, info.sourceCorpusIdVersionId),
                    await TokenizedTextCorpus.Get(mediator, info.sourceCorpusIdVersionId),
                    info.engineVerseMappings, 
                    info.parallelCorpusIdVersionId);
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
        */

        /// <summary>
        /// If parallelCorpusVersionId and parallelCorpusId are null, first create parallelCorpusId then parallelCorpusVersionId and then parallelTokenizedCorpus
        /// else if parallelCorpusVersionId is null (but parallelCorpusId is not), first create parallelCorpusVersionId then parallelTokenizedCorpus.
        /// else create parallelTokenizedCorpus
        /// 
        /// </summary>
        /// <param name="engineParallelTextCorpus"></param>
        /// <param name="mediator"></param>
        /// <param name="parallelCorpusVersionId"></param>
        /// <param name="sourceCorpusId"></param>
        /// <param name="targetCorpusId"></param>
        /// <param name="parallelCorpusId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidTypeEngineException"></exception>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<ParallelTokenizedCorpus> Create(
            this EngineParallelTextCorpus engineParallelTextCorpus,
            IMediator mediator,
            ParallelCorpusVersionId? parallelCorpusVersionId = null,
            ParallelCorpusId? parallelCorpusId = null)
        {
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


            if (engineParallelTextCorpus.GetType() == typeof(ParallelTokenizedCorpus))
            {
                parallelCorpusVersionId = ((ParallelTokenizedCorpus)engineParallelTextCorpus).ParallelCorpusVersionId;
                parallelCorpusId = ((ParallelTokenizedCorpus)engineParallelTextCorpus).ParallelCorpusId;
            }
            else if (parallelCorpusVersionId == null)
            {
                if (parallelCorpusId == null)
                {
                    var createParallelCorpusCommand = new CreateParallelCorpusCommand();
                    var createParallelCorpusCommandResult = await mediator.Send(createParallelCorpusCommand);
                    if (createParallelCorpusCommandResult.Success)
                    {
                        parallelCorpusId = createParallelCorpusCommandResult.Data;
                    }
                    else
                    {
                        throw new MediatorErrorEngineException(createParallelCorpusCommandResult.Message);
                    }
                }
                var createParallelCorpusVersionCommand = new CreateParallelCorpusVersionCommand(
                    parallelCorpusId ?? throw new InvalidStateEngineException(name: "parallelCorpusId", value: "null"),
                    engineParallelTextCorpus);
                
                var createParallelCorpusVersionCommandResult = await mediator.Send(createParallelCorpusVersionCommand);
                if (createParallelCorpusVersionCommandResult.Success)
                {
                    parallelCorpusVersionId = createParallelCorpusVersionCommandResult.Data;
                }
                else
                {
                    throw new MediatorErrorEngineException(createParallelCorpusVersionCommandResult.Message);
                }
            }

            var command = new CreateParallelTokenizedCorpusCommand(
                parallelCorpusVersionId ?? throw new InvalidStateEngineException(name: "parallelCorpusVersionId", value: "null"),
                ((TokenizedTextCorpus)engineParallelTextCorpus.SourceCorpus).TokenizedCorpusId,
                ((TokenizedTextCorpus)engineParallelTextCorpus.TargetCorpus).TokenizedCorpusId);

            var result = await mediator.Send(command);
            if (result.Success)
            {
                var parallelTokenizedCorpusId = result.Data;
                return new ParallelTokenizedCorpus(
                    (TokenizedTextCorpus) engineParallelTextCorpus.SourceCorpus,
                    (TokenizedTextCorpus) engineParallelTextCorpus.TargetCorpus,
                    engineParallelTextCorpus.EngineVerseMappingList ?? throw new InvalidStateEngineException(name: "engineParallelTextCorpus.EngineVerseMappingList", value: "null"),
                    parallelTokenizedCorpusId ?? throw new InvalidStateEngineException(name: "parallelCorpusVersionId", value: "null"),
                    parallelCorpusVersionId,
                    parallelCorpusId ?? throw new InvalidStateEngineException(name: "parallelCorpusVId", value: "null"));
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
