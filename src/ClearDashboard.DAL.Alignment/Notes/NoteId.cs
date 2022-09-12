
using ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Notes
{
    public record NoteId : BaseId
    {
        public NoteId(Guid id) : base(id)
        {
        }
        public NoteId(Guid id, DateTimeOffset created, DateTimeOffset? modified, UserId userId) : base(id)
        {
            Created = created;
            Modified = modified;
            UserId = userId;
        }

        public DateTimeOffset? Created { get; }
        public DateTimeOffset? Modified { get; }
        public UserId? UserId { get; }
    }
}
