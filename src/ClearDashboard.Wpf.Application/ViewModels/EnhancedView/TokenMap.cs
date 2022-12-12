using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    /// <summary>
    /// Manages a sequence of <see cref="Token"/>s for display, adjusting for <see cref="CompositeToken"/>s
    /// as necessary and incorporating padding values determined by an <see cref="EngineStringDetokenizer"/>.
    /// </summary>
    public class TokenMap : PropertyChangedBase
    {
        private TokenCollection _tokens = new();
        private TokenCollection _flattenedTokens = new();
        private PaddedTokenCollection _paddedTokens = new ();
        private EngineStringDetokenizer Detokenizer { get; }

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
            set
            {
                Set(ref _tokens, value);
                FlattenedTokens = new TokenCollection(Tokens.GetPositionalSortedBaseTokens());
            }
        }

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
                PaddedTokens = new PaddedTokenCollection(Detokenizer.Detokenize(_flattenedTokens));
            }
        }

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
        /// Gets the <see cref="CompositeToken"/> that a token is a constituent of, if any.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> to search for.</param>
        /// <returns>The parent <see cref="CompositeToken"/>, if any; null otherwise.</returns>
        public CompositeToken? GetCompositeToken(Token token)
        {
            return Tokens.Where(t => t is CompositeToken).Cast<CompositeToken>().FirstOrDefault(compositeToken => compositeToken.Tokens.Any(t => t.TokenId.IdEquals(token.TokenId)));
        }

        public TokenMap(IEnumerable<Token> tokens, EngineStringDetokenizer detokenizer)
        {
            Detokenizer = detokenizer;
            Tokens = new TokenCollection(tokens);
        }
    }
}
