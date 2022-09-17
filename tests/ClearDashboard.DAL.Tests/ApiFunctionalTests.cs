using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
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

            var manuscriptTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(manuscriptCorpus.Id));
            var zzSurTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(zzSurCorpus.Id));

            var parallelTextCorpus = manuscriptTokenizedTextCorpus.EngineAlignRows(zzSurTokenizedTextCorpus, new());
            var parallelTokenizedCorpus = await parallelTextCorpus.Create(Mediator!);

            ////var verseMappings = new List<VerseMapping>();
            //var command = new CreateParallelCorpusCommand(new TokenizedTextCorpusId(manuscriptCorpus.Id),
            //        new TokenizedTextCorpusId(zzSurCorpus.Id), verseMappings);
            //var result = await Mediator?.Send(command);

        }        
        
        [Fact]
        public async Task GetParallelCorpus()
        {
            var projectDbContextFactory = ServiceProvider.GetService<ProjectDbContextFactory>();
            var assets = await projectDbContextFactory?.Get("EnhancedView");
            var context = assets.ProjectDbContext;

            //var manuscriptCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "Manuscript");
            //var zzSurCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "zz_SUR");

            try
            {
                var parallelCorpus = context.ParallelCorpa.FirstOrDefault();

                var corpus = await ParallelCorpus.Get(Mediator, new ParallelCorpusId(parallelCorpus.Id));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //var verseMappings = new List<VerseMapping>();

            //var command = new GetParallelCorpusQuery(new TokenizedTextCorpusId(manuscriptCorpus.Id),
            //        new TokenizedTextCorpusId(zzSurCorpus.Id), verseMappings);
            //var result = await Mediator?.Send(command);

        }
    }
}
