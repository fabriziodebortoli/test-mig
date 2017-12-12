using System;
using System.Collections;
using System.Xml;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class DataFileParser : SolutionManagerItems
	{
		public DataFileParser()
		{
			defaultLookUpType = LookUpFileType.Misc;
		}

		public override string ToString()
		{
			return "Datafile parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));

				string datafileFolderName = Path.Combine(Path.Combine(mi.Path, "DataManager"), "DataFile");
				if (!Directory.Exists(datafileFolderName))
					continue;

				XmlNode nModule = CreaNodoModule(mi.Name, true, false);

				DirectoryInfo dfDI = new DirectoryInfo(datafileFolderName);
				foreach (DirectoryInfo di in dfDI.GetDirectories())
				{
					foreach (FileInfo fi in di.GetFiles("*.xml"))
					{
						string fileName = fi.Name.Substring(0, fi.Name.IndexOf("."));
						CreaNodoLookUp(nModule, fileName, string.Empty, mi.Name);
						XmlDocument xDoc = new XmlDocument();
						xDoc.Load(fi.FullName);
						foreach (XmlNode n in xDoc.SelectNodes("Auxdata/Header/Fieldtype"))
						{
							string fieldName = n.Attributes["name"].Value.ToString();
							CreaNodoLookUp(nModule, fileName, string.Empty, mi.Name);
						}
					}
				}
			}

			EndRun(true);
		}
	}
}
