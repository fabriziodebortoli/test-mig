
using Microarea.RSWeb.Models;
namespace Microarea.RSWeb.Models
{    
    public struct Message
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string message { get; set; }

        public string page { get; set; }

    }

    public class NamespaceMessage
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string nameSpace { get; set; }
        public string parameters { get; set; }
        public string authtoken { get; set; }
    }

    public class AskDialogElement
    {
        public string id { get; set; }
        public string value { get; set; }
    }
 
}