using System.Security.Cryptography;
using System.Text;

namespace QueueManagement.Api.Utilities
{
    public static class EncryptionHelper
    {
        private static readonly string Key = "QueueMgmtSys2025!"; // Use proper key management in production

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            var key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32));
            aes.Key = key;
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var encryptedBytes = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainText), 0, plainText.Length);

            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string encryptedText)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using var aes = Aes.Create();
            var key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32));
            aes.Key = key;
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}