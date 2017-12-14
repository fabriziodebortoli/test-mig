using Microarea.Console.Core.DataManager.Common;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Import
{
	/// <summary>
	/// ImportWizard (gestione wizard per l'importazione dei dati)
	/// </summary>
	//=========================================================================
	public class ImportWizard : DataManagerWizard
	{
		public ImportSelections	ImportSel = null;
		
		//---------------------------------------------------------------------
		public ImportWizard(ContextInfo contextInfo, DatabaseDiagnostic diagnostic, BrandLoader brandLoader)	
			: base(contextInfo, diagnostic, brandLoader)
		{
			ImportSel = new ImportSelections(contextInfo, brandLoader);
		}

		//---------------------------------------------------------------------
		public override ImportSelections GetImportSelections()
		{
			return ImportSel;
		}

		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle =
				string.Format(DataManagerStrings.WizardTitleText, DataManagerStrings.ImportTitleText, contextInfo.CompanyName);

			AddWizardPage(new PresentationPage());
			AddWizardPage(new FilesParamPage());
			AddWizardPage(new FilesSelectionPage());
			AddWizardPage(new Common.BaseColumnsParamPage());
			AddWizardPage(new Common.ImportParamsPage());
			AddWizardPage(new Common.ErrorParamsPage());
			AddWizardPage(new SummaryPage());
		}
	}
}
