using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public class AquaManager : IAquaManager
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<NoteManager>? Logger { get; }
        private IMediator Mediator { get; }
        private IUserProvider UserProvider { get; }
        public AquaManager(IEventAggregator eventAggregator, ILogger<NoteManager>? logger, IMediator mediator, IUserProvider userProvider)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
            UserProvider = userProvider;

            EventAggregator.SubscribeOnUIThread(this);
        }

        public async Task AddCorpusAnalysisToEnhancedView()
        {
            await EventAggregator.PublishOnUIThreadAsync(new AddAquaCorpusAnalysisToEnhancedViewMessage(new AquaCorpusAnalysisEnhancedViewItemMetadatum()
            {
                IsNewWindow = false
            }));
        }

        public async Task RequestCorpusAnalysis(string paratextProjectId, CancellationToken cancellationToken)
        {
            await SlowTask("RequestCorpusAnalysis", 10, cancellationToken);
        }

        public async Task GetCorpusAnalysis(string paratextProjectId, CancellationToken cancellationToken)
        {
            await SlowTask("GetCorpusAnalysis", 10, cancellationToken);
        }

        protected  async Task<int> ProcessUrlAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Logger!.LogDebug($"{url,-60} {content.Length,10:#,#}");

            return content.Length;
        }

        protected async Task SlowTask(string name, int iterations, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();
                    if (iterations-- == 0)
                        return;
                    Logger!.LogDebug($"{name} Iteration: {iterations}");
                    Thread.Sleep(2000);
                }
            });
        }
    }
}
