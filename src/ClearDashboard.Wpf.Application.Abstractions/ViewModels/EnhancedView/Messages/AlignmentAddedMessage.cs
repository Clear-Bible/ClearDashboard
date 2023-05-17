﻿using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

public record AlignmentAddedMessage(Alignment Alignment, TokenDisplayViewModel SourceTokenDisplayViewModel, TokenDisplayViewModel TargetTokenDisplayViewModel);