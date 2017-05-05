using Newtonsoft.Json;

namespace Microarea.RSWeb.Models
{  
    public class MessageBuilder
    {
        public enum CommandType { NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, UPDATEASK, DATA, STOP, RUNREPORT }

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

