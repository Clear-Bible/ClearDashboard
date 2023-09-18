using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.Corpa;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.Versification;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Models;
using MediatR;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClearDashboard.Wpf.Application.Services;

public class SelectedBookManager : PropertyChangedBase
{
    private ObservableCollection<SelectedBook> _selectedBooks;

    private readonly IMediator _mediator;

    public SelectedBookManager(IMediator mediator)
    {
        _mediator = mediator;
        Initialize();
    }

    public void Initialize()
    {
        _selectedBooks = new(CreateBooks());
    }

    public ObservableCollection<SelectedBook> SelectedBooks
    {
        get => _selectedBooks;
        set
        {
            Set(ref _selectedBooks, value);
            _selectedBooks.HookItemPropertyChanged<SelectedBook>((item, e) =>
            {
                NotifyOfPropertyChange(() => SelectedBooks);
            });
        }
    }

    public async Task InitializeBooks(IDictionary<string, IEnumerable<UsfmError>> usfmErrorsByParatextProjectId, bool enableTokenizedBooks, bool selectAllEnabledBooks, CancellationToken cancellationToken)
    {
        var commonBooks = CreateBooks(true, true).ToDictionary(e => e.Abbreviation, e => e);

        
        foreach (var kvp in usfmErrorsByParatextProjectId.Where(e => e.Key != null))
        {
            var books = await InitializeBooksInternal(kvp.Value, kvp.Key, enableTokenizedBooks, cancellationToken);

            foreach (var book in books.Where(book => book.HasUsfmError || !book.IsEnabled || !book.IsSelected))
            {
                commonBooks[book.Abbreviation].HasUsfmError = commonBooks[book.Abbreviation].HasUsfmError || book.HasUsfmError;
                commonBooks[book.Abbreviation].IsEnabled = commonBooks[book.Abbreviation].IsEnabled && book.IsEnabled; 
                commonBooks[book.Abbreviation].IsSelected = commonBooks[book.Abbreviation].IsSelected && book.IsSelected;
            }
        }

        SelectedBooks = new(commonBooks.Values);

        if (selectAllEnabledBooks)
        {
            var enabledBooks = SelectedBooks.Where(b => b.IsEnabled);
            foreach (var book in enabledBooks)
            {
                book.IsSelected = true;
            }
        }
        NotifyOfPropertyChange(() => SelectedBooks);
    }

    public async Task InitializeBooks(IEnumerable<UsfmError>? usfmErrors, string paratextProjectId, bool enableTokenizedBooks, CancellationToken cancellationToken)
    {
        var books = await InitializeBooksInternal(usfmErrors, paratextProjectId, enableTokenizedBooks, cancellationToken);
         
        SelectedBooks = new(books);
        NotifyOfPropertyChange(() => SelectedBooks);
    }

    private async Task<IEnumerable<SelectedBook>> InitializeBooksInternal(IEnumerable<UsfmError>? usfmErrors, string paratextProjectId, bool enableTokenizedBooks, CancellationToken cancellationToken)
    {
        var books = CreateBooks(true);

        // get those books which actually have text in them from Paratext
        var requestFromParatext = await _mediator.Send(new GetVersificationAndBookIdByParatextProjectIdQuery(paratextProjectId), cancellationToken);

        if (requestFromParatext.Success && requestFromParatext.HasData)
        {
            var booksInProject = requestFromParatext.Data;

            // iterate through and enable those books which have text
            foreach (var book in books)
            {
                var found = booksInProject!.BookAbbreviations!.FirstOrDefault(x => x == book.Abbreviation);
                if (found != null)
                {
                    book.IsEnabled = true;
                    book.IsSelected = false; // set to false so that the end user doesn't automatically just select every book to enter
                }

                // NB:  unremark to enable just the intersection of books
                //else
                //{
                //    book.IsEnabled = false;
                //    book.IsSelected = false;
                //}
            }
        }

        if (enableTokenizedBooks)
        {
            var tokenizedBookRequest = await _mediator.Send(new GetBooksFromTokenizedCorpusQuery(paratextProjectId), cancellationToken);

            if (tokenizedBookRequest.Success && tokenizedBookRequest.HasData)
            {
                var tokenizedBooks = tokenizedBookRequest.Data;

                if (tokenizedBooks != null)
                {
                    // iterate through and enable those books which have text
                    foreach (var book in tokenizedBooks)
                    {
                        if (int.TryParse(book, out var index))
                        {
                            books[index - 1].IsImported = true;
                            books[index - 1].IsEnabled = true;
                            books[index - 1].IsSelected = false;
                            books[index - 1].FontWeight = FontWeight.FromOpenTypeWeight(700);
                            books[index - 1].BookColor = new SolidColorBrush(Colors.Black);
                        }
                    }
                }
            }
        }

        if (usfmErrors != null)
        {
            foreach (var error in usfmErrors)
            {
                var indexString = BookChapterVerseViewModel.GetBookNumFromBookName(error.Reference.Substring(0, 3));
                if (int.TryParse(indexString, out var index))
                {
                    books[index - 1].BookColor = new SolidColorBrush(Colors.Red);
                    books[index - 1].HasUsfmError = true;
                    books[index - 1].IsEnabled = false;
                }
            }
        }

        return books;
    }

