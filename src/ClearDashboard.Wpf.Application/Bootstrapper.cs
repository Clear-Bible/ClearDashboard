using Autofac;
using ClearApplicationFoundation;
using ClearDashboard.DataAccessLayer.Wpf.Extensions;
using FluentValidation;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.Wpf.Application.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ClearApplicationFoundation.Framework;
using ClearApplicationFoundation.ViewModels.Shell;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;
using System;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using WorkSpaceViewModel = ClearDashboard.Wpf.Application.ViewModels.Main.WorkSpaceViewModel;

namespace ClearDashboard.Wpf.Application
{
    internal class Bootstrapper : FoundationBootstrapper
    {

        protected FrameSet FrameSet { get; private set; }

        protected override void SetupLogging()
        {
            SetupLogging(Path.Combine(Path.GetTempPath(), "ClearDashboard\\logs\\ClearDashboard.log"));
        }

        protected override void PopulateServiceCollection(ServiceCollection serviceCollection)
        {
            FrameSet = serviceCollection.AddCaliburnMicro();

            serviceCollection.AddClearDashboardDataAccessLayer();
            serviceCollection.AddValidatorsFromAssemblyContaining<ProjectValidator>();
            serviceCollection.AddValidatorsFromAssemblyContaining<AddParatextCorpusDialogViewModelValidator>();

            base.PopulateServiceCollection(serviceCollection);
        }


        
        protected override void LoadModules(ContainerBuilder builder)
        {
            base.LoadModules(builder);
            builder.RegisterModule<ApplicationModule>();
        }

        protected override async Task NavigateToMainWindow()
        {
            EnsureApplicationMainWindowVisible();

            NavigateToViewModel<WorkSpaceViewModel>();

            // await base.NavigateToMainWindow();
            // Show the StartupViewModel as a dialog, then navigate to HomeViewModel
            // if the dialog result is "true"
            //await ShowStartupDialog<StartupViewModel, MainViewModel>();
            //await ShowStartupDialog<ProjectPickerViewModel, ProjectSetupViewModel>();
            

            
            // Navigate to the LandingView.
            //FrameSet.NavigationService.NavigateToViewModel(typeof(WorkSpaceViewModel));
        }


        /// <summary>
        /// Adds the Frame to the Grid control on the ShellView
        /// </summary>
        /// <param name="frame"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void AddFrameToMainWindow(Frame frame)
        {
            Logger.LogInformation("Adding Frame to ShellView grid control.");

            var mainWindow = Application.MainWindow;
            if (mainWindow == null)
            {
                throw new NullReferenceException("'Application.MainWindow' is null.");
            }


            if (mainWindow.Content is not Grid grid)
            {
                throw new NullReferenceException("The grid on 'Application.MainWindow' is null.");
            }

            Grid.SetRow(frame, 1);
            Grid.SetColumn(frame, 0);
            Panel.SetZIndex(frame, 0);
            grid.Children.Add(frame);
        }
    }
}
