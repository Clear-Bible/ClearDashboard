using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer
{
    public static class LicenseManager
    {
        private static AesCryptoServiceProvider CreateCryptoProvider()
        {
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
            return crypt_provider;
        }

        public static void EncryptToDirectory(LicenseUser licenseUser, string path)
        {
            try
            {
                var crypt_provider = CreateCryptoProvider();

                ICryptoTransform transform = crypt_provider.CreateEncryptor();
                var serialized = JsonSerializer.Serialize<LicenseUser>(licenseUser);

                var decrypted_bytes = ASCIIEncoding.ASCII.GetBytes(serialized);
                byte[] encrypted_bytes = transform.TransformFinalBlock(decrypted_bytes, 0, decrypted_bytes.Length);
                string str = Convert.ToBase64String(encrypted_bytes);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllText(Path.Combine(path, "license.txt"), str);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string DecryptFromFile(string path)
        {
            try
            {
                var str = File.ReadAllText(path);

                return DecryptFromString(str);

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string DecryptFromString(string str)
        {
            try
            {
                var crypt_provider = CreateCryptoProvider();

                ICryptoTransform transform = crypt_provider.CreateDecryptor();
                byte[] encrypted_bytes = Convert.FromBase64String(str);
                byte[] decrypted_bytes = transform.TransformFinalBlock(encrypted_bytes, 0, encrypted_bytes.Length);

                var serialized = ASCIIEncoding.ASCII.GetString(decrypted_bytes);

                return serialized;

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static LicenseUser DecryptedJsonToLicenseUser(string decryptedLicenseKey)
        {
            return JsonSerializer.Deserialize<LicenseUser>(decryptedLicenseKey);
        }

        public static bool CompareGivenUserAndDecryptedUser(LicenseUser given, LicenseUser decrypted)
        {
            if (given.FirstName == decrypted.FirstName &&
                given.LastName == decrypted.LastName)// &&
                //given.LicenseKey == decrypted.LicenseKey) <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
