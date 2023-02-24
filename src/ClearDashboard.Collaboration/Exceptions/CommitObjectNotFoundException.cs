using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class CommitObjectNotFoundException : Exception
{
    public string Objectish { get; private set; }
    public CommitObjectNotFoundException(string objectish) : base($"Lookup failed for commit object: '{objectish}'")
    {
        Objectish = objectish;
	}
}

