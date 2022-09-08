
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Corpus : SynchronizableTimestampedEntity
    {
        public Corpus()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            TokenizedCorpora = new HashSet<TokenizedCorpus>();
            Verses = new HashSet<Verse>();
            Metadata = new Dictionary<string, object>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public bool IsRtl { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Language { get; set; }
        public string? ParatextGuid { get; set; }
        public virtual CorpusType CorpusType { get; set; }
        public virtual ICollection<Verse> Verses { get; set; }
        public virtual ICollection<TokenizedCorpus> TokenizedCorpora { get; set; }


        [Column(TypeName = "jsonb")]
        public Dictionary<string, object> Metadata { get; set; }


    }
}
