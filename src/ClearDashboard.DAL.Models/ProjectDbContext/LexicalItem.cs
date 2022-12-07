
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class LexicalItem : SynchronizableTimestampedEntity
    {
        public LexicalItem()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            LexicalItemDefinitions = new HashSet<LexicalItemDefinition>();
            LexicalItemSurfaceTexts = new HashSet<LexicalItemSurfaceText>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? Language { get; set; }
        public string? TrainingText { get; set; }
        public ICollection<LexicalItemDefinition> LexicalItemDefinitions { get; set; }
        public ICollection<LexicalItemSurfaceText> LexicalItemSurfaceTexts { get; set; }
    }
}
