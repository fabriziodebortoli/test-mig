using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DataManagerEngine
{
	/// <summary>
	/// SampleSelections (struttura in memoria con le opzioni scelte dall'utente)
	/// </summary>
	//=========================================================================
	public class SampleSelections : DefaultSelections
	{
		# region Costruttore
		//---------------------------------------------------------------------
		public SampleSelections(ContextInfo contextInfo, BrandLoader brandLoader) 
			: base (contextInfo, brandLoader)
		{}
		# endregion

		# region Caricamento file contenenti i dati di esempio e ricerca delle varie configurazioni
		//devo effettuare l'importazione dei dati di esempio in base alla lingua scelta dall'utente
		//---------------------------------------------------------------------------
		public void LoadSampleData()
		{
			if (SelectedConfiguration.Length == 0)
				return;

			if (SelectedIsoState.Length == 0)
				return;

			ArrayList moduleList = null;
			ArrayList fileList = new ArrayList();
			GetAllApplication();

			foreach (string appName in applicationList)
			{
				moduleList = new ArrayList(ContextInfo.PathFinder.GetModulesList(appName));
				
				foreach (ModuleInfo modInfo in moduleList)
					AddSampleFiles(appName, modInfo.Name, ref fileList);
			}
			
			StringCollection appendFiles = new StringCollection();
			foreach (FileInfo file in fileList)
			{
				// skippo i file con un nome che inizia con DeploymentManifest (visto che potrebbe essere
				// presente per esigenze di installazione di specifiche configurazioni dati di default/esempio 
				// (esigenza sorta principalmente con i partner polacchi - miglioria 3067)
				if (file.Name.StartsWith("DeploymentManifest"))
					continue;

				// devo inserire in coda quelli con suffisso Append
				if (Path.GetFileNameWithoutExtension(file.Name).EndsWith(DataManagerConsts.Append, StringComparison.InvariantCultureIgnoreCase))
					appendFiles.Add(file.FullName);
				else
					ImportSel.AddItemInImportList(file.FullName);
			}

			//inserisco quelli con suffisso Append
			foreach (string fileName in appendFiles)
				ImportSel.AddItemInImportList(fileName);
		}
		
		//---------------------------------------------------------------------------
		public void AddSampleFiles(string appName, string moduleName, ref ArrayList fileList)
		{	
			DirectoryInfo standardDir = new DirectoryInfo
				(
				ContextInfo.PathFinder.GetStandardDataManagerSamplePath(appName, moduleName, SelectedIsoState) +
				Path.DirectorySeparatorChar + 
				SelectedConfiguration
				);			
		
			DirectoryInfo customDir = new DirectoryInfo
				(
				ContextInfo.PathFinder.GetCustomDataManagerSamplePath(appName, moduleName, SelectedIsoState) +
				Path.DirectorySeparatorChar + 
				SelectedConfiguration
				);

			ContextInfo.PathFinder.GetXMLFilesInPath(standardDir, customDir, ref fileList);
		}
		
		//---------------------------------------------------------------------------
		public void GetSampleConfigurationList(ref StringCollection sampleConfigList, string countryName)
		{
			GetConfigurationList(ref sampleConfigList, NameSolverStrings.Sample, countryName);
			
			// se non esiste la l'edition dell'applicativo e sono in fase di generazione dei
			// dati, inserisco nella combo il nome
			if (mode == ModeType.EXPORT && !sampleConfigList.Contains(SelectedConfiguration))
				sampleConfigList.Add(SelectedConfiguration);
		}		
		# endregion
	}	
}