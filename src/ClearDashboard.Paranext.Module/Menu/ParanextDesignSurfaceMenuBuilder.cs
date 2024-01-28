using Autofac;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Paranext.Module.Models;
using ClearDashboard.Paranext.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace ClearDashboard.Paranext.Module.Menu
{
    public class ParanextDesignSurfaceMenuBuilder : DesignSurfaceMenuBuilder
    {
        public ParanextDesignSurfaceMenuBuilder(IProjectDesignSurfaceViewModel projectDesignSurfaceViewModel, ILifetimeScope lifetimeScope) : base(projectDesignSurfaceViewModel, lifetimeScope)
        {
        }

        public override void CreateOnlyForNonResouceCorpusNodeChildMenu(CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel, TokenizedTextCorpusId tokenizedCorpusId)
        {
            if (corpusNodeMenuItemViewModel?.MenuItems == null)
                throw new InvalidParameterEngineException(name: "MenuItems", value: "null", message: "Cannot add to null menu.");

            corpusNodeMenuItemViewModel.MenuItems.Add(CreateCorpusNodeSeparatorMenuItem());


            var paranextExtensionInfos = new List<(string menuId,  ParanextEnhancedViewItemMetadatum metadatum)>()
            {
                (
                    "AQuA Analysis", 
                    new ParanextEnhancedViewItemMetadatum()
                    {
                        DisplayName = "Dashboard AQuA Analysis Extension",
                        UrlString = "http://localhost:1212/paranextExtensionDashboard_aquaanalysis?assessment_id=211&version_id=71",
                        IsNewWindow = false
                    }
                ),
                (   "Lexicon", 
                    new ParanextEnhancedViewItemMetadatum()
                    {
                        DisplayName = "Dashboard Lexicon Extension",
                        UrlString = "http://localhost:1212/paranextExtensionDashboard_lexicon",
                        IsNewWindow = false
                    }
                )
            };

            foreach (var paranextExtensionInfo in paranextExtensionInfos)
            {
                var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("corpusNodeMenuItemViewModel", corpusNodeMenuItemViewModel),
                new NamedParameter("tokenizedTextCorpusId", tokenizedCorpusId),
                new NamedParameter("paranextExtensionMenuId", paranextExtensionInfo.menuId),
                new NamedParameter("paranextExtensionInfo", paranextExtensionInfo.metadatum),
            };
                var menuItem = LifetimeScope.Resolve<ParanextMenuItemViewModel>(parameters);

                corpusNodeMenuItemViewModel.MenuItems.Add(menuItem);
            }
        }

        //public override void CreateParallelCorpusConnectionMenu(ParallelCorpusConnectionViewModel parallelCorpusConnection, TopLevelProjectIds topLevelProjectIds)
        //{
        //     TODO:  implement if required.
        //}
    }
}
