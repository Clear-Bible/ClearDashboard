using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL;
using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ToolViewModel : PaneViewModel
    {
        #region Member Variables
        private bool _isVisible = true;
        #endregion //Member Variables

        #region Public Properties
        public string Name { get; private set; }


        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    SetProperty(ref _isVisible, value, nameof(IsVisible));
                }
            }
        }
        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor
        public ToolViewModel(string name)
        {
            Name = name;
            Title = name;
        }
        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
