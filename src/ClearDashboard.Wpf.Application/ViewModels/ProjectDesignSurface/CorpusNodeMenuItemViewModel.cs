using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.ViewModels.ProjectDesignSurface
{
    public class ParallelCorpusConnectionMenuItemViewModel : MenuItemViewModel<ParallelCorpusConnectionMenuItemViewModel>
    {
        private ConnectionViewModel? _connectionViewModel;
        public ConnectionViewModel? ConnectionViewModel
        {
            get => _connectionViewModel;
            set => Set(ref _connectionViewModel, value);

        }

        protected override void Execute()
        {
            ProjectDesignSurfaceViewModel?.ExecuteCorpusNodeMenuCommand(this);
        }
    }

    public class CorpusNodeMenuItemViewModel : MenuItemViewModel<CorpusNodeMenuItemViewModel>
    {
        private CorpusNodeViewModel? _corpusNodeViewModel;
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
