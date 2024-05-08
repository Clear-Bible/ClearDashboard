using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Translation;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearBible.Engine.SyntaxTree.Aligner.Translation;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearDashboard.DAL.Alignment.Features.Translation;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Translation;
using SIL.Machine.Translation.Thot;
using SIL.Machine.Utils;
using static ClearDashboard.DAL.Alignment.Translation.ITranslationCommandable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Caliburn.Micro;

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
			// NOTE: 
			//
			// In ThotWordAlignmentModel.cs: 
			//   public WordAlignmentMatrix GetBestAlignment(IReadOnlyList<string> sourceSegment, IReadOnlyList<string> targetSegment) CALLS
			//   Thot.swAlignModel_getBestAlignment(Handle, nativeSourceSegment, nativeTargetSegment, nativeMatrix, ref iLen, ref jLen);
			// In Thot.cc:
			//   double swAlignModel_getBestAlignment(void* swAlignModelHandle, const char* sourceSentence, const char* targetSentence,
			//                           bool** matrix, unsigned int* iLen, unsigned int* jLen) CALLS
			//   LgProb prob = alignmentModel->getBestAlignment(sourceWordIndices, targetWordIndices, waMatrix);
			// In AlignmentModelBase.cc:
			//    LgProb AlignmentModelBase::getBestAlignment(const vector<WordIndex>& srcSentence, const vector<WordIndex>& trgSentence,
			//                                WordAlignmentMatrix& bestWaMatrix) CALLS
			//    bestWaMatrix.init((PositionIndex)srcSentence.size(), (PositionIndex)trgSentence.size());
			// In WordAlignmentMatrix.cc:
			//    void WordAlignmentMatrix::init(unsigned int I_dims, unsigned int J_dims) CALLS
			//    bool* pool = new bool[(size_t)I * J]{false};
			//
			// If either the incoming sourceSegment or targetSegment in the initial call are empty, that last
			// call to "new bool[(size_t)I * J]{false};" will be invalid because either I or J will be zero, which
			// will result in the following error, which crashes the docker container:
			//
			//      terminate called after throwing an instance of 'std::bad_array_new_length'
			//        what():  std::bad_array_new_length
			//
			// This "new []" dynamic memory allocation is part of the c++ standard library and will throw the above 
			// exception under certain conditions, one of which is:  "The number of initializer-clauses exceeds the 
			// number of elements to initialize."  Notice that the call includes an initializer clause: "{false}", which
			// would exceed the number of elements to initialize if it was zero.
			//
			// As a side note, the initializer clause, I think, is unnecessary.  I would guess that using
			// "new bool[(size_t)I * J]{};" instead would have the same initializer result but not throw 
			// the std::bad_array_new_length exception for a zero sized array.  
			// 
			// We're fixing this here instead of in the thot c++ library, since we don't maintain a fork of the thot
			// source code from https://github.com/sillsdev/thot (I believe we pull an older version - 3.3.5 from nuget. 
			// the latest thot code release is 3.4.3).  Also, perhaps from thot's (or Machine's) perspective it is 
			// invalid to call swAlignModel_getBestAlignment (or GetBestAlignment) with either source or target being
			// empty.  

			if (!parallelMappedVerses.SourceSegment.Any() || !parallelMappedVerses.TargetSegment.Any())
			{
				return Enumerable.Empty<AlignedTokenPairs>();
			}

			if (wordAligner is ISyntaxTreeWordAligner)
			{
				var syntaxTreeOrdinalAlignedWordPairs = ((ISyntaxTreeWordAligner)wordAligner).GetBestAlignmentAlignedWordPairs(parallelMappedVerses);
				return parallelMappedVerses.GetAlignedTokenPairs(syntaxTreeOrdinalAlignedWordPairs);
			}
			else
			{
				var smtOrdinalAlignments = wordAligner.GetBestAlignment(parallelMappedVerses.SourceSegment, parallelMappedVerses.TargetSegment);
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
