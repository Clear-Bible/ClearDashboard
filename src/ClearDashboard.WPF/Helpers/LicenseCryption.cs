using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Newtonsoft.Json.Linq;

namespace Helpers
{
    public static class LicenseCryption
    {
        public static string DecryptFromFile(string path)
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
                                var decryptedMessage = decryptReader.ReadToEndAsync();
                                //_output.WriteLine($"The decrypted original message: {decryptedMessage}");
                                return decryptedMessage.Result;
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

        public static async Task EncryptToFile()
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

        public static LicenseUser DecryptedJsonToLicenseUser(string decryptedLicenseKey)
        {
            var jsonLicense = JObject.Parse(decryptedLicenseKey);
            LicenseUser decryptedLicenseUser = new LicenseUser();
            decryptedLicenseUser.FirstName = jsonLicense.GetValue("FirstName").ToString();
            decryptedLicenseUser.LastName = jsonLicense.GetValue("LastName").ToString();
            //decryptedLicenseUser.ParatextUserName = jsonLicense.GetValue("ParatextUserName").ToString();
            //decryptedLicenseUser.LicenseKey = jsonLicense.GetValue("LicenseKey").ToString();
            return decryptedLicenseUser;
        }
    }

}
