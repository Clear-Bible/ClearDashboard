using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using ClearDashboard.DAL.Paratext;
using MvvmHelpers;

namespace ClearDashboard.Wpf.ViewModels
{
    public class CreateNewProjectsViewModel : ObservableObject
    {
        #region props
        public bool ParatextVisible = false;
        public bool ShowWaitingIcon = true;

        public ObservableRangeCollection<ParatextProject> ParatextProjects { get; set; } =
            new ObservableRangeCollection<ParatextProject>();


        #endregion

        public CreateNewProjectsViewModel()
        {
            // get the paratext project
        }

        public async Task Init()
        {
            // detect if Paratext is installed
            ParatextUtils paratextUtils = new ParatextUtils();
            ParatextVisible = await paratextUtils.IsParatextInstalledAsync().ConfigureAwait(true);

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                List<ParatextProject> projects = paratextUtils.GetParatextProjects();
                ParatextProjects.AddRange(projects);
            }



            // get all the Paratext Resources (LWC)


        }
    }
}
