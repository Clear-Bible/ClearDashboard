using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Tests;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Threading;
using System.Xml.Linq;
using Xunit.Abstractions;
using static ClearDashboard.Aqua.Module.Services.IAquaManager;

namespace ClearDashboard.Aqua.Module.Tests
{
    public class AquaManagerTests : TestBase
    {
        protected override void AddServices(ServiceCollection services)
        {
            base.AddServices(services);
            services.AddSingleton<IAquaManager, AquaManager>();
        }
        public AquaManagerTests(ITestOutputHelper output) : base(output)
        {
        }

        private static TextRow TextRow(VerseRef vref, string text = "", bool isSentenceStart = true,
            bool isInRange = false, bool isRangeStart = false)
        {
            return new TextRow(vref)
            {
                Segment = text.Length == 0 ? Array.Empty<string>() : text.Split(),
                IsSentenceStart = isSentenceStart,
                IsInRange = isInRange,
                IsRangeStart = isRangeStart,
                IsEmpty = text.Length == 0
            };
        }

        [Fact]
        public void BibleNLPCorpus__DictionaryTextCorpus_VerseMapping()
        {
            Versification.Table.Implementation.RemoveAllUnknownVersifications();
            string source = "&MAT 1:2 = MAT 1:1\nMAT 1:3 = MAT 1:2\nMAT 1:1 = MAT 1:3\n";
            ScrVers versificationSource;
            using (var reader = new StringReader(source))
            {
                versificationSource = Versification.Table.Implementation.Load(reader, "vers.txt", ScrVers.English, "custom");
            }

            var sourceCorpus = new DictionaryTextCorpus(
                new MemoryText("MAT", new[]
                {
                    TextRow(new VerseRef("MAT 1:1", versificationSource), "source MAT chapter one, verse one."),
                    TextRow(new VerseRef("MAT 1:2", versificationSource), "source MAT chapter one, verse two."),
                    TextRow(new VerseRef("MAT 1:3", versificationSource), "source MAT chapter one, verse three."),
                    TextRow(new VerseRef("MAT 1:4", versificationSource), "source MAT chapter one, verse four."),
                    TextRow(new VerseRef("MAT 1:5", versificationSource), "source MAT chapter one, verse five."),
                    TextRow(new VerseRef("MAT 1:6", versificationSource), "source MAT chapter one, verse six.")
                }),
                new MemoryText("MRK", new[]
                {
                    TextRow(new VerseRef("MRK 1:1", versificationSource), "source MRK chapter one, verse one."),
                    TextRow(new VerseRef("MRK 1:2", versificationSource), "source MRK chapter one, verse two."),
                    TextRow(new VerseRef("MRK 1:3", versificationSource), "source MRK chapter one, verse three."),
                    TextRow(new VerseRef("MRK 1:4", versificationSource), "source MRK chapter one, verse four."),
                    TextRow(new VerseRef("MRK 1:5", versificationSource), "source MRK chapter one, verse five."),
                    TextRow(new VerseRef("MRK 1:6", versificationSource), "source MRK chapter one, verse six.")
                }));

            var foo = AquaManager.CorpusInBibleNLPFormatInVrefsOrder(sourceCorpus);
        }

        [Fact]
        public void BibleNLPCorpus__TokenizedTextCorpus_VerseMapping()
        {
            Versification.Table.Implementation.RemoveAllUnknownVersifications();
            string source = "&MAT 1:2 = MAT 1:1\nMAT 1:3 = MAT 1:2\nMAT 1:1 = MAT 1:3\n";
            ScrVers versificationSource;
            using (var reader = new StringReader(source))
            {
                versificationSource = Versification.Table.Implementation.Load(reader, "vers.txt", ScrVers.English, "custom");
            }

            var sourceCorpus = new DictionaryTextCorpus(
                new MemoryText("MAT", new[]
                {
                    TextRow(new VerseRef("MAT 1:1", versificationSource), "source MAT chapter one, verse one."),
                    TextRow(new VerseRef("MAT 1:2", versificationSource), "source MAT chapter one, verse two."),
                    TextRow(new VerseRef("MAT 1:3", versificationSource), "source MAT chapter one, verse three."),
                    TextRow(new VerseRef("MAT 1:4", versificationSource), "source MAT chapter one, verse four."),
                    TextRow(new VerseRef("MAT 1:5", versificationSource), "source MAT chapter one, verse five."),
                    TextRow(new VerseRef("MAT 1:6", versificationSource), "source MAT chapter one, verse six.")
                }),
                new MemoryText("MRK", new[]
                {
                    TextRow(new VerseRef("MRK 1:1", versificationSource), "source MRK chapter one, verse one."),
                    TextRow(new VerseRef("MRK 1:2", versificationSource), "source MRK chapter one, verse two."),
                    TextRow(new VerseRef("MRK 1:3", versificationSource), "source MRK chapter one, verse three."),
                    TextRow(new VerseRef("MRK 1:4", versificationSource), "source MRK chapter one, verse four."),
                    TextRow(new VerseRef("MRK 1:5", versificationSource), "source MRK chapter one, verse five."),
                    TextRow(new VerseRef("MRK 1:6", versificationSource), "source MRK chapter one, verse six.")
                }));

            var foo = AquaManager.CorpusInBibleNLPFormatInVrefsOrder(sourceCorpus);
        }

