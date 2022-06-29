using CommunicationLibrary.Crypt;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientServerSocketsTCP.Class
{
    internal class Server
    {
        static TcpListener? tcpListener; // Сервер для прослушивания
        List<Client> clients = new List<Client>(); // Список всех клиентов подключившихся к серверу
        //Конфигурация
        IPAddress ipAddress;
        int port;

        internal Server(IPAddress ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }   

        public void AddConnection(Client clientObject)
        {
            clients.Add(clientObject);
        }

        public void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            Client? client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // Прослушивание входящих подключений
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    Client clientObject = new Client(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    Console.WriteLine("Поток для входящих сообщений клиента " + clientObject.Id + " запущен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        public void BroadcastMessage(JsonPacket jsonPacket, string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Comm.SendMessage(jsonPacket); // Передача сообщения всем остальным клиентам
                }
            }
        }

        // отключение всех клиентов
        public void Disconnect()
        {
            if (tcpListener != null) 
            {
                tcpListener.Stop(); //остановка сервера

                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].Close(); //отключение клиента
                }
                Environment.Exit(0); //завершение процесса
            }
        }
    }
}
