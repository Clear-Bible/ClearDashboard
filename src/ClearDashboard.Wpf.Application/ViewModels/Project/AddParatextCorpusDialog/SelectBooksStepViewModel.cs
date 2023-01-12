using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog
{
    public class SelectBooksStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParatextCorpusDialogViewModel, SelectBooksStepViewModel>
    {
        private readonly DashboardProjectManager _projectManager;

        #region Member Variables

        public RelayCommand NtCommand { get; }
        public RelayCommand OtCommand { get; }
        public RelayCommand NoneCommand { get; }
        public RelayCommand AllCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand NextCommand { get; }
        public RelayCommand BackCommand { get; }

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<SelectedBook> _selectedBooks = new();
        public ObservableCollection<SelectedBook> SelectedBooks
        {
            get
            {
                var somethingSelected = _selectedBooks.FirstOrDefault(t => t.IsEnabled && t.IsSelected);
                if (somethingSelected is null)
                {
                    ContinueEnabled = false;
                }
                else
                {
                    ContinueEnabled = true;
                }

                return _selectedBooks;
            }
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
        public Visibility NextVisibility { get; } = Visibility.Collapsed;
        public Visibility OkVisibility { get; } = Visibility.Visible;

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

        public SelectBooksStepViewModel(DialogMode dialogMode, DashboardProjectManager projectManager, bool selectBooksStepNextVisible,
            INavigationService navigationService, ILogger<SelectBooksStepViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator)
        {
            _projectManager = projectManager;
            if (selectBooksStepNextVisible)
            {
                NextVisibility = Visibility.Visible;
                OkVisibility = Visibility.Collapsed;
            }
            else
            {
                NextVisibility = Visibility.Collapsed;
                OkVisibility = Visibility.Visible;
            }
            DialogMode = dialogMode;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            CanOk = true;
            EnableControls = true;

            CanAdd = false;

            ContinueEnabled = false;
            
            // wire up the relay commands
            NtCommand = new RelayCommand(Nt);
            OtCommand = new RelayCommand(Ot);
            NoneCommand = new RelayCommand(UnselectAll);
            AllCommand = new RelayCommand(SelectAll);
            OkCommand = new RelayCommand(Ok);
            NextCommand = new RelayCommand(Next);
            BackCommand = new RelayCommand(BackAsync);

            // initialize the Bible books 
            var books = SelectedBook.Init();
            foreach (var book in books)
            {
                book.IsEnabled = false;
            }

            SelectedBooks = new ObservableCollection<SelectedBook>(books);
        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            // get those books which actually have text in them from Paratext
            CancellationToken cancellationTokenProject = new();
            var request = await _projectManager?.ExecuteRequest(new GetVersificationAndBookIdByParatextProjectIdQuery(ParentViewModel.SelectedProject.Id), cancellationTokenProject);

            if (request.Success && request.HasData)
            {
                if (request.HasData)
                {
                    var books = request.Data;

                    // iterate through and enable those books which have text
                    foreach (var book in SelectedBooks)
                    {
                        var found = books.BookAbbreviations.FirstOrDefault(x => x == book.Abbreviation);
                        if(found != null)
                        {
                            book.IsEnabled = true;
                            book.IsSelected = true;
                        }
                        else
                        {
                            book.IsEnabled =false;
                            book.IsSelected = false;
                        }
                    }

                    NotifyOfPropertyChange(() => SelectedBooks);
                }
            }

            base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor


        #region Methods
        protected override ValidationResult? Validate()
        {
            return null;
        }

        public void Ok(object obj)
        {
            foreach (var book in SelectedBooks)
            {
                if (book.IsEnabled && book.IsSelected)
                {
                    ParentViewModel.BookIds.Add(book.Abbreviation);
                }
            }

            ParentViewModel?.Ok();
        }

        public async void Next(object obj)
        {
            foreach (var book in SelectedBooks)
            {
                if (book.IsEnabled && book.IsSelected)
                {
                    ParentViewModel.BookIds.Add(book.Abbreviation);
                }
            }
            await MoveForwards();
        }
        public void BackAsync(object obj)
        {
            MoveBackwards();
        }

        private void UnselectAll(object obj)
        {
            for (int i = 0; i < _selectedBooks.Count; i++)
            {
                _selectedBooks[i].IsSelected = false;
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }

        private void SelectAll(object obj)
        {
            for (int i = 0; i < _selectedBooks.Count; i++)
            {
                if (_selectedBooks[i].IsEnabled)
                {
                    _selectedBooks[i].IsSelected = true;
                }
                
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }

        private void Nt(object obj)
        {
            for (int i = 39; i < _selectedBooks.Count; i++)
            {
                if (_selectedBooks[i].IsEnabled)
                {
                    _selectedBooks[i].IsSelected = true;
                }
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }

        private void Ot(object obj)
        {
            for (int i = 0; i < 39; i++)
            {
                if (_selectedBooks[i].IsEnabled)
                {
                    _selectedBooks[i].IsSelected = true;
                }
            }
            NotifyOfPropertyChange(() => SelectedBooks);
        }



        #endregion // Methods



    }
}
