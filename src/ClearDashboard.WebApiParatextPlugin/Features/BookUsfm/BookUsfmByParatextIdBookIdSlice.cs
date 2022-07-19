using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.BookUsfm
{
    public class GetBookUsfmByParatextIdBookIdQueryHandler : 
        IRequestHandler<GetBookUsfmByParatextIdBookIdQuery, RequestResult<IEnumerable<(string a, string b)>>>
    {
        //private readonly ILogger<GetBookUsfmByParatextIdBookIdQueryHandler> _logger;
        //private readonly MainWindow _mainWindow;
        
        //public GetBookUsfmByParatextIdBookIdQueryHandler(ILogger<GetBookUsfmByParatextIdBookIdQueryHandler> logger,
        //    MainWindow mainWindow)
        //{
        //    _logger = logger;
        //    _mainWindow = mainWindow;
        //}
        public Task<RequestResult<IEnumerable<(string a, string b)>>>
            Handle(GetBookUsfmByParatextIdBookIdQuery request, CancellationToken cancellationToken)
        {

            
            //_mainWindow.GetUsfmForBook(request.ParatextId, request.BookNum);
            
            //var queryResult =
                //new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>(
                //    new List<(string chapter, string verse, string text, bool isSentenceStart)>());

                
            //try
            //{
            //    queryResult.Data = ProcessBiblicalTerms(_project, biblicalTermList);
            //}
            //catch (Exception ex)
            //{
            //    queryResult.Success = false;
            //    queryResult.Message = ex.Message;
            //}

            //IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)> verse = null;
            
            

            //(string chapter, string verse, string text, bool isSentenceStart) bob = ("1", "2", "some text", false);
            //verse.Append(bob);


            return Task.FromResult(
                default(RequestResult<IEnumerable<(string a, string b)>>));
        }
    }
}
