using System.Collections;
using System.IO;
using System.Xml;

namespace Microarea.Library.SqlScriptUtility
{
	/// <summary>
	/// Summary description for FileWriter.
	/// </summary>
	public class FileWriter
	{
		private ArrayList fileToWrite = new ArrayList();

		public delegate bool BeforeWrite(string fileName);
		public event BeforeWrite OnBeforeWrite;

		public delegate bool AfterWrite(string fileName);
		public event AfterWrite OnAfterWrite;
		
		public FileWriter()
		{
		}

		public void AddTxtFile(string name, string txt, FileMode mode)
		{
			fileToWrite.Add(new TxtFile(name, txt, mode));
		}

		public void AddXmlFile(string name, XmlDocument document)
		{
			fileToWrite.Add(new XmlFile(name, document));
		}

		public bool BeforeSave()
		{
			
			bool bOk = true;

			if (OnBeforeWrite != null)
			{
				foreach (AllFile af in fileToWrite)
					bOk = bOk && OnBeforeWrite(af.FileName);
			}

			return bOk;
		}

		public bool AfterSave()
		{
			
			bool bOk = true;

			if (OnAfterWrite != null)
			{
				foreach (AllFile af in fileToWrite)
					bOk = bOk && OnAfterWrite(af.FileName);
			}

			return bOk;
		}

		public bool Save()
		{
			if (!BeforeSave())
				return false;

			if (!CheckStatus())
			{
				return false;
			}

			bool bOk = true;

			foreach (AllFile file in fileToWrite)
				bOk = bOk && file.Save();

			bOk = bOk & AfterSave();

			if (bOk)
				fileToWrite.Clear();

			return bOk;
		}

		public bool CheckStatus()
		{
			bool bOk = true;

			foreach (AllFile file in fileToWrite)
				bOk = bOk && file.CheckStatus();

			return bOk;
		}
	}

	public class TxtFile : AllFile
	{
		private string text = string.Empty;
		private FileMode writeMode = FileMode.Append;

		public TxtFile(string name, string txt, FileMode mode)
		{
			text = txt;
			FileName = name;
			writeMode = mode;
		}

		public override bool Save()
		{
			CreateDir();

			try
			{
				char[] textArray = text.ToCharArray();

				FileStream fw = new FileStream(FileName, writeMode, FileAccess.Write);
				for (int i = 1; i<=text.Length; i++)
					fw.WriteByte((byte) textArray[i-1]);
				fw.Close();
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void CreateDir()
		{
			int lenPath = FileName.LastIndexOf("\\");
			string fileDiractory = FileName.Substring(0, lenPath);
			if (!Directory.Exists(fileDiractory))
			{
				Directory.CreateDirectory(fileDiractory);
			}
		}

		public override bool CheckStatus()
		{
			if (!File.Exists(FileName))
				return true;

			if ((File.GetAttributes(FileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ||
				(File.GetAttributes(FileName) & FileAttributes.Hidden)   == FileAttributes.Hidden) 
			{
				return false;
			}

			return true;
		}
	}

	public class XmlFile : AllFile
	{
		private XmlDocument xDoc = null;

		public XmlFile(string name, XmlDocument document)
		{
			xDoc = document;
			FileName = name;
		}

		public override bool Save()
		{
			CreateDir();

			try
			{
				xDoc.Save(FileName);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void CreateDir()
		{
			int lenPath = FileName.LastIndexOf("\\");
			string fileDiractory = FileName.Substring(0, lenPath);
			if (!Directory.Exists(fileDiractory))
			{
				Directory.CreateDirectory(fileDiractory);
			}
		}

		public override bool CheckStatus()
		{
			if (!File.Exists(FileName))
				return true;

			if ((File.GetAttributes(FileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ||
				(File.GetAttributes(FileName) & FileAttributes.Hidden)   == FileAttributes.Hidden) 
			{
				return false;
			}

			return true;
		}
	}

	public class AllFile
	{
		public string FileName = string.Empty;

		public virtual bool Save()
		{
			return true;
		}
		
		public virtual bool CheckStatus()
		{
			return true;
		}
		
	}
}
