using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.Aqua.Module.Converters;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Interfaces;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.Services
{
    public class AquaManager : IAquaManager
    {
        protected string BaseAddressString { get; set; } = "https://6pu6b82gdk.us-east-1.awsapprunner.com/";
        //protected string BaseAddressString { get; set; } = "https://webhook.site/";
        protected string BearerAuthenticationKey { get; set; } = "7cf43ae52dw8948ddb663f9cae24488a4";

        //private readonly string versionPath_ = "7360d221-dc02-4a58-bb72-41641b92c6ca";
        private readonly string versionPath_ = "version";
        private readonly string languagePath_ = "language";
        private readonly string scriptPath_ = "script";

        private readonly string revisionPath_ = "revision";
        private readonly string assessmentPath_ = "assessment";
        private readonly string resultPath_ = "result";

        private readonly string revisionVersionIdKey = "version_abbreviation";

        protected IEventAggregator eventAggregator_;
        protected ILogger<AquaManager>? logger_;
        protected IMediator mediator_;
        protected IUserProvider userProvider_;
        protected HttpClient httpClient_;

        public AquaManager(
            IEventAggregator eventAggregator, 
            ILogger<AquaManager>? logger, 
            IMediator mediator, 
            IUserProvider userProvider)
        {
            eventAggregator_ = eventAggregator;
            logger_ = logger;
            mediator_ = mediator;
            userProvider_ = userProvider;

            httpClient_ = new HttpClient
            {
                BaseAddress = new Uri(BaseAddressString),
            };

            httpClient_.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerAuthenticationKey);

            eventAggregator_.SubscribeOnUIThread(this);
        }

        public async Task<IAquaManager.Version?> GetVersion(
            int id,
            CancellationToken cancellationToken = default)
        {
            var versions = await ListVersions(cancellationToken);
            return versions?
                .Where(v => v.id == id)
                .First()
                ?? null;
        }
        public async Task<IAquaManager.Version?> AddVersion(
            IAquaManager.Version version,
            CancellationToken cancellationToken = default
        )
        {

            return await PostJsonAsync(
                httpClient_,
                versionPath_,
                version,
                cancellationToken);

            //ex: https://6pu6b82gdk.us-east-1.awsapprunner.com/version?name=foo&isoLanguage=blah&isoScript=bing&abbreviation=www&machineTranslation=false

            //string versionJson = JsonSerializer.Serialize(version, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            //var versionDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(versionJson);

            //return await PostKeyValueDataAsQueryStringAsync(
            //    httpClient_,
            //    versionPath_,
            //    versionDictionary,
            //    cancellationToken
            //    );

            //await SlowTask("AddVersion", 10, cancellationToken, progress);
            //return "versionId";
        }

        public async Task<IEnumerable<IAquaManager.Version>?> ListVersions(
            CancellationToken cancellationToken = default)
        {
            return await GetFromJsonAsync<IEnumerable<IAquaManager.Version>>(
                httpClient_,
                versionPath_,
                new Dictionary<string, string>(),
                cancellationToken);
        }
        public async Task DeleteVersion(
            string abbreviation,
            CancellationToken cancellationToken = default)
        {
            // https://6pu6b82gdk.us-east-1.awsapprunner.com/version?version_abbreviation=lkj

            await DeleteAsync(
                httpClient_,
                versionPath_,
                new Dictionary<string, string>() { { revisionVersionIdKey, abbreviation } },
                cancellationToken);
            return;
        }

        public async Task<IEnumerable<Language>?> ListLanguages(
            CancellationToken cancellationToken = default)
        {
            return await GetFromJsonAsync<IEnumerable<Language>>(
                httpClient_,
                languagePath_,
                new Dictionary<string, string>(),
                cancellationToken);
        }
        public async Task<IEnumerable<Script>?> ListScripts(
            CancellationToken cancellationToken = default)
        {
            return await GetFromJsonAsync<IEnumerable<Script>>(
                httpClient_,
                scriptPath_,
                new Dictionary<string, string>(),
                cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizedTextCorpusId"></param>
        /// <param name="revision"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progressReporter"></param>
        /// <returns>note that aqua returns "Revision ID" for now so the returned revision will still be empty.</returns>
        public async Task<Revision?> AddRevision(
            TokenizedTextCorpusId tokenizedTextCorpusId,
            Revision revision,
            CancellationToken cancellationToken = default,
            IProgress<ProgressStatus>? progressReporter = null)
        {
            //await SlowTask("AddRevision", 10, cancellationToken, progress);
            //return "revisionId";

            //https://6pu6b82gdk.us-east-1.awsapprunner.com/revision?version_abbreviation=pppp&published=false'

            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator_, tokenizedTextCorpusId);

            var corpusInRevisionFormat = CorpusInBibleNLPFormatInVrefsOrder(tokenizedTextCorpus);

            var queryDict = new Dictionary<string, string>()
                    {
                        {revisionVersionIdKey, revision.version_abbreviation! },
                        {"published", revision.published.ToString() }
                    };
            if (revision.name != null)
                queryDict.Add("name", revision.name);

            var returnedRevision = await PostStringAsFile<Revision>
            (
                httpClient_, 
                QueryHelpers.AddQueryString
                (
                    revisionPath_,
                    queryDict
                ),
                corpusInRevisionFormat,
                cancellationToken: cancellationToken,
                progressReporter: progressReporter
            );

            var revisions = await ListRevisions(
                revision?.version_abbreviation 
                    ?? throw new InvalidStateEngineException(name:"revision.version_abbreviation", value: "null"), 
                cancellationToken);
            return revisions?
                .Where(r => r.id == returnedRevision?.RevisionId)
                .First()
                ?? null;
        }
        public async Task<IEnumerable<Revision>?> ListRevisions(
            string versionAbbreviation,
            CancellationToken cancellationToken = default)
        {
            // https://6pu6b82gdk.us-east-1.awsapprunner.com/revision?version_abbreviation=kjh%27

            return await GetFromJsonAsync<IEnumerable<Revision>>(
                httpClient_,
                revisionPath_,
                new Dictionary<string, string>() { { revisionVersionIdKey, versionAbbreviation} },
                cancellationToken);
        }

        public async Task DeleteRevision(
            int revisionId,
            CancellationToken cancellationToken = default)
        {
            //https://6pu6b82gdk.us-east-1.awsapprunner.com/revision?revision=6

            await DeleteAsync(
                httpClient_,
                revisionPath_,
                new Dictionary<string, string>() { { "revision", revisionId.ToString() } },
                cancellationToken);
            return;
        }

        public async Task<Assessment?> AddAssessment(
            Assessment assessment,
            CancellationToken cancellationToken = default)
        {
            /*
                'https://6pu6b82gdk.us-east-1.awsapprunner.com/assessment' \
                  -H 'accept: application/json' \
                  -H 'Content-Type: application/json' \
                  -d '{
                  "assessment": 0,
                  "revision": 0,
                  "reference": 0,
                  "type": "dummy"
            */

            return await PostJsonAsync(
                httpClient_,
                assessmentPath_,
                assessment,
                cancellationToken);
        }

        private class AssessmentsEnumeration
        {
            public IEnumerable<Assessment>? Assessments { get; set; }
        }
        public async Task<IEnumerable<Assessment>?> ListAssessments(
            int revisionId,
            CancellationToken cancellationToken = default)
        {
            //'https://6pu6b82gdk.us-east-1.awsapprunner.com/assessment

            var assessmentsEnumeration = await GetFromJsonAsync<AssessmentsEnumeration>(
                    httpClient_,
                    assessmentPath_,
                    new Dictionary<string, string>(),
                    cancellationToken);
            var assessments = assessmentsEnumeration?.Assessments 
                ?? throw new InvalidStateEngineException(name: "assessmentsEnumeration", value: "null");

            return assessments?
                .Where(a => a.revision == revisionId)
                ?? throw new InvalidDataEngineException(name: "versions", value: "null", message: $"ListVersions returned a list that didn't contain {revisionId}");
        }
        public async Task DeleteAssessment(
            int assessmentId,
            CancellationToken cancellationToken = default)
        {
            //'https://6pu6b82gdk.us-east-1.awsapprunner.com/assessment?assessment_id=7'

            await DeleteAsync(
                httpClient_,
                assessmentPath_,
                new Dictionary<string, string>() { { "assessment_id", assessmentId.ToString()} },
                cancellationToken);
            return;
        }

        public async Task<Result?> GetResult(
            int assessmentId,
            string bookAbbreviation,
            int chapterNumber,
            CancellationToken cancellationToken = default)
        {
            //'https://6pu6b82gdk.us-east-1.awsapprunner.com/result?assessment_id=8'

            return await GetFromJsonAsync<Result>(
                httpClient_,
                resultPath_,
                new Dictionary<string, string>() {
                    {"assessment_id", assessmentId.ToString()},
                    {"book", bookAbbreviation },
                    {"chapter", chapterNumber.ToString()}
                },
                cancellationToken);
        }


        #region HttpClient utilities
        public static async Task<T?> GetFromJsonAsync<T>(
            HttpClient httpClient, 
            string protocolHostPortFileString, 
            Dictionary<string, string>? query,
            CancellationToken cancellationToken)
        {
            var uri = QueryHelpers.AddQueryString(protocolHostPortFileString, query);
            return await httpClient.GetFromJsonAsync<T>(uri, cancellationToken);
        }

        public static async Task<T?> PostJsonAsync<T>(
            HttpClient httpClient,
            string protocolHostPortFileString,
            T obj,
            CancellationToken cancellationToken = default)
        {
            string serializedObj = JsonSerializer.Serialize(obj,new JsonSerializerOptions() 
            { 
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var content = new System.Net.Http.StringContent(serializedObj, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));
            using HttpResponseMessage response = await httpClient.PostAsync(protocolHostPortFileString, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var contentString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(contentString);
        }

        public static async Task DeleteAsync(
            HttpClient httpClient,
            string protocolHostPortFileString,
            Dictionary<string, string>? query,
            CancellationToken cancellationToken)
        {
            var uri = QueryHelpers.AddQueryString(protocolHostPortFileString, query);

            using HttpResponseMessage response = await httpClient.DeleteAsync(
                uri,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            return;
        }

        public static async Task<string> PostKeyValueDataAsQueryStringAsync(
            HttpClient httpClient,
            string uriString,
            Dictionary<string, string>? keyValueData,
            CancellationToken cancellationToken)
        {
            var uri = QueryHelpers.AddQueryString(uriString, keyValueData);

            using HttpResponseMessage response = await httpClient.PostAsync(
                uri,
                null,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<T?> PostStringAsFile<T>(
            HttpClient httpClient,
            string uriString,
            string stringContent,
            TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null,
            IProgress<ProgressStatus>? progressReporter = null,
            string name = "file",
            string fileName = "corpus.txt",
            string mediaTypeHeaderValueString = "text/plain")
        {
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                var streamContent = new BufferedStringHttpContent(stringContent, progressReporter);
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaTypeHeaderValueString);

                multipartFormContent.Add(streamContent, name: name, fileName: fileName);
                HttpResponseMessage response;
                if (cancellationToken != null)
                    response = await httpClient.PostAsync(uriString, multipartFormContent, (CancellationToken)cancellationToken!);
                else
                    response = await httpClient.PostAsync(uriString, multipartFormContent);

                response.EnsureSuccessStatusCode();
                var returnString = await response.Content.ReadAsStringAsync();
                var returnJson = JsonSerializer.Deserialize<T>(returnString);
                return returnJson;
            }
        }

        private class BufferedStringHttpContent : HttpContent
        {
            readonly int bufferSize_;
            MemoryStream memoryStream_;
            IProgress<ProgressStatus>? progressReporter_;

            public BufferedStringHttpContent(string str, 
                IProgress<ProgressStatus>? progressReporter,
                int bufferSize = 524288) //4096 * 128
            {
                var memoryStream = new MemoryStream();

                var writer = new StreamWriter(memoryStream);
                writer.Write(str);
                writer.Flush();
                memoryStream.Position = 0;
                memoryStream_ = memoryStream;
                progressReporter_ = progressReporter;
                bufferSize_ = bufferSize;
            }
            protected override async Task SerializeToStreamAsync(System.IO.Stream stream, System.Net.TransportContext? context)
            {
                byte[] buffer = new byte[bufferSize_];
                int read;
                int count = 0;
                while ((read = memoryStream_.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, read);
                    count++;
                    progressReporter_?.Report(new ProgressStatus(100.0 * count * buffer.Length / memoryStream_.Length));
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = memoryStream_.Length;
                return true;
            }
        }
        protected  async Task<int> ProcessUrlAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            logger_!.LogDebug($"{url,-60} {content.Length,10:#,#}");

            return content.Length;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textCorpus"></param>
        /// <param name="vrefStrings">A file with VerseRef.ToString(), one per line, in a particular order </param>
        /// <returns></returns>
        public static string CorpusInBibleNLPFormatInVrefsOrder(
            ITextCorpus textCorpus,
            IEnumerable<string>? vrefStrings = null)
        {
            if (vrefStrings == null)
            {
                vrefStrings = File.ReadAllLines(@$"services{Path.DirectorySeparatorChar}vref.txt");
            }
            StringBuilder corpus = new StringBuilder();
            var verseRefStringToTextMap = textCorpus
                .ExtractScripture()
                .ToDictionary(t => t.RefCorpusVerseRef.ToString(), t => t.Text);

            foreach (var vrefString in vrefStrings)
            {
                if (verseRefStringToTextMap.ContainsKey(vrefString))
                    corpus.AppendFormat("{0}{1}", verseRefStringToTextMap[vrefString], Environment.NewLine);
                else
                    corpus.AppendFormat("{0}{1}", "", Environment.NewLine);
            }
            return corpus.ToString();
        }

        #endregion

        #region for experimentation
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
        #endregion
    }
}
