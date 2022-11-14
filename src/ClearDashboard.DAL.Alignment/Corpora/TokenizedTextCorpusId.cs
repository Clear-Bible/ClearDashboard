using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using SIL.Machine.Tokenization;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public class TokenizedTextCorpusId : EntityId<TokenizedTextCorpusId>, IEquatable<TokenizedTextCorpusId>
    {
        public TokenizedTextCorpusId(Guid id)
        {
            Id = id;
            Metadata = new Dictionary<string, object>();
        }
        public TokenizedTextCorpusId(string id)
        {
            Id = Guid.Parse(id);
            Metadata = new Dictionary<string, object>();
        }

        public TokenizedTextCorpusId(Guid id, CorpusId corpusId, string? displayName, string? tokenizationFunction, Dictionary<string, object> metadata, DateTimeOffset created, UserId userId)
        {
            Id = id;
            CorpusId = corpusId;
            DisplayName = displayName;
            TokenizationFunction = tokenizationFunction;
            Metadata = metadata;
            Created = created;
            UserId = userId;
        }

        public CorpusId? CorpusId { get; }
        public string? DisplayName { get; set; }
        public string? TokenizationFunction { get; }
        public EngineStringDetokenizer Detokenizer
        {
            get
            {
                return TokenizationFunction switch
                {
                    "WhitespaceTokenizer" => new EngineStringDetokenizer(new WhitespaceDetokenizer()),
                    "ZwspWordTokenizer" => new EngineStringDetokenizer(new ZwspWordDetokenizer()),
                    _ => new EngineStringDetokenizer(new LatinWordDetokenizer())
                };
            }
        }

        public Dictionary<string, object> Metadata { get; set; }
        public DateTimeOffset? Created { get; }
        public UserId? UserId { get; }

        public override bool Equals(object? obj) => Equals(obj as TokenizedTextCorpusId);
        public bool Equals(TokenizedTextCorpusId? other)
        {
            if (other is null) return false;
            if (!IdEquals(other)) return false;

            if (CorpusId != other.CorpusId || 
                DisplayName != other.DisplayName || 
                TokenizationFunction != other.TokenizationFunction ||
                Created != other.Created ||
                UserId != other.UserId)
            {
                return false;
            }

            return Metadata.SequenceEqual(other.Metadata);
        }
        public override int GetHashCode()
        {
            var mhc = 0;
            foreach (var item in Metadata)
            {
                mhc ^= (item.Key, item.Value).GetHashCode();
            }

            return HashCode.Combine(Id, CorpusId, DisplayName, TokenizationFunction, Created, UserId, mhc);
        }
        public static bool operator ==(TokenizedTextCorpusId? e1, TokenizedTextCorpusId? e2) => object.Equals(e1, e2);
        public static bool operator !=(TokenizedTextCorpusId? e1, TokenizedTextCorpusId? e2) => !(e1 == e2);
    }
}
