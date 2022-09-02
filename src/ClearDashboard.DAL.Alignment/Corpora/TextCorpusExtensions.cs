using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using MediatR;
using SIL.Machine.Corpora;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public static class TextCorpusExtensions
    {
        /// <summary>
        /// Creates a new Corpus from a non-TokenizedTextCorpus, associated CorpusVersion,  a new associated TokenizedCorpus, and all the tokens within the corpus. 
        /// </summary>
        /// <param name="textCorpus">textCorpus must already be tokenized and not of type TokenizedTextCorpus. This is done in this fashion:
        ///     textCorpus
        ///         .Tokenize<LatinWordTokenizer>()
        ///         .Transform<IntoTokensTextRowProcessor>()
        /// </param>
        /// <param name="mediator"></param>
        /// <param name="corpusId"></param>
        /// <param name="tokenizationFunction"></param>
        /// <returns>TokenizedTextCorpus</returns>
        /// <exception cref="InvalidTypeEngineException">textCorpus enumerable is not castable to a TokensTextRow type, or textCorpus is of type TokenizedTextCorpus</exception>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public static async Task<TokenizedTextCorpus> Create(this ITextCorpus textCorpus, IMediator mediator, CorpusId corpusId, string friendlyName, string tokenizationFunction, CancellationToken token = default)
        {
            try
            {
                textCorpus.Cast<TokensTextRow>();
            }
            catch (InvalidCastException)
            {
                throw new InvalidTypeEngineException(message: $"Corpus must be tokenized and transformed into TokensTextRows, e.g. corpus.Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");
            }

            var command = new CreateTokenizedCorpusFromTextCorpusCommand(textCorpus, corpusId, friendlyName, tokenizationFunction);

            var result = await mediator.Send(command, token);
            if (result.Success)
            {
                return result.Data ?? throw new MediatorErrorEngineException(message: "result data is null");
            }
            else
            {
                throw new MediatorErrorEngineException(result.Message);
            }
        }
    }
}
