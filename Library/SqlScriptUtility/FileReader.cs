using System.IO;
using System.Xml;

namespace Microarea.Library.SqlScriptUtility
{
	/// <summary>
	/// Summary description for FileReader.
	/// </summary>
	public class FileReader
	{
		public delegate bool GetLatestVersion(string fileName);
		public event GetLatestVersion OnGetLatestVersion;

		public delegate bool AfterRead(string fileName);
		public event AfterRead OnAfterRead;
		
		public FileReader()
		{
		}

		public string ReadTxtFile(string fileName, bool getLatest)
		{
			string result = string.Empty;

			if (!BeforeReading(fileName, getLatest))
				return string.Empty;

			if (!CheckStatus(fileName))
				return string.Empty;

			try
			{
				FileStream fr = new FileStream(fileName, FileMode.Open, FileAccess.Read);
					
				for (int i = 1; i<=(int)fr.Length; i++)
					result += (char)fr.ReadByte();
				fr.Close();
			}
			catch
			{
				return string.Empty;
			}

			if (!AfterReading(fileName))
				return string.Empty;

			return result;
		}

		public XmlDocument ReadXmlFile(string fileName, bool getLatest)
		{
			XmlDocument result = null;

			if (!BeforeReading(fileName, getLatest))
				return null;

			if (!CheckStatus(fileName))
				return null;

			if (File.Exists(fileName))
			{
				result = new XmlDocument();
				result.Load(fileName);
			}

			if (!AfterReading(fileName))
				return null;

			return result;
		}

		public bool BeforeReading(string fileName, bool getLatest)
		{
			if (OnGetLatestVersion != null)
			{
				return OnGetLatestVersion(fileName);
			}

			return true;
		}

		public bool AfterReading(string fileName)
		{
			if (OnAfterRead != null)
			{
				return OnAfterRead(fileName);
			}

			return true;
		}

		public bool CheckStatus(string fileName)
		{
			return true;
		}
	}
}
