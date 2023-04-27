using ClearDashboard.DataAccessLayer.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ClearDashboard.DataAccessLayer
{
    public static class LicenseManager
    {

        public static string LicenseFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects",
                "license.txt");

        private static Aes CreateCryptoProvider()
        {
            var cryptProvider = Aes.Create();
            cryptProvider.BlockSize = 128;
            cryptProvider.KeySize = 128;

            byte[] key =
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };
            cryptProvider.Key = key;

            cryptProvider.IV = key;

            cryptProvider.Mode = CipherMode.CBC;
            cryptProvider.Padding = PaddingMode.PKCS7;
            return cryptProvider;
        }

        public static void EncryptToFile(User licenseUser, string path)
        {

            var str = EncryptToString(licenseUser, path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(Path.Combine(path, "license.txt"), str);

        }

        public static string EncryptToString(User licenseUser, string path)
        {

            var cryptProvider = CreateCryptoProvider();

            var transform = cryptProvider.CreateEncryptor();
            var serialized = JsonSerializer.Serialize(licenseUser);

            var decryptedBytes = Encoding.ASCII.GetBytes(serialized);
            var encryptedBytes = transform.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
            var str = Convert.ToBase64String(encryptedBytes);

            return str;

        }

        public static User GetUserFromLicense()
        {
            return DecryptLicenseFromFileToUser(LicenseFilePath);
        }

        public static User DecryptLicenseFromFileToUser(string path)
        {
            try
            {
                var json = DecryptLicenseFromFile(path);

                return DecryptedJsonToUser(json);

            }
            catch (Exception)
            {
                return new User();
            }
        }

        public static string DecryptLicenseFromString(string str)
        {
            try
            {
                var cryptProvider = CreateCryptoProvider();

                var transform = cryptProvider.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(str);
                var decryptedBytes = transform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                var serialized = Encoding.ASCII.GetString(decryptedBytes);

                return serialized;

            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string DecryptLicenseFromFile(string path)
        {
            try
            {
                var str = File.ReadAllText(path);

                return DecryptLicenseFromString(str);

            }
            catch (Exception)
            {
                return "";
            }
        }

        public static User DecryptedJsonToUser(string decryptedLicenseKey)
        {
            try
            {
                var licenseUser = JsonSerializer.Deserialize<User>(decryptedLicenseKey);
                return licenseUser ?? new User();
            }
            catch (Exception)
            {
                try
                {
                    var temporaryLicenseUser = JsonSerializer.Deserialize<TemporaryLicenseUser>(decryptedLicenseKey);
                    var licenseUser = new User
                    {
                        FirstName = temporaryLicenseUser.FirstName,
                        LastName = temporaryLicenseUser.LastName,
                        LicenseKey = temporaryLicenseUser.LicenseKey,
                        ParatextUserName = temporaryLicenseUser.ParatextUserName,
                        Id = Guid.Parse(temporaryLicenseUser.Id)
                    };
                    return licenseUser;
                }
                catch
                {
                    return new User();
                }
            }
        }

        public static LicenseUserMatchType CompareGivenUserAndDecryptedUser(User given, User decrypted)
        {
            if (given.FirstName == decrypted.FirstName && given.LastName == decrypted.LastName)// &&
                                                                                               //given.LicenseKey == decrypted.LicenseKey) <-- not the same thing right now.  One is the code that gets decrypted, the other is a Guid
            {
                return LicenseUserMatchType.Match;
            }
            else if (given.FirstName != decrypted.FirstName && given.LastName != decrypted.LastName)
            {
                return LicenseUserMatchType.BothNameMismatch;
            }
            else if (given.FirstName != decrypted.FirstName && given.LastName == decrypted.LastName)
            {
                return LicenseUserMatchType.FirstNameMismatch;
            }
            else if (given.FirstName == decrypted.FirstName && given.LastName != decrypted.LastName)
            {
                return LicenseUserMatchType.LastNameMismatch;
            }

            return LicenseUserMatchType.Error;

        }
    }
}
