using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ClearDashboard.DataAccessLayer.Models;

public partial class StringContent : RawContent, IContent<string>
{
    [NotMapped]
    public string Content
    {
        get => Bytes != null ? Encoding.Unicode.GetString(Bytes, 0, Bytes.Length) : string.Empty;
        set => Bytes = Encoding.Unicode.GetBytes(value);
    }
}