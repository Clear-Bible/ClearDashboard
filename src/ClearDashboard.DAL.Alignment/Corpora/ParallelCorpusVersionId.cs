
namespace ClearBible.Alignment.DataServices.Corpora
{
    public record ParallelCorpusVersionId : BaseId
    {
        public DateTime Created { get; } //must have this set so api users can sort and find latest version.
        public ParallelCorpusVersionId(Guid parallelCorpusVersionId, DateTime created) : base(parallelCorpusVersionId)
        {
            Created = created;
        }
    }
}
