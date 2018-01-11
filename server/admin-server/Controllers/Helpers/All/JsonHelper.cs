using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microarea.AdminServer.Controllers.Helpers.All
{
	//================================================================================
	public interface IJsonHelper
	{
		void Init();
		void AddPlainObject<T>(T val);
		void AddJsonCouple<T>(string name, T val);
		string WriteFromKeysAndClear();
		string WritePlainAndClear();
	}

	//================================================================================
	public class JsonHelper : IJsonHelper
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

		//---------------------------------------------------------------------
		public void AddPlainObject<T>(T val)
		{
			try
			{
				this.plainObject = (T)val;
			}
			catch (Exception)
			{ }
		}

		//---------------------------------------------------------------------
		public void AddJsonCouple<T>(string name, T val)
		{
			try
			{
				this.entries.Add(name, val);
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
	}
}