    public void UnselectAllBooks()
    {
        foreach (var selectedBook in _selectedBooks)
        {
            selectedBook.IsSelected = false;
        }

        NotifyOfPropertyChange(() => SelectedBooks);
    }

    public void SelectAllBooks()
    {
        foreach (var selectedBook in _selectedBooks)
        {
            if (selectedBook.IsEnabled)
            {
                selectedBook.IsSelected = true;
            }
        }

        NotifyOfPropertyChange(() => SelectedBooks);
    }


    private readonly int _firstNewTestamentBookNumber = 39;
    public void SelectNewTestamentBooks()
    {
        for (var i = _firstNewTestamentBookNumber; i < _selectedBooks.Count; i++)
        {
            if (_selectedBooks[i].IsEnabled)
            {
                _selectedBooks[i].IsSelected = true;
            }
        }

        NotifyOfPropertyChange(() => SelectedBooks);
    }



    public void SelectOldTestamentBooks()
    {
        for (var i = 0; i < _firstNewTestamentBookNumber; i++)
        {
            if (_selectedBooks[i].IsEnabled)
            {
                _selectedBooks[i].IsSelected = true;
            }
        }

        NotifyOfPropertyChange(() => SelectedBooks);
    }

    public void Touch()
    {
        NotifyOfPropertyChange(() => SelectedBooks);
    }

    public List<SelectedBook> SelectedAndEnabledBooks => SelectedBooks.Where(b => b.IsEnabled && b.IsSelected).ToList();

    public IEnumerable<string> SelectedAndEnabledBookAbbreviations => SelectedAndEnabledBooks.Select(b => b.Abbreviation);

    public bool HasSelectedAndEnabledOldTestamentBooks => SelectedAndEnabledBooks.Any(b => b.IsOldTestament);

    public bool HasSelectedAndEnabledNewTestamentBooks => SelectedAndEnabledBooks.Any(b => !b.IsOldTestament);

