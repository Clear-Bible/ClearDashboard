

using Autofac;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Aqua.Module.Menu;
using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.Aqua.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Plugin;
using ClearDashboard.Wpf.Application.Services;
using System.Reflection;
using System.Threading;
using System;
using System.Diagnostics;
using Autofac.Features.AttributeFilters;

namespace ClearDashboard.Aqua.Module
{
    public class AquaPluginModule : PluginModule
    {

        protected override void Load(ContainerBuilder builder)
        {
            
            base.Load(builder);
            builder.RegisterAquaDependencies();
        }

        protected override void RegisterJsonDiscriminatorRegistrar(ContainerBuilder builder)
        {
            builder.RegisterType<JsonDiscriminatorRegistrar>().As<IJsonDiscriminatorRegistrar>();
        }

        protected override void RegisterEnhancedViewAbstractions(ContainerBuilder builder)
        {
            //no-op for now
        }
    }

    internal static class ContainerBuilderExtensions
    {
        public static void RegisterAquaDependencies(this ContainerBuilder builder)
        {
            //manager

            builder.RegisterType<AquaManager>().As<IAquaManager>().SingleInstance();

            builder.RegisterType<AquaDesignSurfaceMenuBuilder>().As<IDesignSurfaceMenuBuilder>().WithAttributeFiltering();
            builder.RegisterType<AquaLocalizationService>().Keyed<ILocalizationService>("Aqua");
            builder.RegisterType<AquaCorpusAnalysisMenuItemViewModel>().AsSelf().WithAttributeFiltering();

            //builder.RegisterType<AquaCorpusAnalysisMenuItemViewModel>().AsSelf();

            builder.RegisterType<AquaAddVersionOrListAssessmentsStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 1);

            // THIRDPARTY MODULE TODO: How to reference SelectBooksStepViewModel?
            //builder.RegisterType<SelectBooksStepViewModel>().As<IWorkflowStepViewModel>()
            //    .Keyed<IWorkflowStepViewModel>("AquaDialog")
            //    .WithMetadata("Order", 2);

            builder.RegisterType<AquaAddRevisionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 3);

            builder.RegisterType<AquaInfoStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 4);

        }
    }
}
