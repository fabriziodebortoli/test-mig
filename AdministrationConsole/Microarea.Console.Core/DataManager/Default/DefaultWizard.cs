using Microarea.Console.Core.DataManager.Common;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Default
{
	/// <summary>
	/// DefaultWizard (gestione del wizard dei dati di default)
	/// </summary>
	//=========================================================================
	public class DefaultWizard : DataManagerWizard
	{
		public DefaultSelections DefaultSel = null;

		//---------------------------------------------------------------------
		public DefaultWizard(ContextInfo contextInfo, DatabaseDiagnostic diagnostic, BrandLoader brandLoader)
			: base(contextInfo, diagnostic, brandLoader)
		{
			DefaultSel = new DefaultSelections(contextInfo, brandLoader);
		}

		# region Funzioni di Get dei vari tipi di selezione (Default, Import, Export)
		//---------------------------------------------------------------------
		public override DefaultSelections GetDefaultSelections()
		{
			return DefaultSel;
		}

		//---------------------------------------------------------------------
		public override ImportSelections GetImportSelections()	
		{ 
			return DefaultSel.ImportSel; 
		} 

		//---------------------------------------------------------------------
		public override ExportSelections GetExportSelections()	
		{ 
			return DefaultSel.ExportSel; 
		}
		# endregion

		#  region AddWizardPages
		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle =
				string.Format(DataManagerStrings.WizardTitleText, DataManagerStrings.DefaultTitleText, contextInfo.CompanyName);

			AddWizardPage(new PresentationPage());
			AddWizardPage(new ChooseOperationPage());
			AddWizardPage(new Common.TablesParamPage());
			AddWizardPage(new Common.TablesSelectionsListPage());
			AddWizardPage(new ColumnsSelectionsListPage());
			AddWizardPage(new Common.AddWhereClausePage());
			AddWizardPage(new FilesSelectionPage());
			AddWizardPage(new Common.ImportParamsPage());
			AddWizardPage(new Common.ErrorParamsPage());
			AddWizardPage(new BaseColumnsPage());
			AddWizardPage(new Common.ScriptBeforeExportPage());
			AddWizardPage(new Common.ConfigurationFileParamPage());
			AddWizardPage(new SummaryPage());
		}
		# endregion
	}
}
