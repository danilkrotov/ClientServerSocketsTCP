// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using CommunicationLibrary;
using CommunicationLibrary.Crypt;

string host = "127.0.0.1";
int port = 8888;

TcpClient? client = new TcpClient();
NetworkStream? stream = null;
Communication? comm = null;
try
{
    client.Connect(host, port); // Подключение
    stream = client.GetStream(); // Получаем поток
    Console.WriteLine("Соединение установлено");

    comm = new Communication(stream); // Передаём в библиотеку CommunicationLibrary поток
    comm.StartClientEncrypt(); // Шифруем канал по RSA + AES
        
    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessageThread)); // Запускаем отдельный поток для получения данных
    receiveThread.Start();

    Console.WriteLine("Введите сообщение: ");
    // В бесконечном цикле отправляем сообщения на сервер с помощью Console.ReadLine()
    while (true)
    {
        JsonPacket packet = new JsonPacket("Test", "", Console.ReadLine()); // JsonPacket класс для общения с сервером
        comm.SendMessage(packet); // Отправляем данные на сервер
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    if (stream != null)
        stream.Close(); // Отключение потока
    if (client != null)
        client.Close(); // Отключение клиента
    Environment.Exit(0); // Завершение процесса
}

void ReceiveMessageThread()
{
    // В бесконечном цикле слушаем сервер и принимаем сообщения
    while (true)
    {
        Console.WriteLine(comm.ReceiveMessage());        
    }
}