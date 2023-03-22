using Caliburn.Micro;
using System.Reflection;

namespace PluginManager.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Member Variables   

        
        #endregion //Member Variables



        #region Public Properties

        
        #endregion //Public Properties


        #region Observable Properties

        private string? _version;
        public string? Version
        {
            get => _version;
            set => Set(ref _version, value);
        }


        
        #endregion //Observable Properties


        #region Constructor

        public ShellViewModel()
        {

        }

        protected override async void OnViewLoaded(object view)
        {
            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"Clear Dashboard Plugin Manager  -  Version: {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
        }
        
        #endregion //Constructor



        #region Methods

        

        #endregion // Methods


    }
}
