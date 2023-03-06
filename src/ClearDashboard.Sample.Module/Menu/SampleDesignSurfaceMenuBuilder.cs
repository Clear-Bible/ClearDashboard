using Autofac;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Sample.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System;
using System.Collections.Generic;

namespace ClearDashboard.Sample.Module.Menu
{
    public class SampleDesignSurfaceMenuBuilder : DesignSurfaceMenuBuilder
    {
        public SampleDesignSurfaceMenuBuilder(IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel, ILifetimeScope lifetimeScope) : base(projectDesignSurfaceViewModel, lifetimeScope)
        {
        }

        public override void CreateOnlyForNonResouceCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpusId)
        {
            //if (corpusNodeMenuItemViewModel?.MenuItems == null)
            //    throw new InvalidParameterEngineException(name: "MenuItems", value: "null", message: "Cannot add to null menu.");

            //corpusNodeMenuItemViewModel.MenuItems.Add(CreateCorpusNodeSeparatorMenuItem());

            //var parameters = new List<Autofac.Core.Parameter>
            //{
            //    new NamedParameter("corpusNodeMenuItemViewModel", corpusNodeMenuItemViewModel),
            //    new NamedParameter("tokenizedTextCorpusId", tokenizedCorpusId)
            //};
            //var menuItem = LifetimeScope.Resolve<SampleCorpusAnalysisMenuItemViewModel>(parameters);

            //corpusNodeMenuItemViewModel.MenuItems.Add(menuItem);
        }

        public override void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        {
            // TODO:  implement if required.
        }
    }
}