    public static List<SelectedBook> CreateBooks(bool isEnabledDefault = false, bool isSelectedDefault = false)
    {
        return new List<SelectedBook>
        {
            new()
            {
                BookNum = "01",
                BookName = "Genesis",
                Abbreviation = "GEN",
                ColorText = SelectedBook.BookColors.Pentateuch,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "02",
                BookName = "Exodus",
                Abbreviation = "EXO",
                ColorText = SelectedBook.BookColors.Pentateuch,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "03",
                BookName = "Leviticus",
                Abbreviation = "LEV",
                ColorText = SelectedBook.BookColors.Pentateuch,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "04",
                BookName = "Numbers",
                Abbreviation = "NUM",
                ColorText = SelectedBook.BookColors.Pentateuch,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "05",
                BookName = "Deuteronomy",
                Abbreviation = "DEU",
                ColorText = SelectedBook.BookColors.Pentateuch,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "06",
                BookName = "Joshua",
                Abbreviation = "JOS",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "07",
                BookName = "Judges",
                Abbreviation = "JDG",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "08",
                BookName = "Ruth",
                Abbreviation = "RUT",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "09",
                BookName = "1 Samuel(1 Kings)",
                Abbreviation = "1SA",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "10",
                BookName = "2 Samuel(2 Kings)",
                Abbreviation = "2SA",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "11",
                BookName = "1 Kings(3 Kings)",
                Abbreviation = "1KI",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "12",
                BookName = "2 Kings(4 Kings)",
                Abbreviation = "2KI",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "13",
                BookName = "1 Chronicles",
                Abbreviation = "1CH",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "14",
                BookName = "2 Chronicles",
                Abbreviation = "2CH",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "15",
                BookName = "Ezra",
                Abbreviation = "EZR",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "16",
                BookName = "Nehemiah",
                Abbreviation = "NEH",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "17",
                BookName = "Esther",
                Abbreviation = "EST",
                ColorText = SelectedBook.BookColors.Historical,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "18",
                BookName = "Job",
                Abbreviation = "JOB",
                ColorText = SelectedBook.BookColors.Wisdom,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "19",
                BookName = "Psalms",
                Abbreviation = "PSA",
                ColorText = SelectedBook.BookColors.Wisdom,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "20",
                BookName = "Proverbs",
                Abbreviation = "PRO",
                ColorText = SelectedBook.BookColors.Wisdom,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "21",
                BookName = "Ecclesiastes",
                Abbreviation = "ECC",
                ColorText = SelectedBook.BookColors.Wisdom,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "22",
                BookName = "Song of Solomon",
                Abbreviation = "SNG",
                ColorText = SelectedBook.BookColors.Wisdom,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "23",
                BookName = "Isaiah",
                Abbreviation = "ISA",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "24",
                BookName = "Jeremiah",
                Abbreviation = "JER",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "25",
                BookName = "Lamentations",
                Abbreviation = "LAM",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "26",
                BookName = "Ezekiel",
                Abbreviation = "EZK",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "27",
                BookName = "Daniel",
                Abbreviation = "DAN",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "28",
                BookName = "Hosea",
                Abbreviation = "HOS",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "29",
                BookName = "Joel",
                Abbreviation = "JOL",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "30",
                BookName = "Amos",
                Abbreviation = "AMO",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "31",
                BookName = "Obadiah",
                Abbreviation = "OBA",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "32",
                BookName = "Jonah",
                Abbreviation = "JON",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "33",
                BookName = "Micah",
                Abbreviation = "MIC",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "34",
                BookName = "Nahum",
                Abbreviation = "NAM",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "35",
                BookName = "Habakkuk",
                Abbreviation = "HAB",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "36",
                BookName = "Zephaniah",
                Abbreviation = "ZEP",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "37",
                BookName = "Haggai",
                Abbreviation = "HAG",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "38",
                BookName = "Zechariah",
                Abbreviation = "ZEC",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "39",
                BookName = "Malachi",
                Abbreviation = "MAL",
                ColorText = SelectedBook.BookColors.Prophets,
                IsOldTestament = true,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "40",
                BookName = "Matthew",
                Abbreviation = "MAT",
                ColorText = SelectedBook.BookColors.Gospels,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "41",
                BookName = "Mark",
                Abbreviation = "MRK",
                ColorText = SelectedBook.BookColors.Gospels,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "42",
                BookName = "Luke",
                Abbreviation = "LUK",
                ColorText = SelectedBook.BookColors.Gospels,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "43",
                BookName = "John",
                Abbreviation = "JHN",
                ColorText = SelectedBook.BookColors.Gospels,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "44",
                BookName = "Acts",
                Abbreviation = "ACT",
                ColorText = SelectedBook.BookColors.Acts,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "45",
                BookName = "Romans",
                Abbreviation = "ROM",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "46",
                BookName = "1 Corinthians",
                Abbreviation = "1CO",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "47",
                BookName = "2 Corinthians",
                Abbreviation = "2CO",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "48",
                BookName = "Galatians",
                Abbreviation = "GAL",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "49",
                BookName = "Ephesians",
                Abbreviation = "EPH",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "50",
                BookName = "Philippians",
                Abbreviation = "PHP",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "51",
                BookName = "Colossians",
                Abbreviation = "COL",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "52",
                BookName = "1 Thessalonians",
                Abbreviation = "1TH",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "53",
                BookName = "2 Thessalonians",
                Abbreviation = "2TH",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "54",
                BookName = "1 Timothy",
                Abbreviation = "1TI",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "55",
                BookName = "2 Timothy",
                Abbreviation = "2TI",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "56",
                BookName = "Titus",
                Abbreviation = "TIT",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "57",
                BookName = "Philemon",
                Abbreviation = "PHM",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "58",
                BookName = "Hebrews",
                Abbreviation = "HEB",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "59",
                BookName = "James",
                Abbreviation = "JAS",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "60",
                BookName = "1 Peter",
                Abbreviation = "1PE",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "61",
                BookName = "2 Peter",
                Abbreviation = "2PE",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "62",
                BookName = "1 John",
                Abbreviation = "1JN",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "63",
                BookName = "2 John",
                Abbreviation = "2JN",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "64",
                BookName = "3 John",
                Abbreviation = "3JN",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "65",
                BookName = "Jude",
                Abbreviation = "JUD",
                ColorText = SelectedBook.BookColors.Epistles,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            },
            new()
            {
                BookNum = "66",
                BookName = "Revelation",
                Abbreviation = "REV",
                ColorText = SelectedBook.BookColors.Revelation,
                IsOldTestament = false,
                IsEnabled = isEnabledDefault,
                IsSelected = isSelectedDefault,
            }
        };
    }
}
