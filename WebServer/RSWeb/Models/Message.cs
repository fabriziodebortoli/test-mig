﻿using Newtonsoft.Json;

namespace RSWeb.Models
{
    public struct Message
    {

        public MessageBuilder.CommandType commandType { get; set; }
        public string message { get; set; }
        public string response { get; set; }
    }

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

        public static Message GetMessagFromJson(string jsonMsg)
        {
            Message msg = JsonConvert.DeserializeObject<Message>(jsonMsg);
            return JsonConvert.DeserializeObject<Message>(jsonMsg);
        }

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

