using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Paratext;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests.ProjectManager
{
    public class LoadProjectQueryHandlerTest : TestBase
    {
        private ParatextProxy _paratextProxy;
        private string _projectName;
        private IMediator _mediator;
        private DashboardProjectManager _projectManager;
        private ILogger<DataAccessLayer.ProjectManager> _logger;
        private ProjectDbContextFactory _projectNameDbContextFactory;
        private IEventAggregator _eventAggregator;
        private IWindowManager _windowManager;
        private INavigationService _navigationService;
        private ILifetimeScope _lifetimeScope;

        public LoadProjectQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
            _projectName = "1";
            _projectManager = new DashboardProjectManager( _eventAggregator,  _paratextProxy, _logger,  _windowManager,  _navigationService, _lifetimeScope);
        }

        [Fact]
        public async Task LoadProjectQueryTest()
        {
            //var resultA = await _mediator.Send(new LoadProjectQuery(_projectName),CancellationToken.None);
            //var resultB = _projectManager.LoadProject(_projectName);
            //Assert.Equal(resultA.Data,resultB.Result);
        }
    }
}
