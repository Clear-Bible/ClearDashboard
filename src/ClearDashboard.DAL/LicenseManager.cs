using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer
{
    public static class LicenseManager
    {
        public static readonly string DocumentsDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        public static readonly string CollaborationDirectoryName = "Collaboration";
        public static readonly string CollaborationDirectoryPath = Path.Combine(DocumentsDirectoryPath, CollaborationDirectoryName);

        public static readonly string ClearDashboardProjectsDirectoryName = "ClearDashboard_Projects";
        public static readonly string ClearDashboardProjectsDirectoryPath = Path.Combine(DocumentsDirectoryPath, ClearDashboardProjectsDirectoryName);
        public static readonly string LegacyLicenseFilePath = Path.Combine(ClearDashboardProjectsDirectoryPath, LicenseFileName);

       

        public static readonly string MicrosoftFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft");
        public static readonly string UserSecretsDirectoryName = "UserSecrets";
        public static readonly string UserSecretsFolderPath = Path.Combine(MicrosoftFolderPath, UserSecretsDirectoryName);
        public static readonly string LicenseFolderPath = Path.Combine(UserSecretsFolderPath, "License");
        public static readonly string LicenseFileName = "license.txt";
        public static readonly string LicenseFilePath = Path.Combine(LicenseFolderPath, LicenseFileName);

        public static async Task<bool> DeleteOldLicense()
        {
            try
            {
                if (File.Exists(LicenseFilePath) && File.Exists(LegacyLicenseFilePath))
                {
                    File.Delete(LegacyLicenseFilePath);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

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

        public static void EncryptToFile(User licenseUser, string folderPath)
        {

            var str = EncryptToString(licenseUser);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(Path.Combine(folderPath, LicenseFileName), str);

        }

        public static string EncryptToString(User licenseUser)
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

        public static User DecryptLicenseFromFileToUser(string filePath)
        {
            try
            {
                var json = DecryptLicenseFromFile(filePath);

                return DecryptedJsonToUser(json);

            }
            catch (Exception)
            {
                return new User();
            }
        }

        public static string DecryptLicenseFromString(string str, bool isGenerator = false)
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
            catch (Exception ex)
            {
                ILogger<LicenseUserMatchType> logger = null;
                if (!isGenerator)
                {
                    logger = IoC.Get<ILogger<LicenseUserMatchType>>();
                    logger.LogError("DecryptLicenseFromString failed details: " + ex);
                }

                return "";
            }
        }
        public static string DecryptLicenseFromFile(string filePath)
        {
            try
            {
                var str = GetLicenseFromFile(filePath);

                return DecryptLicenseFromString(str);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetLicenseFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(LicenseFilePath) && filePath == LicenseFilePath)
                {
                    filePath = LegacyLicenseFilePath;
                }

                var str = File.ReadAllText(filePath);
                return str;
            }
            catch (Exception)
            {
                return "";
            }
           
        }

        public static User DecryptedJsonToUser(string decryptedLicenseKey, bool isGenerator=false)
        {
            try
            {
                var licenseUser = JsonSerializer.Deserialize<User>(decryptedLicenseKey);
                return licenseUser ?? new User();
            }
            catch (Exception ex)
            {
                ILogger<LicenseUserMatchType> logger = null;
                if (!isGenerator)
                {
                    logger = IoC.Get<ILogger<LicenseUserMatchType>>();
                    logger.LogError("DecryptedJsonToUser initial deserialization failed: " + ex);
                }
                try
                {
                    var temporaryLicenseUser = JsonSerializer.Deserialize<TemporaryLicenseUser>(decryptedLicenseKey);
                    var licenseUser = new User
                    {
                        FirstName = temporaryLicenseUser.FirstName,
                        LastName = temporaryLicenseUser.LastName,
                        ParatextUserName = temporaryLicenseUser.ParatextUserName,
                        Id = Guid.Parse(temporaryLicenseUser.Id)
                    };
                    return licenseUser;
                }
                catch
                {
                    if (!isGenerator)
                    {
                        logger.LogError("DecryptedJsonToUser second deserialization failed: " + ex);
                    }

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

        public static string EncryptCollabJsonToString(CollaborationConfiguration configuration)
        {
            var cryptProvider = CreateCryptoProvider();

            var transform = cryptProvider.CreateEncryptor();
            var serialized = JsonSerializer.Serialize(configuration);

            var decryptedBytes = Encoding.ASCII.GetBytes(serialized);
            var encryptedBytes = transform.TransformFinalBlock(decryptedBytes, 0, decryptedBytes.Length);
            var str = Convert.ToBase64String(encryptedBytes);

            return str;
        }

        public static CollaborationConfiguration DecryptCollabToConfiguration(string encryptedString)
        {
            var configuration = new CollaborationConfiguration
            {
                UserId = -1
            };

            try
            {
                var cryptProvider = CreateCryptoProvider();

                var transform = cryptProvider.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptedString);
                var decryptedBytes = transform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                var serialized = Encoding.ASCII.GetString(decryptedBytes);

                configuration = JsonSerializer.Deserialize<CollaborationConfiguration>(serialized);
            }
            catch (Exception ex)
            {
                var logger = IoC.Get<ILogger<LicenseUserMatchType>>();
                logger.LogError("DecryptCollabToConfiguration failed: " + ex);
            }

            return configuration;
        }

    }
}
