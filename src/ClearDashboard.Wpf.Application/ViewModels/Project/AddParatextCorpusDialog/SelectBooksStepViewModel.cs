using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.Corpa;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

        private readonly ILocalizationService _localizationService;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<SelectedBook> _selectedBooks = new();
        public ObservableCollection<SelectedBook> SelectedBooks
        {
            get
            {
                var somethingSelected = false;
                var newBooksSelected = false;
                var oldBooksSelected = false;

                foreach (var book in _selectedBooks)
                {
                    if (book.IsEnabled && book.IsSelected && !somethingSelected)
                    {
                        somethingSelected = true;
                    }

                    if (!book.IsImported && book.IsEnabled && book.IsSelected)
                    {
                        newBooksSelected = true;
                    }

                    if (book.IsImported && book.IsEnabled && book.IsSelected)
                    {
                        oldBooksSelected = true;
                    }

                    if (somethingSelected && newBooksSelected && oldBooksSelected)
                    {
                        break;
                    }
                }

                OkButtonText = newBooksSelected
                    ? _localizationService.Get("UpdateParatextCorpusDialog_UpdateAndAdd")
                    : _localizationService.Get("Update");

                if (newBooksSelected && !oldBooksSelected)
                {
                    OkButtonText = _localizationService.Get("UpdateParatextCorpusDialog_Add");
                }

                ContinueEnabled = somethingSelected;

                return _selectedBooks;
            }
            set
            {
                _selectedBooks = value;
                NotifyOfPropertyChange(() => SelectedBooks);
            }
        }

        private string _okButtonText;
        public string OkButtonText
        {
            get { return _okButtonText; }
            set
            {
                _okButtonText = value;
                NotifyOfPropertyChange(() => OkButtonText);
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
        public Visibility SelectBooksVisibility { get; } = Visibility.Collapsed;
        public Visibility UpdateAddVisibility { get; } = Visibility.Visible;

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
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<SelectBooksStepViewModel> validator, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            _localizationService = localizationService;

            _projectManager = projectManager;
            if (selectBooksStepNextVisible)
            {
                NextVisibility = Visibility.Visible;
                OkVisibility = Visibility.Collapsed;

                SelectBooksVisibility = Visibility.Visible;
                UpdateAddVisibility = Visibility.Collapsed;
            }
            else
            {
                NextVisibility = Visibility.Collapsed;
                OkVisibility = Visibility.Visible;

                SelectBooksVisibility = Visibility.Collapsed;
                UpdateAddVisibility = Visibility.Visible;
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
        }

        private void FormatSelectedBooks()
        {
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
            FormatSelectedBooks();
            // get those books which actually have text in them from Paratext
            CancellationToken cancellationTokenProject = new();
            var requestFromParatext = await _projectManager?.ExecuteRequest(new GetVersificationAndBookIdByParatextProjectIdQuery(ParentViewModel.SelectedProject.Id), cancellationTokenProject);
            
            if (requestFromParatext.Success && requestFromParatext.HasData)
            {
                var booksInProject = requestFromParatext.Data;

                // iterate through and enable those books which have text
                foreach (var book in SelectedBooks)
                {
                    var found = booksInProject.BookAbbreviations.FirstOrDefault(x => x == book.Abbreviation);
                    if(found != null)
                    {
                        book.IsEnabled = true;
                        book.IsSelected = false; // set to false so that the end user doesn't automatically just select every book to enter
                    }
                    else
                    {
                        book.IsEnabled =false;
                        book.IsSelected = false;
                    }
                }
            }

            var tokenizedBookRequest = await _projectManager?.ExecuteRequest(new GetBooksFromTokenizedCorpusQuery(ParentViewModel.SelectedProject.Id), cancellationTokenProject);

            if (tokenizedBookRequest.Success && tokenizedBookRequest.HasData)
            {
                var booksTokenized = tokenizedBookRequest.Data;

                if (tokenizedBookRequest.Data.Count == 0)  // TODO : this section is a hack to deal with the fact that some duplicate projects have upper case project ids - you can remove this once the duplicate program is fixed
                {
                    // try again with upper case    
                    tokenizedBookRequest = await _projectManager?.ExecuteRequest(new GetBooksFromTokenizedCorpusQuery(ParentViewModel.SelectedProject.Id.ToUpper()), cancellationTokenProject);

                    if (tokenizedBookRequest.Success && tokenizedBookRequest.HasData)
                    {
                        booksTokenized = tokenizedBookRequest.Data;
                    }   
                }


                // iterate through and enable those books which have text
                foreach (var book in booksTokenized)
                {
                    if (Int32.TryParse(book, out int index))
                    {
                        SelectedBooks[index - 1].IsImported = true;
                        SelectedBooks[index - 1].IsEnabled = true;
                        SelectedBooks[index - 1].IsSelected = false;
                        SelectedBooks[index - 1].FontWeight = FontWeight.FromOpenTypeWeight(700);
                        SelectedBooks[index - 1].BookColor = new SolidColorBrush(Colors.Black);
                    }
                }
            }

            if (ParentViewModel.UsfmErrors != null)
            {
                foreach (var error in ParentViewModel.UsfmErrors)
                {
                    var indexString = BookChapterVerseViewModel.GetBookNumFromBookName(error.Reference.Substring(0, 3));
                    if (Int32.TryParse(indexString, out int index))
                    {
                        SelectedBooks[index - 1].BookColor = new SolidColorBrush(Colors.Red);
                        SelectedBooks[index - 1].HasUsfmError = true;
                    }
                }
            }

            NotifyOfPropertyChange(() => SelectedBooks);

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
            ParentViewModel?.Ok();
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
