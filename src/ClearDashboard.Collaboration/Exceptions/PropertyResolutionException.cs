using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class PropertyResolutionException : Exception
{
	public PropertyResolutionException(string message): base(message)
	{
	}
    public PropertyResolutionException() : base()
    {
    }
}

