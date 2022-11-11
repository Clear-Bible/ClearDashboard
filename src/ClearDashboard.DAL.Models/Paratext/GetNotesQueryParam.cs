
namespace ClearDashboard.DataAccessLayer.Models
{
    public class GetNotesQueryParam 
    {
        public int BookId { get; set; }

        public int ChapterId { get; set; }

        public bool IncludeResolved { get; set; }
    }
}
