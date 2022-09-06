using ClearDashboard.Wpf.Application.Properties;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class UserPreferences
    {
        #region Member Variables

        private double _windowTop;
        private double _windowLeft;
        private double _windowHeight;
        private double _windowWidth;
        private System.Windows.WindowState _windowState;

        #endregion //Member Variables

        #region Public Properties

        public double WindowTop
        {
            get { return _windowTop; }
            set { _windowTop = value; }
        }

        public double WindowLeft
        {
            get { return _windowLeft; }
            set { _windowLeft = value; }
        }

        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }

        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; }
        }

        public System.Windows.WindowState WindowState
        {
            get { return _windowState; }
            set { _windowState = value; }
        }

        #endregion //Public Properties

        #region Constructor

        public UserPreferences()
        {
            //Load the settings
            Load();

            //Size it to fit the current screen
            SizeToFit();

            //Move the window at least partially into view
            MoveIntoView();
        }

        #endregion //Constructor

        #region Methods

        /// <summary>
        /// If the saved window dimensions are larger than the current screen shrink the
        /// window to fit.
        /// </summary>
        public void SizeToFit()
        {
            if (_windowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                _windowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (_windowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                _windowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }
        }

        /// <summary>
        /// If the window is more than half off of the screen move it up and to the left 
        /// so half the height and half the width are visible.
        /// </summary>
        public void MoveIntoView()
        {
            if (_windowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                _windowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (_windowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                _windowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }

            if (_windowTop < System.Windows.SystemParameters.VirtualScreenTop)
            {
                _windowTop = System.Windows.SystemParameters.VirtualScreenTop;
            }
            else if (_windowTop + _windowHeight >
                     System.Windows.SystemParameters.VirtualScreenTop + System.Windows.SystemParameters.VirtualScreenHeight)
            {
                _windowTop = System.Windows.SystemParameters.VirtualScreenTop +
                    System.Windows.SystemParameters.VirtualScreenHeight - _windowHeight;
            }

            if (_windowLeft < System.Windows.SystemParameters.VirtualScreenLeft)
            {
                _windowLeft = System.Windows.SystemParameters.VirtualScreenLeft;
            }
            else if (_windowLeft + _windowWidth >
                     System.Windows.SystemParameters.VirtualScreenLeft + System.Windows.SystemParameters.VirtualScreenWidth)
            {
                _windowLeft = System.Windows.SystemParameters.VirtualScreenLeft +
                    System.Windows.SystemParameters.VirtualScreenWidth - _windowWidth;
            }
        }

        private void Load()
        {
            _windowTop = Settings.Default.WindowTop;
            _windowLeft = Settings.Default.WindowLeft;
            _windowHeight = Settings.Default.WindowHeight;
            _windowWidth = Settings.Default.WindowWidth;
            _windowState = Settings.Default.WindowState;
        }

        public void Save()
        {
            if (_windowState != System.Windows.WindowState.Minimized)
            {
                Settings.Default.WindowTop = _windowTop;
                Settings.Default.WindowLeft = _windowLeft;
                Settings.Default.WindowHeight = _windowHeight;
                Settings.Default.WindowWidth = _windowWidth;
                Settings.Default.WindowState = _windowState;

                Settings.Default.Save();
            }
        }

        #endregion //Functions

    }
}
