using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

        public async Task GetCorpusAnalysis(
            string paratextProjectId, 
            CancellationToken cancellationToken, 
            IProgress<ProgressStatus>? progress = null)
        {
            await SlowTask("GetCorpusAnalysis", 10, cancellationToken, progress);
        }

        protected static async Task<int> ProcessUrlAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            byte[] content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            Console.WriteLine($"{url,-60} {content.Length,10:#,#}");

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
