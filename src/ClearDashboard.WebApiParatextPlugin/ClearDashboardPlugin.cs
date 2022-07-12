using System;
using System.Collections.Generic;
using Paratext.PluginInterfaces;

namespace ClearDashboard.WebApiParatextPlugin
{
    public class ClearDashboardWebApiPlugin : IParatextWindowPlugin
    {
        public const string PluginName = "ClearDashboard Plugin";
        public string Name => PluginName;
        public Version Version => new Version(0, 0, 0, 2);
        public string VersionString => Version.ToString();
        public string Publisher => "Clear Bible Inc.";


        public string GetDescription(string locale)
        {
            return "A plugin used for communication between Paratext 9.2+ and ClearDashboard";
        }

        public IEnumerable<WindowPluginMenuEntry> PluginMenuEntries
        {
            get
            {
                yield return new WindowPluginMenuEntry("ClearDashboard Web API", Run, PluginMenuLocation.ScrTextTools, imagePath: "Plugin.bmp");
            }
        }

        public IDataFileMerger GetMerger(IPluginHost host, string dataIdentifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by Paratext when the menu item created for this plugin was clicked.
        /// </summary>
        private void Run(IWindowPluginHost host, IParatextChildState windowState)
        {
           host.ShowEmbeddedUi(new MainWindow(), windowState.Project);
        }

    }
}
