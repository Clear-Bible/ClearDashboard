using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Commands
{
	public interface IApiCommand
	{

	}

	public interface IApiCommand<R> : IApiCommand
	{
		public Task<R> ExecuteAsync(IComponentContext context, CancellationToken cancellationToken);
	}
}
