using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class User : IdentifiableEntity
    {
        public User()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            AlignmentSets = new HashSet<AlignmentSet>();
            TranslationSets = new HashSet<TranslationSet>();
            // ReSharper restore VirtualMemberCallInConstructor
        }
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [NotMapped] 
        public string? FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string? ParatextUserName { get; set; }

        [NotMapped] 
        public bool? IsInternal { get; set; } = false;

        [NotMapped]
        public int? LicenseVersion { get; set; } = 0;

        public int? LastAlignmentLevelId { get; set; }

        public Guid? DefaultLabelGroupId {  get; set; }

        public virtual ICollection<AlignmentSet> AlignmentSets { get; set; }
        public virtual ICollection<TranslationSet> TranslationSets { get; set; }
    }
}
