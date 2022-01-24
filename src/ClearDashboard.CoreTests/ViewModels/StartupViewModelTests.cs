using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClearDashboard.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Core.ViewModels.Tests
{
    [TestClass()]
    public class StartupViewModelTests
    {
        [TestMethod()]
        public void StartupViewModelTest()
        {
            // To test the CI/CD test runner.
            Assert.AreEqual(1,1);
        }
    }
}

namespace ClearDashboard.CoreTests.ViewModels
{
    internal class StartupViewModelTests
    {
    }
}
