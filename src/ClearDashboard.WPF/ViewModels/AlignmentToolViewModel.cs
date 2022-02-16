using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;

namespace ClearDashboard.Wpf.ViewModels
{
    public class AlignmentToolViewModel: PaneViewModel
    {
        #region Member Variables

        #endregion //Member Variables

        #region Public Properties

        public string ContentID => this.ContentID;

        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor

        #endregion //Constructor

        #region Methods

        #endregion // Methods

        public AlignmentToolViewModel()
        {
            this.Title = "ALIGNMENT TOOL";
            this.ContentId = "{AlignmentTool_ContentId}";
        }
    }
}
