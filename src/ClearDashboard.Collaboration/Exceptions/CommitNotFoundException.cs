using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class CommitNotFoundException : Exception
{
	public string CommitSha { get; private set; }
	public CommitNotFoundException(string commitSha) : base($"Commit '{commitSha}' not found in commit log")
    {
		CommitSha = commitSha;
	}
}

