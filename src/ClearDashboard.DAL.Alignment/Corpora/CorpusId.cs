
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record CorpusId : BaseId
    {
        public CorpusId(Guid id) : base(id)
        {
        }

        [JsonConstructor]
        public CorpusId(string id) : base(id)
        {
        }
    }
}
