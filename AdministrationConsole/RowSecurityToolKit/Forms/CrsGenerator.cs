using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.RowSecurityToolKit.Forms
{
	///<summary>
	/// Da questa form e' possibile criptare i file RowSecurityObjects.xml
	/// presenti in ogni modulo dell'installazione
	/// Tramite le classi di base Crypto della libreria Generic vengono generati i file .crs
	///</summary>
	//================================================================================
	public partial class CrsGenerator : Form
	{
		private bool testDecrypt = false; // per testare la decriptazione (ad uso interno)

		private bool inProgress = false;

		private Diagnostic crsDiagnostic = new Diagnostic("CrsGenerator");
		private StringCollection applicationList;
		private List<RowSecurityObjectsInfo> rowSecurityObjectsList = new List<RowSecurityObjectsInfo>();

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public CrsGenerator()
		{
			InitializeComponent();
			LoadRowSecurityObjectsInfo();
		}

		///<summary>
		/// Load dei vari file RowSecurityObjects.xml presenti nell'installazione
		///</summary>
		//--------------------------------------------------------------------------------
		private void LoadRowSecurityObjectsInfo()
		{
			// considero solo le tabelle degli AddOn di TaskBuilderApplications
			// le tabelle di TB ed Extensions non sono previste
			applicationList = new StringCollection();

			// poi guardo le AddOn di TaskBuilderApplications
			BasePathFinder.BasePathFinderInstance.GetApplicationsList(ApplicationType.TaskBuilderApplication, out applicationList);

			// per tutte le applicazioni trovate vado a leggere le informazioni nei file xml
			foreach (string appName in applicationList)
			{
				IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(appName);
				foreach (BaseModuleInfo modInfo in appInfo.Modules)
				{
					if (!File.Exists(modInfo.GetRowSecurityObjectsPath()))
						continue;

					RowSecurityObjectsInfo rsoi = modInfo.RowSecurityObjectsInfo;
					if (
						rsoi == null ||
						(rsoi.RSEntities != null && rsoi.RSEntities.Count == 0 && rsoi.RSTables != null && rsoi.RSTables.Count == 0)
						)
						continue;
					// considero solo i file che contengono dei nodi significativi
					rowSecurityObjectsList.Add(rsoi);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void BtnCrypt_Click(object sender, EventArgs e)
		{
			if (rowSecurityObjectsList.Count == 0)
			{
				DiagnosticViewer.ShowWarning(Strings.NoFilesAvailable, string.Empty);
				return;
			}

			// test decriptazione (solo ad uso interno)
			if (testDecrypt)
			{
				RunDecrypt();
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			inProgress = true;

			// eseguo un pre-check dei files per capire se sono readonly, in modo da visualizzare un avvertimento
			List<string> filesRO = PrecheckFiles();
			if (filesRO.Count > 0)
			{
				string message = Strings.FilesWithROAttributes + "\r\n";
				foreach (string filePath in filesRO)
					message += filePath + "\r\n";
				message += Strings.FilesWillBeOverwritten;

				if (DiagnosticViewer.ShowQuestion(message, string.Empty) != DialogResult.Yes)
					return;
			}

			foreach (RowSecurityObjectsInfo rsoi in rowSecurityObjectsList)
			{
				try
				{
					// eseguo l'Encrypt di ogni singolo file (nel caso in cui il file .crs esista gia' viene sovrascritto)
					if (CRSFunctions.Encrypt(rsoi.FilePath))
						crsDiagnostic.Set(DiagnosticType.Information, string.Format(Strings.EncryptWithSuccess, rsoi.FilePath));
				}
				catch (Exception ex)
				{
					crsDiagnostic.Set(DiagnosticType.Error, string.Format(Strings.EncryptWithError, rsoi.FilePath), new ExtendedInfo(Strings.Message, ex.Message));
				}
			}

			Cursor.Current = Cursors.Default;
			inProgress = false;

			DiagnosticViewer.ShowDiagnostic(crsDiagnostic);
			this.Close();
		}

		// Test decriptazione files (solo ad uso interno)
		//--------------------------------------------------------------------------------
		private void RunDecrypt()
		{
			foreach (RowSecurityObjectsInfo rsoi in rowSecurityObjectsList)
			{
				string crsFilePath = rsoi.FilePath.Replace(".xml", ".crs");
				if (!File.Exists(crsFilePath))
					continue;

				try
				{
					// eseguo l'Encrypt di ogni singolo file (nel caso in cui il file .crs esista gia' viene sovrascritto)
					if (CRSFunctions.DecryptFile(crsFilePath))
						crsDiagnostic.Set(DiagnosticType.Information, string.Format(Strings.EncryptWithSuccess, rsoi.FilePath));
				}
				catch (Exception ex)
				{
					crsDiagnostic.Set(DiagnosticType.Error, string.Format(Strings.EncryptWithError, crsFilePath), new ExtendedInfo(Strings.Message, ex.Message));
				}
			}

			DiagnosticViewer.ShowDiagnostic(crsDiagnostic);
			this.Close();
		}

		//--------------------------------------------------------------------------------
		private List<string> PrecheckFiles()
		{
			List<string> filesRO = new List<string>();

			foreach (RowSecurityObjectsInfo rsoi in rowSecurityObjectsList)
			{
				string crsFile = Path.Combine(Path.GetDirectoryName(rsoi.FilePath), Path.GetFileNameWithoutExtension(rsoi.FilePath) + NameSolverStrings.CrsExtension);
				if (
					File.Exists(crsFile) &&
					((File.GetAttributes(crsFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) &&
					!filesRO.ContainsNoCase(crsFile)
					)
				{
					filesRO.Add(crsFile);
				}
			}

			return filesRO;
		}

		///<summary>
		/// Se clicco su Esc (e non sto elaborando) chiudo la form
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnCrypt_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == (int)Keys.Escape && !inProgress)
				this.Close();
		}
	}
}
