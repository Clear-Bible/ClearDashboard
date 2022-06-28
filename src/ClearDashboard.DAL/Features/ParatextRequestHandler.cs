using ClearDashboard.DAL.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features
{
    public enum HttpVerb { GET, POST, PUT, DELETE }


    public abstract class ParatextRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse: RequestResult<TData>
    {
        protected ILogger Logger { get; }

        protected HttpClient HttpClient { get; private set; }

        protected ParatextRequestHandler(ILogger logger)
        {
            Logger = logger;

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:9000/api/")
            };
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected async  Task<TResponse> ExecuteRequest(string requestUri, TRequest request, CancellationToken cancellationToken, HttpVerb httpVerb = HttpVerb.POST)
        {
            try
            {
                HttpResponseMessage response;

                switch (httpVerb)
                {
                    case HttpVerb.POST:
                        response = await HttpClient.PostAsJsonAsync<TRequest>(requestUri, request, cancellationToken).ConfigureAwait(false);
                        break;
                    case HttpVerb.PUT:
                        response = await HttpClient.PutAsJsonAsync<TRequest>(requestUri, request, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new Exception("Unsupported HTTP verb " + httpVerb);
                }

                var result = await response.Content.ReadAsAsync<TResponse>(cancellationToken);

                if (result.Success)
                {
                    Logger.LogInformation($"Successfully called {HttpClient.BaseAddress}/{requestUri}.");
                }
                else
                {
                    Logger.LogError($"Call to {HttpClient.BaseAddress}/{requestUri} was not successful: {result.Message}");
                }

                return result;

            }
            catch (Exception ex)
            {
                return (TResponse)new RequestResult<TData>(default(TData), false, ex.Message);
            }
        }

    }
}
