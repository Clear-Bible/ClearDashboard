using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using FluentValidation.Results;
using Validators;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public  class ProjectValidatorTests
    {
        private readonly ITestOutputHelper _output;

        public ProjectValidatorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ValidateProjectDoesNotExist()
        {
            var project = new Project
            {
                ProjectName = Guid.NewGuid().ToString(),
            };
            var projectValidator = new ProjectValidator();

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

                var projectValidator = new ProjectValidator();
                var results = projectValidator.Validate(project);

                Assert.False(results.IsValid);
                Assert.NotEmpty(results.Errors);

                Assert.Collection(results.Errors, error => Assert.Equal($"The project directory {projectDirectory} already exists.", error.ErrorMessage));

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

            var projectValidator = new ProjectValidator();
            var results = projectValidator.Validate(project);

            Assert.False(results.IsValid);
            Assert.NotEmpty(results.Errors);
            Assert.Collection(results.Errors, error=> Assert.Equal($"The project name '{project.ProjectName}' contains illegal characters.  Valid characters include 'A-Z' (lowercase and uppercase), numbers '0-9' and the characters '-' and '_'.",error.ErrorMessage));

            _output.WriteLine("The following validation errors occurred:");

            foreach (var error in results.Errors)
            {
                _output.WriteLine(error.ErrorMessage);
            }
        }
    }
}
