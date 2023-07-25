using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public abstract class VerseAwareConductorAllActive : DashboardConductorAllActive<EnhancedViewItemViewModel>
{

    private bool _paratextSync = true;
    private Dictionary<string, string> _bcvDictionary = new();
    private BookChapterVerseViewModel _currentBcv = new();
    private int _verseOffsetRange;
    private BookInfo? _currentBook;
    private string _verseChange = string.Empty;
    private BindableCollection<TokensTextRow>? _verses;
    private bool _enableBcvControl;
    private EnhancedViewLayout? _enhancedViewLayout;

    protected VerseAwareConductorAllActive(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator, IMediator? mediator,
        ILifetimeScope? lifetimeScope, ILocalizationService localizationService) :
        base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
    {
    }

    private string CurrentBookDisplay =>
        string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";

    public EnhancedViewLayout? EnhancedViewLayout
    {
        get => _enhancedViewLayout;
        set => Set(ref _enhancedViewLayout, value);
    }

    public bool ParatextSync
    {
        get => _paratextSync;
        set
        {
            if (value)
            {
                // update Paratext with the verseId
                //_ = Task.Run(() =>
                //    ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));

                // update this window with the Paratext verse
                CurrentBcv.SetVerseFromId(ProjectManager!.CurrentVerse);
            }

            var set = Set(ref _paratextSync, value);
            if (set)
            {
                EnhancedViewLayout!.ParatextSync = _paratextSync;
            }
        }
    }

    public Dictionary<string, string> BcvDictionary
    {
        get => _bcvDictionary;
        set => Set(ref _bcvDictionary, value);
    }

    public BookChapterVerseViewModel CurrentBcv
    {
        get => _currentBcv;
        set
        {
            var set = Set(ref _currentBcv, value);
            if (set)
            {
                EnhancedViewLayout!.BBBCCCVVV = _currentBcv.BBBCCCVVV;
            }
        }
    }

    public int VerseOffsetRange
    {
        get => _verseOffsetRange;
        set
        {
            var set = Set(ref _verseOffsetRange, value);
            if (set)
            {
                EnhancedViewLayout!.VerseOffset = value;
#pragma warning disable CS4014
                VerseChangeRerender();
#pragma warning restore CS4014
            }
        }
    }

    public BookInfo? CurrentBook
    {
        get => _currentBook;
        set
        {
            Set(ref _currentBook, value);
            NotifyOfPropertyChange(() => CurrentBookDisplay);
        }
    }

    public string VerseChange
    {
        get => _verseChange;
        set
        {
            if (_verseChange == "000000000")
            {
                return;
            }

            if (_verseChange == "")
            {
                _verseChange = value;
                NotifyOfPropertyChange(() => VerseChange);
            }
            else if (_verseChange != value)
            {
                ProjectManager!.CurrentVerse = value;
                // push to Paratext
                if (ParatextSync && !UpdatingCurrentVerse)
                {
                    Task.Run(() =>
                        ExecuteRequest(new SetCurrentVerseCommand(value), CancellationToken.None)
                    );
                }

                _verseChange = value;

#pragma warning disable CS4014
                VerseChangeRerender();
#pragma warning restore CS4014
                NotifyOfPropertyChange(() => VerseChange);
            }
        }
    }

    public BindableCollection<TokensTextRow>? Verses
    {
        get => _verses;
        set => Set(ref _verses, value);
    }

    public bool EnableBcvControl
    {
        get => _enableBcvControl;
        set => Set(ref _enableBcvControl, value);
    }

    protected bool UpdatingCurrentVerse { get; set; }

    public abstract Task LoadData(CancellationToken token);

    protected abstract Task ReloadData(ReloadType reloadType = ReloadType.Refresh);

    protected async Task VerseChangeRerender()
    {
        var sw = Stopwatch.StartNew();

        EnableBcvControl = false;

        try
        {
            await ReloadData();
        }
        finally
        {
            EnableBcvControl = true;
        }

        sw.Stop();
        Logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
    }
}

public abstract class VerseAwareConductorOneActive : DashboardConductorOneActive<EnhancedViewItemViewModel>
{

