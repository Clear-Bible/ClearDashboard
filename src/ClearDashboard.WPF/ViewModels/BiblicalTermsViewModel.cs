using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL.NamedPipes;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Interfaces;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pipes_Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SIL.Extensions;
using Point = System.Windows.Point;

namespace ClearDashboard.Wpf.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BiblicalTermsViewModel : ToolViewModel, IWorkspace
    {
        #region Member Variables

        public enum SelectedBTtype
        {
            OptionAll,
            OptionProject
        }

        public ILogger Logger { get; set; }
        public INavigationService NavigationService { get; set; }
        public ProjectManager _DAL { get; set; }
        private string _currentVerse = "";

        //Dictionary<string, object> filters = new Dictionary<string, object>();
        #endregion //Member Variables

        #region Public Properties

        private string _gloss;

        public string Gloss
        {
            get { return _gloss; }
            set
            {
                _gloss = value;
                NotifyOfPropertyChange(() => Gloss);
            }
        }


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

        private SelectedBTtype _lastSelectedBTtype = SelectedBTtype.OptionProject;

        private SelectedBTtype _selectedBiblicalTermsType = SelectedBTtype.OptionProject;
        public SelectedBTtype SelectedBiblicalTermsType
        {
            get { return _selectedBiblicalTermsType; }
            set
            {
                _selectedBiblicalTermsType = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsType);

                if (_lastSelectedBTtype != _selectedBiblicalTermsType)
                {
                    if (_selectedBiblicalTermsType == SelectedBTtype.OptionProject)
                    {
                        _DAL.SendPipeMessage(ProjectManager.PipeAction.GetBibilicalTermsProject);
                    }
                    else
                    {
                        _DAL.SendPipeMessage(ProjectManager.PipeAction.GetBibilicalTermsAll);
                    }

                    _lastSelectedBTtype = _selectedBiblicalTermsType;
                }
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

        private BiblicalTermsData _selectedBiblicalTermsData;
        public BiblicalTermsData SelectedBiblicalTermsData
        {
            get { return _selectedBiblicalTermsData; }
            set
            {
                _selectedBiblicalTermsData = value;
                NotifyOfPropertyChange(() => SelectedBiblicalTermsData);
                UpdateSelectedTerm(SelectedBiblicalTermsData);
            }
        }

        private ObservableCollection<Verse> _selectedItemVerses = new ObservableCollection<Verse>();
        public ObservableCollection<Verse> SelectedItemVerses
        {
            get { return _selectedItemVerses; }
            set
            {
                _selectedItemVerses = value;
                NotifyOfPropertyChange(() => SelectedItemVerses);
            }
        }

        private ObservableCollection<string> _renderings = new ObservableCollection<string>();
        public ObservableCollection<string> Renderings
        {
            get { return _renderings; }
            set
            {
                _renderings = value;
                NotifyOfPropertyChange(() => Renderings);
            }
        }

        private ObservableCollection<string> _renderingsText = new ObservableCollection<string>();
        public ObservableCollection<string> RenderingsText
        {
            get { return _renderingsText; }
            set
            {
                _renderingsText = value;
                NotifyOfPropertyChange(() => RenderingsText);
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
        public BiblicalTermsViewModel(INavigationService navigationService, ILogger<WorkSpaceViewModel> logger, ProjectManager dal)
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
                        await _DAL.SendPipeMessage((ProjectManager.PipeAction)ActionType.GetBibilicalTermsProject)
                            .ConfigureAwait(false);
                    }

                    break;
                case ActionType.SetBiblicalTerms:
                    // invoke to get it to run in STA mode
                    Application.Current.Dispatcher.Invoke(delegate
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
                            for (int i = 0; i < biblicalTermsList.Count; i++)
                            {
                                _biblicalTerms.Add(biblicalTermsList[i]);

                                foreach (var rendering in biblicalTermsList[i].Renderings)
                                {
                                    _biblicalTerms[i].RenderingString += rendering + " ";
                                }
                            }

                            NotifyOfPropertyChange(() => BiblicalTerms);
                        }
                    });

                    ProgressBarVisibility = Visibility.Collapsed;
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
        /// On BiblicalTerms Click - these are the bottom verse/verse references
        /// </summary>
        /// <param name="selectedBiblicalTermsData"></param>
        private void UpdateSelectedTerm(BiblicalTermsData selectedBiblicalTermsData)
        {
            SelectedItemVerses.Clear();
            Renderings.Clear();
            Gloss = "";
            if (selectedBiblicalTermsData is not null)
            {
                Gloss = selectedBiblicalTermsData.Gloss;

                for (int i = 0; i < selectedBiblicalTermsData.ReferencesLong.Count; i++)
                {
                    string verseRef = selectedBiblicalTermsData.ReferencesLong[i];
                    string verseText = "";
                    if (i < selectedBiblicalTermsData.ReferencesListText.Count)
                    {
                        verseText = selectedBiblicalTermsData.ReferencesListText[i];
                    }

                    _selectedItemVerses.Add(new Verse
                    {
                        VerseID = verseRef,
                        VerseText = verseText
                    });
                }

                foreach (var render in selectedBiblicalTermsData.Renderings)
                {
                    _renderings.Add(render);
                }
            }

            // create inlines of the selected word
            foreach (var verse in _selectedItemVerses)
            {
                var verseText = verse.VerseText;
                List<Point> points = new List<Point>();
                foreach (var render in selectedBiblicalTermsData.Renderings)
                {
                    try
                    {
                        Regex regexObj = new Regex(@"\W*((?i)" + render + @"(?-i))\W*");
                        Match matchResults = regexObj.Match(verseText);
                        while (matchResults.Success)
                        {
                            // matched text: matchResults.Value
                            // match start: matchResults.Index
                            // match length: matchResults.Length
                            Point point = new Point(matchResults.Index, matchResults.Index + matchResults.Length - 1);
                            points.Add(point);

                            matchResults = matchResults.NextMatch();
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        // Syntax error in the regular expression
                    }

                    // interate through in while loop
                    int index = verseText.IndexOf(render, StringComparison.CurrentCultureIgnoreCase);
                    if (index == -1)
                    {
                        verse.Inlines.Add(new Run(verseText));
                        verse.Found = false;
                    }
                    else
                    {
                        verse.Found = true;
                        while (true)
                        {
                            verse.Inlines.AddRange(new Inline[]
                            {
                                new Run(verseText.Substring(0, index)),
                                new Run(verseText.Substring(index, render.Length))
                                {
                                    FontWeight = FontWeights.Bold,
                                    Foreground = Brushes.Orange
                                }
                            });

                            verseText = verseText.Substring(index + render.Length);
                            index = verseText.IndexOf(render, StringComparison.CurrentCultureIgnoreCase);

                            if (index < 0)
                            {
                                verse.Inlines.Add(new Run(verseText));
                                break;
                            }
                        }
                    }

                    //// interate through in reverse
                    //for (int i = points.Count - 1; i >= 0; i--)
                    //{
                    //    tb = new TextBlock();
                    //    try
                    //    {
                    //        string endPart = verseText.Substring((int)points[i].Y - 1);
                    //        string startPart = verseText.Substring(0, (int)points[i].Y - 1 - render.Length);

                    //        var a = new Run(startPart) { FontWeight = FontWeights.Normal };
                    //        tb.Inlines.Add(new Run(render) { FontWeight = FontWeights.Bold, Foreground= Brushes.Orange });
                    //        tb.Inlines.Add(new Run(endPart) { FontWeight = FontWeights.Normal });

                    //        // check if this was the last one
                    //        if (i == 1)
                    //        {
                    //            tb.Inlines.Add(new Run(startPart) { FontWeight = FontWeights.Normal });
                    //        }
                    //        else
                    //        {
                    //            verseText = verseText.Substring(0, verseText.Length - render.Length);
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Console.WriteLine(e);
                    //        throw;
                    //    }
                    //}
                    
                }
            }
            

            NotifyOfPropertyChange(() => SelectedItemVerses);
            NotifyOfPropertyChange(() => Renderings);
            NotifyOfPropertyChange(() => RenderingsText);
        }

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

                await _DAL.SendPipeMessage(ProjectManager.PipeAction.GetBibilicalTermsProject).ConfigureAwait(false);
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
