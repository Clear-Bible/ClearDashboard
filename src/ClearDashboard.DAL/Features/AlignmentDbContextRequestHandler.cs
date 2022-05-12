using ClearDashboard.DataAccessLayer.Data;

namespace ClearDashboard.DataAccessLayer.Features
{
    public class AlignmentDbContextRequestHandler
    {
        private ProjectNameDbContextFactory _projectNameDbContextFactory;

        private AlignmentContext _alignmentContext;
        public AlignmentDbContextRequestHandler(ProjectNameDbContextFactory projectNameDbContextFactory)
        {
            _projectNameDbContextFactory = projectNameDbContextFactory;
            //_alignmentContext = _projectNameDbContextFactory.Get()
        }
    }
}
