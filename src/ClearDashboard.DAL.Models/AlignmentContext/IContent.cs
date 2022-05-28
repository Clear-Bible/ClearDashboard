namespace ClearDashboard.DataAccessLayer.Models;

public interface IContent
{
    // examples - StringContent, BinaryContent, ImageContent, VideoContent
    public byte[]? Content { get; set; }
    public string[]? MimeType { get; set; }
}