using Autofac;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ClearDashboardJsApi;
using ClearDashboardJsApi.Controllers;
using ClearDashboard.DAL.Alignment.Corpora;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    [JsonSerializable(typeof(CorpusId))]
    public partial class CorpusIdSerializerContext : JsonSerializerContext { }

    [JSExport]
    internal async static Task<string> GetCorpusId(string id)
    {
        var corpusController = EnsureContainer().Resolve<CorpusController>();
        var corpus = await corpusController.GetCorpus(Guid.Parse(id));

        return JsonSerializer.Serialize(corpus.CorpusId, typeof(CorpusId), new CorpusIdSerializerContext());
    }

    [JSImport("node.process.version", "main.mjs")]
    internal static partial string GetNodeVersion();

    static IContainer _container;

    public static void Main(string[] args)
    {
        _ = EnsureContainer();
    }

    public static IContainer EnsureContainer()
    {
        if (_container is not null)
        { 
            return _container;
        }

        if (!OperatingSystem.IsBrowser())
        {
            throw new PlatformNotSupportedException("This demo is expected to run on browser platform");
        }

        var builder = new ContainerBuilder();
        builder.RegisterModule(new WebApplicationModule());
        builder.RegisterType<CorpusController>();
        _container = builder.Build();

        AddSerilogConfiguration(_container);

        return _container;
    }

    private static void AddSerilogConfiguration(IContainer container)
    {
        var fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"ClearDashboard_Projects{Path.DirectorySeparatorChar}Logs{Path.DirectorySeparatorChar}ClearDashboardJsApi.log");
        try
        {
            //create if does not exist
            DirectoryInfo di = new DirectoryInfo(fullPath);
            if (di.Exists == false)
            {
                di.Create();
            }
        }
        catch (Exception)
        {

        }

        // configure Serilog
        var log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        var loggerFactory = container.Resolve<ILoggerFactory>();
        loggerFactory.AddSerilog(log);
        Log.Logger = log;
    }
}
