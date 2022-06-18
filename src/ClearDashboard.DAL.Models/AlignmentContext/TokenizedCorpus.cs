namespace ClearDashboard.DataAccessLayer.Models;

public class TokenizedCorpus : SynchronizableTimestampedEntity
{
    public TokenizedCorpus()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Tokens = new HashSet<Token>();
        SourceParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();
        TargetParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public virtual ICollection<Token> Tokens { get; set; }

    public virtual ICollection<ParallelTokenizedCorpus> SourceParallelTokenizedCorpus { get; set; }
    public virtual ICollection<ParallelTokenizedCorpus> TargetParallelTokenizedCorpus { get; set; }

    public string? TokenizationFunction { get; set; }
}