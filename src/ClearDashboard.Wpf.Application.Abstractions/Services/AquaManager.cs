using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
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

        public async Task RequestCorpusAnalysis(
            string paratextProjectId, 
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress = null)
        {
            await SlowTask("RequestCorpusAnalysis", 10, cancellationToken, progress);
        }

        public async Task<string> GetCorpusAnalysis(
            string paratextProjectId, 
            CancellationToken cancellationToken, 
            IProgress<ProgressStatus>? progress = null)
        {
            await SlowTask("GetCorpusAnalysis", 10, cancellationToken, progress);
            return "Analysis 454";
        }

        protected  async Task<int> ProcessUrlAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Logger!.LogDebug($"{url,-60} {content.Length,10:#,#}");

            return content.Length;
        }

        protected static async Task SlowTask(
            string name,
            int iterations,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            int totalIterations = iterations;

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();
                    if (iterations-- == 0)
                        return;
                    Console.WriteLine($"{name} Iteration: {iterations}");
                    progress?.Report(new ProgressStatus(totalIterations - iterations, totalIterations));
                    Thread.Sleep(2000);
                }
            });
            return;
        }
    }
}
