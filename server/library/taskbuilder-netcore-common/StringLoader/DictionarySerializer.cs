using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Microarea.Common.StringLoader
{
	//================================================================================
	public class DictionarySerializerException : Exception
	{
		public DictionarySerializerException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}
	}

	//================================================================================
	public class DictionaryBinaryFile : Hashtable
	{
		public const uint Version = 1;
		private string path;
		private List<DictionaryResourceIndexItem> resourceIndexItems = new List<DictionaryResourceIndexItem>();

		public string Path { get { return path; } } 
		
		//--------------------------------------------------------------------------------
		public DictionaryBinaryFile(string path)
		{
			this.path = path;
		}

		//--------------------------------------------------------------------------------
		public DictionaryBinaryFile()
			: this ("")
		{
		}
		
		//--------------------------------------------------------------------------------
		public void AddResourceIndexItem(string url, uint id)
		{
			resourceIndexItems.Add(new DictionaryResourceIndexItem(id, url));
		}

		//--------------------------------------------------------------------------------
		public void AddDictionaryStringBlock(string type, string id, string name, DictionaryStringBlock block)
		{
			DictionaryBinaryIndexItem item = new DictionaryBinaryIndexItem(type, id, name);
			object existing = this[item];
			if (existing == null)
			{
				this[item] = block;
				return;
			}
			
			DictionaryStringBlock existingBlock = existing as DictionaryStringBlock;
			foreach (DictionaryEntry de in block)
				existingBlock[de.Key] = de.Value; //TODO PERASSO gestione conflitti per traduzioni diverse???
		}

		//--------------------------------------------------------------------------------
		public void Unparse(DictionaryBinaryParser parser)
		{
			parser.Clear();
			parser.UnparseUInt(Version);
			parser.UnparseUInt(Convert.ToUInt32(resourceIndexItems.Count));
			foreach (DictionaryResourceIndexItem item in resourceIndexItems)
				item.Unparse(parser);
			
			parser.UnparseUInt(Convert.ToUInt32(this.Count));
			foreach (DictionaryEntry de in this)
			{
				DictionaryBinaryIndexItem item = de.Key as DictionaryBinaryIndexItem;
				item.Unparse(parser);
				DictionaryStringBlock block = de.Value as DictionaryStringBlock;
				block.Unparse(parser);
			}
		}

		//--------------------------------------------------------------------------------
		public void Parse(DictionaryBinaryParser parser)
		{
			uint localVersion = parser.ParseUInt();
			if (localVersion != Version)
				throw new DictionarySerializerException(StringLoaderStrings.InvalidVersion, Version, localVersion);

			uint count = parser.ParseUInt();
			for (int i = 0; i < count; i++)
			{
				DictionaryResourceIndexItem item = new DictionaryResourceIndexItem();
				item.Parse(parser);
				resourceIndexItems.Add(item);
			}
			count = parser.ParseUInt();
			for (int i = 0; i < count; i++)
			{
				DictionaryBinaryIndexItem item = new DictionaryBinaryIndexItem();
				item.Parse(parser);
				DictionaryStringBlock block = new DictionaryStringBlock();
				block.Parse(parser);

				this[item] = block;
			}

		}
		//--------------------------------------------------------------------------------
		public DictionaryStringBlock GetBlock(string type, string id, string name)
		{
			return GetBlock(new DictionaryBinaryIndexItem(type, id, name));
		}

		//--------------------------------------------------------------------------------
		public DictionaryStringBlock GetBlock(DictionaryBinaryIndexItem targetItem)
		{
			return this[targetItem] as DictionaryStringBlock;
		}
	}

	//================================================================================
	public class DictionaryResourceIndexItem
	{
		public uint Number;
		public string Url;

		//--------------------------------------------------------------------------------
		public DictionaryResourceIndexItem(uint number, string url)
		{
			this.Number = number;
			this.Url = Path.GetFileNameWithoutExtension(url).ToLower();
		}

		//--------------------------------------------------------------------------------
		public DictionaryResourceIndexItem()
			: this (0, string.Empty)
		{
		}

		//--------------------------------------------------------------------------------
		public void Parse(DictionaryBinaryParser parser)
		{
			Number = parser.ParseUInt();
			Url = parser.ParseString();
		}
		//--------------------------------------------------------------------------------
		public void Unparse(DictionaryBinaryParser parser)
		{
			parser.UnparseUInt(Number);
			parser.UnparseString(Url);
		}
	}

	//================================================================================
	[Serializable]
	public class DictionaryStringItem : Hashtable, ISerializable
	{
		private const string TargetTag = "target";
		public string Target  = string.Empty;
		public static explicit operator string(DictionaryStringItem item) { return item.Target; }
		//--------------------------------------------------------------------------------
		public DictionaryStringItem()
		{
		}

		//--------------------------------------------------------------------------------
		public DictionaryStringItem(SerializationInfo info, StreamingContext context)            
        //: base(info, context) TODO rsweb
        {
			Target = info.GetString(TargetTag);
		}
		#region ISerializable Members

		//--------------------------------------------------------------------------------
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			//base.GetObjectData(info, context);      TODO rsweb
			info.AddValue(TargetTag, Target);
		}

		#endregion
		//--------------------------------------------------------------------------------
		public void Parse(DictionaryBinaryParser parser)
		{
			Target = parser.ParseString();
			byte attrCount = parser.ParseByte();
			
			string attrName, attrValue; 
			for (int i = 0; i < attrCount; i++)
			{
				attrName = parser.ParseString();
				attrValue = parser.ParseString();
				this[attrName] = attrValue;
			}
			
		}
		//--------------------------------------------------------------------------------
		public void Unparse(DictionaryBinaryParser parser)
		{
			parser.UnparseString(Target);
			parser.UnparseByte(Convert.ToByte(this.Count));
			string attrName, attrValue; 
			foreach (DictionaryEntry de in this)
			{
				attrName = de.Key as string;
				attrValue = de.Value as string;
				parser.UnparseString(attrName);
				parser.UnparseString(attrValue);
			}
		}

		
	}

	//================================================================================
	[Serializable]
	public class DictionaryStringBlock : Hashtable
	{
		//--------------------------------------------------------------------------------
		public DictionaryStringBlock()
		{
		}

		//--------------------------------------------------------------------------------
		public DictionaryStringBlock(SerializationInfo info, StreamingContext context)
        //: base(info, context)                  TODO rsweb
        {
        }

		//--------------------------------------------------------------------------------
		public void AddStringItem(string baseString, DictionaryStringItem item)
		{
			this[baseString.Trim().Replace("\r\n", "\n")] = item;
		}

		//--------------------------------------------------------------------------------
		public void Parse(DictionaryBinaryParser parser)
		{
			uint count = parser.ParseUInt();
			
			for (int i = 0; i < count; i++)
			{
				string baseString = parser.ParseString();
				DictionaryStringItem item = new DictionaryStringItem();
				item.Parse(parser);
				this[baseString] = item;
			}
		}

		//--------------------------------------------------------------------------------
		public void Unparse(DictionaryBinaryParser parser)
		{
			parser.UnparseUInt(Convert.ToUInt32(this.Count));
			foreach (DictionaryEntry de in this)
			{
				string baseString = de.Key as string;
				DictionaryStringItem item = de.Value as DictionaryStringItem;
				parser.UnparseString(baseString);
				item.Unparse(parser);
			}
					
		}
	}

	//================================================================================
	public class DictionaryBinaryIndexItem
	{
		private uint	type;
		private string	id;
		private string	name;
		private static readonly string[] types = {"","databasescript","dialog","other","report","source","stringtable","xml","menu", "jsonforms"};


		public static string[]	Types	{ get { return types; } }
		public uint		Type			{ get { return type; } }
		public string	TypeDescription	{ get { return UIntToType(type); } }
		public string	Id				{ get { return id; } }
		public string	Name			{ get { return name; } }
		//--------------------------------------------------------------------------------
		public DictionaryBinaryIndexItem(string	type, string id, string name)
		{
			this.type = TypeToUInt(type);
			this.id = id.ToLower();
			this.name = name.ToLower();
		}
			
		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			DictionaryBinaryIndexItem target = obj as DictionaryBinaryIndexItem;
			if (target == null)
				return false;

			return this.type == target.type && this.id == target.id && this.name == target.name;
		}
		
		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return type.GetHashCode() + id.GetHashCode() + name.GetHashCode();
		}
		
		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return string.Format("{0} - {1} - {2}", type, id, name);
		}

		//--------------------------------------------------------------------------------
		public DictionaryBinaryIndexItem()
			: this("", "", "")
		{
		}
		//--------------------------------------------------------------------------------
		public static uint TypeToUInt(string type)
		{
			for (uint i = 0; i< Types.Length; i++) 
			   if (string.Compare(Types[i], type) == 0)
				   return i;
             
			Debug.Fail(string.Format("Invalid type: {0}", type));
			return 0;	
		}

		//--------------------------------------------------------------------------------
		public static string UIntToType(uint type)
		{
			if (type < Types.Length) 
				return Types[type];
			else
			{
			   Debug.Fail(string.Format("Invalid type: {0}", type));
			   return ""; 
			}
		}

		//--------------------------------------------------------------------------------
		public void Parse(DictionaryBinaryParser parser)
		{
			type	= parser.ParseUInt();
			id		= parser.ParseString();
			name	= parser.ParseString();	
		}

		//--------------------------------------------------------------------------------
		public void Unparse(DictionaryBinaryParser parser)
		{
			parser.UnparseUInt(type);
			parser.UnparseString(id);
			parser.UnparseString(name);
		}

		
	}

	//================================================================================
	public class DictionaryBinaryParser : IDisposable
	{
		private Stream stream;
		private const int numberLen = 4; //sizeof(uint)
		public bool EOF { get { return stream.Position == stream.Length; } }
		public long Position { get { return stream.Position; } }

		//--------------------------------------------------------------------------------
		public DictionaryBinaryParser(Stream stream)
		{
			this.stream = stream;
		}

		//--------------------------------------------------------------------------------
		public void Seek(int offset)
		{
			stream.Seek(offset, SeekOrigin.Begin);
		}

		//--------------------------------------------------------------------------------
		public string ParseString()
		{
			int len = Convert.ToInt32(ParseUInt());
			if (len == 0)
				return string.Empty;

			byte[] buff = new byte[len];
			if (stream.Read(buff, 0, len) != len)
				throw new DictionarySerializerException(StringLoaderStrings.InvalidString);
				
			return Encoding.UTF8.GetString(buff, 0, len);

		}

		//--------------------------------------------------------------------------------
		public void UnparseString(string s)
		{
			byte[] buff = Encoding.UTF8.GetBytes(s);
			UnparseUInt(Convert.ToUInt32(buff.Length));
			stream.Write(buff, 0, buff.Length);
		}
		//--------------------------------------------------------------------------------
		public int ParseInt()
		{
			byte[] buff = new byte[numberLen];
			if (stream.Read(buff, 0, numberLen) != numberLen)
				throw new DictionarySerializerException(StringLoaderStrings.InvalidInteger);

			return BitConverter.ToInt32(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseInt(int n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, numberLen);
		}
		//--------------------------------------------------------------------------------
		public uint ParseUInt()
		{
			byte[] buff = new byte[numberLen];
			if (stream.Read(buff, 0, numberLen) != numberLen)
				throw new DictionarySerializerException(StringLoaderStrings.InvalidInteger);
		
			return BitConverter.ToUInt32(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseUInt(uint n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, numberLen);
		}

		//--------------------------------------------------------------------------------
		public byte ParseByte()
		{
			int b;
			if ((b = stream.ReadByte()) == -1)
				throw new DictionarySerializerException(StringLoaderStrings.InvalidInteger);
		
			return Convert.ToByte(b);
		}

		//--------------------------------------------------------------------------------
		public void UnparseByte(byte b)
		{
			stream.WriteByte(b);
		}
		//--------------------------------------------------------------------------------
		public void Clear()
		{
			stream.SetLength(0);
		}

		#region IDisposable Members

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			if (stream != null)
				stream.Dispose();
		}

		#endregion
	}
}
