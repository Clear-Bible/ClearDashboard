using Autofac;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class SelectedBookManagerTests : TestBase
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

                await manager!.InitializeBooks(usfmErorsByParatextProjectId, CancellationToken.None);
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
            }

        }
    }
}
