using Autofac;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System.Threading;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public interface IDesignSurfaceMenuBuilder
    {
        CorpusNodeMenuItemViewModel CreateCorpusNodeSeparatorMenuItem();

        void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode);

        ParallelCorpusConnectionMenuItemViewModel CreateParallelCorpusConnectionSeparatorMenuItem();

        void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds);
        void CreateOnlyForNonResouceCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpus);
        void CreateCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpus);

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
            return new CorpusNodeMenuItemViewModel()
            {
                Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                IsSeparator = true
            };
        }

        public virtual void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode)
        {
        }


        public ParallelCorpusConnectionMenuItemViewModel CreateParallelCorpusConnectionSeparatorMenuItem()
        {
            return new ParallelCorpusConnectionMenuItemViewModel()
            {
                Header = "", Id = "SeparatorId", ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel,
                IsSeparator = true
            };
        }

        public virtual void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        {
        }

        public virtual void CreateOnlyForNonResouceCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpus)
        {
        }

        public virtual void CreateCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpus)
        {
        }
    }
}
