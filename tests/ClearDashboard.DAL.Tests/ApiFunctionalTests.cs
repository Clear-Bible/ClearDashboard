using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core.Lifetime;
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
            SetupProjectDatabase("EnhancedView", false, true);

            var manuscriptCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "Manuscript");
            var zzSurCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "zz_SUR");

            var manuscriptTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(manuscriptCorpus.Id));
            var zzSurTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator, new TokenizedTextCorpusId(zzSurCorpus.Id));

            // ---------------------------------------------------
            // Code to check for TokenId duplicates in manuscript:
            // ---------------------------------------------------
            //var manBookIds = manuscriptTokenizedTextCorpus.Texts.Select(t => t.Id).ToList();

            //foreach (var bookId in manBookIds)
            //{
            //    var dupsFound = 0;

            //    var command = new GetTokensByTokenizedCorpusIdAndBookIdQuery(manuscriptTokenizedTextCorpus.TokenizedTextCorpusId, bookId);
            //    var result = await Mediator!.Send(command);

            //    foreach (var verse in result.Data!)
            //    {
            //        verse.Tokens
            //            .SelectMany(t => (t is CompositeToken) ? ((CompositeToken)t).Tokens : new List<Token>() { t })
            //            .GroupBy(t => t.TokenId)
            //            .Where(g => g.Count() > 1)
            //            .Select(g => g.Key)
            //            .ToList()
            //            .ForEach(tid =>
            //            {
            //                Output.WriteLine($"Manuscript - book ID: {bookId} - duplicate TokenId:  {tid}");
            //                dupsFound++;
            //            });
            //    }

            //    if (dupsFound > 0)
            //    {
            //        Output.WriteLine("");
            //    }
            //}

            // ---------------------------------------------------
            // Code to check for TokenId duplicates in zz_SUR:
            // ---------------------------------------------------
            //var zzBookIds = zzSurTokenizedTextCorpus.Texts.Select(t => t.Id).ToList();

            //foreach (var bookId in zzBookIds)
            //{
            //    var dupsFound = 0;

            //    var command = new GetTokensByTokenizedCorpusIdAndBookIdQuery(zzSurTokenizedTextCorpus.TokenizedTextCorpusId, bookId);
            //    var result = await Mediator!.Send(command);

            //    foreach (var verse in result.Data!)
            //    {
            //        verse.Tokens
            //            .SelectMany(t => (t is CompositeToken) ? ((CompositeToken)t).Tokens : new List<Token>() { t })
            //            .GroupBy(t => t.TokenId)
            //            .Where(g => g.Count() > 1)
            //            .Select(g => g.Key)
            //            .ToList()
            //            .ForEach(tid =>
            //            {
            //                Output.WriteLine($"\tzzSUR - book ID: {bookId} - duplicate TokenId:  {tid}");
            //                dupsFound++;
            //            });
            //    }

            //    if (dupsFound > 0)
            //    {
            //        Output.WriteLine("");
            //    }
            //}

            var parallelTextCorpus = manuscriptTokenizedTextCorpus.EngineAlignRows(zzSurTokenizedTextCorpus, new SourceTextIdToVerseMappingsFromVerseMappings(EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    manuscriptTokenizedTextCorpus.Versification,
                    zzSurTokenizedTextCorpus.Versification)));
            var parallelTokenizedCorpus = await parallelTextCorpus.CreateAsync("manuscript - ZZ_SUR", Container!);

            ////var verseMappings = new List<VerseMapping>();
            //var command = new CreateParallelCorpusCommand(new TokenizedTextCorpusId(manuscriptCorpus.Id),
            //        new TokenizedTextCorpusId(zzSurCorpus.Id), verseMappings);
            //var result = await Mediator?.Send(command);

        }        
        
        [Fact]
        public async Task GetParallelCorpus()
        {
            SetupProjectDatabase("EnhancedView", false, true);

            //var manuscriptCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "Manuscript");
            //var zzSurCorpus = context.TokenizedCorpora.FirstOrDefault(tc => tc.DisplayName == "zz_SUR");

            try
            {
                var parallelCorpus = ProjectDbContext.ParallelCorpa.FirstOrDefault();

                var corpus = await ParallelCorpus.GetAsync(Container!, new ParallelCorpusId(parallelCorpus.Id));

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
