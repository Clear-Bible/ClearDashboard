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
                
                byte[] key =
                {
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                };
                crypt_provider.Key = key;
                
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
