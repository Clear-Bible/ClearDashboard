
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LexicalItem : SynchronizableTimestampedEntity
    {
        public LexicalItem()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            LexicalItemDefinitions = new HashSet<LexicalItemDefinition>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
        public ICollection<LexicalItemDefinition> LexicalItemDefinitions { get; set; }
    }
}
