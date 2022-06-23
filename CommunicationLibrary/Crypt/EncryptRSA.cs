using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Crypt
{
    internal class EncryptRSA
    {
        private static RSA? RSAparam;

        /// <summary>
        /// Создаёт открытый и закрытый ключ RSA. Открывает доступ к открытому ключу.
        /// </summary>
        internal static void CreateKey()
        {
            //Генерируем пару открытый/закрытый ключ.
            RSAparam = RSA.Create();

            //Настраиваем RSAparam так, что-бы командой FromXmlString извлекался только открытый ключ
            RSAparam.ExportParameters(false);
        }

        /// <summary>
        /// Возвращает открытый ключ в виде XML string (Может быть использована только после CreateKey() )
        /// </summary>
        internal static string GetKey()
        {
            //Отдаём его как string для дальнейшей передачи
            return RSAparam.ToXmlString(false);
        }

        /// <summary>
        /// Загружает открытый ключ из XML string
        /// </summary>
        internal static void LoadKeyInXML(string xmlKey)
        {
            //инициализируем переменную перед загрузкой ключа, для устаноки дефолтовых параметров
            RSAparam = RSA.Create();
            RSAparam.ExportParameters(false);
            //
            RSAparam.FromXmlString(xmlKey);
        }

        /// <summary>
        /// Шифрует сообщение с помощью открытого ключа
        /// </summary>
        internal static byte[] Encrypt(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            return RSAparam.Encrypt(data, RSAEncryptionPadding.OaepSHA512);
        }

        /// <summary>
        /// Расшифровывает сообщение с помощью закрытого ключа
        /// </summary>
        internal static string Decrypt(byte[] message)
        {
            byte[] data = RSAparam.Decrypt(message, RSAEncryptionPadding.OaepSHA512);

            return Encoding.UTF8.GetString(data);
        }
    }
}
