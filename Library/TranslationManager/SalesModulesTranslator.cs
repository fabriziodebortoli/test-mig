using System;
using System.Diagnostics;
using System.Xml;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.TranslationManager
{
	public class SalesModulesTranslator : XmlTranslator
	{
		public SalesModulesTranslator()
		{
		}

		//---------------------------------------------------------------------------
		public override string ToString()
		{
			return "Sales Module Translator";
		}

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			BaseModuleInfo mi = (BaseModuleInfo) tm.GetNewApplicationInfo().Modules[0];

			if (mi != null)
			{
				SetProgressMessage("	Traduzione file di attivazione ");
				TranslateActivation			(mi);
			}

			EndRun(false);
		}

		//---------------------------------------------------------------------------
		private bool TranslateActivation(BaseModuleInfo mi)
		{
			FileInfo[] solutionFiles = mi.PathFinder.GetSolutionFiles();

			foreach (FileInfo solutionFile in solutionFiles)
				TranslateNamespace(solutionFile.FullName, "//SalesModule/@name");
			
			string solutionModulePath = Path.Combine(
				mi.PathFinder.GetStandardPath(), 
				string.Concat(NameSolverStrings.Solutions, Path.DirectorySeparatorChar, NameSolverStrings.Modules)); // pre-TB2.7 dir structure
			string[] solutionModuleFiles = Directory.GetFiles(solutionModulePath, "*.xml");

			foreach (string solutionModuleFile in solutionModuleFiles)
			{
				TranslateNamespace(solutionModuleFile, 
					new string[] {	"SalesModule/SalesModuleDependency/@name", 
									 "SalesModule/Application/@name",
									 "SalesModule/Application/Module/@name",
									 "SalesModule/Application/Functionality/@name"});

				FileInfo fi = new FileInfo(solutionModuleFile);
				string newFileName = solutionModulePath + "\\" + tm.GetNameSpaceConversion(LookUpFileType.SalesModule, fi.Name.Replace(".xml", "")) + ".xml";
				
				if (string.Compare(solutionModuleFile, newFileName, true) == 0)
					continue;

				try
				{
					fi.CopyTo(newFileName , true);
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
					SetLogError(exc.Message, ToString());
				}
			}

			string licPath = mi.PathFinder.GetLogManAppDataPath();
			string[] licensedFiles = Directory.GetFiles(licPath, "*.licensed.config");

			foreach (string licensedFile in licensedFiles)
				TranslateNamespace(licensedFile, "//SalesModule/@name");

			return true;
		}
	}	
}
