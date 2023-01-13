

using Caliburn.Micro;
using ClearApplicationFoundation.LogHelpers;
using System.IO;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Services
{
    public class TailBlazerProxy
    {
        public CaptureFilePathHook FilePathHook { get; }
        public ILogger<TailBlazerProxy> Logger { get; }

        public TailBlazerProxy(CaptureFilePathHook filePathHook, ILogger<TailBlazerProxy> logger)
        {
            FilePathHook = filePathHook;
            Logger = logger;
        }

        public void StartTailBlazer()
        {
            var dashboardLogPath = IoC.Get<CaptureFilePathHook>();

            if (File.Exists(FilePathHook.Path) == false)
            {
                return;
            }

            var tailBlazorPath = Path.Combine(Environment.CurrentDirectory, @"Resources\TailBlazor\TailBlazer.exe");

            var fileInfo = new FileInfo(tailBlazorPath);
            if (fileInfo.Exists == false)
            {
                return;
            }

            try
            {
                var process = new Process();
                process.StartInfo.WorkingDirectory = fileInfo.Directory!.FullName;
                process.StartInfo.FileName = fileInfo.FullName;
                process.StartInfo.Arguments = dashboardLogPath.Path;
                process.Start();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
            }
        }

        public void StopTailBlazer()
        {
            try
            {
                var processes = Process.GetProcessesByName("TailBlazer");
                if (processes.Length > 0)
                {
                    Logger.LogInformation($"TailBlazer is running with {processes.Length} instances, stopping all instances.");
                    foreach (var process in processes)
                    {
                        process.Kill(true);
                        Logger.LogInformation($"TailBlazer - instance '{process.Id}' stopped.");
                    }
                    return;
                }

                Logger.LogInformation("TailBlazer is not running.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while stopping TailBlazer.");
            }
        }
    }
}
