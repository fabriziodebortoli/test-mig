using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	public interface IPingFileManager
	{
		//---------------------------------------------------------------------
		string[] ListFiles(string repository);
		string DownloadFile(string filePath);
		void DeleteFile(string filePath);
	}
}
