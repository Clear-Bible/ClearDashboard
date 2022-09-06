namespace ClearDashboard.DataAccessLayer.Models;

public class EngineWordAlignment : IdentifiableEntity
{
    public string? SmtWordAlignerType { get; set; }
    public bool? IsClearAligner { get; set; }
}