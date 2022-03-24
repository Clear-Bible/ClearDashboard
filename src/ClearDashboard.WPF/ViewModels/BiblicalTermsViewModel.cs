using ClearDashboard.Common;
using ClearDashboard.Common.Models;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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

        Dictionary<string, object> filters = new Dictionary<string, object>();
        #endregion //Member Variables

        #region Public Properties

        private BindableCollection<string> _domains;
        public BindableCollection<string> Domains
        {
            get { return _domains; }
            set
            {
                _domains = value;
                NotifyOfPropertyChange(() => Domains);
            }
        }

        private string _selectedDomain = string.Empty;

        public string SelectedDomain
        {
            get { return _selectedDomain; }
            set
            {
                _selectedDomain = value; 
                NotifyOfPropertyChange(() => SelectedDomain);

                //refresh the biblicalterms collection
                BiblicalTermsCollectionView.Refresh();
            }
        }


        #endregion //Public Properties

        #region Observable Properties

        public ICollectionView BiblicalTermsCollectionView { get; }

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

            SetupSemanticDomains();


            // setup the collectionview that binds to the datagrid
            BiblicalTermsCollectionView = CollectionViewSource.GetDefaultView(this._biblicalTerms);

            BiblicalTermsCollectionView.Filter = FilterTerms;
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

        private bool FilterTerms(object obj)
        {
            if (obj is BiblicalTermsData bt)
            {
                //if (! filters.ContainsKey(bt.SemanticDomain))
                //{
                //    filters.Add(bt.SemanticDomain, bt.SemanticDomain);
                //    Debug.WriteLine($"SEMANTIC DOMAIN: {bt.SemanticDomain}");
                //}

                if (SelectedDomain == "" || SelectedDomain == "*" || SelectedDomain is null)
                {
                    return true;
                }

                return bt.SemanticDomain.Contains(SelectedDomain);
            }

            return false;
        }

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

        private void SetupSemanticDomains()
        {
            _domains = new BindableCollection<string>();
            _domains.Add("*");
            _domains.Add("affection");
            _domains.Add("agriculture");
            _domains.Add("animals");
            _domains.Add("area");
            _domains.Add("area; nature");
            _domains.Add("association");
            _domains.Add("construction; religious activities");
            _domains.Add("constructions; animal husbandry");
            _domains.Add("containers; animal husbandry");
            _domains.Add("crafts; cloth");
            _domains.Add("fruits");
            _domains.Add("gemstones");
            _domains.Add("grasses");
            _domains.Add("group");
            _domains.Add("group; area");
            _domains.Add("honor, respect, status");
            _domains.Add("locale");
            _domains.Add("mammals; domestic animals");
            _domains.Add("mammals; wild animals");
            _domains.Add("monument");
            _domains.Add("morals and ethics");
            _domains.Add("mourning");
            _domains.Add("nature");
            _domains.Add("paganism");
            _domains.Add("people");
            _domains.Add("people; authority");
            _domains.Add("people; honor, respect, status");
            _domains.Add("person");
            _domains.Add("purpose");
            _domains.Add("religious activities");
            _domains.Add("sacrifices and offerings");
            _domains.Add("settlement");
            _domains.Add("signs and wonders");
            _domains.Add("supernatural beings and powers");
            _domains.Add("supernatural beings and powers; titles");
            _domains.Add("tools");
            _domains.Add("tools; childbirth");
            _domains.Add("tools; weight; commerce");
            _domains.Add("trees; fruits");
            _domains.Add("trees; perfumes and spices");
            _domains.Add("wisdom, understanding");
            NotifyOfPropertyChange(() => Domains);
        }

        #endregion // Methods

    }
}
