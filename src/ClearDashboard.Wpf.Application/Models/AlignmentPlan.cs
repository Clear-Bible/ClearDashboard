namespace ClearDashboard.Wpf.Application.Models
{
    public enum SmtAlgorithm
    {
        FastAlign,
        IBM4,
        IBM1,
        HMM,
    }

    public class AlignmentPlan
    {
        

        public string Target { get; set; } = "";
        public string TargetID { get; set; } = "";
        public string Source { get; set; } = "";
        public string SourceID { get; set; } = "";
        public bool IsCleanUsfmComplete { get; set; } = false;
        public double CleanUsfmProgress { get; set; } = 0;
        public bool IsAlignmentComplete { get; set; } = false;
        public double AlignmentProgress { get; set; } = 0;
        public SmtAlgorithm SmtAlgorithm { get; set; } = SmtAlgorithm.FastAlign;
    }
}
