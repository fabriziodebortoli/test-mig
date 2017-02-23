﻿using Newtonsoft.Json;

namespace Microarea.RSWeb.Models
{  
    public class MessageBuilder
    {
        public enum CommandType { OK, NAMESPACE, DATA, TEMPLATE, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, PAUSE, STOP }

        private struct MessageInternal
        {

            public CommandType commandType { get; set; }
            public string message { get; set; }
            public string response { get; set; }
        }


        public MessageBuilder() { }


        public static string GetJSONMessage(CommandType cmdt, string message, string response = "")
        {
            Message msg = new Message();
            msg.commandType = cmdt;
            msg.message = message;
            msg.response = response;
            return JsonConvert.SerializeObject(msg);
        }

        public static string GetJSONMessage(Message msg)
        { 
            return JsonConvert.SerializeObject(msg);
        }
    }
}

