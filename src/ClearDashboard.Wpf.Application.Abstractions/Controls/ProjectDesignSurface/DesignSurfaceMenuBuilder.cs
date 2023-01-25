using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Project;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public interface IDesignSurfaceMenuBuilder
    {
        CorpusNodeMenuItemViewModel CreateCorpusNodeSeparatorMenuItem();

        void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode);
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

    }
}
