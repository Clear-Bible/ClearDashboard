namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class InterlinearNote
    {
        public int Id { get; set; }
        public int? TokenId { get; set; }
        public string? Note { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        public virtual Token? Token { get; set; }
        public virtual User? User { get; set; }
    }
}
