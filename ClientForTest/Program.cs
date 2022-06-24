// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using CommunicationLibrary;
using CommunicationLibrary.Crypt;
using Newtonsoft.Json;

string host = "127.0.0.1";
int port = 8888;
TcpClient? client;
NetworkStream? stream = null;

client = new TcpClient();

try
{
    client.Connect(host, port); //подключение клиента
    stream = client.GetStream(); // получаем поток
    Console.WriteLine("Соединение установлено");

    // устанавливаем зашифрованный канал
    Communication.StartClientEncrypt(stream);

    // запускаем новый поток для получения данных
    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessageThread));
    receiveThread.Start(); //старт потока
    Console.WriteLine("Поток для входящих сообщений запущен");

    Console.WriteLine("Введите сообщение: ");

    while (true)
    {
        //testStruct newStr = new testStruct();
        JsonPacket packet = new JsonPacket("Test", "", Console.ReadLine());
        Communication.SendMessage(JsonConvert.SerializeObject(packet), stream);
        //stream.Write((byte*)newStr, 1, 1);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
finally
{
    if (stream != null)
        stream.Close();//отключение потока
    if (client != null)
        client.Close();//отключение клиента
    Environment.Exit(0); //завершение процесса
}

void ReceiveMessageThread()
{
    while (true)
    {
        Console.WriteLine(Communication.ReceiveMessage(stream));
    }
}