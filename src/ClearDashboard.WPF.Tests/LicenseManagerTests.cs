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
            var directoryPath = Path.Combine(LicenseManager.LicenseFolderPath,"LicenseTest");
            var filePath = Path.Combine(directoryPath,LicenseManager.LicenseFileName);

            var originalLicenseUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Smith",
            };

            LicenseManager.EncryptToFile(originalLicenseUser, directoryPath);
            var decryptedJson = LicenseManager.DecryptLicenseFromFile(filePath);
            var decryptedLicenseUser = LicenseManager.DecryptedJsonToUser(decryptedJson);

            Assert.Equal(LicenseUserMatchType.Match,LicenseManager.CompareGivenUserAndDecryptedUser(originalLicenseUser, decryptedLicenseUser));

            await Task.CompletedTask;
        }

        [Fact]
        public async Task EncryptLicenseUserDecryptUserTest()
        {
            var directoryPath = Path.Combine(LicenseManager.LicenseFolderPath, "LicenseTest");
            var filePath = Path.Combine(directoryPath, LicenseManager.LicenseFileName);

            var originalLicenseUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Smith",
            };

            LicenseManager.EncryptToFile(originalLicenseUser, directoryPath);
            //var decryptedJson = LicenseManager.DecryptFromFile(filePath);
            //var decryptedLicenseUser = LicenseManager.DecryptedJsonToLicenseUser(decryptedJson);

            var user = LicenseManager.DecryptLicenseFromFileToUser(filePath);
            Assert.NotNull(user);

            Assert.Equal(user.Id, originalLicenseUser.Id);
            Assert.Equal(user.FirstName, originalLicenseUser.FirstName);
            Assert.Equal(user.LastName, originalLicenseUser.LastName);

            //Assert.True(LicenseManager.CompareGivenUserAndDecryptedUser(originalLicenseUser, decryptedLicenseUser));

            await Task.CompletedTask;
        }

        [Fact]
        public void DecryptFromString()
        {

            var json = LicenseManager.DecryptLicenseFromString(
                "KJPQAD+QnfioxDbZmFnw9VMkcZyWlMUFmoHUUO9YWzS+j0Ir0ZkXY58OXVvRq6Dji/ou+tuViioXpATAdM0RLNQPqjNUi8FPU7zPbhFbHEBbDTCvgpDMGpdBjcUJBOsBcYBtLq2l+YmtgrlT7HNsq2EDEb4sgrf3PGc/tUTXu2BI/l7uMYvyIObqs7NXTfNsC17KSS9b9H2GA1eJFD7vipZ7aeELfuZIi9B5HsLOFKUOAo8Q85y1WHIBSidLboca");
            var user = LicenseManager.DecryptedJsonToUser(json);
            Assert.NotNull(user);
            Assert.Equal("Michael Gerfen", user.FullName);
        }
    }
}
