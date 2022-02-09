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

        public ObservableRangeCollection<ParatextProject> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProject>();


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
            ParatextVisible = await paratextUtils.IsParatextInstalledAsync().ConfigureAwait(true);

            if (ParatextVisible)
            {
                ParatextProjects.Clear();
                // get all the Paratext Projects (Projects/Backtranslations)
                List<ParatextProject> projects = paratextUtils.GetParatextProjects();
                try
                {
                    // TODO - why is the next line causing an error but actually still working?
                    ParatextProjects.AddRange(projects);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            // get all the Paratext Resources (LWC)
            // TODO




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
}
