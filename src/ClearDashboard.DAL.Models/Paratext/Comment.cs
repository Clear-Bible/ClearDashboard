
namespace ClearDashboard.DataAccessLayer.Models
{
    public class Comment
    {
        public List<Content> Contents { get; set; } = new List<Content>();
       
        public Author? Author { get; set; }

        public DateTimeOffset Created { get; set; }

        public SelectedLanguage? SelectedLanguage { get; set; }

        public AssignedUser? AssignedUser { get; set; }
    }
}