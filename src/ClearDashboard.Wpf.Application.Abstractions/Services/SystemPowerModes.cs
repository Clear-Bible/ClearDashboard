using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public class SystemPowerModes
    {
        public bool IsLaptop => CheckIfLaptop();

        private bool CheckIfLaptop()
        {
            return false;
        }

        public async Task TurnOnHighPerformanceMode()
        {

        }

        public async Task TurnOffHighPerformanceMode()
        {

        }

    }
}
