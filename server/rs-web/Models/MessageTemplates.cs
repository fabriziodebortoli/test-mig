
using Microarea.RSWeb.Models;
namespace Microarea.RSWeb.Models
{    
    public struct Message
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string message { get; set; }

        public int page { get; set; }

    }

    public class NamespaceMessage
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string nameSpace { get; set; }
        public string authtoken { get; set; }
    }
 
}