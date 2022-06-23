using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Crypt
{
    internal class AesInfo
    {
        public byte[]? Key { get; set; }
        public byte[]? IV { get; set; }
    }
    internal class EncryptAES
    {
        private static Aes? myAes { get; set; }

        /// <summary>
        /// Создаёт симметричный ключ
        /// </summary>
        internal static void CreateKey()
        {
            myAes = Aes.Create();
        }

        /// <summary>
        /// Конвертирует ключ в json (Может быть использована только после CreateKey() )
        /// </summary>
        internal static string GetKey()
        {
            AesInfo? aes = new AesInfo();
            aes.Key = myAes.Key;
            aes.IV = myAes.IV;
            string? json = null;
            try
            {
                json = JsonConvert.SerializeObject(aes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AES Json Serialize error" + ex.Message);
            }
            return json;
        }

        /// <summary>
        /// Загружает ключ из json
        /// </summary>
        internal static void LoadKeyInJson(string jsonAes)
        {
            AesInfo? aes = null;
            try
            {
                aes = JsonConvert.DeserializeObject<AesInfo>(jsonAes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AES Json Deserialize error" + ex.Message);
            }
            CreateKey(); //создаём новый AES со всеми базовыми настройками
            myAes.Key = aes.Key;
            myAes.IV = aes.IV;
        }

        /// <summary>
        /// Шифрует сообщение
        /// </summary>
        internal static byte[] Encrypt(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (myAes.Key == null || myAes.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (myAes.IV == null || myAes.IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Расшифровывает сообщение
        /// </summary>
        internal static string Decrypt(byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (myAes.Key == null || myAes.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (myAes.IV == null || myAes.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            ICryptoTransform decryptor = myAes.CreateDecryptor(myAes.Key, myAes.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plaintext;
        }

        internal static string StaticEncrypt(string plainText)
        {
            byte[] encrypted;

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = Convert.FromBase64String("AXe8YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=");
                aes.IV = Convert.FromBase64String("bsxnWolsAyO7kCfWuyrnqg==");
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        internal static string StaticDecrypt(string encryptedText)
        {
            string? decrypted = null;
            byte[] cipher = Convert.FromBase64String(encryptedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = Convert.FromBase64String("AXe8YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=");
                aes.IV = Convert.FromBase64String("bsxnWolsAyO7kCfWuyrnqg==");
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipher))
                {
                    using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            decrypted = sr.ReadToEnd();
                        }
                    }
                }
            }

            return decrypted;
        }
    }
}
