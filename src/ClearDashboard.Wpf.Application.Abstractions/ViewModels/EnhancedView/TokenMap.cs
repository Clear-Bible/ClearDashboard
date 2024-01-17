using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// Manages a sequence of <see cref="Token"/>s for display, adjusting for <see cref="CompositeToken"/>s
    /// as necessary and incorporating padding values determined by an <see cref="EngineStringDetokenizer"/>.
    /// </summary>
    public class TokenMap : PropertyChangedBase
    {
        private readonly TokenCollection _tokens = new();
        /// <summary>
        /// Gets or sets the original set of tokens.
        /// </summary>
        /// <remarks>
        /// Some of the tokens in the sequence may be <see cref="CompositeToken"/> instances, which aggregate
        /// two or more tokens into a single unit.  The <see cref="FlattenedTokens"/> collection contains a
        /// sequence of tokens with any CompositeTokens replaced by their constituents.
        ///
        /// The <see cref="PaddedTokens"/> collection contains a sequence of flattened tokens suitable for
        /// display, with padding values determined by the associated detokenizer.
        /// </remarks>
        private TokenCollection Tokens
        {
            get => _tokens;
            init
            {
                Set(ref _tokens, value);
                FlattenedTokens = new TokenCollection(Tokens.GetPositionalSortedBaseTokens());
            }
        }

        private TokenCollection _flattenedTokens = new();
        /// <summary>
        /// Gets or sets a flattened sequence of tokens.
        /// </summary>
        /// <remarks>
        /// If any of the tokens in the original set are <see cref="CompositeToken"/>s, then this collection
        /// has those tokens replaced with their constituent tokens.
        ///
        /// The <see cref="PaddedTokens"/> collection contains a sequence of flattened tokens suitable for
        /// display, with padding values determined by the associated detokenizer.
        /// </remarks>
        private TokenCollection FlattenedTokens
        {
            get => _flattenedTokens;
            set
            {
                Set(ref _flattenedTokens, value);
                PaddedTokens = new PaddedTokenCollection(Detokenizer.Detokenize(FlattenedTokens));
            }
        }

        /// <summary>
        /// Gets the <see cref="EngineStringDetokenizer"/> to be used for detokenizing the tokens.
        /// </summary>
        private EngineStringDetokenizer Detokenizer => Corpus?.TokenizedTextCorpusId.Detokenizer;

        private PaddedTokenCollection _paddedTokens = new();
        /// <summary>
        /// Gets a sequence of flattened tokens suitable for display, with padding values determined
        /// by the associated detokenizer.
        /// </summary>
        public PaddedTokenCollection PaddedTokens
        {
            get => _paddedTokens;
            private set => Set(ref _paddedTokens, value);
        }

        /// <summary>
        /// Gets the <see cref="TokenId"/>s of all the tokens in the map.
        /// </summary>
        public IEnumerable<TokenId> TokenIds => Tokens.TokenIds;

        /// <summary>
        /// Gets the <see cref="TokenizedTextCorpus"/> that the tokens belong to.
        /// </summary>
        public TokenizedTextCorpus Corpus { get; }

        /// <summary>
        /// Gets whether these tokens should be displayed left-to-right (LTR) or right-to-left (RTL).
        /// </summary>
        /// <remarks>
        /// True if the tokens should be displayed RTL; false otherwise.
        /// </remarks>
        public bool IsRtl => Corpus.TokenizedTextCorpusId.CorpusId != null && Corpus.TokenizedTextCorpusId.CorpusId.IsRtl;

        /// <summary>
        /// Gets the <see cref="CompositeToken"/> that a token is a constituent of, if any.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> to search for.</param>
        /// <returns>The parent <see cref="CompositeToken"/>, if any; null otherwise.</returns>
        public CompositeToken? GetCompositeToken(Token token)
        {
            // TODO808: 
            // 1. a Composite(parallel) that overlaps with Composite(null) in parallel view will hide Composite(null) (but show its child tokens).
            //
            // The GetCompositeToken() call should prioritize Composite(parallel) over Composite(null).  Requires a property on CompositeToken to indicate whether
            // it is parallel or not.

            return Tokens.Where(t => t is CompositeToken).Cast<CompositeToken>().FirstOrDefault(compositeToken => compositeToken.Tokens.Any(t => t.TokenId.IdEquals(token.TokenId)));
        }

        private void RebuildPaddedTokens()
        {
            FlattenedTokens = new TokenCollection(Tokens.GetPositionalSortedBaseTokens());
            PaddedTokens = new PaddedTokenCollection(Detokenizer.Detokenize(_flattenedTokens));
        }

        /// <summary>
        /// Adds a <see cref="CompositeToken"/> to the token map, replacing its child tokens.
        /// </summary>
        /// <param name="compositeToken">The <see cref="CompositeToken"/> to add.</param>
        public void AddCompositeToken(CompositeToken compositeToken)
        {
            var firstChild = compositeToken.Tokens.FirstOrDefault();
            if (firstChild != null)
            {
                var matchingToken = Tokens.FirstOrDefault(t => t.TokenId.IdEquals(firstChild.TokenId));
                if (matchingToken != null)
                {
                    var matchingTokenIndex = Tokens.IndexOf(matchingToken);
                    Tokens.Insert(matchingTokenIndex, compositeToken);
                    foreach (var childToken in compositeToken.Tokens)
                    {
                        Tokens.Remove(childToken);
                    }

                    RebuildPaddedTokens();
                }
            }
        }

        /// <summary>
        /// Replaces a token with another, as in the case of splitting an existing token into a composite token.
        /// </summary>
        /// <param name="tokenId">The <see cref="TokenId"/> of the token to be replaced.</param>
        /// <param name="replacementToken">The <see cref="Token"/> to replace it with.  This can be a <see cref="CompositeToken"/>.</param>
        public void ReplaceToken(TokenId tokenId, Token replacementToken)
        {
            var existing = Tokens.FirstOrDefault(t => t.TokenId.IdEquals(tokenId));
            if (existing != null)
            {
                Tokens.Insert(Tokens.IndexOf(existing), replacementToken);
                Tokens.Remove(existing);

                RebuildPaddedTokens();
            }
        }

        /// <summary>
        /// Removes a <see cref="CompositeToken"/> from the token map, reinserting its child tokens.
        /// </summary>
        /// <param name="compositeToken">The <see cref="CompositeToken"/> to remove.</param>
        /// <param name="childTokens">The child tokens to replace the composite with.</param>
        public void RemoveCompositeToken(CompositeToken compositeToken, TokenCollection childTokens)
        {
            var compositeIndex = Tokens.IndexOf(compositeToken);
            if (compositeIndex != -1)
            {
                foreach (var childToken in childTokens)
                {
                    Tokens.Insert(compositeIndex++, childToken);
                }
                Tokens.Remove(compositeToken);

                RebuildPaddedTokens();
            }
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="tokens">The set of <see cref="Token"/>s to be included in the token map.</param>
        /// <param name="corpus">The <see cref="TokenizedTextCorpus"/> that the tokens are part of.</param>
        public TokenMap(IEnumerable<Token> tokens, TokenizedTextCorpus corpus)
        {
            Corpus = corpus;
            Tokens = new TokenCollection(tokens);
        }
    }
}
