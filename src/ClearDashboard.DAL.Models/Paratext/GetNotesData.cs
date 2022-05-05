
namespace ClearDashboard.DataAccessLayer.Models
{
    public class GetNotesData 
    {
        public int BookId { get; set; }

        public int ChapterId { get; set; }

        public bool IncludeResolved { get; set; }
    }
}
