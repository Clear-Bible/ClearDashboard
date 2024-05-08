using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application
{
	public enum BootstrapConfigurationType
	{
		LOCAL,
		REMOTE,
		LOCAL_REMOTE_ENGINE_ONLY
	}

	// TODO:  very, very temporary way to control ParallelCorpusDialogViewModel functionality for testing
	public class BootstrapConfiguration
	{
		public BootstrapConfigurationType BootstrapConfigurationType { get; set; }
	}
}
