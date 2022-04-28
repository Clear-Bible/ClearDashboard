using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;

namespace ClearDashboard.ParatextPlugin
{
    public class ClearDashboardPlugin : IParatextWindowPlugin
    {
        public const string pluginName = "ClearDashboard Plugin";
        public string Name => pluginName;
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
                yield return new WindowPluginMenuEntry("ClearDashboard", Run, PluginMenuLocation.ScrTextTools, imagePath: "Plugin.bmp");
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
