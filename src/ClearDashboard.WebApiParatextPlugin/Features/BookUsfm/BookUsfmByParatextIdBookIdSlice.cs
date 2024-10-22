﻿using System;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace ClearDashboard.WebApiParatextPlugin.Features.BookUsfm
{
    public class GetBookUsfmByParatextIdBookIdQueryHandler : 
        IRequestHandler<GetRowsByParatextProjectIdAndBookIdQuery, RequestResult<List<UsfmVerse>>>
    {
        private readonly ILogger<GetBookUsfmByParatextIdBookIdQueryHandler> _logger;
        private readonly MainWindow _mainWindow;

        public GetBookUsfmByParatextIdBookIdQueryHandler(ILogger<GetBookUsfmByParatextIdBookIdQueryHandler> logger,
            MainWindow mainWindow)
        {
            _logger = logger;
            _mainWindow = mainWindow;
        }
        public Task<RequestResult<List<UsfmVerse>>> Handle(GetRowsByParatextProjectIdAndBookIdQuery request,
            CancellationToken cancellationToken)
        {
            var data = _mainWindow.GetUsfmForBook(request.ParatextProjectId, request.BookId);
            
            // update the isSentenceStart field using Machine's parser
            foreach (var d in data)
            {
                try
                {
                    d.isSentenceStart = SIL.Machine.Utils.StringExtensions.HasSentenceEnding(d.Text);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
            }
            
            var queryResult = new RequestResult<List<UsfmVerse>>(new List<UsfmVerse>());
            
            try
            {
                queryResult.Data = data;
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }
    }
}
