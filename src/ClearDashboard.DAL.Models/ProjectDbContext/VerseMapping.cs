using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public class VerseMapping : SynchronizableTimestampedEntity
{
    public VerseMapping()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        Verses = new HashSet<Verse>();
       

        // ReSharper restore VirtualMemberCallInConstructor
    }

    [ForeignKey(nameof(ParallelCorpusId))]
    public virtual Guid ParallelCorpusId { get; set; }
    public virtual ParallelCorpus? ParallelCorpus { get; set; }

    public virtual ICollection<Verse> Verses { get; set; }
}