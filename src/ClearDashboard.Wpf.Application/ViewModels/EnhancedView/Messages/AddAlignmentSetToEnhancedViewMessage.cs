using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Models;
using System.Collections.Generic;
using ClearDashboard.Wpf.Application.Models.EnhancedView;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record AddAlignmentSetToEnhancedViewMessage(AlignmentEnhancedViewItemMetadatum Metadatum);