    private bool _paratextSync = true;
    private Dictionary<string, string> _bcvDictionary = new();
    private BookChapterVerseViewModel _currentBcv = new();
    private int _verseOffsetRange;
    private BookInfo? _currentBook;
    private string _verseChange = string.Empty;
    private BindableCollection<TokensTextRow>? _verses;
    private bool _enableBcvControl;
    private EnhancedViewLayout? _enhancedViewLayout;

    protected VerseAwareConductorOneActive(DashboardProjectManager? projectManager,
        INavigationService? navigationService, ILogger? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope, ILocalizationService localizationService) :
        base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,
            localizationService)
    {
        
    }

    private string CurrentBookDisplay =>
        string.IsNullOrEmpty(CurrentBook?.Code) ? string.Empty : $"<{CurrentBook.Code}>";

    public EnhancedViewLayout? EnhancedViewLayout
    {
        get => _enhancedViewLayout;
        set => Set(ref _enhancedViewLayout, value);
    }

    public bool ParatextSync
    {
        get => _paratextSync;
        set
        {
            if (value)
            {
                // update Paratext with the verseId
                //_ = Task.Run(() =>
                //    ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));

                // update this window with the Paratext verse
                CurrentBcv.SetVerseFromId(ProjectManager!.CurrentVerse);
            }

            var set = Set(ref _paratextSync, value);
            if (set)
            {
                EnhancedViewLayout!.ParatextSync = _paratextSync;
            }
        }
    }

    public Dictionary<string, string> BcvDictionary
    {
        get => _bcvDictionary;
        set => Set(ref _bcvDictionary, value);
    }

    public BookChapterVerseViewModel CurrentBcv
    {
        get => _currentBcv;
        set
        {
            var set = Set(ref _currentBcv, value);
            if (set)
            {
                EnhancedViewLayout!.BBBCCCVVV = _currentBcv.BBBCCCVVV;
            }
        }
    }

    public int VerseOffsetRange
    {
        get => _verseOffsetRange;
        set
        {
            var set = Set(ref _verseOffsetRange, value);
            if (set)
            {
                EnhancedViewLayout!.VerseOffset = value;
#pragma warning disable CS4014
                VerseChangeRerender();
#pragma warning restore CS4014
            }
        }
    }

    public BookInfo? CurrentBook
    {
        get => _currentBook;
        set
        {
            Set(ref _currentBook, value);
            NotifyOfPropertyChange(() => CurrentBookDisplay);
        }
    }

    public string VerseChange
    {
        get => _verseChange;
        set
        {
            if (_verseChange == "000000000")
            {
                return;
            }

            if (_verseChange == "")
            {
                _verseChange = value;
                NotifyOfPropertyChange(() => VerseChange);
            }
            else if (_verseChange != value)
            {
                ProjectManager!.CurrentVerse = value;
                // push to Paratext
                if (ParatextSync && !UpdatingCurrentVerse)
                {
                    Task.Run(() =>
                        ExecuteRequest(new SetCurrentVerseCommand(value), CancellationToken.None)
                    );
                }

                _verseChange = value;

#pragma warning disable CS4014
                VerseChangeRerender();
#pragma warning restore CS4014
                NotifyOfPropertyChange(() => VerseChange);
            }
        }
    }

    public BindableCollection<TokensTextRow>? Verses
    {
        get => _verses;
        set => Set(ref _verses, value);
    }

    public bool EnableBcvControl
    {
        get => _enableBcvControl;
        set => Set(ref _enableBcvControl, value);
    }

    protected bool UpdatingCurrentVerse { get; set; }

    public abstract Task LoadData(CancellationToken token);

    protected abstract Task ReloadData(ReloadType reloadType = ReloadType.Refresh);

    protected async Task VerseChangeRerender()
    {
        var sw = Stopwatch.StartNew();

        EnableBcvControl = false;

        try
        {
            await ReloadData();
        }
        finally
        {
            EnableBcvControl = true;
        }

        sw.Stop();
        Logger.LogInformation("VerseChangeRerender took {0} ms", sw.ElapsedMilliseconds);
    }
}
