using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;

namespace ClearDashboard.WPF.Tests
{
    public class GenerateUserLicensesTest
    {
        [Fact]
        public void GenerateLicenses()
        {
            GenerateLicense("Steve", "Bissell", Guid.Parse("156F9C2C-64D8-467A-9866-89A89611CA1A"),
                Guid.Parse("71D86C8A-83B8-40BE-A469-7ED288B6CCDD"));
            GenerateLicense("Robertson", "Brinker", Guid.Parse("FBDC7811-EC17-4E66-8D3F-2809B2FD90B3"),
                Guid.Parse("FFF8B11E-66B6-4777-AC6A-E2F08BFEF8F9"));
            GenerateLicense("Michael", "Brinker", Guid.Parse("19C4A670-3507-46A9-8BA3-24F5DB2D9AAC"),
                Guid.Parse("F91282CD-F8E8-4072-B2F2-A97AD2E426AF"));
            GenerateLicense("Michael", "Gerfen", Guid.Parse("0EB01F42-3085-4D07-8292-2706B6105CC6"),
                Guid.Parse("BC80117C-BB7B-41B8-B8B1-B6E5A71459EE"));
            GenerateLicense("Andy", "Gray", Guid.Parse("9D65A7B3-BB63-43F4-B6A3-C1351586EB29"),
                Guid.Parse("82A6D073-BC8C-4A7F-8E33-256BCE61AB18"));
            GenerateLicense("Dirk", "Kaiser", Guid.Parse("928E9B93-E64A-4787-B015-E52FDB11FD4B"),
                Guid.Parse("9D87E18A-449C-4764-8A82-D303C472E268"));
            GenerateLicense("Russell", "Morley", Guid.Parse("2105FBD0-DC74-4277-A7C2-2E5C54C6CA99"),
                Guid.Parse("549B2079-98F2-4C44-80CA-EC5B4E9ED72B"));
            GenerateLicense("Chris", "Morley", Guid.Parse("BAED36ED-9B3D-4A05-8752-DF7ADB2A5567"),
                Guid.Parse("E5CB7858-C056-4C1F-AE19-E7157CD443D2"));
            GenerateLicense("Nifer", "Sims", Guid.Parse("E1EF0079-267B-4601-A467-03EB0E05BA80"),
                Guid.Parse("6583777B-4C96-4D43-B429-0C4B13BFCF32"));
            GenerateLicense("Derek", "Wolfe", Guid.Parse("EB2C0147-5FB2-4570-B058-5D7E4D45C8CB"),
                Guid.Parse("8714E2BD-F44C-4C7E-B4AB-FD180912F741"));
        }

        private void GenerateLicense(string firstName, string lastName, Guid id, Guid licenseKey)
        {
            var folderPath = Path.Combine(LicenseManager.LicenseFolderPath, $"{firstName+lastName}");

            var licenseUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id
            };

            LicenseManager.EncryptToFile(licenseUser, folderPath);
        }
    }
}
