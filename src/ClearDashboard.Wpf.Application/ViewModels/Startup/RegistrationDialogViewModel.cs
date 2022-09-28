using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup;

public class RegistrationDialogViewModel : WorkflowShellViewModel
{
    private IServiceProvider ServiceProvider { get; }

    ILogger _logger;
    //private IServiceProvider serviceProvider;
    private RegistrationViewModel _registrationViewModel;

    public string LicenseKey => _registrationViewModel.LicenseKey;
    public string FirstName => _registrationViewModel.FirstName;
    public string LastName => _registrationViewModel.LastName;
    public RegistrationDialogViewModel(

        INavigationService navigationService,
        ILogger<RegistrationDialogViewModel> logger,
        IEventAggregator eventAggregator,
        IMediator mediator,
        ILifetimeScope? lifetimeScope,
        IServiceProvider serviceProvider)
        : base(navigationService, logger, eventAggregator, mediator, lifetimeScope)
    {
        ServiceProvider = serviceProvider;
    }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);


        MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_Missing", _logger));

        _registrationViewModel = ServiceProvider.GetService<RegistrationViewModel>();

        Steps.Add(_registrationViewModel);

        var step2 = ServiceProvider.GetService<RegistrationViewModel>();
        Steps.Add((IWorkflowStepViewModel)step2);

        CurrentStep = Steps[0];

        IsLastWorkflowStep = (Steps.Count == 1);
        await ActivateItemAsync(Steps[0], cancellationToken);
    }

    public bool CanCancel => true /* can always cancel */;

    private bool _canRegister;
    public bool CanRegister
    {
        get => _canRegister;
        set => Set(ref _canRegister, value);
    }

    public void Cancel()
    {
        System.Windows.Application.Current.Shutdown();
    }

    //public async void Register()
    //{
    //    try
    //    {
    //        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    //        File.Delete(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"));

    //        var decryptedLicenseKey = LicenseManager.DecryptFromString(LicenseKey);
    //        var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedLicenseKey);

    //        LicenseUser givenLicenseUser = new LicenseUser();
    //        givenLicenseUser.FirstName = _registrationViewModel.FirstName;
    //        givenLicenseUser.LastName = _registrationViewModel.LastName;
    //        //givenLicenseUser.LicenseKey = _registrationViewModel.LicenseKey; <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid

    //        bool match = LicenseManager.CompareGivenUserAndDecryptedUser(givenLicenseUser, decryptedLicenseUser);
    //        if (match)
    //        {
    //            File.WriteAllText(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.txt"), LicenseKey);
    //            await TryCloseAsync(true);
    //        }
    //        else
    //        {
    //            MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_MismatchCatch", _logger));
    //        }
    //    }

    //    catch (Exception)
    //    {
    //        MessageBox.Show(LocalizationStrings.Get("RegistrationDialogViewModel_FaultyKey", _logger));
    //    }
    //}
}