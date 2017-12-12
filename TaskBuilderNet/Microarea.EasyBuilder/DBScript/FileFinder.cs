using System;
using System.IO;
using System.Text;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for SearchFiles.
	/// </summary>
	//================================================================================
	public class FileFinder
	{
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public FileFinder()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Find(string Folder, string Pattern, string TextToSearch1)
		{
			return Find(Folder, Pattern, TextToSearch1, string.Empty);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Find(string Folder, string Pattern, string TextToSearch1, string TextToSearch2)
		{
			return Find(Folder, Pattern, TextToSearch1, TextToSearch2, string.Empty);
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Find(string Folder, string Pattern, string TextToSearch1, string TextToSearch2, string TextToSearch3)
		{
			if (!Directory.Exists(Folder))
				return string.Empty;

			foreach (string f in Directory.GetFiles(Folder, Pattern))
			{
				if (FileContainsText(f, TextToSearch1, TextToSearch2, TextToSearch3))
					return f;
			}
			return string.Empty;
		}

		//-----------------------------------------------------------------------------
		private bool FileContainsText(string f, string TextToSearch1, string TextToSearch2, string TextToSearch3)
		{
			bool bFound = false;
			try
			{
				StreamReader textStream = new StreamReader(f, Encoding.Default);
				string l = string.Empty;

				while((l = textStream.ReadLine()) != null && !bFound)
				{
						bFound = l.IndexOf(TextToSearch1) != -1 && 
								 (TextToSearch2 == string.Empty || l.IndexOf(TextToSearch2) != -1) && 
								 (TextToSearch3 == string.Empty || l.IndexOf(TextToSearch3) != -1);
				}
				textStream.Close();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			return bFound;
		}
	}
}
