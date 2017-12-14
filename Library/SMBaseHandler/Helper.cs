using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace Microarea.Library.SMBaseHandler
{
	//=========================================================================
	public class Helper
	{
		
		//---------------------------------------------------------------------
		public Helper()
		{}

		//---------------------------------------------------------------------
		public static string Evaluate(byte[] bytes)
		{
			Helper h  = new Helper();
			return h.Estimate(bytes);
		}

		//---------------------------------------------------------------------
		public static byte[] GetFileAsBytes(string filePath)
		{
			Stream stream = null;
			if (!File.Exists(filePath))
				return null;
			stream = File.OpenRead(filePath);
			long len = stream.Length;
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int)len);
			stream.Close();
			return buffer;
		}
		
		//---------------------------------------------------------------------
		public static bool HasExactExtension(string aValue)
		{ 
			if (aValue == string.Empty || aValue == null)
				return false;
			string extension = Path.GetExtension(aValue);
			if (extension == string.Empty || extension == null)
				return false;
			return String.Compare(extension, Common.SMExtension, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		private string Estimate(byte[] source)
		{
			if (source == null || source.Length == 0)
				return String.Empty;
			
			char ini = Convert.ToChar(source[0]);
			char fin = Convert.ToChar(source[source.Length -1]);
			int iVal = (int)ini;
			int fVal = (int)fin;
			int cfr = Math.Max(iVal, fVal) - Math.Min(iVal, fVal);
			if (cfr == 0)
				cfr = source.Length;
			string dest = string.Empty;   
			for (int i = 1 ; i < source.Length - 1 ; i++)
			{
				char c = Convert.ToChar(source[i]);
				int res = 0;
				
				c = (char)(((int) c) + (cfr / Math.DivRem(source.Length, i, out res)));
				dest += c;
			}
			ArrayList l = new ArrayList();
			l.AddRange(dest.ToCharArray());
			l.Reverse();
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append((char[])l.ToArray(typeof(char)));
			return sb.ToString();
		}

	}
}
