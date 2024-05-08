namespace ClearDashboard.DAL.Alignment.Exceptions;

public class SplitInstructionException : Exception
{
    public string? Details { get; set; }
    public SplitInstructionException(string message, string? details = null) : base(message)
    {
        Details = details;
    }
}