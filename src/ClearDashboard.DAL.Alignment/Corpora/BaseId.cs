

namespace ClearBible.Alignment.DataServices.Corpora
{
    public abstract record BaseId
    {
        public BaseId(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
