using System;
using System.Xml;
using System.IO;
using System.Collections;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class ModuleParser : SolutionManagerItems
	{
		public ModuleParser()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, true);

			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				XmlNode nModule = CreaNodoModule(mi);

				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));

				foreach (LibraryInfo li in mi.Libraries)
				{
					CreaNodoLibrary(mi, nModule, li);
				}
			}

			EndRun(true);
		}

		public override string ToString()
		{
			return "Solution Parser";
		}

		private XmlNode CreaNodoModule(BaseModuleInfo mi)
		{
			XmlNode nRes = base.CreaNodoModule(mi.Name, true, true);

			string enumsFileName = mi.Path + @"\" + mi.Name + "Enums.h";
			string lenFileName = mi.Path + @"\" + mi.Name + "Len.h";
			string constFileName = mi.Path + @"\" + mi.Name + "Const.h";

			if (File.Exists(enumsFileName))
			{
				CreaNodoLookUp(nRes, mi.Name + "Enums.h", string.Empty, mi.Name);
			}

			if (File.Exists(lenFileName))
			{
				CreaNodoLookUp(nRes, mi.Name + "Len.h", string.Empty, mi.Name);
			}

			if (File.Exists(constFileName))
			{
				CreaNodoLookUp(nRes, mi.Name + "Const.h", string.Empty, mi.Name);
			}

			return nRes;
		}

		private XmlNode CreaNodoLibrary(BaseModuleInfo mi, XmlNode nModule, LibraryInfo li)
		{
			XmlNode nRes = base.CreaNodoLibrary(nModule, li.Name, true, li);
			CreaNodoMig_Net(transManager.GetApplicationInfo().Name + "." + mi.Name + "." + li.Name);

			DirectoryInfo di = new DirectoryInfo(li.FullPath);

			string libPath = li.FullPath;

			foreach (FileInfo fi in di.GetFiles())
			{
				if (string.Compare(fi.Extension, ".h", true) == 0	||
					string.Compare(fi.Extension, ".cpp", true) == 0	||
					string.Compare(fi.Extension, ".hrc", true) == 0	||
					string.Compare(fi.Extension, ".rc", true) == 0	)
				{
					CreaNodoLookUp(nRes, fi.Name, string.Empty, mi.Name);
				}
			}

			if (Directory.Exists(Path.Combine(di.FullName, "res")))
			{
				DirectoryInfo resDI = new DirectoryInfo(Path.Combine(di.FullName, "res"));
				
				foreach (FileInfo fi in resDI.GetFiles("*.bmp"))
				{
					CreaNodoLookUp(nRes, fi.Name, string.Empty, mi.Name);
				}
			}
			/*string libCppFileName = string.Format("{0}.cpp", li.Name);
			string libInterfaceFileName = string.Format("{0}Interface.cpp", li.Name);
			string libRcFileName = string.Format("{0}.rc", li.Name);
			

			if (File.Exists(Path.Combine(libPath, libCppFileName)))
			{
				string newFileName = string.Empty;
				//if (nRes.Attributes["target"].Value.ToString() != string.Empty)
					newFileName = nRes.Attributes["target"].Value.ToString() + ".cpp";
				CreaNodoLookUp(nRes, libCppFileName, newFileName, mi.Name);
			}

			if (File.Exists(Path.Combine(libPath, libInterfaceFileName)))
			{
				string newFileName = string.Empty;
				//if (nRes.Attributes["target"].Value.ToString() != string.Empty)
					newFileName = nRes.Attributes["target"].Value.ToString() + "interface.cpp";
				CreaNodoLookUp(nRes, libInterfaceFileName, newFileName, mi.Name);
			}

			if (File.Exists(Path.Combine(libPath, libRcFileName)))
			{
				string newFileName = string.Empty;
				//if (nRes.Attributes["target"].Value.ToString() != string.Empty)
					newFileName = nRes.Attributes["target"].Value.ToString() + ".rc";
				CreaNodoLookUp(nRes, libRcFileName, newFileName, mi.Name);
			}*/

			SaveDocMigration();
			return nRes;
		}
	}
}
