﻿using Newtonsoft.Json;

namespace Microarea.RSWeb.Models
{  
    public class MessageBuilder
    {
        //Tenere allineato con ...\Standard\web\client\web-form\src\app\reporting-studio\reporting-studio.model.ts
        public enum CommandType { WRONG, NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, UPDATEASK, DATA, STOP, RUNREPORT, ENDREPORT, NONE, HOTLINK }


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

