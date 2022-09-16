using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class ApiFunctionalTests : TestBase
    {
        public ApiFunctionalTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CreateParallelCorpus()
        {
            var projectDbContextFactory = ServiceProvider.GetService<ProjectDbContextFactory>();
            var assets = await projectDbContextFactory?.Get("EnhancedView");
            var context = assets.ProjectDbContext;

            var manuscriptCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "Manuscript");
            var zzSurCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "zz_SUR");

            var verseMappings = new List<VerseMapping>();
            var command = new CreateParallelCorpusCommand(new TokenizedTextCorpusId(manuscriptCorpus.Id),
                    new TokenizedTextCorpusId(zzSurCorpus.Id), verseMappings);
            var result = await Mediator?.Send(command);

        }
    }
}
