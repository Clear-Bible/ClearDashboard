﻿using ClearDashboard.Wpf.Application.Collections;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages
{
    public record NoteDeletedMessage(NoteViewModel Note, EntityIdCollection EntityIds);
}