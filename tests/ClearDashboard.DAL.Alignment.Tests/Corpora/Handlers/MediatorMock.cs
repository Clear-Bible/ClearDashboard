using ClearBible.Engine.Exceptions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
{
    internal class MediatorMock : IMediator
    {
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
    }
}
