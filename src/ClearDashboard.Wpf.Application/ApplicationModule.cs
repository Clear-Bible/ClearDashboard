using Autofac;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using System.Reflection;
using Module = Autofac.Module;
using ShellViewModel = ClearDashboard.Wpf.Application.ViewModels.Shell.ShellViewModel;

namespace ClearDashboard.Wpf.Application
{
    internal static class ContainerBuilderExtensions
    {

        public static void OverrideFoundationDependencies(this ContainerBuilder builder)
        {
            // IMPORTANT!  - override the default ShellViewModel from the foundation.
            builder.RegisterType<ShellViewModel>().As<IShellViewModel>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

        }

        public static void RegisterValidationDependencies(this ContainerBuilder builder)
        {
            // Register validators from this assembly.
            builder.RegisterValidators(Assembly.GetExecutingAssembly());
        }

        public static void RegisterLocalizationDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<TranslationSource>().AsSelf().SingleInstance();
        }

        public static void RegisterStartupDialogDependencies(this ContainerBuilder builder)
        {

            builder.RegisterType<ProjectPickerViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("Startup")
                .WithMetadata("Order", 1); 

            builder.RegisterType<ProjectSetupViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("Startup")
                .WithMetadata("Order", 2); 
        }

        public static void RegisterParallelCorpusDialogDependencies(this ContainerBuilder builder)
        {

            builder.RegisterType<ParallelCorpusStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 1); 

            builder.RegisterType<SmtModelStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 2);

            builder.RegisterType<TranslationSetStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 3);

            builder.RegisterType<AlignmentSetStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 4);

        }

    }


    internal class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.OverrideFoundationDependencies();
            builder.RegisterValidationDependencies();
            builder.RegisterLocalizationDependencies();
            builder.RegisterStartupDialogDependencies();
            builder.RegisterParallelCorpusDialogDependencies();
        }

    }
}


