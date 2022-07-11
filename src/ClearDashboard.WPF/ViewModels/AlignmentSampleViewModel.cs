using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ClearDashboard.Wpf.Models;

namespace ClearDashboard.Wpf.ViewModels
{
    public class AlignmentSampleViewModel : ApplicationScreen
    {
        public List<string> Words { get; set; } = new() { "foo", "bar" };

        public List<string> GreekChars { get; set; } = "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ σ τ υ φ χ ψ ω".Split(' ').ToList();

        public List<string> HebrewPsalm { get; set; } = "כִּֽי־אַ֭תָּה תָּאִ֣יר נֵרִ֑י יְהוָ֥ה אֱ֝לֹהַ֗י יַגִּ֥יהַּ חָשְׁכִּֽי׃".Split(' ').ToList();
        public List<string> GreekPsalm { get; set; } = "χι αθθα θαειρ νηρι YHWH ελωαι αγι οσχι".Split(' ').ToList();

        public AlignmentSampleViewModel()
        {
            var a = "כִּֽי־אַ֭תָּה תָּאִ֣יר נֵרִ֑י יְהוָ֥ה אֱ֝לֹהַ֗י יַגִּ֥יהַּ חָשְׁכִּֽי׃";
        }

        public AlignmentSampleViewModel(INavigationService navigationService, 
            ILogger<SettingsViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            var greekChars = "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ σ τ υ φ χ ψ ω".Split(' ');
        }

    }
}
