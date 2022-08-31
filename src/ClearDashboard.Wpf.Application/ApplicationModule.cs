using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Shell;
using MediatR.Extensions.Autofac.DependencyInjection;
using System.Linq;
using System.Reflection;
using ClearDashboard.Wpf.Application.Helpers;
using Module = Autofac.Module;
using ShellViewModel = ClearDashboard.Wpf.Application.ViewModels.Shell.ShellViewModel;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Views.Startup;

namespace ClearDashboard.Wpf.Application
{
    internal class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
           

            // IMPORTANT!  - override the default ShellViewModel from the foundation.
            builder.RegisterType<ShellViewModel>().As<IShellViewModel>();

            // Register validators from this assembly.
            builder.RegisterValidators(Assembly.GetExecutingAssembly());

            // Register Mediator requests and handlers.
            builder.RegisterMediatR(typeof(App).Assembly);

            builder.RegisterType<TranslationSource>().AsSelf();

            builder.RegisterType<ProjectPickerView>().AsSelf();

            builder.RegisterType<ProjectSetupView>().AsSelf();

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("ViewModel"))
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("View"))
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<ProjectPickerViewModel>().As<IWorkflowStepViewModel>();

            builder.RegisterType<ProjectSetupViewModel>().As<IWorkflowStepViewModel>();
        }
    }
}
