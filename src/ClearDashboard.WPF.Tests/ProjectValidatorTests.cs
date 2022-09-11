using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.Wpf.Application.Validators;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.WPF.Tests
{
    public  class ProjectValidatorTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<ProjectValidator>? _logger;

        public ProjectValidatorTests(ITestOutputHelper output) : base(output)
        {
            _output = output;

            _logger = ServiceProvider.GetService<ILogger<ProjectValidator>>();
        }

        [Fact]
        public void ValidateProjectDoesNotExist()
        {
            var project = new Project
            {
                ProjectName = Guid.NewGuid().ToString(),
            };
            var projectValidator = new ProjectValidator(_logger);

            var results = projectValidator.Validate(project);

            Assert.True(results.IsValid);
            Assert.Empty(results.Errors);
        }

        [Fact]
        public void ValidateProjectDirectoryAlreadyExists()
        {
            var projectName = Guid.NewGuid().ToString();
            var projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects", projectName);

            try
            {
                var dir = Directory.CreateDirectory(projectDirectory);

                var project = new Project
                {
                    ProjectName = projectName,
                };

                var projectValidator = new ProjectValidator(_logger);
                var results = projectValidator.Validate(project);

                Assert.False(results.IsValid);
                Assert.NotEmpty(results.Errors);

                Assert.Collection(results.Errors, error => Assert.Equal($"A project with the same name already exists. Please choose a unique name.", error.ErrorMessage));

                _output.WriteLine("The following validation errors occurred:");

                foreach (var error in results.Errors)
                {
                    _output.WriteLine(error.ErrorMessage);
                }
            }
            finally
            {
                Directory.Delete(projectDirectory, true);
            }

          
        }


        [Fact]
        public void ProjectNameNotValid()
        {
            var project = new Project
            {
                ProjectName = "!BANG"
            };

            var projectValidator = new ProjectValidator(_logger);
            var results = projectValidator.Validate(project);

            Assert.False(results.IsValid);
            Assert.NotEmpty(results.Errors);
            Assert.Collection(results.Errors, error=> Assert.Equal($"The given project name contains illegal characters.  Valid characters include 'A-Z' (lowercase and uppercase), numbers '0-9' and the characters '-' and '_'.",error.ErrorMessage));

            _output.WriteLine("The following validation errors occurred:");

            foreach (var error in results.Errors)
            {
                _output.WriteLine(error.ErrorMessage);
            }
        }
    }
}
