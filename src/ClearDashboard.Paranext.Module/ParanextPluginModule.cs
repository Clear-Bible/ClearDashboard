using Autofac;
using Autofac.Features.AttributeFilters;
using ClearDashboard.Paranext.Module.Menu;
using ClearDashboard.Paranext.Module.Services;
using ClearDashboard.Paranext.Module.ViewModels;
using ClearDashboard.Paranext.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Plugin;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Paranext.Module
{
    public class ParanextPluginModule : PluginModule
    {

        protected override void RegisterJsonDiscriminatorRegistrar(ContainerBuilder builder)
        {
            builder.RegisterType<JsonDiscriminatorRegistrar>().As<IJsonDiscriminatorRegistrar>();
        }

        protected override void RegisterPluginAbstractions(ContainerBuilder builder)
        {
            builder.RegisterParanextDependencies();
        }
    }

    internal static class ContainerBuilderExtensions
    {
        public static void RegisterParanextDependencies(this ContainerBuilder builder)
        {
            //managers

            builder.RegisterType<ParanextManager>()
                .As<IParanextManager>()
                .SingleInstance()
                .OnActivated(async e => await e.Instance.LoadRenderer())
                .AutoActivate();

            //menus and localization

            builder.RegisterType<ParanextDesignSurfaceMenuBuilder>().As<IDesignSurfaceMenuBuilder>().WithAttributeFiltering();
            builder.RegisterType<ParanextLocalizationService>().AsSelf().Keyed<ILocalizationService>("ParanextExtension");
            builder.RegisterType<ParanextMenuItemViewModel>().AsSelf().WithAttributeFiltering();

            // EnhancedView 

            builder.RegisterType<ParanextEnhancedViewItemViewModel>().AsSelf().WithAttributeFiltering();

        }
    }
}
