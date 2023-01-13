﻿using Autofac;
using ClearApplicationFoundation.Extensions;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Main;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using System.Reflection;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using Module = Autofac.Module;
using ShellViewModel = ClearDashboard.Wpf.Application.ViewModels.Shell.ShellViewModel;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.Views.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;

namespace ClearDashboard.Wpf.Application
{
    internal static class ContainerBuilderExtensions
    {

        public static void OverrideFoundationDependencies(this ContainerBuilder builder)
        {
            // IMPORTANT!  - override the default ShellViewModel from the foundation.
            builder.RegisterType<ShellViewModel>().As<IShellViewModel>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<BackgroundTasksViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<ProjectDesignSurfaceViewModel>().AsSelf().InstancePerLifetimeScope();
            //builder.RegisterType<DesignSurfaceViewModel>().AsSelf().InstancePerLifetimeScope();
        }

        public static void RegisterValidationDependencies(this ContainerBuilder builder)
        {
            // Register validators from this assembly.
            builder.RegisterValidators(Assembly.GetExecutingAssembly());
        }

        public static void RegisterManagerDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<AlignmentManager>().AsSelf();
            builder.RegisterType<NoteManager>().AsSelf().SingleInstance();
            builder.RegisterType<SelectionManager>().AsSelf().SingleInstance();
            builder.RegisterType<TranslationManager>().AsSelf();
            builder.RegisterType<VerseManager>().AsSelf().SingleInstance();
        }

        public static void RegisterLocalizationDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<TranslationSource>().AsSelf().SingleInstance();
        }

        public static void RegisterDatabaseDependencies(this ContainerBuilder builder)
        {
            // Mediator resolves this from the container, and generally 
            // as a thick client app, there isn't any notion of 'requests',
            // so most likely this will be resolved on the 'root' scope:
            builder.RegisterType<ProjectDbContextFactory>().InstancePerLifetimeScope();

            // Intended to be resolved/disposed at a 'request' level:
            builder.RegisterType<ProjectDbContext>().InstancePerRequest();
            builder.RegisterType<SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>().As<DbContextOptionsBuilder<ProjectDbContext>>().InstancePerRequest();
        }

        public static void RegisterStartupDialogDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<RegistrationViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("Startup")
                .WithMetadata("Order", 1);

            builder.RegisterType<ProjectPickerViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("Startup")
                .WithMetadata("Order", 2); 

            builder.RegisterType<ProjectSetupViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("Startup")
                .WithMetadata("Order", 3); 
        }

        //public static void RegisterSmtModelDialogDependencies(this ContainerBuilder builder)
        //{
        //    builder.RegisterType<SmtModelStepViewModel>().As<IWorkflowStepViewModel>()
        //        .Keyed<IWorkflowStepViewModel>("SmtModelDialog")
        //        .WithMetadata("Order", 1);
        //}


        public static void RegisterParatextDialogDependencies(this ContainerBuilder builder)
        {
            builder.RegisterType<AddParatextCorpusStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AddParatextCorpusDialog")
                .WithMetadata("Order", 1);

            builder.RegisterType<SelectBooksStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AddParatextCorpusDialog")
                .WithMetadata("Order", 2);

            builder.RegisterType<SelectBooksStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("UpdateParatextCorpusDialog")
                .WithMetadata("Order", 1);
        }


        public static void RegisterParallelCorpusDialogDependencies(this ContainerBuilder builder)
        {

            builder.RegisterType<ParallelCorpusStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 1);

           
            builder.RegisterType<SmtModelStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 2);

            builder.RegisterType<AlignmentSetStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 3);


            builder.RegisterType<TranslationSetStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("ParallelCorpusDialog")
                .WithMetadata("Order", 4);
        }

        public static void RegisterAquaDependencies(this ContainerBuilder builder)
        {
            //manager

            builder.RegisterType<AquaManager>().As<IAquaManager>().SingleInstance();

            builder.RegisterType<AquaAddVersionOrListAssessmentsStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 1);

            builder.RegisterType<SelectBooksStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 2);

            builder.RegisterType<AquaAddRevisionStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 3);

            builder.RegisterType<AquaInfoStepViewModel>().As<IWorkflowStepViewModel>()
                .Keyed<IWorkflowStepViewModel>("AquaDialog")
                .WithMetadata("Order", 4);

        }
    }

    

    internal class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LongRunningTaskManager>().AsSelf().SingleInstance();
            builder.RegisterType<TailBlazerProxy>().AsSelf().SingleInstance();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();

            builder.RegisterDatabaseDependencies();
            builder.OverrideFoundationDependencies();
            builder.RegisterManagerDependencies();
            builder.RegisterValidationDependencies();
            builder.RegisterLocalizationDependencies();
            builder.RegisterStartupDialogDependencies();
            builder.RegisterParallelCorpusDialogDependencies();
            builder.RegisterParatextDialogDependencies();

            //builder.RegisterSmtModelDialogDependencies();


            builder.RegisterAquaDependencies();
        }
    }
}


