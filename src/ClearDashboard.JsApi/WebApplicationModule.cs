using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog.Extensions.Logging;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ClearDashboardJsApi
{
    public class WebApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = MediatRConfigurationBuilder
                .Create(
                    typeof(CreateParallelCorpusCommandHandler).Assembly,
                    typeof(ClearDashboard.DataAccessLayer.Features.Versification.GetVersificationAndBookIdByDalParatextProjectIdQueryHandler).Assembly)
                .WithAllOpenGenericHandlerTypesRegistered()
                .Build();

            // this will add all your Request- and Notificationhandler
            // that are located in the same project as your program-class
            builder.RegisterMediatR(configuration);

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new SerilogLoggerProvider());
            builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();

            builder.RegisterInstance(new ApiUserProvider()).As<IUserProvider>().SingleInstance();
            builder.RegisterInstance(new ApiProjectProvider()).As<IProjectProvider>().SingleInstance();

            builder.RegisterType<ProjectDbContextFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectDbContext>().InstancePerLifetimeScope(); // .InstancePerRequest();
            builder.RegisterType<SqliteProjectDbContextOptionsBuilder<ProjectDbContext>>().As<DbContextOptionsBuilder<ProjectDbContext>>().InstancePerLifetimeScope(); //.InstancePerRequest();

            builder.RegisterType<EventAggregator>().As<IEventAggregator>();
            builder.RegisterType<TranslationCommands>();

            // Register Paratext as our "External" lexicon provider / drafting tool:
            builder.RegisterType<ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon.GetLexiconQuery>()
                .As<IRequest<RequestResult<Models.Lexicon_Lexicon>>>()
                .Keyed<IRequest<RequestResult<Models.Lexicon_Lexicon>>>("External");
        }
    }
}
