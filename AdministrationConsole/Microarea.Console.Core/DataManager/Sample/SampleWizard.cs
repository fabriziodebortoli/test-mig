
using Microarea.Console.Core.DataManager.Common;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Sample
{
	/// <summary>
	/// SampleWizard (gestione wizard dati di esempio)
	/// </summary>
	//=========================================================================
	public class SampleWizard : DataManagerWizard
	{
		public SampleSelections SampleSel = null;

		//---------------------------------------------------------------------
		public SampleWizard(ContextInfo contextInfo, DatabaseDiagnostic diagnostic, BrandLoader brandLoader)
			: base(contextInfo, diagnostic, brandLoader)
		{
			SampleSel = new SampleSelections(contextInfo, brandLoader);
		}

		//---------------------------------------------------------------------
		public override SampleSelections GetSampleSelections()
		{
			return SampleSel;
		}

		//---------------------------------------------------------------------
		public override ImportSelections GetImportSelections()	
		{ 
			return SampleSel.ImportSel; 
		} 

		//---------------------------------------------------------------------
		public override ExportSelections GetExportSelections()	
		{ 
			return SampleSel.ExportSel; 
		}

		//---------------------------------------------------------------------
		public void AddWizardPages()
		{
			this.wizardTitle =
				string.Format(DataManagerStrings.WizardTitleText, DataManagerStrings.SampleTitleText, contextInfo.CompanyName);

			AddWizardPage(new PresentationPage());
			AddWizardPage(new ChooseOperationPage());
			AddWizardPage(new Common.TablesParamPage());
			AddWizardPage(new Common.TablesSelectionsListPage());
			AddWizardPage(new Common.AddWhereClausePage());
			AddWizardPage(new Common.ImportParamsPage());
			AddWizardPage(new Common.ErrorParamsPage());
			AddWizardPage(new BaseColumnsPage());
			AddWizardPage(new SummaryPage());
		}
	}
}
