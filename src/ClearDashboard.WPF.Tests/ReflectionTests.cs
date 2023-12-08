using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Lexicon;
using Xunit;

namespace ClearDashboard.WPF.Tests
{
    public  class ReflectionTests
    {

        [Fact]
        public void SetInternalPropertyTest()
        {
           var meaning = new Meaning();
           Assert.False(meaning.IsDirty);
           meaning.SetInternalProperty(nameof(Meaning.IsDirty), true);
           Assert.True( meaning.IsDirty);
        }
    }
}
