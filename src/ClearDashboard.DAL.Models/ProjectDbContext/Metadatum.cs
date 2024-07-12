using System.ComponentModel.DataAnnotations.Schema;

namespace ClearDashboard.DataAccessLayer.Models;

[NotMapped]
public class Metadatum
{
    public string Key { get; set; }

    public string? Value { get; set; }
}