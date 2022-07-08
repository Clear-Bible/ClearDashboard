using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer.Models;

public class TokenizedCorpus : SynchronizableTimestampedEntity
{
    public TokenizedCorpus()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Tokens = new HashSet<Token>();
        //SourceParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();
        //TargetParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();

        Metadata = new Dictionary<string, object>();
        SourceParallelCorpora = new HashSet<ParallelCorpus>();
        TargetParallelCorpora = new HashSet<ParallelCorpus>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public virtual ICollection<Token> Tokens { get; set; }

    //public virtual ICollection<ParallelTokenizedCorpus> SourceParallelTokenizedCorpus { get; set; }
    //public virtual ICollection<ParallelTokenizedCorpus> TargetParallelTokenizedCorpus { get; set; }

    public virtual ICollection<ParallelCorpus> SourceParallelCorpora { get; set; }
    public virtual ICollection<ParallelCorpus> TargetParallelCorpora { get; set; }

    public virtual Guid CorpusId { get; set; }
    public virtual Corpus Corpus { get; set; }

    public string? TokenizationFunction { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; }
   
}