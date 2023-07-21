using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace GenerateLicenseKeyForDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _licenseVersion = 2;
        private readonly CollaborationHttpClientServices _mySqlHttpClientServices;

        public MainWindow()
        {
            _mySqlHttpClientServices = ServiceCollectionExtensions.GetSqlHttpClientServices();
            InitializeComponent();

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            TheWindow.Title += $" - {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            ByEmailRadio.IsChecked = true;
        }



        private async void GenerateLicense_OnClick(object sender, RoutedEventArgs e)
        {
            var emailAlreadyExists = await CheckForPreExistingEmail(EmailBox.Text);

            if (emailAlreadyExists)
            {
                GeneratedLicenseBox.Text = "Email already exists";
            }
            else
            {
                var licenseKey = await GenerateLicense(FirstNameBox.Text, LastNameBox.Text, Guid.NewGuid(), EmailBox.Text);
                GeneratedLicenseBox.Text = licenseKey;
            }
        }

        private async Task<bool> CheckForPreExistingEmail(string email)
        {
            var results = await _mySqlHttpClientServices.GetAllDashboardUsers();

            var dashboardUser = results.FirstOrDefault(du => du.Email == email);

            if (dashboardUser is null)
            {
                return false;
            }

            return true;
        }

        private async Task<string> GenerateLicense(string firstName, string lastName, Guid id, string email)
        {
            var licenseUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                IsInternal = IsInternalCheckBox.IsChecked,
                LicenseVersion = _licenseVersion
            };

            var encryptedLicense = LicenseManager.EncryptToString(licenseUser);

            var dashboardUser = new DashboardUser(licenseUser, email, encryptedLicense);

            var results = await _mySqlHttpClientServices.CreateNewDashboardUser(dashboardUser);

            if (!results)
            {
                encryptedLicense = "User failed to be added to the remote server";
            }

            return encryptedLicense;
        }

        private void DecryptLicense_OnClick(object sender, RoutedEventArgs e)
        {
            var json = LicenseManager.DecryptLicenseFromString(LicenseDecryptionBox.Text, isGenerator: true);
            var licenseUser = LicenseManager.DecryptedJsonToUser(json, isGenerator: true);

            DecryptedFirstNameBox.Text = licenseUser.FirstName;
            DecryptedLastNameBox.Text = licenseUser.LastName;
            DecryptedGuidBox.Text = licenseUser.Id.ToString();
            DecryptedInternalCheckBox.IsChecked = licenseUser.IsInternal;
            DecryptedLicenseVersionBox.Text = licenseUser.LicenseVersion.ToString();
        }

        private void ByIdRadio_OnCheck(object sender, RoutedEventArgs e)
        {
            FetchByEmailInput.Visibility = Visibility.Collapsed;
            FetchByIdInput.Visibility = Visibility.Visible;
        }

        private void ByEmailRadio_OnCheck(object sender, RoutedEventArgs e)
        {
            FetchByEmailInput.Visibility = Visibility.Visible;
            FetchByIdInput.Visibility = Visibility.Collapsed;
        }

        private async void FetchLicenseById_OnClick(object sender, RoutedEventArgs e)
        {
            var fetchByEmail = FetchByEmailInput.IsVisible;

            DashboardUser dashboardUser;
            if (fetchByEmail)
            {
                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsByEmail(FetchByEmailBox.Text);
            }
            else
            {
                Guid.TryParse(FetchByIdBox.Text, out var guid);
                dashboardUser = await _mySqlHttpClientServices.GetDashboardUserExistsById(guid);
            }

            FetchedEmailBox.Text = dashboardUser.Email;
            FetchedLicenseBox.Text = dashboardUser.LicenseKey;
        }

        private async void DeleteLicenseById_OnClick(object sender, RoutedEventArgs e)
        {

            var deleted = await _mySqlHttpClientServices.DeleteDashboardUserExistsById(Guid.Parse(DeleteByIdBox.Text));

            if (deleted)
            {
                DeletedLicenseBox.Text = DeleteByIdBox.Text;
            }
            else
            {
                DeletedLicenseBox.Text = "Delete failed.";
            }
        }

        private void Copy_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Name)
            {
                case "CopyGeneratedLicense":
                    Clipboard.SetText(GeneratedLicenseBox.Text);
                    break;
                case "CopyDecryptedFirstName":
                    Clipboard.SetText(DecryptedFirstNameBox.Text);
                    break;
                case "CopyDecryptedLastName":
                    Clipboard.SetText(DecryptedLastNameBox.Text);
                    break;
                case "CopyDecryptedGuid":
                    Clipboard.SetText(DecryptedGuidBox.Text);
                    break;
                case "CopyFetchedEmail":
                    Clipboard.SetText(FetchedEmailBox.Text);
                    break;
                case "CopyFetchedLicense":
                    Clipboard.SetText(FetchedLicenseBox.Text);
                    break;
                case "CopyDeletedLicense":
                    Clipboard.SetText(DeletedLicenseBox.Text);
                    break;
            }
        }

        private void CheckGenerateLicenseBoxes(object sender, TextChangedEventArgs e)
        {

            if (!Validation.GetHasError(FirstNameBox) &&
                !Validation.GetHasError(LastNameBox) &&
                !Validation.GetHasError(EmailBox) &&
                FirstNameBox.Text.Length > 0 &&
                LastNameBox.Text.Length > 0 &&
                EmailBox.Text.Length > 0)
            {
                GenerateLicenseButton.IsEnabled = true;
            }
            else
            {
                GenerateLicenseButton.IsEnabled = false;
            }
        }
    }
}
