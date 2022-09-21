using System;

namespace ClearDashboard.Wpf.Application.Exceptions;

internal class MissingTokenizedTextCorpusIdException : Exception 
{
    public MissingTokenizedTextCorpusIdException(string message) :base(message)
    {
            
    }
}