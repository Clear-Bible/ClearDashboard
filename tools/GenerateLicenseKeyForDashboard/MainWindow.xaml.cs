using System;
using System.IO;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateLicenseKey_OnClick(object sender, RoutedEventArgs e)
        {
            var licenseKey = GenerateLicense(FirstNameBox.Text, LastNameBox.Text, Guid.NewGuid(), Guid.NewGuid());
            GeneratedLicenseKeyBox.Text = licenseKey;
        }

        private string GenerateLicense(string firstName, string lastName, Guid id, Guid licenseKey)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, $"ClearDashboard_Projects\\{firstName + lastName}");

            var licenseUser = new LicenseUser
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                LicenseKey = licenseKey.ToString("N"),
            };

            LicenseManager.EncryptToFile(licenseUser, filePath);
            var encryptedLicenseKey = LicenseManager.EncryptToString(licenseUser, filePath);

            return encryptedLicenseKey;
        }

        private void DecryptLicenseKey_OnClick(object sender, RoutedEventArgs e)
        {
            var json = LicenseManager.DecryptFromString(LicenseKeyDecryptionBox.Text);
            var licenseUser = LicenseManager.DecryptedJsonToLicenseUser(json);

            DecryptedFirstNameBox.Text = licenseUser.FirstName;
            DecryptedLastNameBox.Text = licenseUser.LastName;
            DecryptedGuidBox.Text = licenseUser.Id.ToString();
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
