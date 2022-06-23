# ClientServerSocketsTCP

Проба пера клиент-серверного приложения на сокетах TCP

## Клиент
Клиент создан для ознакомительных целей

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
    \\
    Communication.SendMessage(JsonConvert.SerializeObject(packet), stream);
```
