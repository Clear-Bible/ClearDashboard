using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using PowerManagerAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PowerModes = ClearDashboard.Wpf.Application.Models.PowerModes;

namespace ClearDashboard.Wpf.Application.Services
{
    public class SystemPowerModes
    {
        private readonly IEventAggregator? _eventAggregator;
        //private readonly Guid _powerSaverPlan = Guid.Parse("a1841308-3541-4fab-bc81-f71556f20b4a");
        //private readonly Guid _balancedPlan = Guid.Parse("381b4222-f694-41f0-9685-ff5bb260df2e");
        private readonly Guid _highPerformancePlan = Guid.Parse("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

        private Guid _activePlanGuid;
        private List<PowerModes> _powerModes = new();

        public bool IsLaptop => CheckIfLaptop();

        public bool IsHighPerformanceEnabled = false;

        public SystemPowerModes()
        {
            _eventAggregator = IoC.Get<IEventAggregator>(); 
        }

        /// <summary>
        /// Grabs the system power state to determine if batteries are
        /// present or not.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfLaptop()
        {
            PowerState state = PowerState.GetPowerState();
            if (state.BatteryFlag == BatteryFlag.NoSystemBattery)
            {
                return false;
            }

            return true;
        }

        public async Task TurnOnHighPerformanceMode()
        {
            if (IsHighPerformanceEnabled)
            {
                return;
            }


            // get the active plan
            _activePlanGuid = PowerManager.GetActivePlan();


            if (_activePlanGuid == _highPerformancePlan ||
                PowerManager.GetPlanName(_activePlanGuid) == "Clear High Performance")
            {
                // we are already running the high performance plan
                return;
            }

            // grab the list of what power plans are available
            var list = PowerManager.ListPlans();

            foreach (var plan in list)
            {
                var name = PowerManager.GetPlanName(plan);

                var mode = _powerModes.FirstOrDefault(x => x.PowerModeGuid == plan);
                if (mode is null)
                {
                    PowerModes powerModes = new PowerModes
                    {
                        IsPresent = true,
                        Name = name,
                        PowerModeGuid = plan
                    };

                    if (plan == _activePlanGuid)
                    {
                        powerModes.IsActive = true;
                    }

                    _powerModes.Add(powerModes);

                }
                else
                {
                    mode.IsPresent = true;

                    if (mode.PowerModeGuid == _activePlanGuid)
                    {
                        mode.IsActive = true;
                    }

                }

            }

            var highPerformancePlan = _powerModes.FirstOrDefault(x => x.PowerModeGuid == _highPerformancePlan || x.Name == "Clear High Performance");

            // check to see if the high performance plan exists or not
            if (highPerformancePlan is null)
            {
                try
                {
                    // create a new plane based off the high performance plan
                    var res = PowerManager.DuplicatePlan(_highPerformancePlan);
                    PowerManager.SetPlanName(res, "Clear High Performance");

                    // set as the active plan
                    PowerManager.SetActivePlan(res);

                }
                catch (Exception)
                {
                    PowerManager.SetActivePlan(_activePlanGuid);
                    IsHighPerformanceEnabled = false;
                    await _eventAggregator.PublishOnUIThreadAsync(new PerformanceModeMessage(false));
                    return;
                }
            }
            else
            {
                // set as the active plan
                PowerManager.SetActivePlan(highPerformancePlan.PowerModeGuid);
            }
            IsHighPerformanceEnabled = true;
            await _eventAggregator.PublishOnUIThreadAsync(new PerformanceModeMessage(true));
        }

        public async Task TurnOffHighPerformanceMode()
        {
            // set as the active plan
            PowerManager.SetActivePlan(_activePlanGuid);
            IsHighPerformanceEnabled = false;

            await _eventAggregator.PublishOnUIThreadAsync(new PerformanceModeMessage(false));

        }

    }


}
