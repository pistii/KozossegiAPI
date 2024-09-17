using Google.Rpc;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KozoskodoAPI.Security
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

        public string EncryptAesManaged(string raw)
        {
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).
                // Same key must be used in encryption and decryption
                using (AesManaged aes = new AesManaged())
                {
                    // Encrypt string
                    byte[] encrypted = Encrypter(raw, aes.Key, aes.IV);
                    // Print encrypted string
                    Console.WriteLine($"Encrypted data: {System.Text.Encoding.UTF8.GetString(encrypted)}");
                    //decrypt the bytes to a string.
                    string decrypted = Decrypter(encrypted, aes.Key, aes.IV);
                    // Print decrypted string. It should be same as raw data
                    Console.WriteLine($"Decrypted data: {decrypted}");
                    return decrypted;
                }
                return null;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.ReadKey();
            return null;

        }

        static byte[] Encrypter(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream
                    // to encrypt
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data
            return encrypted;
        }

        static string Decrypter(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            // Create AesManaged
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
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

        public string EncryptAesManaged(string raw);
    }
}
