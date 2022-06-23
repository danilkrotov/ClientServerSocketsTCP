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
}
