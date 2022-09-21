using System;

namespace ClearDashboard.Wpf.Application.Exceptions
{
    internal class MissingCorpusNodeException : Exception
    {
        public MissingCorpusNodeException(string message):base (message)
        {
            
        }
    }
}
