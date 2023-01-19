using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using ClearDashboard.Aqua.Module.ViewModels.Menus;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;

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
                new NamedParameter("menuItems", corpusNode.MenuItems),
                new NamedParameter("corpusNodeViewModel", corpusNode)
            };
            var menuItem = LifetimeScope.Resolve<AquaCorpusAnalysisMenuItemViewModel>(parameters);
           
            corpusNode.MenuItems.Add(menuItem); 
           ;
        }
    }
}
