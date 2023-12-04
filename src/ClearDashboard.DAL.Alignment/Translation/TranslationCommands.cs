using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Aligner.Translation;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Translation;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using SIL.Machine.Translation.Thot;
using SIL.Machine.Utils;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Translation
{
    public class TranslationCommands : ITranslationCommandable
    {
        private readonly ILogger<TranslationCommands> _logger;

        // FIXME:  remove this constructor
        public TranslationCommands()
        {
            // FIXME:  use constructor injection (i.e. the other constructor)
            // instead of relying on Caliburn Micro (the unit tests are not set
            // up to use Caliburn Micro).  
            _logger = IoC.Get<ILogger<TranslationCommands>>();
        }

        public TranslationCommands(ILogger<TranslationCommands> logger)
        {
            _logger = logger;
        }

        public IEnumerable<AlignedTokenPairs> PredictAllAlignedTokenIdPairs(IWordAligner wordAligner, EngineParallelTextCorpus parallelCorpus)
        {
            return parallelCorpus.SelectMany(row => PredictParallelMappedVersesAlignedTokenIdPairs(wordAligner, (row as EngineParallelTextRow)!));
        }

        public IEnumerable<AlignedTokenPairs> PredictParallelMappedVersesAlignedTokenIdPairs(
            IWordAligner wordAligner, 
            EngineParallelTextRow parallelMappedVerses)
        {
            if (wordAligner is ISyntaxTreeWordAligner)
            {
                var syntaxTreeOrdinalAlignedWordPairs = ((ISyntaxTreeWordAligner) wordAligner).GetBestAlignmentAlignedWordPairs(parallelMappedVerses);
                return parallelMappedVerses.GetAlignedTokenPairs(syntaxTreeOrdinalAlignedWordPairs);
            }
            else
            {
                var smtOrdinalAlignments =  wordAligner.GetBestAlignment(parallelMappedVerses.SourceSegment, parallelMappedVerses.TargetSegment);
                return parallelMappedVerses.GetAlignedTokenPairs(smtOrdinalAlignments);
            }
        }

        public async Task<IWordAlignmentModel> TrainSmtModel(
            SmtModelType smtModelType, 
            EngineParallelTextCorpus parallelCorpus, 
            IProgress<ProgressStatus>? progress = null, 
            SymmetrizationHeuristic? symmetrizationHeuristic = null)
        {
            var sw = Stopwatch.StartNew();
            sw.Start();

            if (symmetrizationHeuristic != SymmetrizationHeuristic.None)
            {
                if (smtModelType == SmtModelType.FastAlign)
                {
                    var srcTrgModel = new ThotFastAlignWordAlignmentModel();
                    var trgSrcModel = new ThotFastAlignWordAlignmentModel();

                    var symmetrizedModel = new SymmetrizedWordAlignmentModel(srcTrgModel, trgSrcModel)
                    {
                        Heuristic = symmetrizationHeuristic ?? SymmetrizationHeuristic.None // should never be null
                    };

                    using var trainer = symmetrizedModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");


                    return symmetrizedModel;
                }
                else if (smtModelType == SmtModelType.Hmm)
                {
                    var srcTrgModel = new ThotHmmWordAlignmentModel();
                    var trgSrcModel = new ThotHmmWordAlignmentModel();

                    var symmetrizedModel = new SymmetrizedWordAlignmentModel(srcTrgModel, trgSrcModel)
                    {
                        Heuristic = symmetrizationHeuristic ?? SymmetrizationHeuristic.None // should never be null
                    };

                    using var trainer = symmetrizedModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");

                    return symmetrizedModel;
                }
                else if (smtModelType == SmtModelType.IBM4)
                {
                    var srcTrgModel = new ThotIbm4WordAlignmentModel();
                    var trgSrcModel = new ThotIbm4WordAlignmentModel();

                    var symmetrizedModel = new SymmetrizedWordAlignmentModel(srcTrgModel, trgSrcModel)
                    {
                        Heuristic = symmetrizationHeuristic ?? SymmetrizationHeuristic.None // should never be null
                    };

                    using var trainer = symmetrizedModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");


                    return symmetrizedModel;
                }
                else
                {
                    sw.Stop();
                    throw new InvalidConfigurationEngineException(message: "Selected smt model type is not implemented.");
                }
            }
            else
            {
                if (smtModelType == SmtModelType.FastAlign)
                {
                    var srcTrgModel = new ThotFastAlignWordAlignmentModel();

                    using var trainer = srcTrgModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");

                    return srcTrgModel;
                }
                else if (smtModelType == SmtModelType.Hmm)
                {
                    var srcTrgModel = new ThotHmmWordAlignmentModel();

                    using var trainer = srcTrgModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");

                    return srcTrgModel;
                }
                else if (smtModelType == SmtModelType.IBM4)
                {
                    var srcTrgModel = new ThotIbm4WordAlignmentModel();

                    using var trainer = srcTrgModel.CreateTrainer(parallelCorpus);
                    trainer.Train(progress);
                    await trainer.SaveAsync();

                    sw.Stop();
                    _logger.LogInformation(
                        $"Ran SMT [{smtModelType}] with SymmetrizationHeuristic: [{symmetrizationHeuristic}] in {sw.ElapsedMilliseconds.ToString()} ms");


                    return srcTrgModel;
                }
                else
                {
                    sw.Stop();
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
            IProgress<ProgressStatus>? progress = null,
            string? syntaxTreesPath = null
            )
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
