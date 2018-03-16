using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Common.Applications
{
    public class MessageJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string strValue = value.ToString();

            if (String.IsNullOrWhiteSpace(strValue))
            {
                // {}
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else
            {   // scrive {..object..}  e non "{..object..}"
                writer.WriteRawValue(strValue);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(string).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return false; }
        }
    }


    public struct Message
    {
        public MessageBuilder.CommandType commandType { get; set; }

        [JsonConverter(typeof(MessageJsonConverter))]
        public string message { get; set; }

        public string page { get; set; }
    }

  
    public class MessageBuilder
    {
        //Tenere allineato con ...\Standard\web\client\web-form\src\app\reporting-studio\reporting-studio.model.ts
        public enum CommandType { WRONG, NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, UPDATEASK, ABORTASK, DATA, STOP, RUNREPORT, ENDREPORT, NONE, PREVASK, RERUN, EXPORTEXCEL, EXPORTDOCX, SNAPSHOT}
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
    public class NamespaceMessage
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string nameSpace { get; set; }
        public string parameters { get; set; }
        public string authtoken { get; set; }
        public string tbLoaderName { get; set; }
        //used to communicate to caller document in case this report is a document's report        
        public string componentId { get; set; }
        [JsonProperty(Required = Required.AllowNull)]
        public ReportSnapshot snapshot { get; set; }
    }

    public class ReportSnapshot
    {
        public string name { get; set; }
        public string date { get; set; }
        public bool allUsers { get; set; }
    }
}
