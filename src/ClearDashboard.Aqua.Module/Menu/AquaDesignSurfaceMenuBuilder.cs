using Autofac;
using ClearDashboard.Aqua.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System.Collections.Generic;

namespace ClearDashboard.Aqua.Module.Menu
{
    public  class AquaDesignSurfaceMenuBuilder : DesignSurfaceMenuBuilder
    {
        public AquaDesignSurfaceMenuBuilder(IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel, ILifetimeScope lifetimeScope) : base(projectDesignSurfaceViewModel, lifetimeScope)
        {
        }

        public override void CreateCorpusNodeMenu(CorpusNodeViewModel corpusNode)
        {
            corpusNode.MenuItems.Add(CreateCorpusNodeSeparatorMenuItem());

            var parameters = new List<Autofac.Core.Parameter>
            {
                //new NamedParameter("menuItems", corpusNode.MenuItems),
                new NamedParameter("corpusNodeViewModel", corpusNode)
            };
            var menuItem = LifetimeScope.Resolve<AquaCorpusAnalysisMenuItemViewModel>(parameters);

            corpusNode.MenuItems.Add(menuItem); 
           ;
        }
    }
}
