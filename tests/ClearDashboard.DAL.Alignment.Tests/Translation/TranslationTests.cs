using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.SyntaxTree.Aligner.Persistence;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Translation;
using ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers;
using ClearDashboard.DAL.Alignment.Translation;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Machine.Translation;
using SIL.Machine.Utils;
using Xunit;
using Xunit.Abstractions;
using static ClearDashboard.DAL.Alignment.Translation.ITranslationCommandable;

namespace ClearDashboard.DAL.Alignment.Tests.Translation
{
    public class TranslationTests
    {
        public static readonly string CorpusProjectPath = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "Translation", "data", "WEB-PT");
        public static readonly string HyperparametersFiles = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "hyperparametersfiles");


        private readonly ITestOutputHelper output_;
        protected readonly IMediator mediator_;

        public TranslationTests(ITestOutputHelper output)
        {
            output_ = output;
            mediator_ = new MediatorMock(); //FIXME: inject mediator
        }
        [Fact]
        [Trait("Category", "Example")]
        public async Task Translation__SyntaxTreeAlignment()
        {
            try
            {
                var syntaxTree = new SyntaxTrees();
                var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree);

                var targetCorpus = new ParatextTextCorpus(CorpusProjectPath)
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();

                var parallelTextCorpus = sourceCorpus.EngineAlignRows(targetCorpus, new());

                FunctionWordTextRowProcessor.Train(parallelTextCorpus);

                parallelTextCorpus.SourceCorpus = parallelTextCorpus.SourceCorpus
                    .Filter<FunctionWordTextRowProcessor>();

                {
                    var translationCommandable = new TranslationCommands(mediator_);

                    using var smtWordAlignmentModel = await translationCommandable.TrainSmtModel(
                        SmtModelType.FastAlign,
                        parallelTextCorpus,
                        new DelegateProgress(status =>
                            output_.WriteLine($"Training symmetrized Fastalign model: {status.PercentCompleted:P}")),
                        SymmetrizationHeuristic.GrowDiagFinalAnd);

                    // set the manuscript tree aligner hyperparameters
                    var hyperparameters = await FileGetSyntaxTreeWordAlignerHyperparams.Get().SetLocation(HyperparametersFiles).GetAsync();

                    using var syntaxTreeWordAlignmentModel = await translationCommandable.TrainSyntaxTreeModel(
                        parallelTextCorpus,
                        smtWordAlignmentModel,
                        hyperparameters,
                        new DelegateProgress(status =>
                            output_.WriteLine($"Training syntax tree alignment model: {status.PercentCompleted:P}")));

                    // now best alignments for first 5 verses.
                    foreach (var engineParallelTextRow in parallelTextCorpus.Take(5).Cast<EngineParallelTextRow>())
                    {
                        //display verse info
                        var verseRefStr = engineParallelTextRow.Ref.ToString();
                        output_.WriteLine(verseRefStr);

                        //display source
                        var sourceVerseText = string.Join(" ", engineParallelTextRow.SourceSegment);
                        output_.WriteLine($"Source: {sourceVerseText}");
                        var sourceTokenIds = string.Join(" ", engineParallelTextRow.SourceTokens?
                            .Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
                        output_.WriteLine($"SourceTokenIds: {sourceTokenIds}");

                        //display target
                        var targetVerseText = string.Join(" ", engineParallelTextRow.TargetSegment);
                        output_.WriteLine($"Target: {targetVerseText}");
                        var targetTokenIds = string.Join(" ", engineParallelTextRow.TargetTokens?
                            .Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
                        output_.WriteLine($"TargetTokenIds: {targetTokenIds}");

                        //predict primary smt aligner alignments only then display - ONLY FOR COMPARISON
                        var smtOrdinalAlignments = smtWordAlignmentModel.GetBestAlignment(engineParallelTextRow.SourceSegment, engineParallelTextRow.TargetSegment);
                        IEnumerable<AlignedTokenPairs> smtSourceTargetTokenIdPairs = engineParallelTextRow.GetAlignedTokenPairs(smtOrdinalAlignments);
                            // (Legacy): Alignments as ordinal positions in versesmap
                        output_.WriteLine($"SMT Alignment        : {smtOrdinalAlignments}");
                            // Alignments as source token to target token pairs
                        output_.WriteLine($"SMT Alignment        : {string.Join(" ", smtSourceTargetTokenIdPairs.Select(t => $"{t.SourceToken.TokenId}->{t.TargetToken.TokenId}"))}");


                        //predict syntax tree aligner alignments then display
                        // (Legacy): Alignments as ordinal positions in versesmap - ONLY FOR COMPARISON
                        // NOTE: uncommenting the next line doubles time for alignment to run.
                        //output_.WriteLine($"Syntax tree Alignment: {string.Join(" ", syntaxTreeWordAlignmentModel.GetBestAlignmentAlignedWordPairs(engineParallelTextRow).Select(a => a.ToString()))}");
                            // ALIGNMENTS as source token to target token pairs
                        var syntaxTreeAlignments = translationCommandable.PredictParallelMappedVersesAlignedTokenIdPairs(syntaxTreeWordAlignmentModel, engineParallelTextRow);
                        output_.WriteLine($"Syntax tree Alignment: {string.Join(" ", syntaxTreeAlignments.Select(t => $"{t.SourceToken.TokenId}->{t.TargetToken.TokenId}"))}");
                    }
                }
            }
            catch (EngineException eex)
            {
                output_.WriteLine(eex.ToString());
                throw eex;
            }
        }
    }
}