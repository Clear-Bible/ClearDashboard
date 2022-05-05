using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Features;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class TestBase
    {
        protected ITestOutputHelper Output { get; private set; }

        protected readonly ServiceCollection Services = new ServiceCollection();
        private IServiceProvider? _serviceProvider = null;
        protected IServiceProvider ServiceProvider => _serviceProvider ??= Services.BuildServiceProvider();

        protected TestBase(ITestOutputHelper output)
        {
            Output = output;
            // ReSharper disable once VirtualMemberCallInConstructor
            SetupDependencyInjection();
        }

        protected virtual void SetupDependencyInjection()
        {
           Services.AddClearDashboardDataAccessLayer();
           Services.AddMediatR(typeof(IMediatorRegistrationMarker));
            Services.AddLogging();
        }

        protected async Task<RequestResult<TData>> ExecuteAndTestRequest<TRequest, TResult, TData>(TRequest query)
            where TRequest : IRequest<RequestResult<TData>>
            where TResult : RequestResult<TData>, new()
            where TData : class, new()
        {
            var mediator = ServiceProvider.GetService<IMediator>();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = new RequestResult<TData>(new TData(), false);
            try
            {
                result = await mediator.Send(query);
            }
            finally
            {
                stopwatch.Stop();
            }

            Assert.NotNull(result);
            Assert.True(result.Success);

            var type = result.Data.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {

                Assert.NotEmpty((IEnumerable)result.Data);

                Output.WriteLine($"Returned {((IEnumerable<object>)(result.Data)).Count()} records in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
            else
            {
                Output.WriteLine($"Returned {result.Data.GetType().Name} in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }

            return result;

        }
    }
}
