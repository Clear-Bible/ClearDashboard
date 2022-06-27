using MediatR;

using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Aligner.Translation;
using static ClearBible.Alignment.DataServices.Translation.ITranslationCommandable;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Alignment.DataServices.Features.Corpora;

using SIL.Machine.Corpora;
using SIL.Machine.Translation;
using SIL.Machine.Translation.Thot;
using SIL.Machine.Utils;


namespace ClearBible.Alignment.DataServices.Translation
{
    public class TranslationCommands : ITranslationCommandable
    {
        private readonly IMediator mediator_;

        public TranslationCommands(IMediator mediator)
        {
            mediator_ = mediator;
        }

        public IEnumerable<(Token sourceToken, Token targetToken, double score)> PredictAllAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextCorpus parallelCorpus)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(Token sourceToken, Token targetToken, double score)> PredictParallelMappedVersesAlignedTokenIdPairs(
            IWordAligner wordAligner, 
            EngineParallelTextRow parallelMappedVerses)
        {
            if (wordAligner is ISyntaxTreeWordAligner)
            {
                var syntaxTreeOrdinalAlignedWordPairs = ((ISyntaxTreeWordAligner) wordAligner).GetBestAlignmentAlignedWordPairs(parallelMappedVerses);
                return parallelMappedVerses.GetAlignedTokenIdPairs(syntaxTreeOrdinalAlignedWordPairs);
            }
            else
            {
                var smtOrdinalAlignments =  wordAligner.GetBestAlignment(parallelMappedVerses.SourceSegment, parallelMappedVerses.TargetSegment);
                return   parallelMappedVerses.GetAlignedTokenIdPairs(smtOrdinalAlignments);
            }
        }

        public async Task PutAlignments(Corpora.ParallelCorpusId parallelCorpusId, IEnumerable<(Token, Token, double)> sourceTokenToTargetTokenAlignments)
        {
            var result = await mediator_.Send(new PutAlignmentsCommand(sourceTokenToTargetTokenAlignments, parallelCorpusId));
            if (!result.Success)
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }

        public async Task<IWordAlignmentModel> TrainSmtModel(
            SmtModelType smtModelType, 
            EngineParallelTextCorpus parallelCorpus, 
            IProgress<ProgressStatus>? progress = null, 
            SymmetrizationHeuristic? symmetrizationHeuristic = null)
        {
            if (symmetrizationHeuristic != null)
            {
                if (smtModelType == SmtModelType.FastAlign)
                {
                    var srcTrgModel = new ThotFastAlignWordAlignmentModel();
                    var trgSrcModel = new ThotFastAlignWordAlignmentModel();

                    var symmetrizedModel = new SymmetrizedWordAlignmentModel(srcTrgModel, trgSrcModel)
                    {
                        Heuristic = symmetrizationHeuristic ?? SymmetrizationHeuristic.None // should never be null
                    };

                    using var trainer = symmetrizedModel.CreateTrainer(parallelCorpus.Lowercase());
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    return symmetrizedModel;
                }
                else if (smtModelType == SmtModelType.Hmm)
                {
                    throw new NotImplementedException();
                }
                else if (smtModelType == SmtModelType.IBM4)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new InvalidConfigurationEngineException(message: "Selected smt model type is not implemented.");
                }
            }
            else
            {
                if (smtModelType == SmtModelType.FastAlign)
                {
                    throw new NotImplementedException();
                }
                else if (smtModelType == SmtModelType.Hmm)
                {
                    throw new NotImplementedException();
                }
                else if (smtModelType == SmtModelType.IBM4)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new InvalidConfigurationEngineException(message: "Selected smt model type is not implemented.");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parallelCorpus"></param>
        /// <param name="smtTrainedWordAlignmentModel"></param>
        /// <param name="hyperparameters">For now just pass in new SyntaxTreeWordAlignerHyperparameters() and optionally set 
        /// ApprovedAlignedTokenIdPairs property. </param>
        /// <param name="progress"></param>
        /// <param name="syntaxTreesPath"></param>
        /// <returns></returns>
        public async Task<SyntaxTreeWordAlignmentModel> TrainSyntaxTreeModel(
            EngineParallelTextCorpus parallelCorpus, 
            IWordAlignmentModel smtTrainedWordAlignmentModel, 
            SyntaxTreeWordAlignerHyperparameters hyperparameters,
            string syntaxTreesPath,
            IProgress<ProgressStatus>? progress = null)
        {
            var manuscriptTree = new SyntaxTrees(syntaxTreesPath);

            // create the manuscript word aligner. Engine's main implementation is specifically a tree-based aligner.
            ISyntaxTreeTrainableWordAligner syntaxTreeTrainableWordAligner = new SyntaxTreeWordAligner(
                new List<IWordAlignmentModel>() { smtTrainedWordAlignmentModel },
                0,
                hyperparameters,
                manuscriptTree);

            // initialize a manuscript word alignment model. At this point it has not yet been trained.
            var syntaxTreeWordAlignmentModel = new SyntaxTreeWordAlignmentModel(syntaxTreeTrainableWordAligner);
            using var manuscriptTrainer = syntaxTreeWordAlignmentModel.CreateTrainer(parallelCorpus);

            // Trains the manuscriptmodel using the pre-trained SMT model(s)
            manuscriptTrainer.Train(progress);
            await manuscriptTrainer.SaveAsync();

            return syntaxTreeWordAlignmentModel;
        }
    }
}
