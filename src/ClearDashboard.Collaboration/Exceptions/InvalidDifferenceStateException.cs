using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class InvalidDifferenceStateException : Exception
{
	public InvalidDifferenceStateException(string message): base(message)
	{
	}
}

