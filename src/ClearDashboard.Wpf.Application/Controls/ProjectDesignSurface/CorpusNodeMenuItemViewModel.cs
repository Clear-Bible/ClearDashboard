namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public class CorpusNodeMenuItemViewModel : MenuItemViewModel<CorpusNodeMenuItemViewModel>
    {
        private CorpusNodeViewModel? _corpusNodeViewModel;
        private string _tokenizer1;

        public CorpusNodeViewModel? CorpusNodeViewModel
        {
            get => _corpusNodeViewModel;
            set => Set(ref _corpusNodeViewModel, value);

        }

        protected override void Execute()
        {
            ProjectDesignSurfaceViewModel?.ExecuteCorpusNodeMenuCommand(this);
        }

        
    }
}
