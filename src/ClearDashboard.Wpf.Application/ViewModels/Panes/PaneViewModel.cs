﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
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
    public class PaneViewModel : DashboardApplicationScreen,  IPaneViewModel
    {
        #region Member Variables

        public ICommand RequestCloseCommand { get; set; }
        private string? _contentId;
        private bool _isSelected;
        private bool _isActive;
        #endregion //Member Variables

        #region Public Properties

        public Guid PaneId { get; set;  }
        public DockSide DockSide { get; set; }

        #endregion //Public Properties

        #region Observable Properties
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
            PaneId = Guid.NewGuid();
        }

        public PaneViewModel(INavigationService navigationService, ILogger logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base( projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            RequestCloseCommand = new RelayCommandAsync(RequestClose);
            PaneId  = Guid.NewGuid();
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
