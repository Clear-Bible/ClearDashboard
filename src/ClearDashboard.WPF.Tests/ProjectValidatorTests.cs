using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using ClearApplicationFoundation.Services;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public  class ProjectValidatorTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly ILogger<ProjectValidator>? _logger;
        private readonly ILocalizationService? _localizationService;

        public ProjectValidatorTests(ITestOutputHelper output) : base(output)
        {
            _output = output;

            _logger = ServiceProvider.GetService<ILogger<ProjectValidator>>();
            _localizationService = ServiceProvider.GetService<ILocalizationService>();
        }

        [Fact]
        public void ValidateProjectDoesNotExist()
        {
            var project = new Project
            {
                ProjectName = Guid.NewGuid().ToString(),
            };
            var projectValidator = new ProjectValidator(_logger!,_localizationService);

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

                var projectValidator = new ProjectValidator(_logger!,_localizationService);
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

            var projectValidator = new ProjectValidator(_logger!,_localizationService);
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
