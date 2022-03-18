using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.Wpf.ViewModels.Panes;

namespace ClearDashboard.Wpf.ViewModels
{
    public class WordMeaningsViewModel : ToolViewModel
    {

        #region Member Variables

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor
        public WordMeaningsViewModel()
        {
            this.Title = "⌺ WORD MEANINGS";
            this.ContentId = "WORDMEANINGS";
            this.DockSide = EDockSide.Left;
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