        [Fact]
        public async void Foo()
        {
            var versionInfo = new IAquaManager.Version(null, "name1", "language1", "isoscript1", "abbreviation1", "rights1", 2, 4, true);
            var versionInfoJson = JsonConvert.SerializeObject(versionInfo, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(versionInfoJson);

            versionInfo = new IAquaManager.Version(null, "name1", "language1", "isoscript1", "abbreviation1");
            versionInfoJson = JsonConvert.SerializeObject(versionInfo, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(versionInfoJson);
        }

        [Fact]
        public async void PostFileEndpoint__BufferingAndProgress()
        {
            string content = string.Join("", Enumerable.Range(0, 10000000).Select(t => t.ToString()));
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:8080")
            };

            using var webServer = new SimpleRequestResponseWebServer(output: Output);

            EventWaitHandle startedWaitHandle = new AutoResetEvent(false);
            var startTask = webServer.Start(startedWaitHandle);

            startedWaitHandle.WaitOne();

            var returnString = await AquaManager.PostStringAsFile<string>(
                httpClient,
                "",
                content,
                progressReporter: new DelegateProgress(status =>
                {
                    Output.WriteLine($"Percent complete: {Math.Round((decimal)status.PercentCompleted, 0)}");
                })
            );

            Output.WriteLine(returnString);
            startTask.Wait();
            webServer.Stop();


            startedWaitHandle.Reset();
            startTask = webServer.Start(startedWaitHandle);

            startedWaitHandle.WaitOne();

            var streamContent = new StringContent(content);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");

            using var multipartFormContent = new MultipartFormDataContent();
            multipartFormContent.Add(streamContent, name: "file", fileName: "corpus.txt");
            HttpResponseMessage response = await httpClient.PostAsync("", multipartFormContent);

            response.EnsureSuccessStatusCode();
            var nonBufferedResponseAsPostString = await response.Content.ReadAsStringAsync();

            Assert.Equal(returnString.Length, nonBufferedResponseAsPostString.Length);
            //trim off about 45 characters at beginning and also at end to remove unique boundary guids.
            Assert.Equal(returnString.Substring(45, returnString.Length - 90), nonBufferedResponseAsPostString.Substring(45, returnString.Length - 90));
        }

        [Fact]
        public async void PostVersion()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            var version = await aquaManager.AddVersion(new IAquaManager.Version(
                null,
                "dw-testversion3",
                "eng",
                "Latn",
                "dwtv7"));

        }

        [Fact]
        public async void ListVersions()
        {
            try
            {
                var aquaManager = Container!.Resolve<IAquaManager>();

                var versions = await aquaManager.ListVersions();
            }
            catch (Exception ex)
            {
            }

        }

        [Fact]
        public async void GetVersion()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            var version = await aquaManager.GetVersion(14);

        }

        [Fact]
        public async void DeleteVersion()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            await aquaManager.DeleteVersion("dwtv4");

        }

        [Fact]
        public async void GetRevisions()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            var revisions = await aquaManager.ListRevisions("dwtv15");

        }

        [Fact]
        public async void GetLanguages()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            var languages = await aquaManager.ListLanguages();

        }

        [Fact]
        public async void GetAssessments()
        {
            var aquaManager = Container!.Resolve<IAquaManager>();

            var languages = await aquaManager.ListAssessments(2388);

        }
    }
}