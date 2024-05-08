using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Exceptions;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    internal class MediatorMock : IMediator
    {
		public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            dynamic? handler = GetType().Assembly.CreateInstance($"{GetType().Namespace}.{request.GetType().Name}Handler");
            dynamic dynamicRequest = request; // so that it represents its actual type and not its interface
            return handler?.Handle(dynamicRequest, cancellationToken) ?? throw new InvalidTypeEngineException(name: "handler", value: "null");
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

		public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
		{
			throw new NotImplementedException();
		}
	}
}
