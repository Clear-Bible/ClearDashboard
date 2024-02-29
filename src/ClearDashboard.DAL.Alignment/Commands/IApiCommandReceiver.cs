using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Commands
{
	public interface IApiCommandReceiver<C, R>
		where C : IApiCommand<R>
	{
		public Task<R> RequestAsync(C command, CancellationToken cancellationToken);
	}
}
