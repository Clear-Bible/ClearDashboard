using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClearDashboard.Wpf.ViewModels
{
    public class ProcessUSFMViewModel : ApplicationScreen
    {
        #region   Member Variables

      
        public DashboardProject DashboardProject { get; set; }

        #endregion


        #region Observable Objects

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => FlowDirection);
            }
        }

        private ObservableCollection<AlignmentPlan> _alignmentPlan = new ();
        public ObservableCollection<AlignmentPlan> AlignmentPlan
        {
            get => _alignmentPlan;
            set
            {
                _alignmentPlan = value;
                NotifyOfPropertyChange(() => AlignmentPlan);
            }
        }


        #endregion


        #region Constructor
        public ProcessUSFMViewModel()
        {
            
        }

        public ProcessUSFMViewModel(ProjectManager projectManager, INavigationService navigationService, 
            ILogger<LandingViewModel> logger) : base(navigationService, logger, projectManager)
        {
            FlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var target = DashboardProject.TargetProject.Name;
            var targetId = DashboardProject.TargetProject.Guid;

            AlignmentPlan.Clear();
            _alignmentPlan.Add(new AlignmentPlan
            {
                Source = "Manuscript",
                SourceID = "",
                Target = target,
                TargetID = targetId
            });

            foreach (var proj in DashboardProject.BackTranslationProjects)
            {
                _alignmentPlan.Add(new AlignmentPlan
                {
                    Source = proj.Name,
                    SourceID = proj.Guid,
                    Target = target,
                    TargetID = targetId
                });
            }

            foreach (var proj in DashboardProject.LanguageOfWiderCommunicationProjects)
            {
                _alignmentPlan.Add(new AlignmentPlan
                {
                    Source = proj.Name,
                    SourceID = proj.Guid,
                    Target = target,
                    TargetID = targetId
                });
            }

            if (DashboardProject.InterlinearizerProject is not null)
            {
                _alignmentPlan.Add(new AlignmentPlan
                {
                    Source = DashboardProject.InterlinearizerProject.Name,
                    SourceID = DashboardProject.InterlinearizerProject.Guid,
                    Target = target,
                    TargetID = targetId
                });
            }

            NotifyOfPropertyChange(() => AlignmentPlan);
            
            return base.OnActivateAsync(cancellationToken);
        }

        #endregion



        #region Methods


        #endregion

    }
}
