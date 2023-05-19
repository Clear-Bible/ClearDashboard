using System;
using ClearDashboard.Collaboration.DifferenceModel;

namespace ClearDashboard.Collaboration.Exceptions;

public class MergeConflictException : Exception
{
	public IModelDifference ModelDifference { get; private set; }
	public MergeConflictException(IModelDifference modelDifference)
	{
		ModelDifference = modelDifference;
	}
}

