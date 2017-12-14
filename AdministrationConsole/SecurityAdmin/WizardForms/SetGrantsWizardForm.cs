using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public partial class SetGrantsWizardForm : InteriorWizardPage
	{
		#region Data Member Private
		private WizardParameters					wizardParameters	= null;
		private GrantsDataGrid						grantsDataGrid		= null;
		#endregion

		#region Costruttore
		//---------------------------------------------------------------------
		public SetGrantsWizardForm()
		{
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		#endregion

		#region form attiva quindi alla visualizzazione
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
 
			wizardParameters = ((SecurityWizardManager)this.WizardManager).GetImportSelections();

			if (wizardParameters == null)
				return false;
			
			if (wizardParameters.GrantsDataTable == null || wizardParameters.GrantsDataTable.Rows.Count == 0)
			{
				InitializeMyControls();
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			}
			else
			{
				if (wizardParameters.ApplyType == WizardParametersType.ApplyGrantsAllObjects)
					this.AllObjectsCheckBox.Checked = true;
			}

			return true;
		}
		//---------------------------------------------------------------------
		private void LoadParameters()
		{
		//	for (int i=0; i < grantsArrayList.Count; i++)
	/*		grantsDataGrid.DataSource.Tables[0].Clear();
			for (int i=0; i < wizardParameters.GrantsDataTable.Rows.Count; i++)
			{
				grantsDataGrid.DataSource.Tables[0].Rows.Add(wizardParameters.GrantsDataTable.Rows[i]);
				//if (grantsArrayList[i] != null && grantsArrayList[i] is GrantsRow)
				//	grantsDataGrid.AddGrantsRow((GrantsRow)grantsArrayList[i]);
			}

			grantsDataGrid.ResizeToFit(this);
*/
		}
		//---------------------------------------------------------------------
		
		#endregion

		#region funzioni di inizializzazione
		private void InitializeMyControls()
		{
			this.m_titleLabel.Text		= Strings.SetGrantsTitle;
            this.m_subtitleLabel.Text = Strings.SetGrantsDescription;
			AddGrantsGrid();
		}
		//---------------------------------------------------------------------
		private void AddGrantsGrid()
		{
			ArrayList grantsArrayList = FindGrantsForDocument();

			grantsDataGrid = new GrantsDataGrid(true, wizardParameters.ShowObjectsTreeForm.IsRoleLogin, this);
			grantsDataGrid.OnModifyColumnValueHandle += new GrantsDataGrid.ModifyColumnValueHandle(ModifyValue);

			grantsDataGrid.Left   = m_subtitleLabel.Location.X;
			grantsDataGrid.Top    = m_subtitleLabel.Location.Y + m_subtitleLabel.Height + 10;

			if (grantsArrayList != null && grantsArrayList.Count > 0)
			{
				for (int i=0; i< grantsArrayList.Count; i++)
				{
					if (grantsArrayList[i] != null && grantsArrayList[i] is GrantsRow)
						grantsDataGrid.AddGrantsRow((GrantsRow)grantsArrayList[i]);
				}
			}
			grantsDataGrid.ResizeToFit(this);	

			Controls.Add(grantsDataGrid);
			this.AllObjectsCheckBox.Left	= grantsDataGrid.Left + 20 + grantsDataGrid.Width;
			this.AllowAllButton.Left		= grantsDataGrid.Left + 20 + grantsDataGrid.Width;
			this.DenyAllButton.Left			= this.AllowAllButton.Left + this.DenyAllButton.Width + 30;

			WarningLabel.Left = AllObjectsCheckBox.Left;
		}
		//---------------------------------------------------------------------
		private void ModifyValue(object sender, int rowNumber)
		{
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}
		//---------------------------------------------------------------------
		private ArrayList FindGrantsForDocument()
		{
			if (wizardParameters.ShowObjectsTreeForm.Connection == null || 
				wizardParameters.ShowObjectsTreeForm.Connection.State != ConnectionState.Open)
			{
				Debug.Fail("Error in SetEasyGrantsForm.FindGrantsForDocument : Invalid connection.");
				return null;
			}

			ArrayList grantsArrayList = null;
			//Mi tiro su tutti i tipi di permesso per quel tipo di oggetto			
            string select = @"SELECT DISTINCT MSD_ObjectTypeGrants.GrantName, MSD_ObjectTypeGrants.GrantMask			   
								FROM MSD_ObjectTypeGrants JOIN MSD_ObjectTypes 
								ON MSD_ObjectTypeGrants.TypeId = MSD_ObjectTypes.TypeId
								ORDER BY MSD_ObjectTypeGrants.GrantMask";

			SqlCommand mySqlCommand = new SqlCommand(select, wizardParameters.ShowObjectsTreeForm.Connection);
			SqlDataReader myReader = null;
			try
			{
				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					if (grantsArrayList == null)
						grantsArrayList	= new ArrayList();
					
					grantsArrayList.Add(new GrantsRow(Convert.ToInt32(myReader["GrantMask"]), 3, GrantsString.GetGrantDescription(myReader["GrantName"].ToString())));
				}
				myReader.Close();
				mySqlCommand.Dispose();
			}
			catch (SqlException err) 
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();

				mySqlCommand.Dispose();
				
				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);	
			}
			
			return grantsArrayList;
		}
		//---------------------------------------------------------------------
		#endregion

		#region form disattiva quindi quando clicco su avanti
		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			grantsDataGrid.Focus();

			if (wizardParameters == null)
				wizardParameters = new WizardParameters();

			if (this.AllObjectsCheckBox.Checked)
				wizardParameters.ApplyType = WizardParametersType.ApplyGrantsAllObjects;
			else
				wizardParameters.ApplyType = WizardParametersType.ApplyGrantsOnlyProtectedObjects;
			
			if (wizardParameters.GrantsDataTable != null)
				wizardParameters.GrantsDataTable.Rows.Clear();
			else
				wizardParameters.GrantsDataTable = new DataTable();
			
			foreach(DataRow dr in grantsDataGrid.DataSource.Tables[0].Rows)
				AddGrantsRow(dr);
			
			return base.OnKillActive();
		}
		//---------------------------------------------------------------------
		public  void AddGrantsRow(DataRow aDataRow)
		{
			if (aDataRow == null)
				return;

			DataRow dr = wizardParameters.GrantsDataTable.NewRow();

			dr[securityGrants.Grant] = aDataRow[securityGrants.Grant];
			dr[securityGrants.GrantMask] = aDataRow[securityGrants.GrantMask];
			dr[securityGrants.Inherit] = aDataRow[securityGrants.Inherit];
			dr[securityGrants.Role] = aDataRow[securityGrants.Role];
			dr[securityGrants.User] = aDataRow[securityGrants.User];
			dr[securityGrants.Total] = aDataRow[securityGrants.Total];
			dr[securityGrants.Assign] = aDataRow[securityGrants.Assign];
			dr[securityGrants.OldValue] = aDataRow[securityGrants.OldValue];
			
			wizardParameters.GrantsDataTable.Rows.Add(dr);
		}

		//---------------------------------------------------------------------
		private void AllowAllButton_Click(object sender, System.EventArgs e)
		{
			GrantsFunctions.SetValueForAllGrants( this.grantsDataGrid, GrantOperationType.Allow);	
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		//---------------------------------------------------------------------
		private void DenyAllButton_Click(object sender, System.EventArgs e)
		{
			GrantsFunctions.SetValueForAllGrants( this.grantsDataGrid, GrantOperationType.Deny);
			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		#endregion
	}
}

