using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Crypt
{
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
}
