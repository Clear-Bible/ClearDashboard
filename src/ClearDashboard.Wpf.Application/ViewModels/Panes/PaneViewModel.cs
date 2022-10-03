using System;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Panes
{
    /// <summary>
    /// 
    /// </summary>
    public class PaneViewModel : DashboardApplicationScreen, IAvalonDockWindow
    {
        public enum EDockSide
        {
            Left,
            Bottom,
        }


        #region Member Variables
       
        private string _title = null;
        private string _contentId = null;
        private bool _isSelected = false;
        private bool _isActive = false;
        #endregion //Member Variables

        #region Public Properties

        public Guid Guid = Guid.NewGuid();
        public EDockSide DockSide = EDockSide.Bottom;

        #endregion //Public Properties

        #region Observable Properties
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        public ImageSource IconSource { get; protected set; }

        public string ContentId
        {
            get => _contentId;
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    NotifyOfPropertyChange(() => ContentId);
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        public new bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyOfPropertyChange(() => IsActive);

                    if (this.ContentId == "ENHANCEDVIEW" && value == true)
                    {
                        // send out a notice that the active document has changed
                        EventAggregator.PublishOnUIThreadAsync(new ActiveDocumentMessage(this.Guid));
                    }
                }
            }
        }

        #endregion //Observable Properties

        #region Constructor
        public PaneViewModel(): base()
        {
        }

        public PaneViewModel(INavigationService navigationService, ILogger logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base( projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {

        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods

    }
}
