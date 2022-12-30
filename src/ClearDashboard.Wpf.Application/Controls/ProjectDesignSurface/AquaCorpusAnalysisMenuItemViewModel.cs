namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected override void Execute()
        {
            ProjectDesignSurfaceViewModel?.ExecuteAquaCorpusAnalysisMenuCommand(this);
        }
    }
}
