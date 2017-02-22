
using Microarea.RSWeb.Models;
namespace Microarea.RSWeb.Models
{    
    public struct Message
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string message { get; set; }
        public string response { get; set; }
    }

    public class InitialMessage
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string nameSpace { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string company { get; set; }

    }
}