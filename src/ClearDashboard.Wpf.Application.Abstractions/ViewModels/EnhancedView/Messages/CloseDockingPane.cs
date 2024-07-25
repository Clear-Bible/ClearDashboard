using System;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record CloseDockingPane(Guid Guid, bool showConfirmation = true);