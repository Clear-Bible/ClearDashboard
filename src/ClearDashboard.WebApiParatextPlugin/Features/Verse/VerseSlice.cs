using ClearDashboard.DAL.CQRS;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ClearDashboard.WebApiParatextPlugin.Features.Verse
{
    public class GetCurrentVerseQueryHandler : IRequestHandler<GetCurrentVerseQuery, RequestResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly ILogger<GetCurrentVerseQueryHandler> _logger;

        public GetCurrentVerseQueryHandler(IVerseRef verseRef, ILogger<GetCurrentVerseQueryHandler> logger)
        {
            _verseRef = verseRef;
            _logger = logger;
        }

        public Task<RequestResult<string>> Handle(GetCurrentVerseQuery request, CancellationToken cancellationToken)
        {
            var queryResult = new RequestResult<string>(string.Empty);
            try
            {
                var verseId = _verseRef.BBBCCCVVV.ToString();
                if (verseId.Length < 8)
                {
                    verseId = verseId.PadLeft(9, '0');
                }

                queryResult.Data = verseId;

            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }

            return Task.FromResult(queryResult);
        }
    }

    public class SetCurrentVerseCommandHandler : IRequestHandler<SetCurrentVerseCommand, RequestResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly ILogger<SetCurrentVerseCommandHandler> _logger;
        private readonly IProject _project;
        private readonly MainWindow _mainWindow;


        public SetCurrentVerseCommandHandler(IVerseRef verseRef, ILogger<SetCurrentVerseCommandHandler> logger,
            IProject project, MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _project = project;
            _verseRef = verseRef;
            _logger = logger;
        }

        public Task<RequestResult<string>> Handle(SetCurrentVerseCommand request, CancellationToken cancellationToken)
        {

            // get back on UI thread
            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
            {
                // ReSharper disable once InconsistentNaming
                var BBBCCCVVV = request.Verse.PadLeft(9, '0');
                int book = 1;
                int chapter = 1;
                int verse = 1;

                try
                {
                    book = int.Parse(BBBCCCVVV.Substring(0, 3));
                    chapter = int.Parse(BBBCCCVVV.Substring(3, 3));
                    verse = int.Parse(BBBCCCVVV.Substring(6, 3));
                    _mainWindow.SwitchVerseReference(book, chapter, verse);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                


                //// set up a new Versification reference for this verse
                //var newVerseRef = _project.Versification.CreateReference(book, chapter, verse);

                //// call the new verse for this sync group
                //_host.SetReferenceForSyncGroup(newVerseRef, _host.ActiveWindowState.SyncReferenceGroup);
            }));


            return Task.FromResult(new RequestResult<string>(request.Verse));
        }
    }
}