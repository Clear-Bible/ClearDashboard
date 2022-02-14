using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL;
using MvvmHelpers;
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
    public class StartPageViewModel : PaneViewModel
    {
        #region Member Variables

        #endregion //Member Variables

        #region Public Properties

        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public StartPageViewModel()
        {
            this.Title = "Start Page";
            this.ContentId = "{StartPage_ContentId}";
        }
        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
