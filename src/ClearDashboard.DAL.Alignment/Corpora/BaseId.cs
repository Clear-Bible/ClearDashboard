

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public abstract record BaseId
    {
        protected BaseId(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
