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
                /*
                // получаем имя пользователя
                string message = GetMessage();
                userName = message;

                message = userName + " вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                */

                Communication.StartServerEncrypt(Stream);

                string message;
                // в бесконечном цикле получаем сообщения от клиента
                bool loop = true;
                while (loop)
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
                            //UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jsonPacket.AuthData);
                            if (jsonPacket == null)
                            {
                                Console.WriteLine("[" + Id + "] " + "jsonPacket null ");
                            }
                            else
                            {
                                switch (jsonPacket.Header)
                                {
                                    case "Test":
                                        Console.WriteLine("Test Packet: " + jsonPacket.Message);
                                        jsonPacket.Message = "Ответ";
                                        Communication.SendMessage(JsonConvert.SerializeObject(jsonPacket), Stream);
                                        break;                                        
                                    default:
                                        Console.WriteLine("[" + Id + "] " + "Пришел пакет с именем: " + jsonPacket.Header + " такой пакет не был распознан");
                                        loop = false;
                                        break;
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

        // закрытие подключения
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
