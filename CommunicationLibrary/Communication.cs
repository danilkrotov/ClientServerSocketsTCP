using CommunicationLibrary.Crypt;
using System;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary
{
    public class Communication
    {
        #region Отправка сообщения
        private static void SendToStreamMessage(string json, NetworkStream stream)
        {
            // Разбираем json на byte[]
            byte[] mainData = Encoding.UTF8.GetBytes(json);

            //Создаем пакет размером в 4 байта + размер изначального пакета.
            byte[] bytePacket = new byte[4 + mainData.Length];

            //Размер пакета помещаем в первые 4 байта (Размер Int32 всегда умещается в 4, не превышать Int32 2 147 483 648)
            byte[] bytePacketLenght = BitConverter.GetBytes(mainData.Length);

            //В bytePacket с 0 индекса вкладываем bytePacketLenght (должен влезть до 4)
            bytePacketLenght.CopyTo(bytePacket, 0);

            //В bytePacket с 4 индекса, ложим оставшуюся дату
            mainData.CopyTo(bytePacket, 4);

            // Отправка
            stream.Write(bytePacket, 0, bytePacket.Length);
        }
        #endregion

        #region Получение сообщения
        private static string ReceiveToStreamMessage(NetworkStream stream)
        {
            byte[] bytePacketLenght = new byte[4]; // создаем байтовый массив (куда запишем длину пакета)
            //Пытаемся считать первые 4 байта, если такие присутствуют, то это необходимый нам пакет затем пробуем посмотреть его размер
            int firstByte = stream.ReadByte();
            int secondByte = stream.ReadByte();
            int thirdByte = stream.ReadByte();
            int fourthByte = stream.ReadByte();
            if (firstByte == -1 || secondByte == -1 || thirdByte == -1 || fourthByte == -1)
            {
                //Если хоть один из этих байтов -1 , значит выходим, такие данные в нашем потоке нам не нужны
                Console.WriteLine("В первых 4 байтах не найден размер пакета, stream пуст");
                return null;
            }
            else
            {
                //Считываем первые 4 байта
                bytePacketLenght[0] = Convert.ToByte(firstByte); // Считывает байт из NetworkStream и перемещает позицию в потоке на один байт или возвращает –1, если достигнут конец потока.
                bytePacketLenght[1] = Convert.ToByte(secondByte);
                bytePacketLenght[2] = Convert.ToByte(thirdByte);
                bytePacketLenght[3] = Convert.ToByte(fourthByte);
            }

            try
            {
                StringBuilder builder = new StringBuilder();
                byte[] data;
                int dataLength = 0;
                int exitCycle = 0;

                dataLength = BitConverter.ToInt32(bytePacketLenght, 0); // Конвертируем байты в числа
                data = new byte[dataLength]; // готовим буфер под необходимый размер
                int allBytes = 0; // инициализация, общее количество принятых байтов
                do
                {
                    //Console.WriteLine("WAIT READ");
                    int bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    exitCycle++;
                    allBytes = allBytes + bytes;
                }
                while (allBytes < dataLength || exitCycle == 50); //выходим если за 50 повторов не удалось собрать пакет

                if (exitCycle == 50)
                {
                    //за 50 циклов не удалось собрать пакет, выдаём null
                    Console.WriteLine("За 50 циклов не удалось собрать пакет");
                    return null;
                }
                else
                {
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Подключение прервано CommunicationLibrary: " + ex.Message); //соединение было прервано
                return null;
            }
        }
        #endregion

        #region Шифрование на стороне сервера
        /// <summary>
        /// Используется для создания зашифрованного канала, обмена ключами, предполагается запуск на стороне сервера
        /// </summary>
        public static void StartServerEncrypt(NetworkStream stream)
        {
            EncryptRSA.CreateKey(); // Создаёт открытый и закрытый ключ RSA. Открывает доступ к открытому ключу.
            string rsaKey = EncryptRSA.GetKey(); // Получаем открытый ключ
            SendToStreamMessage(rsaKey, stream);  //Отправляем клиенту открытый ключ

            string streamString = ReceiveToStreamMessage(stream); // ждём сообщение от клиента
            byte[] encryptData = Convert.FromBase64String(streamString); // переводим сообщение обратно в byte[]
            string aesKey = EncryptRSA.Decrypt(encryptData); // расшифровываем сообщение с помощью закрытого ключа
            EncryptAES.LoadKeyInJson(aesKey); // загружаем симметричный AES ключ
        }
        #endregion

        #region Шифрование на стороне клиента
        /// <summary>
        /// Используется для создания зашифрованного канала, обмена ключами, предполагается запуск на стороне клиента
        /// </summary>
        public static void StartClientEncrypt(NetworkStream stream)
        {
            string rsaKey = ReceiveToStreamMessage(stream); //ждём сообщение от сервера с открытым ключом
            EncryptRSA.LoadKeyInXML(rsaKey); //загружаем открытый ключ сервера

            EncryptAES.CreateKey(); //Создаём симметричный ключ
            string aesKey = EncryptAES.GetKey(); //Получаем симметричный ключ в виде строки
            byte[] encryptData = EncryptRSA.Encrypt(aesKey); //зашифровываем симметричный ключ открытым ключом сервера
            SendToStreamMessage(Convert.ToBase64String(encryptData), stream);  //Отправляем серверу зашифрованное сообщение
        }
        #endregion

        #region Отправка \ Получение сообщений
        public static void SendMessage(string message, NetworkStream stream)
        {
            string encryptData = Convert.ToBase64String(EncryptAES.Encrypt(message)); //зашифровываем информацию
            SendToStreamMessage(encryptData, stream);
        }

        public static string ReceiveMessage(NetworkStream stream)
        {
            string encryptData = ReceiveToStreamMessage(stream);
            return EncryptAES.Decrypt(Convert.FromBase64String(encryptData)); //расшифровываем информацию
        }
        #endregion
    }
}
