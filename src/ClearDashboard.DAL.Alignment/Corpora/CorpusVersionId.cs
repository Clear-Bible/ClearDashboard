
namespace ClearBible.Alignment.DataServices.Corpora
{
    public record CorpusVersionId : BaseId
    {
        public DateTime Created { get; } //must have this set so api users can sort and find latest version.

        public CorpusVersionId(Guid id, DateTime created) : base(id)
        {
            Created = created;
        }
    }
}
