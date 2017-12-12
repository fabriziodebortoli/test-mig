using System;

namespace Microarea.Library.Internet.BitsWrap
{
	public class BITSFile
	{
		IBackgroundCopyFile m_File;

		internal BITSFile(IBackgroundCopyFile jobFile)
		{
			this.m_File = jobFile;
		}

		public FileProgress Progress
		{
			get
			{
				_BG_FILE_PROGRESS tmpFileProgress;
				m_File.GetProgress(out tmpFileProgress);
				return new FileProgress(tmpFileProgress);
			}
		}

		public string LocalName
		{
			get
			{
				string tmpLocalName;
				m_File.GetLocalName(out tmpLocalName);
				return tmpLocalName;
			}
		}

		public string RemoteName
		{
			get
			{
				string tmpRemoteName;
				m_File.GetRemoteName(out tmpRemoteName);
				return tmpRemoteName;
			}
		}
	}
}