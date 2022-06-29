using ClientServerSocketsTCP.Class;
using System.Net;

Server? server = new Server(IPAddress.Any, 8888); // Настройка сервера
Thread? listenThread; // Поток для прослушивания

try
{
    listenThread = new Thread(new ThreadStart(server.Listen));
    listenThread.Start(); // Старт потока
}
catch (Exception ex)
{
    server.Disconnect();
    Console.WriteLine(ex.Message);
}