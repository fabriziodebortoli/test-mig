﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RCToJson
{
	internal class MyJObject : List<MyJProperty>
	{
		public object this[string name]
		{
			get
			{
				foreach (var item in this)
				{
					if (item.Name == name) return item.Value;
				}
				return null;
			}
			set
			{
				foreach (var item in this)
				{
					if (item.Name == name)
					{
						if (value == null)
							Remove(item);
						else
							item.Value = value;
						return;
					}
				}
				if (value != null)
				{
					MyJProperty prop = new MyJProperty();
					prop.Name = name;
					prop.Value = value;
					Add(prop);
				}
			}
		}
		internal static MyJObject Parse(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();//all'inizio mi posiziono sullo start object
			}
			MyJObject obj = new MyJObject();
			MyJProperty currentProp = null;
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.PropertyName:
						currentProp = new MyJProperty();
						currentProp.Name = reader.Value.ToString();
						obj.Add(currentProp);
						break;
					case JsonToken.StartObject:
                        if (currentProp != null)
						    currentProp.Value = MyJObject.Parse(reader);
						break;
					case JsonToken.EndObject:
						return obj;
					case JsonToken.StartArray:
						currentProp.Value = MyJArray.Parse(reader);
						break;
					case JsonToken.Comment:
						currentProp.Comment = reader.Value.ToString();
						break;
					default:
						currentProp.Value = reader.Value;
						break;

				}
			}
			return obj;
		}

		internal void ToString(JsonTextWriter jtw)
		{
			jtw.WriteStartObject();
			foreach (MyJProperty prop in this)
			{
				prop.ToString(jtw);
			}
			jtw.WriteEndObject();
		}

        internal WndObjType GetWndObjType()
		{
			object t = this["type"];
			if (t == null)
				return WndObjType.Undefined;
			if (t is Int32)
				return (WndObjType)t;
            if (t is Int64)
				return (WndObjType)Convert.ToInt32(t);
			if (t is string)
				return (WndObjType)Enum.Parse(typeof(WndObjType), (string)t, true);
			throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
		}
	}

}