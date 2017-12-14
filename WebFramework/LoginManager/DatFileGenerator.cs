using System;
using System.IO;


namespace Microarea.WebServices.LoginManager
{
	/// <summary>
	/// DatFileGenerator.
	/// </summary>
	//=========================================================================
	internal class DatFileGenerator
	{
		private static Random rnd ;
		//Lunghezza massima e minima in byte
		private const int maxLength = 10003;
		private const int minLength = 9001;

		//---------------------------------------------------------------------
		private DatFileGenerator()
		{}

		//---------------------------------------------------------------------
		static DatFileGenerator()
		{
			rnd = new Random(DateTime.Now.Millisecond);
		}

		//---------------------------------------------------------------------
		internal static MemoryStream GetDatFileStream()
		{
			MemoryStream datFile	= new MemoryStream();
			int			 dimInBytes = GetDatFileLength();

			for (int i = 0; i < dimInBytes; i++)
				datFile.WriteByte(Convert.ToByte(rnd.Next(0, 255)));

			datFile.Close();
			return datFile;
		}

		//---------------------------------------------------------------------
		internal static byte[] GetDatFileBytes()
		{
			return GetDatFileStream().ToArray();
		}

		//---------------------------------------------------------------------
		internal static int GetDatFileLength()
		{
			return rnd.Next(minLength, maxLength);
		}

		//---------------------------------------------------------------------
		internal static bool IsLengthValid(long val)
		{
			return val > minLength && val < maxLength;
		}
	}
}
