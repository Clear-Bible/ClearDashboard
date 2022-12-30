namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record ReloadDataMessage(ReloadType ReloadType = ReloadType.Refresh);