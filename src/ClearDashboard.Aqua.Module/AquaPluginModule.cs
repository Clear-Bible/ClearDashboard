

using Autofac;
using Autofac.Features.AttributeFilters;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Aqua.Module.Menu;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.Validators;
using ClearDashboard.Aqua.Module.ViewModels;
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
                .As<IValidator<AquaVersionStepViewModel>>();
            builder.RegisterType<AquaRevisionStepViewModelValidator>()
                .As<IValidator<AquaRevisionStepViewModel>>();
            builder.RegisterType<AquaAssessmentStepViewModelValidator>()
                .As<IValidator<AquaAssessmentStepViewModel>>();

            //menus and localization

            builder.RegisterType<AquaDesignSurfaceMenuBuilder>().As<IDesignSurfaceMenuBuilder>().WithAttributeFiltering();
            builder.RegisterType<AquaLocalizationService>().AsSelf().Keyed<ILocalizationService>("Aqua");
            builder.RegisterType<AquaCorpusAnalysisMenuItemViewModel>().AsSelf().WithAttributeFiltering();

            // Enhanced View

            builder.RegisterType<AquaCorpusAnalysisEnhancedViewItemViewModel>().AsSelf().WithAttributeFiltering();

            //AquaDialog

            builder.RegisterType<AquaDialogViewModel>().AsSelf().WithAttributeFiltering();

            builder.RegisterType<AquaCorpusAnalysisEnhancedViewItemViewModel>().AsSelf().WithAttributeFiltering();

            builder.RegisterType<AquaVersionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithAttributeFiltering()
                .WithMetadata("Order", 1);
            builder.RegisterType<AquaRevisionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithAttributeFiltering()
                .WithMetadata("Order", 2);
            builder.RegisterType<AquaAssessmentStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithAttributeFiltering()
                .WithMetadata("Order", 3);

          

            //builder.RegisterValidators(typeof(AquaAddVersionOrListAssessmentsStepViewModel).Assembly);
        }
    }
}
