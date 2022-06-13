using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class User
    {
        public User()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            AlignmentVersions = new HashSet<AlignmentVersion>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [NotMapped] public string FullName => $"{FirstName} {LastName}";
        public int? LastAlignmentLevelId { get; set; }

        //public virtual InterlinearNote? InterlinearNote { get; set; }
        public virtual ICollection<AlignmentVersion> AlignmentVersions { get; set; }
    }
}
