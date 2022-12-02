using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Models.ProjectSerialization
{
    /// <summary>
    /// Represents a serialized enhanced view.
    /// </summary>
    public class EnhancedViewSerializationModel
    {
        public string Title { get; set; } = string.Empty;
        public bool ParatextSync { get; set; }
        public string BBBCCCVVV { get; set; } = "001001001";
        public List<EnhancedViewItemMetadatum> EnhancedViewItems { get; set; } = new List<EnhancedViewItemMetadatum>();
        public int VerseOffset { get; set; }
    }
}
