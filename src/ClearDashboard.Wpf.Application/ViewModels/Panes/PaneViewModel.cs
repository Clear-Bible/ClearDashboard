using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.ViewModels.Panes
{
    /// <summary>
    /// 
    /// </summary>
    public class PaneViewModel : DashboardApplicationScreen, IAvalonDockWindow, IPaneViewModel
    {
        #region Member Variables

        public ICommand RequestCloseCommand { get; set; }

        //private string _title = null;
        private string? _contentId;
        private bool _isSelected;
        private bool _isActive;
        #endregion //Member Variables

        #region Public Properties

        public Guid PaneId  => Guid.NewGuid();
        public DockSide DockSide { get; set; }

        #endregion //Public Properties

        #region Observable Properties
        //public string Title
        //{
        //    get => _title;
        //    set
        //    {
        //        if (_title != value)
        //        {
        //            _title = value;
        //            NotifyOfPropertyChange(() => Title);
        //        }
        //    }
        //}

        public ImageSource? IconSource { get; protected set; }

        public string? ContentId
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

        //public DockSide DockSide { get; }

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
                    Set(ref _isActive, value);

                    if (this.ContentId == "ENHANCEDVIEW" && value)
                    {
                        // send out a notice that the active document has changed
                        EventAggregator.PublishOnUIThreadAsync(new ActiveDocumentMessage(PaneId));
                    }
                }
            }
        }

        #endregion //Observable Properties

        #region Constructor
        public PaneViewModel()
        {
            RequestCloseCommand = new RelayCommandAsync(RequestClose);
        }

        public PaneViewModel(INavigationService navigationService, ILogger logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base( projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            RequestCloseCommand = new RelayCommandAsync(RequestClose);
        }



        #endregion //Constructor

        #region Methods

        public async Task RequestClose(object obj)
        {
            await EventAggregator.PublishOnUIThreadAsync(new CloseDockingPane(this.PaneId));
        }

        #endregion // Methods

    }
}
