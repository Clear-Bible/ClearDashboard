using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class DeleteParallelizationLineViewModel : DashboardApplicationScreen, IHandle<SetIsCheckedAlignment>
    {
        #region Member Variables   

        private readonly ILogger<SlackMessageViewModel> _logger;

        #endregion //Member Variables


        #region Public Properties

        private List<AlignmentSetId> _alignmentSetIds;
        public List<AlignmentSetId> AlignmentSetIds
        {
            get => _alignmentSetIds;
            set
            {
                _alignmentSetIds = value;

                SelectableAlignmentSetIds.Clear();
                foreach (var alignmentSetId in _alignmentSetIds)
                {
                    SelectableAlignmentSetIds.Add(new SelectableAlignmentSetId(EventAggregator!)
                    {
                        IsChecked = false,
                        AlignmentSetId = alignmentSetId,
                    });
                }
            }
        }


        private List<TranslationSetId> _translationSetIds;
        public List<TranslationSetId> TranslationSetIds
        {
            get => _translationSetIds;
            set
            {
                _translationSetIds = value;

                SelectableTranslationSetIds.Clear();
                foreach (var translationSetId in _translationSetIds)
                {
                    SelectableTranslationSetIds.Add(new SelectableTranslationSetId
                    {
                        IsChecked = false,
                        TranslationSetId = translationSetId,
                    });
                }
            }
        }

        public DesignSurfaceViewModel DesignSurfaceViewModel { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                NotifyOfPropertyChange(() => ProgressBarVisibility);
            }
        }

        private bool _isWindowEnabled = true;
        public bool IsWindowEnabled
        {
            get => _isWindowEnabled;
            set
            {
                _isWindowEnabled = value;
                NotifyOfPropertyChange(() => IsWindowEnabled);
            }
        }




        private ObservableCollection<SelectableAlignmentSetId> _selectableAlignmentSetIds = new();
        public ObservableCollection<SelectableAlignmentSetId> SelectableAlignmentSetIds
        {
            get => _selectableAlignmentSetIds;
            set
            {
                _selectableAlignmentSetIds = value;
                NotifyOfPropertyChange(() => SelectableAlignmentSetIds);
            }
        }

        private ObservableCollection<SelectableTranslationSetId> _selectableTranslationSetIds = new();
        public ObservableCollection<SelectableTranslationSetId> SelectableTranslationSetIds
        {
            get => _selectableTranslationSetIds;
            set
            {
                _selectableTranslationSetIds = value;
                NotifyOfPropertyChange(() => SelectableTranslationSetIds);
            }
        }

        

        #endregion //Observable Properties


        #region Constructor

#pragma warning disable CS8618
        public DeleteParallelizationLineViewModel()
#pragma warning restore CS8618
        {
            // no-op
        }

        public DeleteParallelizationLineViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion //Constructor


        #region Methods

        public void DeleteSelected()
        {
            IsWindowEnabled = false;

            // remove any translationSets first since they are dependent on an AlignmentSet
            for (int i = _selectableTranslationSetIds.Count - 1; i >= 0; i--)
            {
                if (_selectableTranslationSetIds[i].IsChecked)
                {
                    var translationSetId = _selectableTranslationSetIds[i].TranslationSetId;

                    OnUIThread(() =>
                    {
                        DesignSurfaceViewModel.DeleteTranslationFromMenus(translationSetId);

                        _selectableTranslationSetIds.RemoveAt(i);
                        NotifyOfPropertyChange(() => SelectableAlignmentSetIds);

                    });

                    Task.Factory.StartNew(async () =>
                    {
                        await TranslationSet.Delete(Mediator!, translationSetId);
                    });
                }
            }

            // remove any AlignmentSets
            for (int i = _selectableAlignmentSetIds.Count - 1; i >= 0; i--)
            {
                if (_selectableAlignmentSetIds[i].IsChecked)
                {
                    var alignmentSetId = _selectableAlignmentSetIds[i].AlignmentSetId;

                    OnUIThread(() =>
                    {
                        DesignSurfaceViewModel.DeleteAlignmentFromMenus(alignmentSetId);
                        
                        _selectableAlignmentSetIds.RemoveAt(i);
                        NotifyOfPropertyChange(() => SelectableAlignmentSetIds);

                    });

                    Task.Factory.StartNew(async () =>
                    {
                        await AlignmentSet.Delete(Mediator!, alignmentSetId);
                    });
                }
            }
            
            IsWindowEnabled = true;
        }

        public void DeleteAll()
        {
            // let the PDS view model know to remove the line and all alignments/glosses
            TryCloseAsync(true);
        }

        public void Close()
        {
            if (SelectableTranslationSetIds.Count == 0 && SelectableAlignmentSetIds.Count == 0)
            {
                // everything was deleted so we need to kill off the line from the PDS
                TryCloseAsync(true);
            }
            else
            {
                TryCloseAsync(false);
            }

        }


        #endregion // Methods


        #region Event Aggragator Handlers

        public Task HandleAsync(SetIsCheckedAlignment message, CancellationToken cancellationToken)
        {
            var alignmentSetGuid = message.AlignmentSetId.Id;

            foreach (var selectableTranslationSetId in SelectableTranslationSetIds)
            {
                if (selectableTranslationSetId.TranslationSetId.AlignmentSetGuid == alignmentSetGuid)
                {
                    selectableTranslationSetId.IsChecked = message.IsChecked;
                    break;
                }
            }

            return Task.CompletedTask;
        }

        #endregion
    }


    public class SelectableAlignmentSetId : Screen
    {
        private IEventAggregator _eventAggrator;

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);

                _eventAggrator.PublishOnUIThreadAsync(new SetIsCheckedAlignment(this.AlignmentSetId, value));
            }
        }

        public AlignmentSetId AlignmentSetId { get; set; }

        public SelectableAlignmentSetId(IEventAggregator eventAggregator)
        {
            _eventAggrator = eventAggregator;
        }
    }

    public class SelectableTranslationSetId : Screen
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }
        public TranslationSetId TranslationSetId { get; set; }
    }
}
