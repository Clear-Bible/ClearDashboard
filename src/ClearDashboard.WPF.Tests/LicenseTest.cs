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
            AesCryptoServiceProvider crypt_provider = new();
            crypt_provider.BlockSize = 128;
            crypt_provider.KeySize = 128;
            //crypt_provider.GenerateIV();//
            //crypt_provider.GenerateKey();//

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
            EncryptToFile(crypt_provider);
            await DecryptFromFile(crypt_provider);
        }

        

        private void EncryptToFile(AesCryptoServiceProvider crypt_provider)
        {
            try
            {

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    LicenseKey = Guid.NewGuid().ToString(),
                    FirstName = "Bob",
                    LastName = "Smith",
                };

                ICryptoTransform transform = crypt_provider.CreateEncryptor();
                var serialized = JsonSerializer.Serialize<User>(user);

                var decrypted_bytes = ASCIIEncoding.ASCII.GetBytes(serialized);
                byte[] encrypted_bytes = transform.TransformFinalBlock(decrypted_bytes, 0, decrypted_bytes.Length);
                string str = Convert.ToBase64String(encrypted_bytes);
                
                File.WriteAllText("C:\\Users\\rober\\Documents\\ClearDashboard_Projects\\license.txt", str);

                //using (FileStream fileStream = new("C:\\Users\\rober\\Documents\\ClearDashboard_Projects\\license.txt", FileMode.OpenOrCreate))
                //{
                //    using (Aes aes = Aes.Create())
                //    {
                //        byte[] key =
                //        {
                //            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                //            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                //        };
                //        aes.Key = key;

                //        byte[] iv = aes.IV;
                //        fileStream.Write(iv, 0, iv.Length);

                //        var encrypter = aes.CreateEncryptor();

                //        using (CryptoStream cryptoStream = new(
                //                   fileStream,
                //                   aes.CreateEncryptor(),
                //                   CryptoStreamMode.Write))
                //        {
                //            using (StreamWriter encryptWriter = new(cryptoStream))
                //            {
                //                //encryptWriter.WriteLine($"LicenseKey: {Guid.NewGuid()}");
                //                //encryptWriter.WriteLine($"UserId: {Guid.NewGuid()}");
                //                //encryptWriter.WriteLine($"FirstName: Bob");
                //                //encryptWriter.WriteLine($"LastName: Smith");

                //                encryptWriter.WriteLine(JsonSerializer.Serialize<User>(user));
                //            }
                //        }
                //    }
                //}



                ////var serialized = JsonSerializer.Serialize<User>(user);
                ////string numbers = "";
                ////foreach (char c in serialized)
                ////{
                ////    numbers += String.Format("{0}", Convert.ToByte(c)).PadLeft(3, '0');
                ////}

                ////File.WriteAllText("C:\\Users\\rober\\Documents\\ClearDashboard_Projects\\license.txt", numbers.ToString());

                _output.WriteLine("The file was encrypted.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"The encryption failed. {ex}");
            }
        }

        private async Task DecryptFromFile(AesCryptoServiceProvider crypt_provider)
        {
            try
            {
                //using (FileStream fileStream = new("C:\\Users\\rober\\Documents\\ClearDashboard_Projects\\license.txt", FileMode.Open))
                //{
                //    using (Aes aes = Aes.Create())
                //    {
                //        byte[] iv = new byte[aes.IV.Length];
                //        int numBytesToRead = aes.IV.Length;
                //        int numBytesRead = 0;
                //        while (numBytesToRead > 0)
                //        {
                //            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                //            if (n == 0) break;

                //            numBytesRead += n;
                //            numBytesToRead -= n;
                //        }

                //        byte[] key =
                //        {
                //            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                //            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                //        };

                //        using (CryptoStream cryptoStream = new(
                //                   fileStream,
                //                   aes.CreateDecryptor(key, iv),
                //                   CryptoStreamMode.Read))
                //        {
                //            using (StreamReader decryptReader = new(cryptoStream))
                //            {
                //                string decryptedMessage = await decryptReader.ReadToEndAsync();
                //                _output.WriteLine($"The decrypted original message: {decryptedMessage}");
                //            }
                //        }
                //    }
                //}

                var str = File.ReadAllText("C:\\Users\\rober\\Documents\\ClearDashboard_Projects\\license.txt");

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
    //public class LicensingTests
    //{
    //    private readonly ITestOutputHelper _output;

    //    public LicensingTests(ITestOutputHelper output)
    //    {
    //        _output = output;
    //    }

    //    [Fact]
    //    public async Task EncryptDecryptTest()
    //    {
    //        await EncryptToFile();
    //        await DecryptFromFile();

    //    }

    //    private async Task DecryptFromFile()
    //    {
    //        try
    //        {
    //            using (FileStream fileStream = new("license.key", FileMode.Open))
    //            {
    //                using (Aes aes = Aes.Create())
    //                {
    //                    byte[] iv = new byte[aes.IV.Length];
    //                    int numBytesToRead = aes.IV.Length;
    //                    int numBytesRead = 0;
    //                    while (numBytesToRead > 0)
    //                    {
    //                        int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
    //                        if (n == 0) break;

    //                        numBytesRead += n;
    //                        numBytesToRead -= n;
    //                    }

    //                    byte[] key =
    //                    {
    //                        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    //                        0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
    //                    };

    //                    using (CryptoStream cryptoStream = new(
    //                               fileStream,
    //                               aes.CreateDecryptor(key, iv),
    //                               CryptoStreamMode.Read))
    //                    {
    //                        using (StreamReader decryptReader = new(cryptoStream))
    //                        {
    //                            string decryptedMessage = await decryptReader.ReadToEndAsync();
    //                            _output.WriteLine($"The decrypted original message: {decryptedMessage}");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _output.WriteLine($"The decryption failed. {ex}");
    //        }
    //    }


    //    //private async Task DecryptFromFile()
    //    //{
    //    //    try
    //    //    {
    //    //        using (FileStream fileStream = new("license.key", FileMode.Open))
    //    //        {
    //    //            using (Aes aes = Aes.Create())
    //    //            {
    //    //                byte[] iv = new byte[aes.IV.Length];
    //    //                int numBytesToRead = aes.IV.Length;
    //    //                int numBytesRead = 0;
    //    //                while (numBytesToRead > 0)
    //    //                {
    //    //                    int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
    //    //                    if (n == 0) break;

    //    //                    numBytesRead += n;
    //    //                    numBytesToRead -= n;
    //    //                }

    //    //                byte[] key =
    //    //                {
    //    //                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    //    //                    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
    //    //                };

    //    //                using (CryptoStream cryptoStream = new(
    //    //                           fileStream,
    //    //                           aes.CreateDecryptor(key, iv),
    //    //                           CryptoStreamMode.Read))
    //    //                {
    //    //                    using (StreamReader decryptReader = new(cryptoStream))
    //    //                    {
    //    //                        string decryptedMessage = await decryptReader.ReadToEndAsync();
    //    //                        _output.WriteLine($"The decrypted original message: {decryptedMessage}");
    //    //                    }
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        _output.WriteLine($"The decryption failed. {ex}");
    //    //    }
    //    //}

    //    //private async Task EncryptToFile()
    //    //{
    //    //    try
    //    //    {

    //    //        var user = new User
    //    //        {
    //    //            Id = Guid.NewGuid(),
    //    //            LicenseKey = Guid.NewGuid().ToString(),
    //    //            FirstName = "Bob",
    //    //            LastName = "Smith"
    //    //        };
    //    //        using (FileStream fileStream = new("license.key", FileMode.OpenOrCreate))
    //    //        {
    //    //            using (Aes aes = Aes.Create())
    //    //            {
    //    //                byte[] key =
    //    //                {
    //    //                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    //    //                    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
    //    //                };
    //    //                aes.Key = key;

    //    //                byte[] iv = aes.IV;
    //    //                fileStream.Write(iv, 0, iv.Length);

    //    //                using (CryptoStream cryptoStream = new(
    //    //                           fileStream,
    //    //                           aes.CreateEncryptor(),
    //    //                           CryptoStreamMode.Write))
    //    //                {
    //    //                    using (StreamWriter encryptWriter = new(cryptoStream))
    //    //                    {
    //    //                        //encryptWriter.WriteLine($"LicenseKey: {Guid.NewGuid()}");
    //    //                        //encryptWriter.WriteLine($"UserId: {Guid.NewGuid()}");
    //    //                        //encryptWriter.WriteLine($"FirstName: Bob");
    //    //                        //encryptWriter.WriteLine($"LastName: Smith");

    //    //                        encryptWriter.WriteLine(JsonSerializer.Serialize<User>(user));
    //    //                    }
    //    //                }
    //    //            }
    //    //        }

    //    //        _output.WriteLine("The file was encrypted.");
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        _output.WriteLine($"The encryption failed. {ex}");
    //    //    }
    //    //}
    //}
}
