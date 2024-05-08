using System.Threading;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public class CorpusNodeMenuItemViewModel : MenuItemViewModel<CorpusNodeMenuItemViewModel>
    {
        private CorpusNodeViewModel? _corpusNodeViewModel;

		public CorpusNodeViewModel? CorpusNodeViewModel
        {
            get => _corpusNodeViewModel;
            set => Set(ref _corpusNodeViewModel, value);

        }

        private string? _tokenizer;

		public string? Tokenizer
        {
            get => _tokenizer;
            set => Set(ref _tokenizer, value);
        }

        protected override void Execute(CancellationToken token)
        {
           
            ProjectDesignSurfaceViewModel?.ExecuteCorpusNodeMenuCommand(this, token);
        }

        
    }
}
