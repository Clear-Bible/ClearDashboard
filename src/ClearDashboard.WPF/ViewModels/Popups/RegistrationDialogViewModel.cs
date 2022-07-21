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
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Models;
using ClearDashboard.Wpf.ViewModels.Workflows;
using ClearDashboard.Wpf.ViewModels.Workflows.CreateNewProject;
using Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using LicenseUser = ClearDashboard.DataAccessLayer.Models.LicenseUser;

namespace ClearDashboard.Wpf.ViewModels.Popups;

public class RegistrationDialogViewModel : WorkflowShellViewModel
{
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

        _registrationViewModel = ServiceProvider.GetService<RegistrationViewModel>();


        Steps.Add(_registrationViewModel);

        var step2 = ServiceProvider.GetService<RegistrationViewModel>();
        Steps.Add(step2);

        CurrentStep = Steps[0];

        IsLastWorkflowStep = (Steps.Count == 1);
        await ActivateItemAsync(Steps[0], cancellationToken);
    }

    public bool CanCancel => true /* can always cancel */;

    public async void Cancel()
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
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        File.Delete(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"));
        File.WriteAllText(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"), LicenseKey);

        //decrypt code
        var decryptedLicenseKey = LicenseCryption.DecryptFromFile(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"));//fix this
        //decryptedLicenseKey = "{\"FirstName\":\"Bob\",\"LastName\":\"Smith\",\"LicenseKey\":\"61809dd9-fdfe-4f25-bc64-a6a9e2f5138d\",\"FullName\":\"Bob Smith\",\"ParatextUserName\":null,\"LastAlignmentLevelId\":null,\"AlignmentVersions\":[],\"AlignmentSets\":[],\"Id\":\"1a0f98d3-5661-4256-bc99-357a8f8290e3\"}";

        //validate contents (not null or empty)
        try
        {
            var decryptedLicenseUser = LicenseCryption.DecryptedJsonToLicenseUser(decryptedLicenseKey);

            LicenseUser givenLicenseUser = new LicenseUser();
            givenLicenseUser.FirstName = _registrationViewModel.FirstName;
            givenLicenseUser.LastName = _registrationViewModel.LastName;
            //givenLicenseUser.ParatextUserName = _registrationViewModel.LicenseKey;
            //givenLicenseUser.LicenseKey = _registrationViewModel.FirstName;

            bool match = CompareGivenUserAndDecryptedUser(givenLicenseUser, decryptedLicenseUser);
            if (match)
            {
                await TryCloseAsync(true);
                //load projects
            }
            else
            {
                MessageBox.Show("The contents of the decrypted license key do not match the information you provided.");
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show("The key provided is faulty.  When decrypted, certain Json elements could not be found.");
        }
    }

    public bool CompareGivenUserAndDecryptedUser(LicenseUser given, LicenseUser decrypted)
    {
        if (given.FirstName == decrypted.FirstName && 
            given.LastName == decrypted.LastName && 
            given.ParatextUserName == decrypted.ParatextUserName && 
            given.LicenseKey == decrypted.LicenseKey)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}