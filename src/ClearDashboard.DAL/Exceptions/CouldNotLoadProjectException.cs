#nullable disable
using System;

namespace ClearDashboard.DataAccessLayer.Exceptions;

public class CouldNotLoadProjectException : Exception
{
    public CouldNotLoadProjectException(string projectName): base($"Could not load a project with the name '{projectName}.")
    {
           
    }
}