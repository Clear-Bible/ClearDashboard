

using System;

namespace ClearDashboard.DAL.Alignment.Corpora
{
    public abstract record BaseId
    {
        protected BaseId(Guid id)
        {
            Id = id;
        }

        protected BaseId(string id)
        {
            Id = Guid.Parse(id);
        }

        public Guid Id { get; set; }
    }
}
