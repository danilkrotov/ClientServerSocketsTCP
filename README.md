# Simple ClientServerSocketsTCP

Клиент-серверное приложение на сокетах TCP
* Реализовано асимметричное и симметричное шифрование RSA + AES
* Сервер .NET 6 Console
* Библиотека CommunicationLibrary собрана .NET Standart 2.0 что позволит общаться с сервером с Andriod или iOS на Unity

## Клиент
Клиент создан для примера работы и автоматически подключается к серверу при запуске <br><br>

Использование:
```c#
TcpClient? client = new TcpClient();
client.Connect(host, port); // Подключение
stream = client.GetStream(); // Получаем поток

Communication comm = new Communication(stream); //Передаём библиотеке CommunicationLibrary поток
comm.StartClientEncrypt(); // Шифруем канал по RSA + AES
```
Отправка и получение сообщений:
```c#
// Отправка
comm.SendMessage(packet);
// Получение
comm.ReceiveMessage()
```
## Библиотека CommunicationLibrary
Для отправки и приёма сообщений необходимо использовать класс JsonPacket
```c#
public class JsonPacket
{
    /// <summary>
    /// Заголовок (string)
    /// </summary>
    public string Header;
    /// <summary>
    /// Данные авторизации (json)
    /// </summary>
    public string AuthData;
    /// <summary>
    /// Сообщение (json)
    /// </summary>
    public string Message;

    public JsonPacket(string header, string authData, string Message) 
    {
        this.Header = header;
        this.AuthData = authData;
        this.Message = Message;
    }
}
```
Header - Необходимо прописать текстовый заголовок для понимания сервером что делать с сообщением <br>
AuthData - Служит для отправки json строки с логином и хэшем пароля или других данных для аутентификации <br>
Message - Служит для передачи объекта на сервер в формате json

## Сервер
На стороне сервера аналогично клиенту должно быть выполнено:
```c#
сomm.StartServerEncrypt();
```
для каждого подключившегося клиента.<br><br>
Работа сервера вынесена в два метода Authentication для проверки логина и пароля и Actions для разбора заголовка и выполнения необходимого действия
```c#
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
            JsonPacket responseJsonPacket = new JsonPacket(jsonPacket.Header, null, "Test response, your message: " + jsonPacket.Message);
            Comm.SendMessage(responseJsonPacket);
            //Раскомментировать broadcast для тестовой рассылки данных всем клиентам
            //server.BroadcastMessage(responseJsonPacket, Id);
            break;
        default:
            Console.WriteLine("[" + Id + "] " + "Пришел пакет с именем: " + jsonPacket.Header + " такой пакет не был распознан");
            break;
    }
}
```
