using System;
using System.Drawing.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.ViewModels.Workflows;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using LicenseUser = ClearDashboard.DataAccessLayer.Models.LicenseUser;

namespace ClearDashboard.Wpf.ViewModels.Popups;

public class RegistrationDialogViewModel : WorkflowShellViewModel
{
    ILogger _logger;
    private RegistrationViewModel _registrationViewModel;

    public string LicenseKey => _registrationViewModel.LicenseKey;
    public string FirstName => _registrationViewModel.FirstName;
    public string LastName => _registrationViewModel.LastName;
    public RegistrationDialogViewModel(DashboardProjectManager projectManager, IServiceProvider serviceProvider, ILogger<WorkflowShellViewModel> logger, INavigationService navigationService, IEventAggregator eventAggregator)
        : base(projectManager, serviceProvider, logger, navigationService, eventAggregator)
    {
    }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);

        
        MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_Missing", _logger));

        _registrationViewModel = ServiceProvider.GetService<RegistrationViewModel>();


        Steps.Add(_registrationViewModel);

        var step2 = ServiceProvider.GetService<RegistrationViewModel>();
        Steps.Add(step2);

        CurrentStep = Steps[0];

        IsLastWorkflowStep = (Steps.Count == 1);
        await ActivateItemAsync(Steps[0], cancellationToken);
    }

    public bool CanCancel => true /* can always cancel */;

    public void Cancel()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private bool _canRegister;
    public bool CanRegister
    {
        get => _canRegister;
        set => Set(ref _canRegister, value);
    }

    public async void Register()
    {
        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            File.Delete(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"));

            var decryptedLicenseKey = LicenseManager.DecryptFromString(LicenseKey);
            var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedLicenseKey);
            
            LicenseUser givenLicenseUser = new LicenseUser();
            givenLicenseUser.FirstName = _registrationViewModel.FirstName;
            givenLicenseUser.LastName = _registrationViewModel.LastName;
            //givenLicenseUser.LicenseKey = _registrationViewModel.LicenseKey; <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid

            bool match = LicenseManager.CompareGivenUserAndDecryptedUser(givenLicenseUser, decryptedLicenseUser);
            if (match)
            {
                File.WriteAllText(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"), LicenseKey);
                await TryCloseAsync(true);
            }
            else
            {
                MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_MismatchCatch", _logger));
            }
        }

        catch (Exception)
        {
            MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_FaultyKey", _logger));
        }
    }
}