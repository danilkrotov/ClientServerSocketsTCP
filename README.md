# ClientServerSocketsTCP

Проба пера клиент-серверного приложения на сокетах TCP

## Клиент
Клиент создан для тестирования шифрования и в ознакомительных целях

- Клиент автоматически подключается при запуске
- Клиент получает сообщение вместе с отправкой своего сообщения

Для общения с сервером необходимо инициализировать шифрование
```c#
Communication.StartClientEncrypt(stream);
```
Для отправки сообщений необходимо использовать класс JsonPacket
```c#
public class JsonPacket
{
    /// <summary>
    /// Заголовок (string)
    /// </summary>
    public string Header { get; set; }
    /// <summary>
    /// Данные авторизации (json)
    /// </summary>
    public string AuthData { get; set; }
    /// <summary>
    /// Сообщение (json)
    /// </summary>
    public string Message { get; set; }
}
```
Header - Необходимо прописать текстовый заголовок для понимания сервером что делать с сообщением
AuthData - Служит для отправки json строки с логином и хэшем пароля
Message - Служит для передачи объекта на сервер в формате json

## Сервер
Расшифровывает JsonPacket, предполагает работу в блоке switch case
```c#
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
```
