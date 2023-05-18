using System;

namespace ClearDashboard.Collaboration.Exceptions;

public class InvalidModelStateException : Exception
{
	public InvalidModelStateException(string message) : base(message)
	{
	}
}

