using System;
using System.IO;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class BinaryParserException : Exception 
	{
		public BinaryParserException(string message)
			: base(message)
		{ 
		}

	}
	//================================================================================
	public class BinaryParser : IDisposable
	{
		private Stream stream;
		private const int intLen = 4; //sizeof(uint)

		public bool EOF { get { return stream.Position == stream.Length; } }
		public long Position { get { return stream.Position; } }

		//--------------------------------------------------------------------------------
		public BinaryParser(Stream stream)
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
			int len = Convert.ToInt32(ParseInt());
			if (len == 0)
				return string.Empty;

			byte[] buff = new byte[len];
			if (stream.Read(buff, 0, len) != len)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(string)));

			return Encoding.UTF8.GetString(buff, 0, len);

		}

		//--------------------------------------------------------------------------------
		public void UnparseString(string s)
		{
			byte[] buff = Encoding.UTF8.GetBytes(s);
			UnparseInt(Convert.ToInt32(buff.Length));
			stream.Write(buff, 0, buff.Length);
		}
		//--------------------------------------------------------------------------------
		public string ParseAnsiString()
		{

			try
			{
				int len = Convert.ToInt32(ParseInt());
				if (len == 0)
					return string.Empty;

				byte[] buff = new byte[len];
				if (stream.Read(buff, 0, len) != len)
					throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(string)));

				return Encoding.Default.GetString(buff, 0, len);
			}
			catch (Exception)
			{
				
				throw;
			}
		}
		//--------------------------------------------------------------------------------
		public void UnparseAnsiString(string s)
		{
			byte[] buff = Encoding.Default.GetBytes(s);
			UnparseInt(Convert.ToInt32(buff.Length));
			stream.Write(buff, 0, buff.Length);
		}

		//--------------------------------------------------------------------------------
		public bool ParseBool()
		{
			byte[] buff = new byte[1];
			if (stream.Read(buff, 0, 1) != 1)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(bool)));

			return BitConverter.ToBoolean(buff, 0);
			
		}
		//--------------------------------------------------------------------------------
		public void UnparseBool(bool b)
		{
			stream.Write(BitConverter.GetBytes(b), 0, 1);
		}
		//--------------------------------------------------------------------------------
		public int ParseInt()
		{
			byte[] buff = new byte[intLen];
			if (stream.Read(buff, 0, intLen) != intLen)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(int)));

			return BitConverter.ToInt32(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseInt(int n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, intLen);
		}
		
		//--------------------------------------------------------------------------------
		public uint ParseUInt()
		{
			byte[] buff = new byte[intLen];
			if (stream.Read(buff, 0, intLen) != intLen)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(uint)));

			return BitConverter.ToUInt32(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseUInt(uint n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, intLen);
		}

		//--------------------------------------------------------------------------------
		public Int16 ParseInt16()
		{
			byte[] buff = new byte[2];
			if (stream.Read(buff, 0, 2) != 2)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(Int16)));

			return BitConverter.ToInt16(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseInt16(Int16 n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, 2);
		}

		//--------------------------------------------------------------------------------
		public Int32 ParseInt32()
		{
			byte[] buff = new byte[4];
			if (stream.Read(buff, 0, 4) != 4)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(Int32)));

			return BitConverter.ToInt32(buff, 0);
		}

		//--------------------------------------------------------------------------------
		public void UnparseInt32(Int32 n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, intLen);
		}
		//--------------------------------------------------------------------------------
		public byte ParseByte()
		{
			int b;
			if ((b = stream.ReadByte()) == -1)
				throw new BinaryParserException(string.Format(GenericStrings.ParseError, typeof(byte)));

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

		//--------------------------------------------------------------------------------
		public byte[] ParseBytes (int count)
		{
			byte[] buff = new byte[count];
			stream.Read(buff, 0, count);
			return buff;
		}
		#region IDisposable Members

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			if (stream != null)
				stream.Close();
		}

		#endregion



		
	}
}
