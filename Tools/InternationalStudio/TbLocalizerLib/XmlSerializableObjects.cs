using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class SerializationConverter
	{
		
		//--------------------------------------------------------------------------------
		public static string[] ToStringArray(object[] objs)
		{
			try
			{
				string[] strings = new string[objs.Length];
				for (int i = 0; i < objs.Length; i++)
					strings[i] = objs[i].ToString();
				return strings;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
			}

			return new string[0];
		}

		//--------------------------------------------------------------------------------
		public static object[] ToObjectArray(string[] strings, Type type)
		{	
			try
			{
				object[] objs = new object[strings.Length];
				for (int i = 0; i < strings.Length; i++)
				{
					object[] args = new object[1];
					args[0] = strings[i];
					objs[i] = Activator.CreateInstance(type, args);
				}

				return objs;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
			}

			return new object[0];
		}
	}
	
	//========================================================================
	/// <summary>
	/// utilizzata per la serializzazione xml, all'interno della solution, delle estensioni
	/// che identificano i files xml
	/// </summary>
	public struct xml
	{
		public xml(string extension)
		{
			this.extension = extension;
		}

		public override string ToString()
		{
			return this.extension;
		}

		[XmlAttribute]
		public string extension;	
	}
	
	//========================================================================
	/// <summary>
	/// utilizzata per la serializzazione xml, all'interno della solution, degli elementi
	/// di progetto
	/// </summary>
	public struct project
	{
		public project(string path)
		{
			this.path = path;
		}

		public override string ToString()
		{
			return this.path;
		}

		[XmlAttribute]
		public string path;	
	}
		
	//========================================================================
	/// <summary>
	/// utilizzata per la serializzazione xml, all'interno della solution, dei lle estensioni
	/// che identificano i files xml
	/// </summary>
	public struct path
	{
		public path(string name)
		{
			this.name = name;
		}

		public override string ToString()
		{
			return this.name;
		}

		[XmlAttribute]
		public string name;	
	}

}
