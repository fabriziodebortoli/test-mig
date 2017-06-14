using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class JsonHelper
	{
		StringBuilder sb;
		StringWriter sw;
		JsonWriter jsonWriter;
		Dictionary<string, object> entries;
		object plainObject;

		//---------------------------------------------------------------------
		public JsonHelper()
		{
			CleanAll();
		}

		//---------------------------------------------------------------------
		public void Init()
		{
			CleanAll();
		}

		public void AddPlainObject<T>(T val)
		{
			try
			{
				this.plainObject = (T)val;
				//this.jsonWriter.WritePropertyName(name);
				//this.jsonWriter.WriteValue(val);
			}
			catch (Exception)
			{ }
		}

		public void AddJsonCouple<T>(string name, T val)
		{
			try
			{
				this.entries.Add(name, val);
				//this.jsonWriter.WritePropertyName(name);
				//this.jsonWriter.WriteValue(val);
			}
			catch (Exception)
			{ }
		}

		//---------------------------------------------------------------------
		bool IsSimple(object obj)
		{
			TypeInfo type = obj.GetType().GetTypeInfo();

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				// nullable type, check if the nested type is simple.
				return IsSimple((type.GetGenericArguments()[0]).GetTypeInfo());
			}
			return type.IsPrimitive
			  || type.IsEnum
			  || type.Equals(typeof(string))
			  || type.Equals(typeof(decimal));
		}
		//---------------------------------------------------------------------
		public string WriteFromKeysAndClear()
		{
			this.jsonWriter.WriteStartObject();
			object kObj;

			foreach(KeyValuePair<string, object> kvp in this.entries)
			{
				kObj = kvp.Value;

				if (!IsSimple(kObj))
				{
					this.jsonWriter.WritePropertyName(kvp.Key);
					this.jsonWriter.WriteRawValue(JsonConvert.SerializeObject(kObj));
					continue;
				}

				this.jsonWriter.WritePropertyName(kvp.Key);
				this.jsonWriter.WriteValue(kvp.Value);
			}

			this.jsonWriter.WriteEndObject();

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

		//---------------------------------------------------------------------
		public string WritePlainAndClear()
		{
			if (this.plainObject == null)
				return String.Empty;

			this.jsonWriter.WriteRawValue(JsonConvert.SerializeObject(this.plainObject));

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

		//---------------------------------------------------------------------
		void CleanAll()
		{
			this.entries = new Dictionary<string, object>();
			this.sb = new StringBuilder();
			this.sw = new StringWriter(sb);
			this.jsonWriter = new JsonTextWriter(sw);
			this.jsonWriter.Formatting = Formatting.Indented;
		}

		//---------------------------------------------------------------------
		public void Read(string jsonText)
		{
			CleanAll();

			JObject jObject = null;

			try
			{
				jObject = JObject.Parse(jsonText);

				foreach (JProperty property in jObject.Properties())
				{
					JToken tok = property.Value;
					string tagName = tok.Path;
					JTokenType tagType = tok.Type;
					object tagValue = ((JValue)tok).Value;

					switch (tagType)
					{
						case JTokenType.Integer:
							AddJsonCouple<int>(tagName, (int)tagValue);
							break;
						case JTokenType.Float:
							AddJsonCouple<float>(tagName, (float)tagValue);
							break;
						case JTokenType.String:
							AddJsonCouple<string>(tagName, (string)tagValue);
							break;
						case JTokenType.Boolean:
							AddJsonCouple<bool>(tagName, (bool)tagValue);
							break;
						case JTokenType.Date:
							AddJsonCouple<DateTime>(tagName, (DateTime)tagValue);
							break;
						case JTokenType.Guid:
							AddJsonCouple<Guid>(tagName, (Guid)tagValue);
							break;
						case JTokenType.Undefined:
						case JTokenType.Null:
						case JTokenType.None:
						case JTokenType.Object:
						case JTokenType.Array:
						case JTokenType.Constructor:
						case JTokenType.Property:
						case JTokenType.Comment:
						case JTokenType.Raw:
						case JTokenType.Bytes:
						case JTokenType.Uri:
						case JTokenType.TimeSpan:
						default:
							break;
					}
				}

				/*JEnumerable<JToken> children = jObject.Children<JToken>();

				foreach (JToken item in children)
				{
					string p = item.Path;
					JTokenType t = item.Type;
					JProperty prop = ((JProperty)item);
					JToken tok = prop.Value;
					JValue va = ((JValue)tok);
					object sva = va.Value;

					//JValue val = ((JValue)((JProperty)item).Value).Value;
				}*/
			}
			catch (JsonReaderException)
			{
				throw;
			}
		}
	}
}
