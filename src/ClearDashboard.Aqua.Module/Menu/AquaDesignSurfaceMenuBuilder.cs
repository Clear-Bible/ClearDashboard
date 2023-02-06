using Autofac;
using ClearDashboard.Aqua.Module.ViewModels.Menus;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System;
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
                //FIXMEAQUA: need to add in tokenizedTextCorpus submenu and provide something
                // enabling MeuItemViewModel to determine TokenizedTextCorpusId
                
                //new NamedParameter("menuItems", corpusNode.MenuItems),
                new NamedParameter("corpusNodeViewModel", corpusNode),
                new NamedParameter("tokenizedTextCorpusId", new TokenizedTextCorpusId(Guid.NewGuid()))
            };
            var menuItem = LifetimeScope.Resolve<AquaCorpusAnalysisMenuItemViewModel>(parameters);

            corpusNode.MenuItems.Add(menuItem);
        }

        public override void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        {
            throw new NotImplementedException();
        }
    }
}
