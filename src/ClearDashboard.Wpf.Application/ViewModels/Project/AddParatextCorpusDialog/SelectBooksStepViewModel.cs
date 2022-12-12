using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class SelectBooksStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, SelectBooksStepViewModel>
    {
        private readonly DashboardProjectManager _projectManager;

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<SelectedBook> _selectedBooks = new();
        public ObservableCollection<SelectedBook> SelectedBooks
        {
            get => _selectedBooks; 
            set
            {
                _selectedBooks = value;
                NotifyOfPropertyChange(() => SelectedBooks);
            }
        }


        private DialogMode _dialogMode;
        public DialogMode DialogMode
        {
            get => _dialogMode;
            set => Set(ref _dialogMode, value);
        }

        private bool _canOk;
        public bool CanOk
        {
            get => _canOk;
            set => Set(ref _canOk, value);
        }

        private bool _canAdd;
        public bool CanAdd
        {
            get => _canAdd;
            set => Set(ref _canAdd, value);
        }

        private bool _continueEnabled;
        public bool ContinueEnabled
        {
            get { return _continueEnabled; }
            set
            {
                _continueEnabled = value;
                NotifyOfPropertyChange(() => ContinueEnabled);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public SelectBooksStepViewModel()
        {
            // no-op
        }

        public SelectBooksStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<SelectBooksStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            _projectManager = projectManager;

            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            CanOk = true;
            EnableControls = true;

            CanAdd = false;

            ContinueEnabled = false;
            var books = SelectedBook.Init();
            foreach (var book in books)
            {
                book.IsEnabled = false;
            }

            SelectedBooks = new ObservableCollection<SelectedBook>(books);
        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            CancellationToken cancellationTokenProject = new();

            //ParentViewModel.CurrentStepTitle = LocalizationStrings.Get("ParatextCorpusDialog_SelectBooks", Logger);

            //var alignment = LocalizationStrings.Get("AddParatextCorpusDialog_Alignment", Logger);

            var request = await _projectManager?.ExecuteRequest(new GetVersificationAndBookIdByParatextProjectIdQuery(ParentViewModel.SelectedProject.Id), cancellationTokenProject);

            if (request.Success && request.HasData)
            {
                if (request.HasData)
                {
                    var books = request.Data;


                }
            }

            
            base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor


        #region Methods
        protected override ValidationResult? Validate()
        {
            // TODO
            return null;
        }

        public void Ok()
        {
            ParentViewModel?.Ok();
        }

        public async void Add()
        {
            await Add(true);
        }

        public async Task Add(object nothing)
        {
            CanAdd = false;
            _ = await Task.Factory.StartNew(async () =>
            {
 
            }, CancellationToken.None);
        }

        private void PerformUnselectAll()
        {
            for (int i = 0; i < _selectedBooks.Count; i++)
            {
                _selectedBooks[i].IsSelected = false;
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }

        private void PerformSelectAll()
        {
            for (int i = 0; i < _selectedBooks.Count; i++)
            {
                _selectedBooks[i].IsSelected = true;
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }

        private void PerformNT()
        {
            bool toggle = !_selectedBooks[39].IsSelected;

            for (int i = 39; i < _selectedBooks.Count; i++)
            {
                _selectedBooks[i].IsSelected = toggle;
            }
            NotifyOfPropertyChange(() => SelectedBooks);
            }

        private void PerformOT()
        {
            bool toggle = !_selectedBooks[0].IsSelected;

            for (int i = 0; i < 39; i++)
            {
                _selectedBooks[i].IsSelected = toggle;
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }



        #endregion // Methods



    }
}
