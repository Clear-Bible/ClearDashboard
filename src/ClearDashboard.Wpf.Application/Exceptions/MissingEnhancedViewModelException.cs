#nullable disable
using System;

namespace ClearDashboard.Wpf.Application.Exceptions;

internal class MissingEnhancedViewModelException : Exception
{
    public MissingEnhancedViewModelException(string message) : base(message)
    {

    }
}