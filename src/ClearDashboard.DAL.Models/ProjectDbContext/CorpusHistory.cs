using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer.Models;

public class CorpusHistory : SynchronizableTimestampedEntity
{
    public CorpusHistory()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        TokenizedCorpora = new HashSet<TokenizedCorpus>();
        Verses = new HashSet<Verse>();
        Metadata = new Dictionary<string, object>();
        // ReSharper restore VirtualMemberCallInConstructor
    }

    public bool IsRtl { get; set; }
    public string? Name { get; set; }
    public string? Language { get; set; }
    public string? ParatextGuid { get; set; }
    public virtual CorpusType CorpusType { get; set; }

    //public string RawMetadata { get; set; }
    //[NotMapped]
    //public Dictionary<string, object> Metadata
    //{
    //    get => (string.IsNullOrEmpty(RawMetadata) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(RawMetadata)) ?? new Dictionary<string, object>();
    //    set => RawMetadata = JsonSerializer.Serialize(value);
    //}
    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; }

    public virtual ICollection<Verse> Verses { get; set; }
    public virtual ICollection<TokenizedCorpus> TokenizedCorpora { get; set; }
}