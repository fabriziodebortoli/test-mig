using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    public class JsonUtilities
    {
        public static NameValueCollection InputStreamJsonToNameValueCollection(Stream stream)
        {
            NameValueCollection inputObjects = new NameValueCollection();

            // Fetch list of objects from input stream.
            try
            {
                //stream.Seek(0, SeekOrigin.Begin);
                JsonTextReader jsonReader = new JsonTextReader(new StreamReader(stream));
                while (jsonReader.Read())
                {
                    // Not interested in nested objects/arrays!
                    if (jsonReader.Depth > 1)
                        jsonReader.Skip();
                    else if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        string key = jsonReader.Value.ToString();
                        if (jsonReader.Read())
                            switch (jsonReader.TokenType)
                            {
                                case JsonToken.Boolean:
                                case JsonToken.Date:
                                case JsonToken.Float:
                                case JsonToken.Integer:
                                case JsonToken.Null:
                                case JsonToken.String:
                                    inputObjects[key] = jsonReader.Value.ToString();
                                    break;
                            }
                    }
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return inputObjects;
        }
    }
}
