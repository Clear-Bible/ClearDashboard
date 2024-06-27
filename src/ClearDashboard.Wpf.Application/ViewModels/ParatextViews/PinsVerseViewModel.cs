using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ClearDashboard.DAL.Wpf.ViewModels;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    public class PinsVerseViewModel : ToolViewModel
    {

        #region Member Variables
     
        public string ProjectDir;
        private readonly ILogger<PinsViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IMediator _mediator;
        private readonly LongRunningTaskManager _longRunningTaskManager;

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

        private ObservableCollection<PinsVerseListViewModel> _selectedItemVerses = new();
        public ObservableCollection<PinsVerseListViewModel> SelectedItemVerses
        {
            get => _selectedItemVerses;
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

        public string FontFamily { get; set; } = FontNames.DefaultFontFamily;

        public float FontSize { get; } = 12;

        private bool _isRtl;
        public bool IsRtl
        {
            get => _isRtl;
            set
            {
                _isRtl = value;
                NotifyOfPropertyChange(() => IsRtl);
            }
        }

        private string _lastSelectedPinsDataTableSource = "";
        public string LastSelectedPinsDataTableSource
        {
            get => _lastSelectedPinsDataTableSource;
            set
            {
                value ??= string.Empty;

                _lastSelectedPinsDataTableSource = value;
                NotifyOfPropertyChange(() => LastSelectedPinsDataTableSource);
            }
        }

        private string _verseFilterText;
        public string VerseFilterText
        {
            get
            {
                return _verseFilterText;
            }
            set
            {
                _verseFilterText = value;
                this._verseCollection.View.Refresh();
                NotifyOfPropertyChange(() => VerseFilterText);
            }
        }

        private CollectionViewSource _verseCollection;
        public ICollectionView VerseCollection
        {
            get
            {
                return this._verseCollection.View;
            }
        }

        private bool _backTranslationFound = false;
        public bool BackTranslationFound
        {
            get => _backTranslationFound;

            set
            {
                _backTranslationFound = value;
                NotifyOfPropertyChange(() => BackTranslationFound);
            }
        }

        private bool _showBackTranslation = false;
        public bool ShowBackTranslation
        {
            get => _showBackTranslation;

            set
            {
                _showBackTranslation = value;

                foreach (var verse in SelectedItemVerses)
                {
                    if (verse.BackTranslation != string.Empty)
                    {
                        verse.ShowBackTranslation = value;
                    }
                }

                Settings.Default.PinsShowBackTranslation = value;

                NotifyOfPropertyChange(() => ShowBackTranslation);
            }
        }

        #endregion //Observable Properties

        #region Commands

        public ICommand VerseClickCommand { get; set; }

        #endregion //Commands


        #region Constructor

        // ReSharper disable once UnusedMember.Global
        public PinsVerseViewModel()
        {
        }

        // ReSharper disable once UnusedMember.Global
        public PinsVerseViewModel(INavigationService navigationService, ILogger<PinsViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, 
            LongRunningTaskManager longRunningTaskManager, ILocalizationService localizationService, 
            string lastSelectedPinsDataTableSource, CollectionViewSource verseCollection, 
            bool showBackTranslation, bool backTranslationFound, ObservableCollection<PinsVerseListViewModel> selectedItemVerses)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            Title = "⍒ " + LocalizationService!.Get("Windows_PINS");
            this.ContentId = "PINS";

            _logger = logger;
            _projectManager = projectManager;
            _mediator = mediator;
            _longRunningTaskManager = longRunningTaskManager;


            // pull out the project font family
            if (ProjectManager!.CurrentParatextProject is not null)
            {
                var paratextProject = ProjectManager.CurrentParatextProject;
                FontFamily = paratextProject.Language.FontFamily;
                FontSize = paratextProject.Language.Size;
                IsRtl = paratextProject.Language.IsRtol;
            }

            ShowBackTranslation = Settings.Default.PinsShowBackTranslation;

            LastSelectedPinsDataTableSource = lastSelectedPinsDataTableSource;
            _verseCollection = verseCollection;
            ShowBackTranslation = showBackTranslation;
            BackTranslationFound = backTranslationFound;
            SelectedItemVerses = selectedItemVerses;

            _verseCollection.Filter += VerseCollection_Filter;
        }

        void VerseCollection_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(VerseFilterText))
            {
                e.Accepted = true;
                return;
            }

            PinsVerseListViewModel pinsVerse = e.Item as PinsVerseListViewModel;
            if (pinsVerse.VerseText.ToUpper().Contains(VerseFilterText.ToUpper()) ||
                pinsVerse.VerseIdShort.ToUpper().Contains(VerseFilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        #endregion // Methods
    }

}
