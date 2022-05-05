# CQRS Cheat Sheet

This serves as a guide for adding CQRS Feature Slices to the DAL, Paratext plug-in and eventually Server.

## Background

The `feature slice` or `vertical slice` architecutre is built on top of the CQRS and the Mediator design pattern.  CQRS stands 
for "Command Query Responsibility Segregation". This pattern mandates separate read and write operations in applications. 
In this pattern `read` operations are called `Queries` and `write` operations are called `Commands`. Adopting the 
`feature slice` architecture typically reduces difficult source code merges as individual developers are working in a single 
feature slice and are not making changes to a more traditional application layer at the same time as their team mates thus reducing 
the likelihood of conflicting code changes.

The Dashboard Data Access Layer (DAL) uses the open source library `MediatR` to realize the feature slice architecture. All data 
manipulation (reads and writes) should be implemented by creating a query/query handler pair or a command/command handler pair.

## Project setup

The query/query handler and command/command/handler pairs can be organized in the same assembly or split into individual assemblies. 
The latter organizational pattern is used when a query or command is used to fetch or persist data in a distributed architecture like 
a call to get or save data via a RESTful web API.

A good example using a query/command in a distributed architecture is executing a query to get data from Paratext.  In this case 
a single query is created and the client side query handler is responsible for making an HTTP POST to the Paratext plug-in WEB API.  
When the HTTP POST is received, tne controller deserializes the request into the query and another query handler is invoked to get the data from Paratext and return the result back to the original
query handler, which forwards the result to the calling process.


### A concrete example

#### The query

``` csharp
 public record GetBiblicalTermsByTypeQuery(BiblicalTermsType BiblicalTermsType) : IRequest<RequestResult<List<BiblicalTermsData>>>
 {
        public BiblicalTermsType BiblicalTermsType { get; } = BiblicalTermsType;
 }
 ```

 #### The client side query handler
 ``` csharp
  public class GetBiblicalTermsByTypeQueryHandler : ParatextRequestHandler<GetBiblicalTermsByTypeQuery, RequestResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>
  {

        public GetBiblicalTermsByTypeQueryHandler([NotNull] ILogger<GetBiblicalTermsByTypeQueryHandler> logger) : base(logger)
        {
            //no-op
        }

        public override async Task<RequestResult<List<BiblicalTermsData>>> Handle(GetBiblicalTermsByTypeQuery request, CancellationToken cancellationToken)
        {
            return await ExecuteRequest("biblicalterms", request, cancellationToken);
        }
        
  }

  public abstract class ParatextRequestHandler<TRequest, TResponse, TData> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse: RequestResult<TData>
        where TData : new()
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

        protected async  Task<TResponse> ExecuteRequest(string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync<TRequest>(requestUri, request, cancellationToken).ConfigureAwait(false);

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
                return (TResponse)new RequestResult<TData>(new TData(), false, ex.Message);
            }
        }

  }
 ```

 #### The Paratext query handler
 ``` csharp
 public class BiblicalTermsController : FeatureSliceController
 {
        public BiblicalTermsController(IMediator mediator, ILogger<BiblicalTermsController> logger) : base(mediator, logger)
        {

        }

        [HttpPost]
        public async Task<RequestResult<List<BiblicalTermsData>>> GetAsync([FromBody] GetBiblicalTermsByTypeQuery command)
        {
            var result = await ExecuteRequestAsync<RequestResult<List<BiblicalTermsData>>, List<BiblicalTermsData>>(command, CancellationToken.None);
            return result;

        }

  }

  public class GetBiblicalTermsByTypeQueryHandler : IRequestHandler<GetBiblicalTermsByTypeQuery, RequestResult<List<BiblicalTermsData>>>
    {
        private readonly IWindowPluginHost _host;
        private readonly IProject _project;
        private readonly ILogger<GetBiblicalTermsByTypeQueryHandler> _logger;

        public GetBiblicalTermsByTypeQueryHandler(IWindowPluginHost host, IProject project, ILogger<GetBiblicalTermsByTypeQueryHandler> logger)
        {
            _host = host;
            _project = project;
            _logger = logger;
        }
        public Task<RequestResult<List<BiblicalTermsData>>> Handle(GetBiblicalTermsByTypeQuery request, CancellationToken cancellationToken)
        {

            var biblicalTermList = request.BiblicalTermsType == BiblicalTermsType.All
                ? _host.GetBiblicalTermList(BiblicalTermListType.All)
                : _project.BiblicalTermList;

            var queryResult = new RequestResult<List<BiblicalTermsData>>(new List<BiblicalTermsData>());
            try
            {
                queryResult.Data = ProcessBiblicalTerms(_project, biblicalTermList);
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }
        ... other methods elided for clarity
  }

 ```