using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Shell;
using MediatR.Extensions.Autofac.DependencyInjection;
using System.Linq;
using System.Reflection;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Main;
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
            builder.RegisterType<ShellViewModel>().As<IShellViewModel>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

            // Register validators from this assembly.
            builder.RegisterValidators(Assembly.GetExecutingAssembly());


            builder.RegisterType<TranslationSource>().AsSelf();

            builder.RegisterType<ProjectPickerView>().AsSelf();

            builder.RegisterType<ProjectSetupView>().AsSelf();

            builder.RegisterType<ProjectPickerViewModel>().As<IWorkflowStepViewModel>();

            builder.RegisterType<ProjectSetupViewModel>().As<IWorkflowStepViewModel>();

      
        }
    }
}
