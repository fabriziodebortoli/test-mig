using System;

namespace Microarea.EasyBuilder
{
	class EBFileNotFoundException : Exception
	{
		private string metaDataFile;

		public string MetaDataFile
		{
			get { return metaDataFile; }
			set { metaDataFile = value; }
		}

		public EBFileNotFoundException(string metaDataFile)
		{
			this.metaDataFile = metaDataFile;
		}
	}
}
