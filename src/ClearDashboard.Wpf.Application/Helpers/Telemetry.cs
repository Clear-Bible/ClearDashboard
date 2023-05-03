using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class Telemetry
    {
        private static TelemetryClient _telemetry = GetAppInsightsClient();
        private static IOperationHolder<RequestTelemetry> AppRunTelemetry;

        public static bool Enabled { get; set; } = true;
        public static Dictionary<string, string> PropertiesDictionary { get; set; } = new();
        public static Dictionary<string, double> MetricsDictionary { get; set; } = new();
        public static Dictionary<TelemetryDictionaryKeys, Stopwatch> StopwatchDictionary { get; set; } = new();

        private static TelemetryClient GetAppInsightsClient()
        {
            var config = new TelemetryConfiguration();
            config.ConnectionString = "InstrumentationKey=23ded420-f989-4a10-a980-8ed61e5599f2;" +
                                      "IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;" +
                                      "LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/";
            //config.InstrumentationKey = TelemetryKey;
            config.TelemetryChannel = new Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.ServerTelemetryChannel();
            //config.TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel(); // Default channel
            config.TelemetryChannel.DeveloperMode = Debugger.IsAttached;
#if DEBUG
            config.TelemetryChannel.DeveloperMode = true;
#endif
            TelemetryClient client = new TelemetryClient(config);
            client.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            client.Context.Session.Id = Guid.NewGuid().ToString();
            client.Context.User.Id = (Environment.UserName + Environment.MachineName).GetHashCode().ToString();
            client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            AppRunTelemetry = client.StartOperation<RequestTelemetry>($"{client.Context.Component.Version} - {"InternalUseCount"} - {"UnlockKey.LicenseKey"}");
            AppRunTelemetry.Telemetry.Start();

            return client;
        }

        public static void SetUser(string user)
        {
            _telemetry.Context.User.AuthenticatedUserId = user;
        }

        public static void SendFullReport(string key)
        {
            if (Enabled)
            {
                AddStopwatchDictionaryToMetricsDictionary();
                _telemetry.TrackEvent(key, PropertiesDictionary, MetricsDictionary);
            }
        }

        public static void TrackEvent(string key, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (Enabled)
            {
                _telemetry.TrackEvent(key, properties, metrics);
            }
        }

        public static void TrackException(Exception ex)
        {
            if (ex != null && Enabled)
            {
                var telex = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                _telemetry.TrackException(telex);
                Flush();
            }
        }

        internal static void Flush()
        {
            _telemetry.Flush();
        }

        public static void StartStopwatch(TelemetryDictionaryKeys key)
        {
            var sw = new Stopwatch();
            sw.Start();
            StopwatchDictionary.Add(key, sw);
        }

        public static void StopStopwatch(TelemetryDictionaryKeys key)
        {
            StopwatchDictionary[key].Stop();
        }

        public static void AddStopwatchDictionaryToMetricsDictionary()
        {
            foreach (var stopWatchDefinition in StopwatchDictionary)
            {
                MetricsDictionary.Add(stopWatchDefinition.Key.ToString(), stopWatchDefinition.Value.Elapsed.TotalHours);
            }
        }

        public static void IncrementMetric(TelemetryDictionaryKeys key, int increment)
        {
            var keyString = key.ToString();

            if (!MetricsDictionary.ContainsKey(keyString))
            {
                MetricsDictionary.Add(keyString, 0);
            }

            MetricsDictionary[keyString] += increment;
        }

        public enum TelemetryDictionaryKeys
        {
            AppHours,
            NoteCreationCount,
            BcvChangeCount,
            NotePushCount,
            NoteClosedCount,
            NoteReplyCount,
            InterlinearViewAddedCount,
            AlignmentViewAddedCount,
            VerseViewAddedCount,
            TimerStartCount
        }
    }
}
