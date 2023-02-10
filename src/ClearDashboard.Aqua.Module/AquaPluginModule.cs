

using Autofac;
using Autofac.Features.AttributeFilters;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Aqua.Module.Menu;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.Validators;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.Aqua.Module.ViewModels.Menus;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Plugin;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;

namespace ClearDashboard.Aqua.Module
{
    public class AquaPluginModule : PluginModule
    {

        protected override void RegisterJsonDiscriminatorRegistrar(ContainerBuilder builder)
        {
            builder.RegisterType<JsonDiscriminatorRegistrar>().As<IJsonDiscriminatorRegistrar>();
        }

        protected override void RegisterPluginAbstractions(ContainerBuilder builder)
        {
            builder.RegisterAquaDependencies();
        }
    }

    internal static class ContainerBuilderExtensions
    {
        public static void RegisterAquaDependencies(this ContainerBuilder builder)
        {
            //managers

            builder.RegisterType<AquaManager>().As<IAquaManager>().SingleInstance();


            //Validation
            builder.RegisterType<AquaAddVersionOrListAssessmentsStepViewModelValidator>()
                .As<IValidator<AquaAddVersionOrListAssessmentsStepViewModel>>();

            //menus and localization

            builder.RegisterType<AquaDesignSurfaceMenuBuilder>().As<IDesignSurfaceMenuBuilder>().WithAttributeFiltering();
            builder.RegisterType<AquaLocalizationService>().AsSelf().Keyed<ILocalizationService>("Aqua");
            builder.RegisterType<AquaCorpusAnalysisMenuItemViewModel>().AsSelf().WithAttributeFiltering();

            //AquaDialog

            builder.RegisterType<AquaAddVersionOrListAssessmentsStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 1);
            builder.RegisterType<AquaAddRevisionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 2);
            builder.RegisterType<AquaInfoStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 3);

          

            //builder.RegisterValidators(typeof(AquaAddVersionOrListAssessmentsStepViewModel).Assembly);
        }
    }
}
