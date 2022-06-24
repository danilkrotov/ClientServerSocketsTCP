using ClientServerSocketsTCP.Class;
using System.Net;

Server? server = new Server(IPAddress.Any, 8888); // сервер
Thread? listenThread; // поток для прослушивания

try
{
    listenThread = new Thread(new ThreadStart(server.Listen));
    listenThread.Start(); //старт потока
}
catch (Exception ex)
{
    server.Disconnect();
    Console.WriteLine(ex.Message);
}