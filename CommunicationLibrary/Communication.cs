using CommunicationLibrary.Crypt;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace CommunicationLibrary
{
    public class Communication
    {
        private NetworkStream stream; // Сохранённый поток
        private Aes aes; // Сохранённое шифрование
        /// <summary>
        /// Класс выполняющий шифрование, отправку и получение данных. Необходимо указать NetworkStream.
        /// </summary>
        public Communication(NetworkStream networkStream) 
        {
            stream = networkStream;
        }

        #region Отправка сообщения
        private void SendToStreamMessage(string json)
        {
            // Разбираем json на byte[]
            byte[] mainData = Encoding.UTF8.GetBytes(json);

            //Создаем массив байт размером в 4 байта + размер изначального массива байт.
            byte[] bytePacket = new byte[4 + mainData.Length];

            //Размер массива байт mainData помещаем в первые 4 байта (Размер Int32 всегда умещается в 4, не превышать Int32 2 147 483 648)
            byte[] bytePacketLenght = BitConverter.GetBytes(mainData.Length);

            //В bytePacket с 0 индекса вкладываем bytePacketLenght (до 4)
            bytePacketLenght.CopyTo(bytePacket, 0);

            //В bytePacket начиная с 4 индекса копируем основную информацию mainData
            mainData.CopyTo(bytePacket, 4);

            // Отправка
            stream.Write(bytePacket, 0, bytePacket.Length);
        }
        #endregion

        #region Получение сообщения
        private string ReceiveToStreamMessage()
        {
            // Создаем байтовый массив (Куда будем пробовать считать длину входящего сообщения (bytePacket) )
            byte[] bytePacketLenght = new byte[4];
            //Пытаемся считать первые 4 байта
            int firstByte = stream.ReadByte(); // Считывает байт из NetworkStream и перемещает позицию в потоке на один байт или возвращает –1, если достигнут конец потока.
            int secondByte = stream.ReadByte();
            int thirdByte = stream.ReadByte();
            int fourthByte = stream.ReadByte();
            if (firstByte == -1 || secondByte == -1 || thirdByte == -1 || fourthByte == -1)
            {
                //Если хоть один из этих байтов -1 , создаём исключение т.к. первые 4 байта должны существовать при любом дроблении TCP пакета, ситуация непредвиденная и нуждается в проверке
                throw new Exception("CommunicationLibrary: В первых 4 байтах не найден размер пакета или stream пуст");
            }
            else
            {
                //Считываем первые 4 байта
                bytePacketLenght[0] = Convert.ToByte(firstByte);
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

                dataLength = BitConverter.ToInt32(bytePacketLenght, 0); // Считываем массив байтов и конвертируем его в целое число размера входящих данных
                data = new byte[dataLength]; // Готовим буфер под необходимый размер
                int allBytes = 0; // Инициализация, общее количество принятых байтов

                //Собираем в builder (string) информацию которая пришла с помощью цикла do while
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    exitCycle++;
                    allBytes = allBytes + bytes;
                }
                while (allBytes < dataLength || exitCycle == 50); // Выходим если за 50 повторов циклов не получилось собрать информацию

                if (exitCycle == 50)
                {
                    throw new Exception("CommunicationLibrary: В течении 50 циклов не удалось достигнуть размера входящих данных (dataLength)");
                }
                else
                {
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CommunicationLibrary: Подключение прервано по причине " + ex.Message); //соединение было прервано
                return null;
            }
        }
        #endregion

        #region Шифрование на стороне сервера
        /// <summary>
        /// Используется для создания зашифрованного канала, обмена ключами, предполагается запуск на стороне сервера
        /// </summary>
        public void StartServerEncrypt()
        {
            
            EncryptRSA.CreateKey(); // Создаёт открытый и закрытый ключ RSA. Открывает доступ к открытому ключу.
            string rsaKey = EncryptRSA.GetKey(); // Получаем открытый ключ
            SendToStreamMessage(rsaKey); // Отправляем клиенту открытый ключ
            
            string streamString = ReceiveToStreamMessage(); // Ждём сообщение от клиента
            byte[] encryptData = Convert.FromBase64String(streamString); // Переводим сообщение обратно в byte[]
            string aesKey = EncryptRSA.Decrypt(encryptData); // Расшифровываем сообщение с помощью закрытого ключа
            EncryptAES.LoadKeyInJson(aesKey); // Загружаем симметричный AES ключ

            aes = EncryptAES.myAes; // Сохраняем AES для последующего шифрования
        }
        #endregion

        #region Шифрование на стороне клиента
        /// <summary>
        /// Используется для создания зашифрованного канала, обмена ключами, предполагается запуск на стороне клиента
        /// </summary>
        public void StartClientEncrypt()
        {            
            string rsaKey = ReceiveToStreamMessage(); // Ждём сообщение от сервера с открытым ключом
            EncryptRSA.LoadKeyInXML(rsaKey); // Загружаем открытый ключ сервера
            
            EncryptAES.CreateKey(); // Создаём симметричный ключ
            string aesKey = EncryptAES.GetKey(); // Получаем симметричный ключ в виде строки
            byte[] encryptData = EncryptRSA.Encrypt(aesKey); // Зашифровываем симметричный ключ открытым ключом сервера
            SendToStreamMessage(Convert.ToBase64String(encryptData)); // Отправляем серверу зашифрованное сообщение

            aes = EncryptAES.myAes; // Сохраняем AES для последующего шифрования
        }
        #endregion

        #region Отправка \ Получение сообщений
        public void SendMessage(JsonPacket message)
        {
            string encryptData = Convert.ToBase64String(EncryptAES.Encrypt(JsonConvert.SerializeObject(message), aes)); // Зашифровываем информацию с помощью AES
            SendToStreamMessage(encryptData);
        }

        public string ReceiveMessage()
        {
            return EncryptAES.Decrypt(Convert.FromBase64String(ReceiveToStreamMessage()), aes); // Расшифровываем информацию с помощью AES
        }
        #endregion
    }
}
