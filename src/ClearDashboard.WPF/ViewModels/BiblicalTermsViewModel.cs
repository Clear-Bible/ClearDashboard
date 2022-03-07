using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel
    {
        #region Member Variables

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties
        private ObservableCollection<BiblicalTermsData> _biblicalTerms = new ObservableCollection<BiblicalTermsData>();
        public ObservableCollection<BiblicalTermsData> BiblicalTerms
        {
            get => _biblicalTerms;
            set
            {
                _biblicalTerms = value;
                NotifyOfPropertyChange(() => BiblicalTerms);
            }
        }

        #endregion //Observable Properties

        #region Constructor
        public BiblicalTermsViewModel()
        {
            this.Title = "🕮 BIBLICAL TERMS";
            this.ContentId = "BIBLICALTERMS";
            this.DockSide = EDockSide.Left;
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
