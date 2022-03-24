using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ClearDashboard.DAL.NamedPipes;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Interfaces;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pipes_Shared;
using Action = System.Action;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel, IWorkspace
    {
        #region Member Variables

        public ILogger Logger { get; set; }
        public INavigationService NavigationService { get; set; }
        public StartUp _DAL { get; set; }
        private string _currentVerse = "";


        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties
        private ObservableCollection<BiblicalTermsData> _biblicalTerms = new ObservableCollection<BiblicalTermsData>();
        public ObservableCollection<BiblicalTermsData> BiblicalTerms
        {
            get => _biblicalTerms;
            set
            {
                _biblicalTerms = value;
                NotifyOfPropertyChange(() => BiblicalTerms);
            }
        }

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


        #endregion //Observable Properties

        #region Constructor
        public BiblicalTermsViewModel(INavigationService navigationService, ILogger<WorkSpaceViewModel> logger, StartUp dal)
        {
            this.NavigationService = navigationService;
            this.Logger = logger;
            this._DAL = dal;

            this.Title = "🕮 BIBLICAL TERMS";
            this.ContentId = "BIBLICALTERMS";
            this.DockSide = EDockSide.Left;

            _DAL.NamedPipeChanged += HandleEventAsync;
        }


        public async void HandleEventAsync(object sender, NamedPipesClient.PipeEventArgs args)
        {
            if (args == null) return;

            PipeMessage pipeMessage = args.PM;

            switch (pipeMessage.Action)
            {
                case ActionType.CurrentVerse:
                    if (_currentVerse != pipeMessage.Text)
                    {
                        // ask for Biblical Terms
                        await _DAL.SendPipeMessage((StartUp.PipeAction)ActionType.GetBibilicalTerms)
                            .ConfigureAwait(false);
                    }

                    break;
                case ActionType.SetBiblicalTerms:
                    // invoke to get it to run in STA mode
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _biblicalTerms.Clear();

                        // deserialize the list
                        List<BiblicalTermsData> biblicalTermsList = new List<BiblicalTermsData>();
                        try
                        {
                            biblicalTermsList = JsonConvert.DeserializeObject<List<BiblicalTermsData>>((string)pipeMessage.Payload);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError($"BiblicalTermsViewModel Deserialize BibilicalTerms: {e.Message}");
                        }

                        if (biblicalTermsList.Count > 0)
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                _biblicalTerms.Add(biblicalTermsList[i]);

                                foreach (var rendering in biblicalTermsList[i].Renderings)
                                {
                                    _biblicalTerms[i].RenderingString += rendering.ToString() + " ";
                                }
                            }

                            NotifyOfPropertyChange(() => BiblicalTerms);


                        }
                    });

                    await Task.Run(() =>
                    {
                        ProgressBarVisibility = Visibility.Collapsed;
                    }).ConfigureAwait(false);
                    System.Windows.Forms.Application.DoEvents();
                    break;
            }

            await Task.CompletedTask;
        }



        protected override void OnViewAttached(object view, object context)
        {
            Debug.WriteLine("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            Debug.WriteLine("OnViewLoaded");
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            Debug.WriteLine("OnViewReady");
            base.OnViewReady(view);
        }

        protected override void Dispose(bool disposing)
        {
            _DAL.NamedPipeChanged -= HandleEventAsync;

            Debug.WriteLine("Dispose");
            base.Dispose(disposing);
        }

        #endregion //Constructor

        #region Methods

        /// <summary>
        /// Send message to server that we want the Biblical Terms list
        /// </summary>
        public async void ReloadBiblicalTerms()
        {
            if (_DAL.IsPipeConnected)
            {
                await Task.Run(() =>
                {
                    ProgressBarVisibility = Visibility.Visible;
                }).ConfigureAwait(false);
                System.Windows.Forms.Application.DoEvents();
                
                await _DAL.SendPipeMessage(StartUp.PipeAction.GetBibilicalTerms).ConfigureAwait(false);
            }
        }

        
        #endregion // Methods

    }
}
