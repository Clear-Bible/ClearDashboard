using System;
using ClearDashboard.Collaboration.Model;

namespace ClearDashboard.Collaboration.Exceptions;

public class PropertyResolverNotFoundException : Exception
{
	public PropertyResolverNotFoundException(string message): base(message)
	{
	}
}

