using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer.Models;

public class TokenizedCorpus : SynchronizableTimestampedEntity
{
    public TokenizedCorpus()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        TokenComponents = new HashSet<TokenComponent>();
        //SourceParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();
        //TargetParallelTokenizedCorpus = new HashSet<ParallelTokenizedCorpus>();

        SourceParallelCorpora = new HashSet<ParallelCorpus>();
        TargetParallelCorpora = new HashSet<ParallelCorpus>();
        Metadata = new Dictionary<string, object>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public virtual ICollection<TokenComponent> TokenComponents { get; set; }

    [NotMapped]
    public IEnumerable<Token> Tokens
    {
        get
        {
            return TokenComponents.Where(tc => tc.GetType() == typeof(Token)).Select(tc => (tc as Token)!);
        }
    }

    [NotMapped]
    public IEnumerable<TokenComposite> TokenComposites
    {
        get
        {
            return TokenComponents.Where(tc => tc.GetType() == typeof(TokenComposite)).Select(tc => (tc as TokenComposite)!);
        }
    }

    //public virtual ICollection<ParallelTokenizedCorpus> SourceParallelTokenizedCorpus { get; set; }
    //public virtual ICollection<ParallelTokenizedCorpus> TargetParallelTokenizedCorpus { get; set; }

    public virtual ICollection<ParallelCorpus> SourceParallelCorpora { get; set; }
    public virtual ICollection<ParallelCorpus> TargetParallelCorpora { get; set; }

    public virtual Guid CorpusId { get; set; }
    public virtual Corpus? Corpus { get; set; }

    public virtual Guid? CorpusHistoryId { get; set; }
    public virtual CorpusHistory? CorpusHistory { get; set; }
    public string? DisplayName { get; set; }

    public string? TokenizationFunction { get; set; }
    public int ScrVersType { get; set; }
    public string? CustomVersData { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; }
}