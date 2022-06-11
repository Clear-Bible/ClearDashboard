using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

public partial class BinaryContent : RawContent, IContent<byte[]?>
{
    [NotMapped]
    public byte[]? Content
    {
        get => Bytes; 
        set => Bytes = value;
    }
}