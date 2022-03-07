using Serilog;
using System;

namespace ClearDashboard.Wpf
{
    public class Log : Caliburn.Micro.ILog
    {
        #region Fields

        #endregion

        private readonly ILogger logger;
        #region Constructors
        public Log()
        {
            logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(@"ClearDashboard.log", rollingInterval: RollingInterval.Day).CreateLogger();

        }
        #endregion

        #region Helper Methods
        private string CreateLogMessage(string format, params object[] args)
        {
            return string.Format("[{0}] {1}",
                DateTime.Now.ToString("o"),
                string.Format(format, args));
        }
        #endregion

        #region ILog Members
        public void Error(Exception exception)
        {
            logger.Error(CreateLogMessage(exception.ToString()), "ERROR");
        }

        public void Info(string format, params object[] args)
        {
            if (Array.FindIndex(args, t => t.ToString().Contains("Something i dont want to log")) >= 0)
                return;
            logger.Information(CreateLogMessage(format, args), "INFO");
        }

        public void Warn(string format, params object[] args)
        {
            logger.Warning(CreateLogMessage(format, args), "WARN");
        }
        #endregion
    }
}
