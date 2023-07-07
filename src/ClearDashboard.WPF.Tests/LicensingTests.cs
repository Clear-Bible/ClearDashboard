using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{
    public  class LicensingTests
    {
        private readonly ITestOutputHelper _output;

        public LicensingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task EncryptDecryptTest()
        {
            await EncryptToFile();
            await DecryptFromFile();
        }

        private  async Task DecryptFromFile()
        {
            try
            {
                using (FileStream fileStream = new("license.key", FileMode.Open))
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
                                _output.WriteLine($"The decrypted original message: {decryptedMessage}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"The decryption failed. {ex}");
            }
        }

        private  async Task EncryptToFile()
        {
            try
            {

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Bob",
                    LastName = "Smith"
                };
                using (FileStream fileStream = new("license.key", FileMode.OpenOrCreate))
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

                _output.WriteLine("The file was encrypted.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"The encryption failed. {ex}");
            }
        }
    }
}
