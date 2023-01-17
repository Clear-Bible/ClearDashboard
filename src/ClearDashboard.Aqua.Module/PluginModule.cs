

using Autofac;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.Wpf.Application;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Aqua.Module
{
    public class PluginModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            //var assembly = typeof(PluginModule).Assembly;
            //builder.RegisterAssemblyTypes(assembly)
            //    .Where(type => type.Name.EndsWith("ViewModel"))
            //    .AsSelf()
            //    .InstancePerDependency();
        }
    }

    internal static class ContainerBuilderExtensions
    {
        public static void RegisterAquaDependencies(this ContainerBuilder builder)
        {
            //manager

            builder.RegisterType<AquaManager>().As<IAquaManager>().SingleInstance();

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
