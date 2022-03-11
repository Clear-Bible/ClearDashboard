using ClearDashboard.Common.Models;
using ClearDashboard.DAL.Paratext;
using ClearDashboard.Wpf.Helpers;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using MdXaml;

namespace ClearDashboard.Wpf.ViewModels
{
    public class CreateNewProjectsViewModel : Screen
    {
        #region props
        private readonly ILog _logger;

        public bool ParatextVisible = false;
        public bool ShowWaitingIcon = true;


        private string _helpText;
        public string HelpText
        {
            get => _helpText;
            set
            {
                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
            }
        }


        private bool _ButtonEnabled;
        public bool ButtonEnabled
        {
            get => _ButtonEnabled;
            set
            {
                _ButtonEnabled = value;
                NotifyOfPropertyChange(() => ButtonEnabled);
            }
        }



        private List<ParatextProject> _LWCprojects = new List<ParatextProject>();
        private ParatextProject _TargetProject = null; 
        private List<ParatextProject> _BackTransProjects = new List<ParatextProject>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextResources { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();


        private Task TextXamlChangeEvent;
        public string _textXaml;
        public string TextXaml
        {
            get { return _textXaml; }
            set
            {
                if (_textXaml == value) return;
                _textXaml = value;
                if (TextXamlChangeEvent == null || TextXamlChangeEvent.Status >= TaskStatus.RanToCompletion)
                {
                    TextXamlChangeEvent = Task.Run(() =>
                    {
                        Task.Delay(100);
                        retry:
                        var oldVal = _textXaml;

                        Thread.MemoryBarrier();
                        _textXaml = value;
                        NotifyOfPropertyChange(() => TextXaml);

                        Thread.MemoryBarrier();
                        if (oldVal != _textXaml) goto retry;
                    });
                }
            }
        }

        #endregion

        #region commands
        private ICommand createNewProjectCommand { get; set; }
        public ICommand CreateNewProjectCommand
        {
            get
            {
                return createNewProjectCommand;
            }
            set
            {
                createNewProjectCommand = value;
            }
        }

        #endregion

        #region Startup

        public CreateNewProjectsViewModel()
        {
            _logger = ((App)Application.Current).Log;
            createNewProjectCommand = new RelayCommand(CreateNewProject);
        }


        // NB:  GERFEN - calling Init here, instead of in the view.
        protected override async  void OnViewAttached(object view, object context)
        {
            await Init();
            base.OnViewAttached(view, context);
        }

        public async Task Init()
        {
            // get the right help text
            // TODO Work on the help regionalization
            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var helpFile = Path.Combine(fi.Directory.ToString(), @"HelpFiles\NewProjectHelp_us.md");

            if (File.Exists(helpFile))
            {
                string markdownTxt = File.ReadAllText(helpFile);
                HelpText = String.Join("\r\n", Regex.Split(markdownTxt, "\r?\n").Select(ln => ln.TrimStart()));
            }


            // detect if Paratext is installed
            ParatextUtils paratextUtils = new ParatextUtils();
            ParatextVisible = paratextUtils.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                List<ParatextProject> projects = await paratextUtils.GetParatextProjectsOrResources(ParatextUtils.eFolderType.Projects);
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectDisplay>();
                    foreach (var project in projects)
                    {
                        ParatextProjects.Add(TinyMapper.Map<ParatextProjectDisplay>(project));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                List<ParatextProject> resources = paratextUtils.GetParatextResources();
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectDisplay>();
                    foreach (var resource in resources)
                    {
                        ParatextResources.Add(TinyMapper.Map<ParatextProjectDisplay>(resource));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }

        #endregion

        public void CreateNewProject(object obj)
        {
            if (_TargetProject == null)
            {
                // unlikely to be true
                return;
            }

            DashboardProject dashboardProject = new DashboardProject();
            dashboardProject.TargetProject = _TargetProject;
            dashboardProject.LWCProjects = _LWCprojects;
            dashboardProject.BTProjects = _BackTransProjects;
            dashboardProject.CreationDate = DateTime.Now;
            dashboardProject.ParatextUser = "";

        }

        internal void SetProjects(List<ParatextProject> lWCproject, ParatextProject targetProject, List<ParatextProject> backTransProject, ParatextProject _interlinearizerProject)
        {
            _LWCprojects = new List<ParatextProject>(lWCproject);
            _TargetProject = targetProject;
            _BackTransProjects = new List<ParatextProject>(backTransProject);

            // check to see if we have at least a target project
            if (_TargetProject is null)
            {
                ButtonEnabled = false;
            }
            else
            {
                ButtonEnabled = true;
            }
        }

    }

    public class ParatextProjectDisplay : ParatextProject
    {
        private bool _inUse;
        public bool InUse
        {
            get => _inUse;
            set
            {
                _inUse = value;
                OnPropertyChanged(nameof(InUse));
            }
        }

    }
}
