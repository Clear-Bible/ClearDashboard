using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.CQRS.Features
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
                BaseAddress = new Uri("http://localhost:9000/api/"),
                Timeout = TimeSpan.FromSeconds(120)
            };
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected async  Task<TResponse> ExecuteRequest(string requestUri, TRequest request, CancellationToken cancellationToken, HttpVerb httpVerb = HttpVerb.POST)
        {
            //return await ExecuteTypedRequest<TRequest, TResponse, TData>(requestUri, request, cancellationToken, httpVerb);
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
                    Logger.LogInformation($"Successfully called {HttpClient.BaseAddress}{requestUri} - {JsonSerializer.Serialize(request)}");
                }
                else
                {
                    Logger.LogError($"Call to {HttpClient.BaseAddress}{requestUri} was not successful: {result.Message}");
                }

                return result;

            }
            catch (Exception ex)
            {
                return (TResponse)new RequestResult<TData>(default(TData), false, ex.Message);
            }
        }

        //protected async Task<TResponse2> ExecuteTypedRequest<TRequest2, TResponse2, TData2>(string requestUri, TRequest2 request, CancellationToken cancellationToken, HttpVerb httpVerb = HttpVerb.POST)
        //    where TRequest2 : IRequest<TResponse>
        //    where TResponse2 : RequestResult<TData2>
        //{
        //    try
        //    {
        //        HttpResponseMessage response;

        //        switch (httpVerb)
        //        {
        //            case HttpVerb.POST:
        //                response = await HttpClient.PostAsJsonAsync<TRequest2>(requestUri, request, cancellationToken).ConfigureAwait(false);
        //                break;
        //            case HttpVerb.PUT:
        //                response = await HttpClient.PutAsJsonAsync<TRequest2>(requestUri, request, cancellationToken).ConfigureAwait(false);
        //                break;
        //            default:
        //                throw new Exception("Unsupported HTTP verb " + httpVerb);
        //        }

        //        var result = await response.Content.ReadAsAsync<TResponse2>(cancellationToken);

        //        if (result.Success)
        //        {
        //            Logger.LogInformation($"Successfully called {HttpClient.BaseAddress}{requestUri}.");
        //        }
        //        else
        //        {
        //            Logger.LogError($"Call to {HttpClient.BaseAddress}{requestUri} was not successful: {result.Message}");
        //        }

        //        return result;

        //    }
        //    catch (Exception ex)
        //    {
        //        return (TResponse2)new RequestResult<TData2>(default(TData2), false, ex.Message);
        //    }
        //}

    }
}
