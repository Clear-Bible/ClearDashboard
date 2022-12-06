namespace ClearDashboard.DataAccessLayer.Models.Common;

public class WorkedOnBook
{
    public int BookId { get; set; }
    public string BookCode { get; set; } = String.Empty;
    public bool IsWorkedOn { get; set; } = false;
}