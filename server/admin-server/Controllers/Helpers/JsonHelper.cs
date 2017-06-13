﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers
{
	public class JsonHelper
	{
		StringBuilder sb;
		StringWriter sw;
		JsonWriter jsonWriter;
		Dictionary<string, object> entries;

		public JsonHelper()
		{
			cleanAll();
		}

		public void Init()
		{
			cleanAll();
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

		public string WriteAndClear()
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

		void cleanAll()
		{
			this.entries = new Dictionary<string, object>();
			this.sb = new StringBuilder();
			this.sw = new StringWriter(sb);
			this.jsonWriter = new JsonTextWriter(sw);
			this.jsonWriter.Formatting = Formatting.Indented;
		}

	}
}
