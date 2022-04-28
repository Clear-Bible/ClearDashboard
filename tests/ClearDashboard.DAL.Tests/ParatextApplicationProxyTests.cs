

using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Paratext;
using Microsoft.Extensions.DependencyInjection;
using ParaTextPlugin.Data.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class ParatextApplicationProxyTests : TestBase
    {
        public ParatextApplicationProxyTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override void SetupDependencyInjection()
        {
            Services.AddScoped<IParatextApplicationProxy, ParatextApplicationProxy>();
            base.SetupDependencyInjection();
        }

        [Fact]
        public async Task GetCurrentVerseTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();

            var verse = await proxy.GetCurrentVerse();

            Assert.NotNull(verse);
            Output.WriteLine($"Received current verse: {verse}");

        }

        [Fact]
        public async Task GetProjectBiblicalTermsTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();
            var biblicalTerms = await proxy.GetProjectBiblicalTerms();
            Assert.NotNull(biblicalTerms);
            Output.WriteLine($"Received {biblicalTerms.Count} biblical terms from Paratext");

        }

        [Fact]
        public async Task GetAllBiblicalTermsTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();
            var biblicalTerms = await proxy.GetAllBiblicalTerms();
            Assert.NotNull(biblicalTerms);
            Output.WriteLine($"Received {biblicalTerms.Count} biblical terms from Paratext");

        }

        [Fact]
        public async Task GetParatextProjectTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();
            var paraTextProject = await proxy.GetParaTextProject();
            Assert.NotNull(paraTextProject);
            Output.WriteLine($"Received {paraTextProject.LongName} project from Paratext");

        }

        [Fact]
        public async Task GetUsxTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();
            var usx = await proxy.GetUsx();
            Assert.NotNull(usx);
            Output.WriteLine($"USX: {usx}");

        }

        [Fact(Skip="The Paratext plugin is having issues serializing notes.")]
        public async Task GetNotesTest()
        {
            var proxy = ServiceProvider.GetService<IParatextApplicationProxy>();
            var notes = await proxy.GetNotes(new GetNotesData {BookId = 1, ChapterId = 2, IncludeResolved = true});
            Assert.NotNull(notes);
            Output.WriteLine($"Received {notes.Count} notes from Paratext.");

        }
    }
}
