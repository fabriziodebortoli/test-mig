using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
//
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.RegressionTestLibrary
{
	# region Definizione classe RegressionTestManager
	/// <summary>
	/// Summary description for RegressionTestManager
	/// </summary>
	//=========================================================================
	public class RegressionTestManager
	{
		public RegressionTestSelections DataSelections = null;

		public	DatabaseDiagnostic		DBDiagnostic	= new DatabaseDiagnostic(); 
		private ContextInfo				Context			= null;
		private RegressionTestWizard	wizard			= null;

		# region Costruttore
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------------	
		public RegressionTestManager(ContextInfo context)
		{
			DataSelections = new RegressionTestSelections(context);
			Context = context;

			LoadConfiguration();
		}
		# endregion

		# region Distruttore
		/// <summary>
		/// Distruttore
		/// </summary>
		//---------------------------------------------------------------------------	
		~RegressionTestManager()
		{
		}
		# endregion

		# region Caricamento e salvataggio del file di configurazione
		//---------------------------------------------------------------------------	
		private void LoadConfiguration()
		{
			string settingsDirectory = Context.PathFinder.GetRegressionTestSettingsPath();
			if (!Directory.Exists(settingsDirectory))
				settingsDirectory = GetDefaultSettingsFolder();

			if (string.IsNullOrEmpty(settingsDirectory))
				return;

			string settingsFileName = Path.Combine(settingsDirectory, "RegressionTest.config");

			if (!File.Exists(settingsFileName))
				return;

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(settingsFileName);

			foreach (XmlNode n in xDoc.SelectNodes("ParameterSettings/Section/Setting"))
			{
				switch (n.Attributes["name"].Value)
				{
					case "TestRepositoryRoot":
						DataSelections.RepositoryPath = n.Attributes["value"].Value;
						break;
					case "WinZipPath":
						DataSelections.WinZipPath = n.Attributes["value"].Value;
						break;
				}
			}
		}

		//---------------------------------------------------------------------------	
		private void SaveConfiguration()
		{
			string settingsDirectory = Context.PathFinder.GetRegressionTestSettingsPath();
			
			if (string.IsNullOrEmpty(settingsDirectory))
				return;

			if (!Directory.Exists(settingsDirectory))
				Directory.CreateDirectory(settingsDirectory);

			XmlDocument xDoc = new XmlDocument();

			string settingsFileName = Path.Combine(settingsDirectory, "RegressionTest.config");

			xDoc.CreateXmlDeclaration("1.0", "UTF-8", "");
			XmlNode nMain = xDoc.CreateNode(XmlNodeType.Element, "ParameterSettings", "");
			
			XmlNode nSection = xDoc.CreateNode(XmlNodeType.Element, "Section", "");
			XmlAttribute aSectionName = xDoc.CreateAttribute("", "name", "");
			aSectionName.Value = "RegressionTest";
			nSection.Attributes.Append(aSectionName);
			
			XmlNode nSetting1 = xDoc.CreateNode(XmlNodeType.Element, "Setting", "");
			XmlAttribute aSetting1Name = xDoc.CreateAttribute("", "name", "");
			aSetting1Name.Value = "TestRepositoryRoot";
			XmlAttribute aSetting1Type = xDoc.CreateAttribute("", "type", "");
			aSetting1Type.Value = "string";
			XmlAttribute aSetting1Value = xDoc.CreateAttribute("", "value", "");
			aSetting1Value.Value = DataSelections.RepositoryPath;
			nSetting1.Attributes.Append(aSetting1Name);
			nSetting1.Attributes.Append(aSetting1Type);
			nSetting1.Attributes.Append(aSetting1Value);

			XmlNode nSetting2 = xDoc.CreateNode(XmlNodeType.Element, "Setting", "");
			XmlAttribute aSetting2Name = xDoc.CreateAttribute("", "name", "");
			aSetting2Name.Value = "WinZipPath";
			XmlAttribute aSetting2Type = xDoc.CreateAttribute("", "type", "");
			aSetting2Type.Value = "string";
			XmlAttribute aSetting2Value = xDoc.CreateAttribute("", "value", "");
			aSetting2Value.Value = DataSelections.WinZipPath;
			nSetting2.Attributes.Append(aSetting2Name);
			nSetting2.Attributes.Append(aSetting2Type);
			nSetting2.Attributes.Append(aSetting2Value);

			nSection.AppendChild(nSetting1);
			nSection.AppendChild(nSetting2);

			nMain.AppendChild(nSection);

			xDoc.AppendChild(nMain);
			
			xDoc.Save(settingsFileName);
		}

		//---------------------------------------------------------------------------	
		private string GetDefaultSettingsFolder()
		{
			IBaseApplicationInfo bai = Context.PathFinder.GetApplicationInfoByName("RegressionTest");
			if (bai == null)
				return string.Empty;

			IBaseModuleInfo bmi = bai.GetModuleInfoByName("Main");
			if (bmi == null)
				return string.Empty;

			string settingsDirectory = bmi.GetStandardSettingsPath();

			if (!Directory.Exists(settingsDirectory))
				return string.Empty;
			
			return settingsDirectory;
		}
		# endregion

		# region Apertura wizard aggiornamento
		/// <summary>
		/// Lancio del wizard di aggiornamento dati
		/// </summary>
		//---------------------------------------------------------------------------	
		public RegressionTestSelections RunRegressionTestWizard()
		{
			wizard = new RegressionTestWizard(DataSelections, DBDiagnostic);
			
			wizard.AddWizardPages();
			
			// aggancio eventi
			wizard.OnFinishWizard += new RegressionTestWizard.FinishWizard(wizard_OnFinishWizard);
			
			wizard.Run();

			return wizard.DataSelections;
		}
		# endregion
		
		/// <summary>
		/// fa lo Show dell'ExecutionForm
		/// </summary>
		//---------------------------------------------------------------------------
		public void wizard_OnFinishWizard()
		{
			SaveConfiguration();
			wizard.DataSelections.IsOk = true;
		}
	}
	# endregion

	# region Definizione classe DBUpdateProcess
	/// <summary>
	/// class DBUpdateProcess
	/// dove vengono richiamati (in sequenza e sulla base delle selezioni effettuate) le
	/// funzioni per effettuare un'aggiornamento completa di un database
	/// </summary>
	//=========================================================================
	public class DBUpdateProcess
	{
		private	RegressionTestSelections	dataSel;
		private DatabaseDiagnostic		dbDiagnostic;

		# region Costruttore
		//---------------------------------------------------------------------
		public DBUpdateProcess(RegressionTestSelections sel, DatabaseDiagnostic diagnostic)
		{
			dataSel			= sel;
			dbDiagnostic	= diagnostic;
		}
		# endregion

		# region Entry-point per il lancio dell'aggiornamento + thread separato
		/// <summary>
		/// entry-point per iniziare il processo vero e proprio di aggiornamento
		/// </summary>
		//---------------------------------------------------------------------
		public Thread StartDBUpdateProcess()
		{
			Thread t = new Thread(new ThreadStart(InternalDBUpdateProcess));
			t.Start();
			return t;
		}

		/// <summary>
		/// sul thread separato inizia il processo vero e proprio di 'aggiornamento
		/// </summary>
		//---------------------------------------------------------------------
		private void InternalDBUpdateProcess()
		{
			foreach (AreaItem aItem in dataSel.AreaItems.Values)
				foreach (DataSetItem dItem in aItem.DataSetItems.Values)
					ExpandZip(dItem);

			dbDiagnostic.SetFinish("Elaborazione Terminata!");
		}

		//---------------------------------------------------------------------
		public void ExpandZip(DataSetItem item)
		{
			string zipFileName = string.Format("{0}.xml.zip", item.Path);
			if (File.Exists(zipFileName))
			{
				Directory.CreateDirectory(item.Path);

				Process myProcess = new Process();

				// VECCHIA GESTIONE WINZIP 	        
				// string commandLine = string.Format("{0}\\wzunzip", dataSel.WinZipPath);
				// string commandArgs = string.Format("-d -o \"{0}\" \"{1}\"", zipFileName, item.Path);

				// GESTIONE 7-ZIP
				string commandLine = string.Format("{0}\\7z", dataSel.WinZipPath);
				string commandArgs = string.Format("x -aoa \"{0}\" -o\"{1}\"", zipFileName, item.Path);

				try
				{
					myProcess.StartInfo.FileName = commandLine;
					myProcess.StartInfo.Arguments = commandArgs;
					myProcess.StartInfo.CreateNoWindow = true;
					myProcess.StartInfo.UseShellExecute = false;
					myProcess.Start();
					myProcess.WaitForExit();
				}
				catch
				{ }
			}
		}
		# endregion
	}
	# endregion
}
