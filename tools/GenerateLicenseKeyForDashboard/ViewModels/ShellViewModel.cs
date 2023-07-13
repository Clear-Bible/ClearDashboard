using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Extensions;
using ClearDashboard.Wpf.Application.Services;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer.Models.Common;

namespace GenerateLicenseKeyForDashboard.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        #region Member Variables   

        private int _licenseVersion = 2;
        private readonly CollaborationHttpClientServices _mySqlHttpClientServices;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }


        private bool _emailChecked;
        public bool EmailChecked
        {
            get => _emailChecked;
            set => Set(ref _emailChecked, value);
        }


        private bool _idChecked;
        public bool IdChecked
        {
            get => _idChecked;
            set => Set(ref _idChecked, value);
        }


        private bool _isInternalChecked;
        public bool IsInternalChecked
        {
            get => _isInternalChecked;
            set => Set(ref _isInternalChecked, value);
        }

        private string _emailBox;
        public string EmailBox
        {
            get => _emailBox;
            set => Set(ref _emailBox, value);
        }


        private string _generatedLicenseBoxText;
        public string GeneratedLicenseBoxText
        {
            get => _generatedLicenseBoxText;
            set => Set(ref _generatedLicenseBoxText, value);
        }



        private string _firstNameBox;
        public string FirstNameBox
        {
            get => _firstNameBox;
            set => Set(ref _firstNameBox, value);
        }

        private string _lastNameBox;
        public string LastNameBox
        {
            get => _lastNameBox;
            set => Set(ref _lastNameBox, value);
        }

        #endregion //Observable Properties


        #region Constructor

        public ShellViewModel()
        {
            _mySqlHttpClientServices = ServiceCollectionExtensions.GetSqlHttpClientServices();
            

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            Title = $"License Key Manager - {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
            EmailChecked = true;
        }

        #endregion //Constructor


        #region Methods


        private async void GenerateLicense_OnClick(object sender, RoutedEventArgs e)
        {
            var emailAlreadyExists = await CheckForPreExistingEmail(EmailBox);

            if (emailAlreadyExists)
            {
                GeneratedLicenseBoxText = "Email already exists";
            }
            else
            {
                var licenseKey = await GenerateLicense(FirstNameBox, LastNameBox, Guid.NewGuid(), EmailBox);
                GeneratedLicenseBoxText = licenseKey;
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

        #endregion // Methods


    }
}
