using System.Security.Cryptography;
using System.Text;

namespace KozossegiAPI.Security
{
    //https://medium.com/@adarsh-d/encryption-and-decryption-using-c-and-js-954d3836753a
    public class EncodeDecode : IEncodeDecode
    {
        //Titkosítás
        public byte[] Encrypt(string plainText, string key)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            byte[] encryptedBytes = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = new byte[16]; // Megfelelő hosszúságú véletlenszerű IV generálása
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            return encryptedBytes;
        }

        //Dekódolás
        //TODO: make it more secure, like decrypt the key with hmacsha256/1 or some hashing algorithm
        public string Decrypt(string base64CipherText, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;

                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                //aes.Key = MD5.HashData(key);
                var cipherBytes = Convert.FromBase64String(base64CipherText);
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        public string DecryptFromString(string base64CipherText)
        {
            var secret = "I love chocolate";
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
                  
            byte[] ivBytes = keyBytes;
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;

                aes.IV = new byte[16];
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                //aes.Key = MD5.HashData(key);
                var cipherBytes = Convert.FromBase64String(base64CipherText);
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

    }

    public interface IEncodeDecode
    {
        byte[] Encrypt(string plainText, string key);
        string Decrypt(string cipherBytes, string pass);
        
        string DecryptFromString(string base64CipherText);
    }
}
