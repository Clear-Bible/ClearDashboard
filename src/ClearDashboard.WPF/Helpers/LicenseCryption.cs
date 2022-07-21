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
                var str = File.ReadAllText(path);

                AesCryptoServiceProvider crypt_provider = new();
                crypt_provider.BlockSize = 128;
                crypt_provider.KeySize = 128;

                //crypt_provider.GenerateKey();
                byte[] key =
                {
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                };
                crypt_provider.Key = key;

                //crypt_provider.GenerateIV();
                crypt_provider.IV = key;

                crypt_provider.Mode = CipherMode.CBC;
                crypt_provider.Padding = PaddingMode.PKCS7;
                
                ICryptoTransform transform = crypt_provider.CreateDecryptor();
                byte[] encrypted_bytes = Convert.FromBase64String(str);
                byte[] decrypted_bytes = transform.TransformFinalBlock(encrypted_bytes, 0, encrypted_bytes.Length);
                
                var serialized = ASCIIEncoding.ASCII.GetString(decrypted_bytes);

                return serialized;

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
