using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public class LicensingTests
    {
        private readonly ITestOutputHelper _output;

        public LicensingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task EncryptDecryptTest()
        {

            string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string FilePath = Path.Combine(DocumentsPath, "ClearDashboard_Projects\\license.txt");

            AesCryptoServiceProvider crypt_provider = new();
            crypt_provider.BlockSize = 128;
            crypt_provider.KeySize = 128;

            byte[] key =
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };
            crypt_provider.Key = key;
            crypt_provider.IV = key;

            crypt_provider.Mode = CipherMode.CBC;
            crypt_provider.Padding = PaddingMode.PKCS7;
            EncryptToFile(crypt_provider, FilePath);
            await DecryptFromFile(crypt_provider, FilePath);
        }

        private void EncryptToFile(AesCryptoServiceProvider crypt_provider, string path)
        {
            try
            {
                var licenseUser = new LicenseUser
                {
                    Id = Guid.NewGuid().ToString("N"),
                    LicenseKey = Guid.NewGuid().ToString("N"),
                    FirstName = "Bob",
                    LastName = "Smith",
                };

                ICryptoTransform transform = crypt_provider.CreateEncryptor();
                var serialized = JsonSerializer.Serialize<LicenseUser>(licenseUser);

                var decrypted_bytes = ASCIIEncoding.ASCII.GetBytes(serialized);
                byte[] encrypted_bytes = transform.TransformFinalBlock(decrypted_bytes, 0, decrypted_bytes.Length);
                string str = Convert.ToBase64String(encrypted_bytes);
                
                File.WriteAllText(path, str);

                _output.WriteLine("The file was encrypted.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"The encryption failed. {ex}");
            }
        }

        private async Task DecryptFromFile(AesCryptoServiceProvider crypt_provider, string path)
        {
            try
            {
                var str = File.ReadAllText(path);

                ICryptoTransform transform = crypt_provider.CreateDecryptor();

                byte[] encrypted_bytes = Convert.FromBase64String(str);
                byte[] decrypted_bytes = transform.TransformFinalBlock(encrypted_bytes, 0, encrypted_bytes.Length);
                var serialized = ASCIIEncoding.ASCII.GetString(decrypted_bytes);

                _output.WriteLine($"The decrypted original message: {serialized}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"The decryption failed. {ex}");
            }
        }

        private async Task EncryptNumbersFile()
        {
            string text = "Hello";
            string numbers = "";
                foreach (char c in text)
            {
                numbers += String.Format("{0}", Convert.ToByte(c)).PadLeft(3,'0');
            }
        }
    }
}
