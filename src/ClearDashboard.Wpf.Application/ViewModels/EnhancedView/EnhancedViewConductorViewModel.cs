using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
#pragma warning disable CS8618

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

public class EnhancedViewConductorViewModel : DashboardConductorAllActive<object>, IPaneViewModel, IAvalonDockWindow
{
    public Guid PaneId => Guid.NewGuid();
   
    public ImageSource? IconSource { get; }
    public bool IsSelected { get; set; }
    public string ContentId { get; set; }


    #region Commands
    public ICommand RequestCloseCommand { get; set; }
    public ICommand MoveCorpusDownRowCommand { get; set; }
    public ICommand MoveCorpusUpRowCommand { get; set; }
    public ICommand DeleteCorpusRowCommand { get; set; }


    #endregion

    public async Task RequestClose(object obj)
    {
        await EventAggregator.PublishOnUIThreadAsync(new CloseDockingPane(PaneId));
    }


    public EnhancedViewConductorViewModel()
    {
        Initialize();
    }

    private void InitializeCommands()
    {
        RequestCloseCommand = new RelayCommandAsync(RequestClose);
        MoveCorpusDownRowCommand = new RelayCommand(MoveCorpusDown);
        MoveCorpusUpRowCommand = new RelayCommand(MoveCorpusUp);
        DeleteCorpusRowCommand = new RelayCommand(DeleteCorpusRow);
    }


    public EnhancedViewConductorViewModel(DashboardProjectManager? projectManager,
                                          INavigationService? navigationService, 
                                          ILogger<EnhancedViewConductorViewModel>? logger,
                                          IEventAggregator? eventAggregator,
                                          IMediator? mediator,
                                          ILifetimeScope? lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
       
        Initialize();
    }

    private void Initialize()
    {
        Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedView", Logger!);
        ContentId = "ENHANCEDVIEW";

        InitializeCommands();
    }


    private void MoveCorpusUp(object obj)
    {
        //var row = obj as VersesDisplay;

        //var index = VersesDisplay.Select((element, index) => new { element, index })
        //    .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

        //if (index < 1)
        //{
        //    return;
        //}

        //VersesDisplay.Move(index, index - 1);
    }

    private void MoveCorpusDown(object obj)
    {
        //var row = obj as VersesDisplay;

        //var index = VersesDisplay.Select((element, index) => new { element, index })
        //    .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

        //if (index == VersesDisplay.Count - 1)
        //{
        //    return;
        //}

        //VersesDisplay.Move(index, index + 1);
    }

    private void DeleteCorpusRow(object obj)
    {
        //var row = (VersesDisplay)obj;

        //// remove from the display
        //var index = VersesDisplay.Select((element, index) => new { element, index })
        //    .FirstOrDefault(x => x.element.Equals(row))?.index ?? -1;

        //VersesDisplay.RemoveAt(index);
        //// remove from the grouping for saving
        //DisplayOrder.RemoveAt(index);

        //// remove from stored collection
        //var project = _parallelProjects.FirstOrDefault(x => x.ParallelCorpusId == row.ParallelCorpusId);
        //if (project != null)
        //{
        //    _parallelProjects.Remove(project);
        //}

        //var parallelMessages = _parallelMessages.FirstOrDefault(x => Guid.Parse(x.ParallelCorpusId) == row.ParallelCorpusId);
        //if (parallelMessages is not null)
        //{
        //    _parallelMessages.Remove(parallelMessages);
        //}



        //// remove stored collection
        //var tokenProject = _tokenProjects.FirstOrDefault(x => x.CorpusId == row.CorpusId);
        //if (tokenProject is not null)
        //{
        //    _tokenProjects.Remove(tokenProject);
        //}

        //// remove stored message
        //var tokenMessage = _projectMessages.FirstOrDefault(x => x.CorpusId == row.CorpusId);
        //if (tokenMessage is not null)
        //{
        //    _projectMessages.Remove(tokenMessage);
        //}

    }
}