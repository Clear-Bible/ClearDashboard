
using System;
using System.Text.Json.Serialization;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public record UserId : BaseId
    {
        public UserId(Guid id) : base(id)
        {
        }

        [JsonConstructor]
        public UserId(string id) : base(id)
        {
        }
    }
}
