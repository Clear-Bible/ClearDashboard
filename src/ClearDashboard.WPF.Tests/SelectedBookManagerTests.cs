using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public sealed class IgnoreXunitAnalyzersRule1013Attribute : Attribute { }

    [IgnoreXunitAnalyzersRule1013]
    public class EventHandlerAttribute : Attribute { }
    public class SelectedBookManagerTests : TestBase, IHandle<BackgroundTaskChangedMessage>
    {
        public SelectedBookManagerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SelectedBooksTest()
        {
            /*
3f0f2b0426e1457e8e496834aaa30fce00000000abcdefff / BYZ
ab4261bb84031c8d984468f2e9d86df7ffe52809abcdefff / CRSB
3f0f2b0426e1457e8e496834aaa30fce00000001abcdefff / GRK
3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff / HEB/GRK
3f0f2b0426e1457e8e496834aaa30fce00000003abcdefff / LXA
3f0f2b0426e1457e8e496834aaa30fce00000004abcdefff / LXX/GRK
71c6eab17ae5b66792820b598a4ba91f7f9bed88abcdefff / NIV11
3f0f2b0426e1457e8e496834aaa30fce000000f1abcdefff / OGRK
3f0f2b0426e1457e8e496834aaa30fce000000f2abcdefff / OHEB/OGRK
3f0f2b0426e1457e8e496834aaa30fce00000005abcdefff / SYA
3f0f2b0426e1457e8e496834aaa30fce00000006abcdefff / SYR
2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f / zz_SUR
7b73e1e76a73a722e21de6f5154c06dd96cabd34 / zz_SURBT
             */

            try
            {
                await StartParatextAsync();

                var manager = Container!.Resolve<SelectedBookManager>();
                var mediator = Container!.Resolve<IMediator>();

                var zzSurProjectId = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f";
                var zzSurBtProjectId = "7b73e1e76a73a722e21de6f5154c06dd96cabd34";

                var resultZzSur = await mediator!.Send(new GetCheckUsfmQuery(zzSurProjectId), CancellationToken.None);
                Assert.True(resultZzSur.Success);
                var resultZzSurBt = await mediator!.Send(new GetCheckUsfmQuery(zzSurBtProjectId), CancellationToken.None);
                Assert.True(resultZzSurBt.Success);

                var usfmErorsByParatextProjectId = new Dictionary<string, IEnumerable<UsfmError>>
                {
                    { zzSurProjectId, resultZzSur.Data!.UsfmErrors },
                    { zzSurBtProjectId, resultZzSurBt.Data!.UsfmErrors}
                };

                await manager!.InitializeBooks(usfmErorsByParatextProjectId, false, false, CancellationToken.None);
                var selectedBooks = manager.SelectedBooks;

                Output.WriteLine("\nBooks with Usfm Errors");
                foreach ( var book in selectedBooks.Where(b => b.HasUsfmError) )
                {
                    Output.WriteLine($"{book.Abbreviation} / {book.BookName}");
                }

                Output.WriteLine($"\nBooks Enabled [count: {selectedBooks.Where(b => b.IsEnabled).Count()}]");
                foreach (var book in selectedBooks.Where(b => !b.IsEnabled))
                {
                    Output.WriteLine($"{book.Abbreviation} / {book.BookName}");
                }

                Output.WriteLine($"\nBooks Disabled [count: {selectedBooks.Where(b => !b.IsEnabled).Count()}]");
                foreach (var book in selectedBooks.Where(b => !b.IsEnabled))
                {
                    Output.WriteLine($"{book.Abbreviation} / {book.BookName}");
                }
            }
            finally
            {
                await StopParatextAsync();
                await DeleteDatabaseContext();
            }

        }

        [Fact]
        public async Task TestProcessRunner()
        {
            try
            {
                await StartParatextAsync();

                var processRunner = Container!.Resolve<ProjectTemplateProcessRunner>();
                var mediator = Container!.Resolve<IMediator>();
                var eventAggregator = Container!.Resolve<IEventAggregator>();
                eventAggregator.SubscribeOnUIThread(this);

                var projectMetadatas = await mediator.Send(new GetProjectMetadataQuery(), CancellationToken.None);
                projectMetadatas.ThrowIfCanceledOrFailed();

                // FIXME:   maybe use builder pattern?  Caller could add(), add() etc. and then
                // eventually call "RunAsync()".  
                ParatextProjectMetadata paratextProject = projectMetadatas.Data!.First(e => e.Name == "zz_SUR");
                ParatextProjectMetadata paratextBtProject = projectMetadatas.Data!.First(e => e.Name == "zz_SURBT");
                ParatextProjectMetadata paratextLwcProject = projectMetadatas.Data!.First(e => e.Name == "NIV11");
                //IEnumerable<string> bookIds = FileGetBookIds.BookIds.Select(e => e.silCannonBookAbbrev);
                IEnumerable<string> bookIds = new List<string> { "GEN", "EXO" };

                processRunner.StartRegistration();

                var hebrewTaskName = processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptHebrew);
                var greekTaskName = processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptGreek);
                var paratextTaskName = processRunner.RegisterParatextProjectCorpusTask(paratextProject, Tokenizers.LatinWordTokenizer, bookIds, true);
                var paratextBtTaskName = processRunner.RegisterParatextProjectCorpusTask(paratextBtProject, Tokenizers.LatinWordTokenizer, bookIds, true);
                var paratextLwcTaskName = processRunner.RegisterParatextProjectCorpusTask(paratextLwcProject, Tokenizers.LatinWordTokenizer, bookIds, false);

                var taskNameSetParatextLwc = processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    paratextLwcTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());

                var taskNameSetLwcBt = processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    paratextBtTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());

                var taskNameLwcHebrewBt = processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    hebrewTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());

                var taskNameParatextGreekBt = processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    greekTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());

                Stopwatch sw = new();
                await processRunner.RunRegisteredTasks(sw);
                sw.Stop();
            }
            finally
            {
                await StopParatextAsync();
//                await DeleteDatabaseContext();
            }
        }

        [Fact]
        public async Task TestLongProcessing()
        {
            Stopwatch sw = new();
            sw.Start();
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            Output.WriteLine($"Starting {nameof(Long1)} Elapsed={sw.Elapsed}");
            var long1 = Long1(token);
            Output.WriteLine($"Starting {nameof(Long2)} Elapsed={sw.Elapsed}");
            var long2 = Long2(token);
            Output.WriteLine($"Starting {nameof(Long3)} Elapsed={sw.Elapsed}");
            var long3 = Long3(token);
            //var long4 = Long4(token);
            //var long5 = Long5(token);

            Output.WriteLine($"Starting {nameof(AfterLong1And2)} Elapsed={sw.Elapsed}");
            var afterLong1And2 = AfterLong1And2(long1, long2, sw, tokenSource);
            Output.WriteLine($"Starting {nameof(AfterLong1And3)} Elapsed={sw.Elapsed}");
            var afterLong1And3 = AfterLong1And3(long1, long3, sw, tokenSource);

            var whenAllTask = Task.WhenAll(afterLong1And2, afterLong1And3);

            try
            {
                Output.WriteLine($"awaiting Task.WhenAll({nameof(AfterLong1And2)}, {nameof(AfterLong1And3)})");
                Output.WriteLine($"{afterLong1And2.IsFaulted} Elapsed={sw.Elapsed}");
                await whenAllTask;
            }
            catch (Exception ex)
            {
                Output.WriteLine($"{ex.GetType().Name}:  {ex.Message} Elapsed={sw.Elapsed}");
                Output.WriteLine($"IsFaulted:  {whenAllTask.IsFaulted}");
                if (whenAllTask.Exception != null)
                {
                    Output.WriteLine($"Exception: {whenAllTask.Exception}");
                }
                tokenSource.Cancel();
            
            }
            finally
            {
                await StopParatextAsync();
                await DeleteDatabaseContext();
                sw.Stop();
                Output.WriteLine($"Elapsed={sw.Elapsed}");
            }
        }

        private async Task AfterLong1And2(Task long1, Task long2, Stopwatch sw, CancellationTokenSource tokenSource)
        {
            Output.WriteLine($"{nameof(AfterLong1And2)} before WhenAll  Elapsed={sw.Elapsed}");
            var whenAllTask = Task.WhenAll(long1, long2);

            try
            {
                await whenAllTask;
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Exception thrown by AfterLong1And2.WhenAll: {ex.GetType().Name} {ex.Message}  Elapsed={sw.Elapsed}");
                tokenSource.Cancel();
                throw;
            }

            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(AfterLong1And2)} before delay (in Task.Run)  Elapsed={sw.Elapsed}");
                await Task.Delay(8000, tokenSource.Token);
                tokenSource.Token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(AfterLong1And2)} after delay (in Task.Run)  Elapsed={sw.Elapsed}");
            }, tokenSource.Token);
        }

        private async Task AfterLong1And3(Task long1, Task long3, Stopwatch sw, CancellationTokenSource tokenSource)
        {
            Output.WriteLine($"{nameof(AfterLong1And3)} before WhenAll  Elapsed={sw.Elapsed}");
            await Task.WhenAll(long1, long3);

            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(AfterLong1And3)} before delay (in Task.Run)  Elapsed={sw.Elapsed}");
                await Task.Delay(10000, tokenSource.Token);
                tokenSource.Token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(AfterLong1And3)} after delay (in Task.Run)  Elapsed={sw.Elapsed}");
            }, tokenSource.Token);

        }

        private async Task Long1(CancellationToken token)
        {
            Output.WriteLine($"{nameof(Long1)} before Task.Run");
            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(Long1)} before delay (in Task.Run)");
                await Task.Delay(5000, token);
                token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(Long1)} after delay (in Task.Run)");
            }, token);
        }

        private async Task Long2(CancellationToken token)
        {
            Output.WriteLine($"{nameof(Long2)} before Task.Run");
            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(Long2)} before delay (in Task.Run)");
                Output.WriteLine("\t****Throwing ArgumentException***");
                throw new ArgumentException("what?", "boo");
                await Task.Delay(4000, token);
                token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(Long2)} after delay (in Task.Run)");
            }, token);
        }

        private async Task Long3(CancellationToken token)
        {
            Output.WriteLine($"{nameof(Long3)} before Task.Run");
            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(Long3)} before delay (in Task.Run)");
                await Task.Delay(7000, token);
                token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(Long3)} after delay (in Task.Run)");
            }, token);
        }

        private async Task Long4(CancellationToken token)
        {
            Output.WriteLine($"{nameof(Long4)} before Task.Run");
            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(Long4)} before delay (in Task.Run)");
                await Task.Delay(5000, token);
                token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(Long4)} after delay (in Task.Run)");
            }, token);
        }

        private async Task Long5(CancellationToken token)
        {
            Output.WriteLine($"{nameof(Long5)} before Task.Run");
            await Task.Run(async () =>
            {
                Output.WriteLine($"\t{nameof(Long5)} before delay (in Task.Run)");
                await Task.Delay(5000, token);
                token.ThrowIfCancellationRequested();
                Output.WriteLine($"\t{nameof(Long5)} after delay (in Task.Run)");
            }, token);
        }

        [EventHandler]
        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            Output.WriteLine($"{message.Status.TaskLongRunningProcessStatus}: {message.Status.Name}: {message.Status.Description}");
            await Task.CompletedTask;
        }
    }
}
