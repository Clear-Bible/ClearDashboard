using System;
using System.Threading;
using System.Windows;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;

public class ParallelCorpusConnectionMenuItemViewModel : MenuItemViewModel<ParallelCorpusConnectionMenuItemViewModel>
{
    private ParallelCorpusConnectionViewModel? _connectionViewModel;
	public ParallelCorpusConnectionViewModel? ConnectionViewModel
    {
        get => _connectionViewModel;
        set => Set(ref _connectionViewModel, value);

    }

    public string? SourceParatextId { get; set; } = "";
    public string? TargetParatextId { get; set; } = "";

    public bool IsRtl { get; set; }
    // ReSharper disable once InconsistentNaming
    public bool IsTargetRtl { get; set; }
    public string? AlignmentSetId { get; set; } = string.Empty;
    public string? TranslationSetId { get; set; } = string.Empty;
    public string? DisplayName { get; set; } = string.Empty;
    public string? ParallelCorpusId { get; set; } = string.Empty;
    public string? ParallelCorpusDisplayName { get; set; } = string.Empty;
    public Guid ConnectionId { get; set; }

    private bool _isEnabled = true;

    public string SmtModel { get; set; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            NotifyOfPropertyChange(() => IsEnabled);
        }
    }

    public Visibility Visibility = Visibility.Visible;

	protected override void Execute(CancellationToken token)
    {
        ProjectDesignSurfaceViewModel?.ExecuteConnectionMenuCommand(this, token);
    }
}