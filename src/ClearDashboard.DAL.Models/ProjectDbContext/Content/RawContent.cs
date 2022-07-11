namespace ClearDashboard.DataAccessLayer.Models;

public abstract class RawContent : IdentifiableEntity
{
    protected RawContent()
    {
        Bytes = Array.Empty<byte>();
    }

    public byte[]? Bytes { get; protected set; }
    public string? ContentType { get; set; }

}