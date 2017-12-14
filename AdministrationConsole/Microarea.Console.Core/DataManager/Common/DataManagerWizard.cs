using System.IO;
using System.Reflection;
using System.Threading;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	/// <summary>
	/// DataManagerWizard
	/// </summary>
	//=========================================================================
	public class DataManagerWizard : WizardManager
	{
		protected	DatabaseDiagnostic	dbDiagnostic;
		protected	ContextInfo			contextInfo;
		protected	BrandLoader			brandLoader;
		
		public		DatabaseDiagnostic	DBDiagnostic	{ get { return dbDiagnostic; } }
		public		BrandLoader			BrandLoader		{ get { return brandLoader; } }
	
		// Events e Delegates
		public delegate void FinishWizard(string operation, Thread t);
		public event FinishWizard OnFinishWizard;

		public delegate bool CompanyDBIsFree();
		public event CompanyDBIsFree OnCompanyDBIsFree;

		// Metodi virtuali (da reimplementare nei figli)
		public virtual ImportSelections		GetImportSelections()	{ return null; } 
		public virtual ExportSelections		GetExportSelections()	{ return null; }
		public virtual DefaultSelections	GetDefaultSelections()	{ return null; }
		public virtual SampleSelections		GetSampleSelections()	{ return null; }

		//---------------------------------------------------------------------
		public DataManagerWizard(ContextInfo context, DatabaseDiagnostic diagnostic, BrandLoader brand)
		{
			Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DataManagerConsts.NamespaceDataManagerImg + ".DataManager.ico");
			if (iconStream != null)
				this.wizardFormIcon =  new System.Drawing.Icon(iconStream);

			contextInfo		= context;
			dbDiagnostic	= diagnostic;
			brandLoader		= brand;
		}

		/// <summary>
		/// questo evento viene intercettato sia sul pulsante Annulla che sulla
		/// X che comanda la chiusura della finestra.
		/// </summary>
		//---------------------------------------------------------------------
		private void Wizard_Closed(object sender, System.EventArgs e)
		{
			contextInfo.UndoImpersonification();
			contextInfo.CloseConnection();
		}

		/// <summary>
		/// evento intercettato sul pulsante Finish di qualsiasi wizard per lanciare
		/// il thread separato per l'esecuzione delle operazioni
 		/// </summary>
		//---------------------------------------------------------------------
		public void OnFinishPage(Thread t)
		{
			if (OnFinishWizard != null)
				OnFinishWizard("FinishWizard", t);
		}
		
		/// <summary>
		/// evento intercettato sul pulsante Next nelle pagine di scelta operazione di 
		/// importazione della gestione dati di default e di esempio, da "rimbalzare" 
		/// alla Console per sapere se altri utenti sono collegati al db aziendale
		/// </summary>
		//---------------------------------------------------------------------
		public bool CheckFreeCompanyDBStatus()
		{
			if (OnCompanyDBIsFree != null)
				return OnCompanyDBIsFree();

			return false;
		}
	}
}