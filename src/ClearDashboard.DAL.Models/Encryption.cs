using System.Security.Cryptography;

namespace ClearDashboard.DataAccessLayer.Models
{
    public static class Encryption
    {
        //This security key should be very complex and Random for encrypting the text. This playing vital role in encrypting the text.
        private const string AesKey = "ZXe8YwuIn1zxt3FPWTZFlAa14EHdvAdN9FaZ9RQWihc=";
        private const string AesIv = "CsxnWolsAyO7kCiWuyrnqg==";

        public static string Encrypt(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return string.Empty;
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(AesKey);
            aes.IV = Convert.FromBase64String(AesIv);
            byte[] encryptedBytes = EncryptStringToBytes_Aes(rawText, aes.Key, aes.IV);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string encryption)
        {
            if (!string.IsNullOrEmpty(encryption))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(AesKey);
                    aes.IV = Convert.FromBase64String(AesIv);
                    byte[] data = Convert.FromBase64String(encryption);
                    return DecryptStringFromBytes_Aes(data, aes.Key, aes.IV);
                }
            }
            return string.Empty;
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0) throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0) throw new ArgumentNullException("iv");

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            var encrypted = msEncrypt.ToArray();

            return encrypted;
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0) throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0) throw new ArgumentNullException("iv");
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new MemoryStream(cipherText);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            string plaintext = srDecrypt.ReadToEnd();
            return plaintext;
        }
    }
}
