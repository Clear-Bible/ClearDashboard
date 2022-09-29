
namespace ClearDashboard.Wpf.Application.Models
{
    public class TranslationSetInfo
    {
        public string TranslationSetId { get; set; } = string.Empty;
        public string? DisplayName { get; set; } = string.Empty;

        public string ParallelCorpusId { get; set; } = string.Empty;
        public string? ParallelCorpusDisplayName { get; set; } = string.Empty;

        public string AlignmentSetId { get; set; } = string.Empty;
        public string? AlignmentSetDisplayName { get; set; } = string.Empty;
    }
}
