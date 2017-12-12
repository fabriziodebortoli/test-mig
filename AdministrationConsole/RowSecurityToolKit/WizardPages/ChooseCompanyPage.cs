using System;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per la selezione di un'azienda, il cui database deve risultare completo
	///</summary>
	//================================================================================
	public partial class ChooseCompanyPage : InteriorWizardPage
	{
		private RSSelections rsSelections = null;

		//--------------------------------------------------------------------------------
		public ChooseCompanyPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			rsSelections = ((RSWizard)this.WizardManager).Selections;

			// inizializzo i controls
			SetControlsValue();

			return true;
		}

		//---------------------------------------------------------------------
		public override bool OnKillActive()
		{
			return base.OnKillActive();
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			if (!CheckValues())
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.CompanyDBNotComplete, string.Empty);
				return WizardForm.NoPageChange;
			}

			GetControlsValue();

			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
		private bool CheckValues()
		{
			if (rsSelections.CompaniesList.Count == 0)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.NoCompaniesAvailable, string.Empty);
				return false;
			}

			CompanyItem selectedCompany = (CompanyItem)CompaniesCBox.SelectedItem;
			if (selectedCompany == null)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(Strings.SelectAValidCompany, string.Empty);
				return false;
			}

			DatabaseStatus ds = ((RSWizard)this.WizardManager).GetCompanyStatus(selectedCompany.CompanyId);

			string message = string.Empty;

			switch (ds)
			{
				case DatabaseStatus.EMPTY:
					message = string.Format(DatabaseLayerStrings.MsgEmptyDB, DatabaseLayerConsts.ERPSignature);
					break;
				case DatabaseStatus.NOT_EMPTY:
					message = string.Format(DatabaseLayerStrings.MsgFullDB, DatabaseLayerConsts.ERPSignature);
					break;
				case DatabaseStatus.NEED_RECOVERY:
					message = Strings.DatabaseNeedsRecovery;
					break;
				case DatabaseStatus.NEED_UPGRADE:
					message = Strings.DatabaseNeedsUpgrade;
					break;
				case DatabaseStatus.NEED_MANDATORY_COLUMNS:
					message = DatabaseLayerStrings.MsgNeedMandatoryColumns;
					break;
                case DatabaseStatus.NEED_TBGUID_COLUMN:
                    message = DatabaseLayerStrings.MsgNeedMandatoryTBGuidCol;
                    break;
				case DatabaseStatus.NEED_ROWSECURITY_COLUMNS:
					message = DatabaseLayerStrings.MsgNeedMandatoryColumnsForRS;
					break;
				case DatabaseStatus.UNRECOVERABLE:
					message = string.Format(DatabaseLayerStrings.ErrDBMarkNotExist, DatabaseLayerConsts.TB_DBMark);
					break;
				default:
					break;
			}

			MessageLabel.Text = message;

			StatusPictureBox.Image = (ds == DatabaseStatus.NOT_EMPTY)
				? ((RSWizard)this.WizardManager).StateImageList.Images[PlugInTreeNode.GetGreenSemaphoreStateImageIndex]
				: ((RSWizard)this.WizardManager).StateImageList.Images[PlugInTreeNode.GetRedSemaphoreStateImageIndex];

			return (ds == DatabaseStatus.NOT_EMPTY);
		}
		
		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			GetControlsValue();

			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
		private void ChooseCompanyPage_Load(object sender, EventArgs e)
		{
			((RSWizard)this.WizardManager).LoadCompaniesList();
			SetControlsValue();
		}

		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			CompaniesCBox.DataSource = null;
			// carico le aziende nella combobox
			CompaniesCBox.BeginUpdate();
			CompaniesCBox.Items.Clear();
			CompaniesCBox.DataSource = rsSelections.CompaniesList;
			CompaniesCBox.DisplayMember = ConstStrings.Company;
			CompaniesCBox.ValueMember = ConstStrings.CompanyId;
			CompaniesCBox.EndUpdate();

			if (rsSelections.CompaniesList.Count > 0)
			{
				int i = (rsSelections.CompanyInfo != null) ? CompaniesCBox.FindStringExact(rsSelections.CompanyInfo.Company, -1) : 0;
				CompaniesCBox.SelectedIndex = (i >= 0) ? i : 0;
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			}
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}

		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			rsSelections.CompanyInfo = (CompanyItem)CompaniesCBox.SelectedItem;
		}

		//---------------------------------------------------------------------
		private void CompaniesCBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			MessageLabel.Text = string.Empty;
			StatusPictureBox.Image = null;
		}
	}
}
