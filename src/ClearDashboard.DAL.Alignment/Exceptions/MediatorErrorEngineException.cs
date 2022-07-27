using ClearBible.Engine.Exceptions;


namespace ClearDashboard.DAL.Alignment.Exceptions
{
    public class MediatorErrorEngineException : EngineException
    {
        public MediatorErrorEngineException(
            string message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
            : base(message, new Dictionary<string, string>(), memberName, sourceFilePath, sourceLineNumber)
        {
        }
    }
}
