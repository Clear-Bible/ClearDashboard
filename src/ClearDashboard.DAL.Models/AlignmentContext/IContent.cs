using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public interface IContent<T>
{
    // examples - StringContent, BinaryContent, ImageContent, VideoContent
    //string? ContentType { get; set; }
    T Content { get; set; }
}

public static class ContentType
{
    public const string Binary = "Binary";
    public const string Image = "Image";
    public const string String = "String";
    public const string Video = "Video";
}

public abstract class RawContent : ClearEntity
{
    protected RawContent()
    {
        Bytes = Array.Empty<byte>();
    }

    public byte[]? Bytes { get; protected set; }
    public string? ContentType { get; set; }

}

public class StringContent : RawContent, IContent<string>
{
    public string Content
    {
        get => Bytes != null ? Encoding.Unicode.GetString(Bytes, 0, Bytes.Length) : string.Empty;
        set => Bytes = Encoding.Unicode.GetBytes(value);
    }
}