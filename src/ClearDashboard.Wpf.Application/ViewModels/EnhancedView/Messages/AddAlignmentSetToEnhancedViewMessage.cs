using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record AddAlignmentSetToEnhancedViewMessage(AlignmentEnhancedViewItemMetadatum Metadatum);