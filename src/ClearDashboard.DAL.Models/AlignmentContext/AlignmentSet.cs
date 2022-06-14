namespace ClearDashboard.DataAccessLayer.Models;

public class AlignmentSet : IdentifiableEntity
{
    public AlignmentSet()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        AlignmentTokensPairs = new HashSet<AlignmentTokenPair>();
        // ReSharper restore VirtualMemberCallInConstructor
    }
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }

    public virtual ICollection<AlignmentTokenPair> AlignmentTokensPairs { get; set; }
}