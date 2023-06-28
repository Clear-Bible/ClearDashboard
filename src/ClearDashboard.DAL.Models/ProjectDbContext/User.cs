using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("fullName")]
        [NotMapped] 
        public string? FullName => $"{FirstName} {LastName}";

        [JsonPropertyName("paratextUserName")]
        [NotMapped]
        public string? ParatextUserName { get; set; }

        [JsonPropertyName("isInternal")]
        [NotMapped] 
        public bool? IsInternal { get; set; } = false;

        [JsonPropertyName("licenseVersion")]
        [NotMapped]
        public int? LicenseVersion { get; set; } = 0;

        public int? LastAlignmentLevelId { get; set; }

        public virtual ICollection<AlignmentSet> AlignmentSets { get; set; }
        public virtual ICollection<TranslationSet> TranslationSets { get; set; }
    }
}
