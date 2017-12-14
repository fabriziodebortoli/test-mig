using System;
using System.IO;

using System.Web;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Crea un file temporaneo
	/// </summary>
	public  class FileProvider
	{
		private string			extension;
		private WoormDocument	woorm;

		private static long		counter = 0;	//mi serve per dare un nome univoco al file generato
		
		// file di appoggio (lo cancello nella dispose)
		private string			genericTmpFile			= string.Empty;
		
		
		//--------------------------------------------------------------------------------
		public string GenericTmpFile
		{
			get
			{
				if (genericTmpFile == string.Empty)
				{
					lock(typeof(FileProvider))
					{
						string fileName;
					
						do
						{
							counter++;
							if (counter == long.MaxValue) counter = 0;

							fileName = counter.ToString();
					
							fileName = Path.ChangeExtension(fileName, extension);

							genericTmpFile = ImagesHelper.GetImagePath(fileName);
						} 
						while (File.Exists(genericTmpFile));

						woorm.Disposed += new EventHandler(Woorm_Disposed);
					}
				}

				return genericTmpFile;
			}
		}

		//--------------------------------------------------------------------------------
		void Woorm_Disposed (object sender, EventArgs e)
		{
			try
			{
				if (File.Exists(genericTmpFile))
					File.Delete(genericTmpFile);
			}
			catch
			{
				//non interferisco col processo di dispose, eventualmente verra` cancellato in seguito dall'application
			}
		}
		
		//--------------------------------------------------------------------------------
		public string GenericTmpFileRelPath 
		{
			get
			{
				if (GenericTmpFile == string.Empty)
					return string.Empty;

				string rootPath = HttpContext.Current.Server.MapPath(".");
				if (GenericTmpFile.IndexOf(rootPath) == -1)
					return string.Empty;

				return GenericTmpFile.Substring(rootPath.Length + 1);
			}
		}
	
		//--------------------------------------------------------------------------------
		public FileProvider(WoormDocument woorm, string extension)
		{
			this.extension = extension;
			this.woorm = woorm;
		}
	}
}
