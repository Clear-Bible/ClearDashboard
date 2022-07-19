using System;
using System.Drawing.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
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
        await TryCloseAsync(false);
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
        File.WriteAllText(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.key"), LicenseKey);

        //decrypt code
        var decryptedLicenseKey = await DecryptFromFile(Path.Combine(documentsPath, "ClearDashboard_Projects", "license.key"));//fix this
        decryptedLicenseKey = "{\"FirstName\":\"Bob\",\"LastName\":\"Smith\",\"LicenseKey\":\"61809dd9-fdfe-4f25-bc64-a6a9e2f5138d\",\"FullName\":\"Bob Smith\",\"ParatextUserName\":null,\"LastAlignmentLevelId\":null,\"AlignmentVersions\":[],\"AlignmentSets\":[],\"Id\":\"1a0f98d3-5661-4256-bc99-357a8f8290e3\"}";

        //validate contents (not null or empty)
        var jsonLicense = JObject.Parse(decryptedLicenseKey);
        LicenseUser licenseUser = new LicenseUser();
        try
        {
            licenseUser.FirstName = jsonLicense.GetValue("FirstName").ToString();
            licenseUser.LastName = jsonLicense.GetValue("LastName").ToString();
            licenseUser.ParatextUserName = jsonLicense.GetValue("ParatextUserName").ToString();
            licenseUser.LicenseKey = jsonLicense.GetValue("LicenseKey").ToString();
        }
        catch (Exception ex)
        {
            //complain that the Key is bad
        }

        //load projects
    }

    private async Task<string> DecryptFromFile(string path)
    {
        try
        {
            using (FileStream fileStream = new(path, FileMode.Open))
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.IV.Length];
                    int numBytesToRead = aes.IV.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                        if (n == 0) break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }

                    byte[] key =
                    {
                            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                        };

                    using (CryptoStream cryptoStream = new(
                               fileStream,
                               aes.CreateDecryptor(key, iv),
                               CryptoStreamMode.Read))
                    {
                        using (StreamReader decryptReader = new(cryptoStream))
                        {
                            string decryptedMessage = await decryptReader.ReadToEndAsync();
                            //_output.WriteLine($"The decrypted original message: {decryptedMessage}");
                            return decryptedMessage;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //_output.WriteLine($"The decryption failed. {ex}");
            return "";
        }
    }

    private async Task EncryptToFile()
    {
        try
        {

            var user = new User
            {
                Id = Guid.NewGuid(),
                LicenseKey = Guid.NewGuid().ToString(),
                FirstName = "Bob",
                LastName = "Smith"
            };

            using (FileStream fileStream = new("TestData.txt", FileMode.OpenOrCreate))
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] key =
                    {
                            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                        };
                    aes.Key = key;

                    byte[] iv = aes.IV;
                    fileStream.Write(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new(
                               fileStream,
                               aes.CreateEncryptor(),
                               CryptoStreamMode.Write))
                    {
                        using (StreamWriter encryptWriter = new(cryptoStream))
                        {
                            //encryptWriter.WriteLine($"LicenseKey: {Guid.NewGuid()}");
                            //encryptWriter.WriteLine($"UserId: {Guid.NewGuid()}");
                            //encryptWriter.WriteLine($"FirstName: Bob");
                            //encryptWriter.WriteLine($"LastName: Smith");

                            encryptWriter.WriteLine(JsonSerializer.Serialize<User>(user));
                        }
                    }
                }
            }

            //_output.WriteLine("The file was encrypted.");
        }
        catch (Exception ex)
        {
            //_output.WriteLine($"The encryption failed. {ex}");
        }
    }
}