namespace ClearDashboard.Wpf.Application.Models
{
    public class NodeTokenization
    {
        public string TokenizationFriendlyName { get; set; } = string.Empty;
        public string TokenizationName { get; set; } = string.Empty;
        public string CorpusId { get; set; } = string.Empty;
        public string TokenizedTextCorpusId { get; set; } = string.Empty;
        public bool IsSelected { get; set; } 
        public bool IsPopulated { get; set; }
    }
}
