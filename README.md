# ClientServerSocketsTCP (В разработке)

Проба пера клиент-серверного приложения на сокетах TCP
* Реализовано асимметричное и симметричное шифрование RSA + AES
* Библиотека CommunicationLibrary пересобрана в .NET Standart 2.0 в неё вложен Json.NET - Newtonsoft что позволит клиенту запускатся на Unity под Android и iOS

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
AuthData - Служит для отправки json строки с логином и хэшем пароля
Message - Служит для передачи объекта на сервер в формате json

## Сервер
Работа сервера вынесена в два метода Authentication для проверки логина и пароля и Actions для разбора заголовка и выполнения необходимого действия
```c#
    private bool Authentication(string authData)
    {
        //Предполагается использование объекта с данными авторизации
        //UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(AuthData);
        return true;
    }

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
```
## Библиотека CommunicationLibrary
Библиотека CommunicationLibrary создана для использования одних и тех же классов на клиенте и сервере. Осуществляет отправку сообщения на сервер с помощью метода
```c#
Communication.SendMessage(JsonConvert.SerializeObject(jsonPacket), Stream);
```
и получения информации от сервера
```c#
string data = Communication.ReceiveMessage(Stream);
```
используя json формат.
Библиотека сконфигурирована для работы на Unity 3D при Build'e на Android или iOS (для этого необходимо добавить CommunicationLibrary.dll в проект Unity)
