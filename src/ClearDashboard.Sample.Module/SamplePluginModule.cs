

using Autofac;
using Autofac.Features.AttributeFilters;
using ClearApplicationFoundation.Services;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Sample.Module.Menu;
using ClearDashboard.Sample.Module.Services;
using ClearDashboard.Sample.Module.Validators;
using ClearDashboard.Sample.Module.ViewModels.Menus;
using ClearDashboard.Sample.Module.ViewModels.SampleDialog;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Plugin;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;

namespace ClearDashboard.Sample.Module
{
    public class SamplePluginModule : PluginModule
    {

        protected override void RegisterJsonDiscriminatorRegistrar(ContainerBuilder builder)
        {
            builder.RegisterType<JsonDiscriminatorRegistrar>().As<IJsonDiscriminatorRegistrar>();
        }

        protected override void RegisterPluginAbstractions(ContainerBuilder builder)
        {
            builder.RegisterSampleDependencies();
        }
    }

    internal static class ContainerBuilderExtensions
    {
        public static void RegisterSampleDependencies(this ContainerBuilder builder)
        {
            //managers

            builder.RegisterType<SampleManager>().As<ISampleManager>().SingleInstance();


            //Validation
            builder.RegisterType<SampleAddVersionOrListAssessmentsStepViewModelValidator>()
                .As<IValidator<SampleAddVersionOrListAssessmentsStepViewModel>>();

            //menus and localization

            builder.RegisterType<SampleDesignSurfaceMenuBuilder>().As<IDesignSurfaceMenuBuilder>().WithAttributeFiltering();
            builder.RegisterType<SampleLocalizationService>().AsSelf().Keyed<ILocalizationService>("Sample");
            builder.RegisterType<SampleCorpusAnalysisMenuItemViewModel>().AsSelf().WithAttributeFiltering();

            //SampleDialog

            builder.RegisterType<SampleAddVersionOrListAssessmentsStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("SampleDialog")
                .WithMetadata("Order", 1);
            builder.RegisterType<SampleAddRevisionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("SampleDialog")
                .WithMetadata("Order", 2);
            builder.RegisterType<SampleInfoStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("SampleDialog")
                .WithMetadata("Order", 3);
        }
    }
}
