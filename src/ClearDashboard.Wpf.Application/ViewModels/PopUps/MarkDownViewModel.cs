using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class MarkDownViewModel : DashboardApplicationScreen
    {

        #region Member Variables   

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private string _markdown;
        public string Markdown
        {
            get { return _markdown; }
            set
            {
                _markdown = value;
                NotifyOfPropertyChange(() => Markdown);
            }
        }



        #endregion //Observable Properties


        #region Constructor

        public MarkDownViewModel()
        {
            // no op
        }

        public MarkDownViewModel(INavigationService navigationService, ILogger<SlackMessageViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator,
            ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            try
            {
                string filePath = Path.Combine(Environment.CurrentDirectory, @"resources\markdown-cheat-sheet.md");
                if (File.Exists(filePath))
                {
                    var markdown = File.ReadAllText(filePath);
                    Markdown = markdown;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error in MarkDownViewModel constructor");
            }
        }

        #endregion //Constructor


        #region Methods


        public async void Close()
        {
            await TryCloseAsync();
        }

        #endregion // Methods




    }
}
