using Autofac;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public interface IDesignSurfaceMenuBuilder
    {
        CorpusNodeMenuItemViewModel CreateCorpusNodeSeparatorMenuItem();

        void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode);

        ParallelCorpusConnectionMenuItemViewModel CreateParallelCorpusConnectionSeparatorMenuItem();

        void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds);
    }

    public abstract class DesignSurfaceMenuBuilder : IDesignSurfaceMenuBuilder
    {

        protected IProjectDesignSurfaceViewModel ProjectDesignSurfaceViewModel { get; }
        protected ILifetimeScope LifetimeScope { get; }

        protected DesignSurfaceMenuBuilder(IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel, ILifetimeScope lifetimeScope)
        {
            ProjectDesignSurfaceViewModel = projectDesignSurfaceViewModel;
            LifetimeScope = lifetimeScope;
        }

        public CorpusNodeMenuItemViewModel CreateCorpusNodeSeparatorMenuItem()
        {
            return new CorpusNodeMenuItemViewModel
            {
                Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                IsSeparator = true
            };
        }

        public abstract void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode);


        public ParallelCorpusConnectionMenuItemViewModel CreateParallelCorpusConnectionSeparatorMenuItem()
        {
            return new ParallelCorpusConnectionMenuItemViewModel
            {
                Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                IsSeparator = true
            };
        }

        public abstract void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds);

    }
}
