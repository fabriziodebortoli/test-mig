using Newtonsoft.Json;

namespace Microarea.RSWeb.Models
{  
    public class MessageBuilder
    {
        public enum CommandType { OK, NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, DATA, STOP }

        public MessageBuilder() { }


        public static string GetJSONMessage(CommandType cmdt, string message)
        {
            Message msg = new Message();
            msg.commandType = cmdt;
            msg.message = message;
            return JsonConvert.SerializeObject(msg);
        }

        public static string GetJSONMessage(Message msg)
        { 
            return JsonConvert.SerializeObject(msg);
        }
    }
}

