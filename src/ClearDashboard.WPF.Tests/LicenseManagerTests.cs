using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class LicenseManagerTests
    {
        private readonly ITestOutputHelper _output;

        public LicenseManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task EncryptDecryptTest()
        {
            var directoryPath = "LicenseTest";
            var filePath = "LicenseTest\\license.txt";

            var originalLicenseUser = new LicenseUser
            {
                Id = Guid.NewGuid().ToString("N"),
                LicenseKey = Guid.NewGuid().ToString("N"),
                FirstName = "Bob",
                LastName = "Smith",
            };

            LicenseManager.EncryptToDirectory(originalLicenseUser, directoryPath);
            var decryptedJson = LicenseManager.DecryptFromFile(filePath);
            var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedJson);

            Assert.Equal(true, LicenseManager.CompareGivenUserAndDecryptedUser(originalLicenseUser, decryptedLicenseUser));
        }
    }
}
