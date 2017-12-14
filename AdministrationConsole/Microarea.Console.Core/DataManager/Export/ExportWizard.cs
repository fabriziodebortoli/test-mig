using Microarea.Console.Core.DataManager.Common;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Export
{
	/// <summary>
	/// ExportWizard (gestione wizard di esportazione dati)
	/// </summary>
	//=========================================================================
	public class ExportWizard : DataManagerWizard
	{
		public ExportSelections	ExportSel = null;		
	
		//---------------------------------------------------------------------
		public ExportWizard(ContextInfo contextInfo, DatabaseDiagnostic diagnostic, BrandLoader brandLoader)
			: base(contextInfo, diagnostic, brandLoader)
		{
			ExportSel = new ExportSelections(contextInfo, brandLoader);
		}

		//---------------------------------------------------------------------
		public override ExportSelections GetExportSelections()
		{
			return ExportSel;
		}

		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle =
				string.Format(DataManagerStrings.WizardTitleText, DataManagerStrings.ExportTitleText, contextInfo.CompanyName);

			AddWizardPage(new PresentationPage());
			AddWizardPage(new Common.TablesParamPage());
			AddWizardPage(new Common.TablesSelectionsListPage());
			AddWizardPage(new ColumnsSelectionsListPage());
			AddWizardPage(new Common.AddWhereClausePage());
			AddWizardPage(new Common.BaseColumnsParamPage());
			AddWizardPage(new XmlParamsPage());
			AddWizardPage(new SummaryPage());
		}
	}
}
