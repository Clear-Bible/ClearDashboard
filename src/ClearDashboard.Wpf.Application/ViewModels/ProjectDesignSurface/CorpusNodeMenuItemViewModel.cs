using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System;

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

        public string SourceParatextId { get; set; } = "";
        public string TargetParatextId { get; set; } = "";

        public bool IsRTL { get; set; }
        public bool IsTargetRTL { get; set; }
        public string AlignmentSetId { get; set; } = string.Empty;
        public string TranslationSetId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ParallelCorpusId { get; set; } = string.Empty;
        public string? ParallelCorpusDisplayName { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }


        protected override void Execute()
        {
            ProjectDesignSurfaceViewModel?.ExecuteConnectionMenuCommand(this);
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
