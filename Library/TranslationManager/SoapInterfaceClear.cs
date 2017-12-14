using System;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;

namespace Microarea.Library.TranslationManager
{
	
	public class SoapInterfaceClear: BaseTranslator
	{
		protected TranslationManager	tm;

		public SoapInterfaceClear()
		{
		}

		public override string ToString()
		{
			return "SaopInterface.cpp Clear";
		}

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			SetProgressMessage("	Cerco i Files ");
			SeachFiles();
			EndRun(false);
		}

		//---------------------------------------------------------------------------
		public void SeachFiles()
		{
			if(tm == null || tm.GetApplicationInfo().PathFinder == null)
				return;

			string enumsFile = string.Empty;
			//Loop su ogni modulo dell'applizaione
			foreach(BaseModuleInfo mod in tm.GetApplicationInfo().Modules)
			{
				if (mod == null)
					return;
				
				DeleteWebMethods(mod);

				foreach(LibraryInfo lib in mod.Libraries)
				{
					string libraryPath = lib.FullPath;
					if (libraryPath == string.Empty)
						continue;

					if (!Directory.Exists(libraryPath))
						continue;
				
					string file = Path.Combine(libraryPath, "SoapInterface.cpp");
					if (!File.Exists(file))
						continue;

					File.Delete(file);
					File.Create(file);

				}
			}
		}

		//---------------------------------------------------------------------------
		private  void DeleteWebMethods(BaseModuleInfo mod)
		{
			string moduleObjPath = mod.GetModuleObjectPath();
			if (moduleObjPath == string.Empty || moduleObjPath == null)
				return;

			if (!Directory.Exists(moduleObjPath))
				return;

			moduleObjPath = Path.Combine(moduleObjPath , "WebMethods.xml");
				
			if (File.Exists(moduleObjPath))
			{
				File.Delete(moduleObjPath);
				XmlDocument doc = new XmlDocument();
				//Dichiarazione XML
				XmlDeclaration configDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
				if (configDeclaration != null)
					doc.AppendChild(configDeclaration);
			
				XmlElement elements = doc.CreateElement("FunctionObjects");
				if (elements == null)
					return;
				doc.AppendChild(elements);

				XmlElement functions = doc.CreateElement("Functions");
				if (functions == null)
					return;
				elements.AppendChild(functions);

				doc.Save(moduleObjPath);
			}
		}

	}
}
