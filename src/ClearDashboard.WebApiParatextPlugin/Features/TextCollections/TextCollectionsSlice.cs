using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Features.TextCollections
{
    public class TextCollectionsSlice
    {
        public class GetTextCollectionsQueryHandler : IRequestHandler<GetTextCollectionsQuery, RequestResult<List<TextCollection>>>
        {
            private readonly IProject _project;
            private readonly ILogger<GetTextCollectionsQueryHandler> _logger;
            private readonly IPluginHost _host;
            private readonly IVerseRef _verseRef;
            private readonly MainWindow _mainwindow;

            public GetTextCollectionsQueryHandler(IProject project, ILogger<GetTextCollectionsQueryHandler> logger, IPluginHost host, IVerseRef verseRef, MainWindow mainwindow)
            {
                _project = project;
                _logger = logger;
                _host = host;
                _verseRef = verseRef;
                _mainwindow = mainwindow;
            }
            public Task<RequestResult<List<TextCollection>>> Handle(GetTextCollectionsQuery request, CancellationToken cancellationToken)
            {
                var textCollections = _mainwindow.GetTextCollectionsData();
                var result = new RequestResult<List<TextCollection>>(textCollections);
                return Task.FromResult(result);
            }
        }
    }
}
