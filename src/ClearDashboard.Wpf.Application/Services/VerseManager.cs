using MediatR;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class VerseManager : PropertyChangedBase
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<NoteManager>? Logger { get; }
        private IMediator Mediator { get; }

        public VerseManager(IEventAggregator eventAggregator, ILogger<NoteManager>? logger, IMediator mediator)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
        }
    }
}
