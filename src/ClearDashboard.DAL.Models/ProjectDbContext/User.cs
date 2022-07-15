using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class User : IdentifiableEntity
    {
        public User()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            AlignmentVersions = new HashSet<AlignmentVersion>();
            AlignmentSets = new HashSet<AlignmentSet>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [NotMapped] 
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string ParatextUserName { get; set; }

        public int? LastAlignmentLevelId { get; set; }

        public virtual ICollection<AlignmentVersion> AlignmentVersions { get; set; }

        public virtual ICollection<AlignmentSet> AlignmentSets { get; set; }
    }
}
