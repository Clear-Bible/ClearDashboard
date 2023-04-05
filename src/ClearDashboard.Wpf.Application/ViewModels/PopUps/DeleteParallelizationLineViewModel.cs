using ClearDashboard.Wpf.Application.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.ObjectModel;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using ControlzEx.Standard;
using System.Windows;
using ClearDashboard.DAL.Alignment;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class DeleteParallelizationLineViewModel : DashboardApplicationScreen
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
                    SelectableAlignmentSetIds.Add(new SelectableAlignmentSetId
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

        public DeleteParallelizationLineViewModel()
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

        #endregion //Constructor


        #region Methods

        public async void DeleteSelected()
        {
            var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            IsWindowEnabled = false;
            await Task.Factory.StartNew(async () =>
            {
                for (int i = _selectableAlignmentSetIds.Count - 1; i >= 0; i--)
                {
                    if (_selectableAlignmentSetIds[i].IsChecked)
                    {
                        // TODO Waiting on Chris to give me this working function
                        //await DAL.Alignment.Corpora.ParallelCorpus.Delete(Mediator!, connection.ParallelCorpusId);

                        var alignmentSet = _selectableAlignmentSetIds[i];

                        var alignmentSetId = alignmentSet.AlignmentSetId;

                        //AlignmentSet alignment = null;
                        //await alignment.DeleteAlignment(alignmentSetId.);


                        OnUIThread(() =>
                        {
                            _selectableAlignmentSetIds.RemoveAt(i);
                            NotifyOfPropertyChange(() => SelectableAlignmentSetIds);
                        });
                    }
                }
            });

            await Task.Factory.StartNew(async () =>
            {
                for (int i = _selectableTranslationSetIds.Count - 1; i >= 0; i--)
                {
                    if (_selectableTranslationSetIds[i].IsChecked)
                    {
                        // TODO Waiting on Chris to give me this working function
                        //await DAL.Alignment.Corpora.ParallelCorpus.Delete(Mediator!, connection.ParallelCorpusId);

                        OnUIThread(() =>
                        {
                            _selectableTranslationSetIds.RemoveAt(i);
                            NotifyOfPropertyChange(() => SelectableTranslationSetIds);
                        });
                    }
                }
            });

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








    }


    public class SelectableAlignmentSetId
    {
        public bool IsChecked { get; set; }
        public AlignmentSetId AlignmentSetId { get; set; }
    }

    public class SelectableTranslationSetId
    {
        public bool IsChecked { get; set; }
        public TranslationSetId TranslationSetId { get; set; }
    }
}
