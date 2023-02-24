using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class SerializedDataException : Exception
{
	public SerializedDataException(string message) : base(message)
    {
	}
}

