using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class BuilderReferenceException : Exception
{
	public BuilderReferenceException(string message) : base(message)
    {
	}
}

