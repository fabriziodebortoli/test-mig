using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers
{
    public class JsonHelper
    {
        StringBuilder sb;
        StringWriter sw;
        JsonWriter jsonWriter;

        public JsonHelper()
        {
            cleanAll();
        }

        public void Init()
        {
            cleanAll();
        }

        public void AddJsonObject(string name, object objToSerialize)
        {
            try
            {
                this.jsonWriter.WritePropertyName(name);
                this.jsonWriter.WriteRaw(JsonConvert.SerializeObject(objToSerialize));
            }
            catch (Exception)
            { }
        }

        public void AddJsonCouple<T>(string name, T val)
        {
            try
            {
                this.jsonWriter.WritePropertyName(name);
                this.jsonWriter.WriteValue(val);
            }
            catch (Exception)
            {}
        }

        public string WriteAndClear()
        {
            try
            {
                return this.sb.ToString();
            }
            catch (Exception)
            {
            }
            finally
            {
                this.sb.Clear();
                this.sw = new StringWriter(sb);
            }

            return String.Empty;            
        }

        void cleanAll()
        {
            this.sb = new StringBuilder();
            this.sw = new StringWriter(sb);
            this.jsonWriter = new JsonTextWriter(sw);
            this.jsonWriter.Formatting = Formatting.Indented;
        }

    }
}
