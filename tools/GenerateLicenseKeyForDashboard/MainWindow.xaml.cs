using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;

namespace GenerateLicenseKeyForDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _licenseVersion = 1;

        public MainWindow()
        {
            InitializeComponent();

            //get the assembly version
            var thisVersion = Assembly.GetEntryAssembly().GetName().Version;
            TheWindow.Title +=  $" - {thisVersion.Major}.{thisVersion.Minor}.{thisVersion.Build}.{thisVersion.Revision}";
        }

        

        private void GenerateLicenseKey_OnClick(object sender, RoutedEventArgs e)
        {
            var licenseKey = GenerateLicense(FirstNameBox.Text, LastNameBox.Text, Guid.NewGuid(), Guid.NewGuid());
            GeneratedLicenseKeyBox.Text = licenseKey;
        }

        private string GenerateLicense(string firstName, string lastName, Guid id, Guid licenseKey)
        {
            var folderPath = Path.Combine(LicenseManager.LicenseFolderPath, $"{firstName + lastName}");

            var licenseUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                LicenseKey = licenseKey.ToString("N"),
                IsInternal = IsInternalCheckBox.IsChecked,
                LicenseVersion = _licenseVersion
            };

            LicenseManager.EncryptToFile(licenseUser, folderPath);
            var encryptedLicenseKey = LicenseManager.EncryptToString(licenseUser, folderPath);

            return encryptedLicenseKey;
        }

        private void DecryptLicenseKey_OnClick(object sender, RoutedEventArgs e)
        {
            var json = LicenseManager.DecryptLicenseFromString(LicenseKeyDecryptionBox.Text);
            var licenseUser = LicenseManager.DecryptedJsonToUser(json, isGenerator:true);

            DecryptedFirstNameBox.Text = licenseUser.FirstName;
            DecryptedLastNameBox.Text = licenseUser.LastName;
            DecryptedGuidBox.Text = licenseUser.Id.ToString();
            DecryptedInternalCheckBox.IsChecked = licenseUser.IsInternal;
        }

        private void Copy_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            switch (button.Content)
            {
                case "Copy license key to clipboard.":
                    Clipboard.SetText(GeneratedLicenseKeyBox.Text);
                    break;
                case "Copy first name to clipboard.":
                    Clipboard.SetText(DecryptedFirstNameBox.Text);
                    break;
                case "Copy last name to clipboard.":
                    Clipboard.SetText(DecryptedLastNameBox.Text);
                    break;
                case "Copy guid to clipboard.":
                    Clipboard.SetText(DecryptedGuidBox.Text);
                    break;
            }
        }
    }
}
