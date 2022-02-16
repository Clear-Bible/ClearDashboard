using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SourceContextViewModel : ToolViewModel
    {

        #region Member Variables

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor
        public SourceContextViewModel(string name) : base(name)
        {
            this.Title = name;
            this.ContentId = "{SourceContext_ContentId}";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
