using System;
namespace ClearDashboard.Collaboration.Exceptions;

public class GitRepositoryNotFoundException : Exception
{
	public string Path { get; private set; }
	public GitRepositoryNotFoundException(string path) : base($"Git repository not found at path '{path}'")
    {
		Path = path;
	}
}

