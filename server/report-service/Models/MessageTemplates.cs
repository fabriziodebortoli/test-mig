
using System;
using Microarea.RSWeb.Models;
using Newtonsoft.Json;

namespace Microarea.RSWeb.Models
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

    public class NamespaceMessage
    {
        public MessageBuilder.CommandType commandType { get; set; }
        public string nameSpace { get; set; }
        public string parameters { get; set; }
        public string authtoken { get; set; }
        public string tbLoaderName { get; set; }
        //used to communicate to caller document in case this report is a document's report
        public string componentId { get; set; }
    }

    public class AskDialogElement
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class HotlinkDescr
    {
        public string ns { get; set; }
        public string filter { get; set; }
        public string selection_type { get; set; }
        public string name { get; set; }
    }

    public class Snapshot
    {
        public object[] pages;
    }

}