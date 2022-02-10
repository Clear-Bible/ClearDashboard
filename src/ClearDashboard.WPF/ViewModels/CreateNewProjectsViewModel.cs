using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL.Paratext;
using ClearDashboard.Wpf.Helpers;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;

namespace ClearDashboard.Wpf.ViewModels
{
    public class CreateNewProjectsViewModel : ObservableObject
    {
        #region props
        public bool ParatextVisible = false;
        public bool ShowWaitingIcon = true;


        private FlowDocument _helpText;
        public FlowDocument HelpText
        {
            get => _helpText;
            set { SetProperty(ref _helpText, value, nameof(HelpText)); }
        }


        private bool _ButtonEnabled;
        public bool ButtonEnabled
        {
            get => _ButtonEnabled;
            set { SetProperty(ref _ButtonEnabled, value, nameof(ButtonEnabled)); }
        }



        private List<ParatextProject> _LWCprojects = new List<ParatextProject>();
        private ParatextProject _TargetProject = null; 
        private List<ParatextProject> _BackTransProjects = new List<ParatextProject>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();

        public ObservableRangeCollection<ParatextProjectDisplay> ParatextResources { get; set; } =
            new ObservableRangeCollection<ParatextProjectDisplay>();

        #endregion
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
        #region commands


        #endregion

        public CreateNewProjectsViewModel()
        {
            createNewProjectCommand = new RelayCommand(CreateNewProject);
        }

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

        public async Task Init()
        {
            // get the right help text
            // TODO Work on the help regionalization
            ResourceDictionary dict = new ResourceDictionary { Source = new Uri("/HelpFiles/NewProjectHelp_us.xaml", UriKind.Relative) };
            var text = dict["helpText_us"] as FlowDocument;
            if (text != null)
            {
                HelpText = text;
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

        internal void SetProjects(List<ParatextProject> lWCproject, ParatextProject targetProject, List<ParatextProject> backTransProject)
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
