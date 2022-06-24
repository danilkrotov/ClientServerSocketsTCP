using CommunicationLibrary;
using CommunicationLibrary.Crypt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerSocketsTCP.Class
{
    internal class Client
    {
        public string Id { get; private set; }
        public NetworkStream Stream { get; private set; }
        TcpClient client;
        Server server; // объект сервера

        public Client(TcpClient tcpClient, Server serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                Communication.StartServerEncrypt(Stream);
                //Broadcast
                //server.BroadcastMessage(message, this.Id);

                while (true)
                {
                    try
                    {
                        Console.WriteLine("[" + Id + "] " + "Прослушивание стрима");
                        string data = Communication.ReceiveMessage(Stream);
                        if (data == null)
                        {
                            server.RemoveConnection(this.Id);
                            Console.WriteLine("[" + Id + "] " + "Сервер закрыл соединение, по причине пустого потока");
                            Close();
                        }
                        else
                        {
                            Console.WriteLine("[" + Id + "] " + "Data: " + data);
                            JsonPacket? jsonPacket = JsonConvert.DeserializeObject<JsonPacket>(data);
                            if (jsonPacket == null)
                            {
                                Console.WriteLine("[" + Id + "] " + "Не удалось десериализовать входящее сообщение (JsonPacket null)");
                            }
                            else
                            {                                
                                if (Authentication(jsonPacket.AuthData))
                                {
                                    Actions(jsonPacket);
                                }
                                else 
                                {
                                    Console.WriteLine("[" + Id + "] " + "Ошибка аутентификации");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //message = ("[" + Id + "] " + "Exception: " + ex);
                        //Console.WriteLine(message);
                        //server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Console.WriteLine("[" + Id + "] " + "Сервер закрыл соединение");
                Close();
            }
        }

        /// <summary>
        /// Закрывает подключение
        /// </summary>
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

        /// <summary>
        /// Возващает true при успешной аутентификации
        /// </summary>
        private bool Authentication(string authData)
        {
            //Предполагается использование объекта с данными авторизации
            //UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(AuthData);
            return true;
        }

        /// <summary>
        /// Выполняет определенное действие в зависимости от сообщения в header
        /// </summary>
        private void Actions(JsonPacket jsonPacket) 
        {
            switch (jsonPacket.Header)
            {
                case "Test":
                    //Выполняем действие с пришедшим заголовком Test
                    Console.WriteLine("Test Packet: " + jsonPacket.Message);
                    //Отправляем обратно данные если это необходимо
                    JsonPacket responseJsonPacket = new JsonPacket(jsonPacket.Header, null, "Ответ");
                    Communication.SendMessage(JsonConvert.SerializeObject(responseJsonPacket), Stream);
                    break;
                default:
                    Console.WriteLine("[" + Id + "] " + "Пришел пакет с именем: " + jsonPacket.Header + " такой пакет не был распознан");
                    break;
            }
        }
    }
}
