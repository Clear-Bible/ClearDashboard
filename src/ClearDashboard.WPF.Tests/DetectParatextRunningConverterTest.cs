using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ClearDashboard.WPF;
using System.Diagnostics;

namespace ClearDashboard.WPF.Tests
{
    public class DetectParatextRunningConverterTest
    {
        public DetectParatextRunningConverterTest() {

        }

        [Fact]
        public void ParatextDetectionTest() 
        { 
        }

        public void IsParatextRunning()
        {
            if (Process.GetProcessesByName("Paratext").Length > 0)
            {
                // stuff to do while paratext is running 
                System.Console.WriteLine("yo");
            }
        }

    }
}
