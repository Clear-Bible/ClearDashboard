using Caliburn.Micro;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        public async Task<string> AddVersion(
            string paratextProjectId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            await SlowTask("AddVersion", 10, cancellationToken, progress);
            return "versionId";
        }
        public async Task<string> AddRevision(
            string versionId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            await SlowTask("AddRevision", 10, cancellationToken, progress);
            return "revisionId";
        }

        public async Task<IEnumerable<string>> GetRevisions(
            string versionId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            await SlowTask("GetRevisions", 10, cancellationToken, progress);
            return new List<string>() { "Revision1", "Revision2" };
        }

        public async Task<IEnumerable<string>> GetAssessmentStatuses(
            string revisionId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            await SlowTask("GetAssessments", 10, cancellationToken, progress);
            return new List<string>() { "AssessmentStatus1", "AssessmentStatus2" };
        }

        public async Task<string> GetAssessmentResult(
            string assessmentId,
            CancellationToken cancellationToken,
            IProgress<ProgressStatus>? progress)
        {
            await SlowTask("GetAssessmentResult", 10, cancellationToken, progress);
            return "AssessmentResult";
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

        static async Task<T?> GetFromJsonAsync<T>(
            HttpClient httpClient, 
            string protocolHostPortFileString, 
            Dictionary<string, string>? query,
            CancellationToken cancellationToken)
        {
            var uri = QueryHelpers.AddQueryString(protocolHostPortFileString, query);
            return await httpClient.GetFromJsonAsync<T>(uri, cancellationToken);
        }

        static async Task<string> PostKeyValueDataAsync(
            HttpClient httpClient,
            string protocolHostPortFileString,
            Dictionary<string, string>? keyValueData,
            CancellationToken cancellationToken)
        {
            var uri = QueryHelpers.AddQueryString(protocolHostPortFileString, keyValueData);

            using HttpResponseMessage response = await httpClient.PostAsync(
                uri,
                null,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        static async Task<string> PostStringAsFile(
            HttpClient httpClient,
            string url,
            string fileName,
            string content,
            string mediaTypeHeaderValueString = "text/pain")
        {
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                var fileStreamContent = new StringContent(content);
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaTypeHeaderValueString);

                multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

                var response = await httpClient.PostAsync(url, multipartFormContent);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
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
