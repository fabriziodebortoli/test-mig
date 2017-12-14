using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.UI
{
	//=============================================================================
	/// <remarks/>
	internal partial class DBTEditor : ThemedForm
	{
		//-----------------------------------------------------------------------------
		private MDocument document;
		private MDBTObject parent;
		private MDBTObject dataManager;
		Dictionary<string, IRecord> dbObjects = null;
		private IRecord currentTable;

		private string initialTableName = string.Empty;
		private readonly string[] plurals = new string[] { "ies", "es", "s" };
		public const string EB_LocalData = "EB_LocalData";

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public MDBTObject DataManager { get { return dataManager; } }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public DBTEditor(MDocument document, string initialTableName = "", MDBTObject dbt = null)
		{
			InitializeComponent();
			this.initialTableName = initialTableName;
			
			btnOk.Enabled = false;

			this.document = document;
			if (document == null)
				return;

			grdRelationShip.Columns.Clear();

			this.parent = dbt == null ? (MDBTObject)document.Master : dbt;

			if (parent == null) //sto aggiungendo il master
			{
				rbnSlave11.Visible = false;
				rbnSlave1n.Visible = false;
				grdRelationShip.Enabled = false;
				pnlRelation.Visible = false;

				Collapse();
			}
			else
			{
				rbnSlave11.Checked = true;
				
				lblMasterTableName.Text = this.parent.Record.Name;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			this.Update();

			UseWaitCursor = true;
			dataManagerControl.SetMessage(Resources.DBTLoadingDbObjects);
			Color oldColor = dataManagerControl.SetMessageColor(Color.Red);

			FillCatalogCombo();

			bool hasError = false;
			//provo a selezionare la tabella che ha scelto l'utente, se non ci riesco devo disabilitare la finestra
			if (!dataManagerControl.InitTables(initialTableName))
			{
				dataManagerControl.SetMessage(Resources.InvalidTableForDbt);
				hasError = true;
			}

			UseWaitCursor = false;
			if (!hasError)
			{
				dataManagerControl.SetMessage(parent == null 
					? Resources.DBTCreatedAsMaster 
					: Resources.DBTCreatedAsSlave);
				dataManagerControl.SetMessageColor(oldColor);
			}
		}

		//-----------------------------------------------------------------------------
		private void Collapse()
		{
			int bnHeigth = btnOk.Height;
			btnOk.Top = pnlRelation.Top + 2;
			btnOk.Height = bnHeigth;
			btnCancel.Top = btnOk.Top;
			btnCancel.Height = btnOk.Height;

			// bottone + titolo + un po' di margine
			Height = btnOk.Bottom + (bnHeigth * 2);
		}

		//-----------------------------------------------------------------------------
		private void FillCatalogCombo ()
		{
			// non avendo un tag di un item a disposizione mi tengo
			// una lista di corrispondenza alle tabelle della combo
			dbObjects = new Dictionary<string,IRecord>();
			if (document.Batch)
			{
				dbObjects.Add(EB_LocalData, new MSqlRecord(EB_LocalData));
				dataManagerControl.TableChangeable = false;
			}
			else
			{
				MSqlCatalog catalog = new MSqlCatalog();
				IList masterFields = parent == null ? null : parent.Record.PrimaryKeyFields;
				// una view non può essere un master
				foreach (IRecord record in (parent == null ? catalog.Tables : catalog.TablesAndViews))
				{
					if (!record.IsRegistered)
						continue;

                    // la tabella non ha master del dbt master deve avere master table == true;
                    if (parent == null)
                    {
                        MSqlRecord rec = record as MSqlRecord;
                        if (!rec.IsMasterTable)
                            continue;
                    }
					IList slaveFields = record.RecordType == DataModelEntityType.View ? record.Fields : record.PrimaryKeyFields;
					if (masterFields == null || catalog.HaveMasterSlaveRelationship(masterFields, slaveFields))
					{
						dbObjects.Add(record.Name, record);
					}
				}
			}
			dataManagerControl.FillCatalogCombo(dbObjects);
		}


		//-----------------------------------------------------------------------------
		void dataManagerControl_NoSelection(object sender, System.EventArgs e)
		{
			btnOk.Enabled = false;
			grdRelationShip.Rows.Clear();
				
		}

		//-----------------------------------------------------------------------------
		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (!dataManagerControl.ValidateData())
				return;

			if (document.GetDBT(dataManagerControl.ObjectName) != null)
			{
				MessageBox.Show(this, Resources.DbtAlreadyExisting);
				return;
			}
			dataManager = CreateDbt();

			DialogResult = DialogResult.OK;
			this.Close();
		}

        //-----------------------------------------------------------------------------
        // completo la creazione che prima veniva fatta on demand
        private void CompleteIntegrationOf(MDBTObject dbt)
        {
            dbt.OnAfterCreateComponents();
            MSqlRecord record = dbt.Record as MSqlRecord;
            if (record != null)
                record.CallCreateComponents();

            MSqlRecord oldRecord = dbt.OldRecord as MSqlRecord;
            if (oldRecord != null)
                oldRecord.CallCreateComponents();
        }
        //-----------------------------------------------------------------------------
        private MDBTObject CreateDbt ()
		{
			string tableName = dataManagerControl.TableName;
			if (parent == null)
			{
				MDBTMaster dbtMaster = new MDBTMaster(tableName, dataManagerControl.ObjectName, document, false);
                CompleteIntegrationOf(dbtMaster);
                document.AttachMaster(dbtMaster);

                return dbtMaster;
			}

			MDBTSlave slave = null;
			if (rbnSlave11.Checked)
				slave = new MDBTSlave(tableName, dataManagerControl.ObjectName, document, false);
			else
				slave = new MDBTSlaveBuffered(tableName, dataManagerControl.ObjectName, document, false);
            CompleteIntegrationOf(slave);
            AssignForeignKey(slave);
			if (parent is MDBTSlaveBuffered)
				((MDBTSlaveBuffered)parent).AttachSlave(slave);
			else
				document.AttachSlave(slave);


            return slave;
		}

		//-----------------------------------------------------------------------------
		private void AssignForeignKey (MDBTSlave slave)
		{
			List<MForeignKeyField> masterSlaveRelation = new List<MForeignKeyField>();

			foreach (DataGridViewRow row in grdRelationShip.Rows)
			{
				IRecordField masterField = row.Cells[0].Tag as IRecordField;
				string fieldName = row.Cells[1].Value as string;
				IRecordField currentTableField = currentTable.GetField(fieldName);
				if (currentTableField != null)
					masterSlaveRelation.Add(new MForeignKeyField(currentTableField.Name, masterField.Name, currentTable.Name, masterField.Record.Name));
			}

			slave.MasterForeignKey = masterSlaveRelation;
		}

		//-----------------------------------------------------------------------------
		private void BtnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void CbxTableName_SelectedTableChanged(object sender, EventArgs e)
		{
			string tableName = dataManagerControl.TableName;
			if (string.IsNullOrEmpty(tableName) || tableName == Resources.NoneItem)
			{
				btnOk.Enabled = false;
				return;
			}

			currentTable = dbObjects[tableName] as IRecord;

			if (parent != null)
				LoadRelationShip();

			int nPos = tableName.IndexOf("_");
			if (nPos > 0)
				tableName = tableName.Substring(nPos + 1);
			dataManagerControl.ObjectName = GetUniqueName(tableName);
			btnOk.Enabled = true;
		}
		
		//-----------------------------------------------------------------------------
		private string GetUniqueName(string tableName)
		{
			if (parent == null)
				return tableName;

			string dbtName = tableName;
			int nr = 0;
			while (document.GetDBT(dbtName) != null)
			{
				dbtName = string.Format("{0}{1}", dbtName, ++nr);
			}

			return dbtName;
		}

		//-----------------------------------------------------------------------------
		private void LoadRelationShip()
		{
			grdRelationShip.Columns.Clear();
			grdRelationShip.Rows.Clear();

			if (currentTable == null || currentTable.RecordType != DataModelEntityType.Table)
				return;

			DataGridViewTextBoxColumn colMaster = new DataGridViewTextBoxColumn();
			colMaster.HeaderText = Resources.DBTMasterField;
			colMaster.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colMaster.ReadOnly = true;

			DataGridViewComboBoxColumn colSlave = new DataGridViewComboBoxColumn();
			colSlave.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colSlave.HeaderText = Resources.DBTSlaveField;

			grdRelationShip.Columns.Add(colMaster);
			grdRelationShip.Columns.Add(colSlave);

			// le view non hanno chiave primaria
			IList tablePKFields = currentTable.RecordType == DataModelEntityType.Table ?
													currentTable.PrimaryKeyFields :
													currentTable.Fields;

			foreach (IRecordField field in parent.Record.PrimaryKeyFields)
			{
				DataGridViewRow row = new DataGridViewRow();
				
				// master column
				DataGridViewTextBoxCell masterCell = new DataGridViewTextBoxCell();
				masterCell.Value = field.Name;
				masterCell.Tag = field;
				row.Cells.Add(masterCell);

				// slave column
				DataGridViewComboBoxCell slaveCell = new DataGridViewComboBoxCell();
				IList compatibleFields = MSqlRecord.GetCompatibleFieldsWith(field, tablePKFields);
	
				btnOk.Enabled = (compatibleFields != null || compatibleFields.Count > 0);
				
				if (btnOk.Enabled)
				{
				IRecordField f = (IRecordField) compatibleFields[0];
				slaveCell.Value = f.Name;
				slaveCell.Tag = f;
				}
				else
					slaveCell.Value = Microarea.EasyBuilder.Properties.Resources.NotFound;

				string selection = string.Empty; 
				foreach (IRecordField compatibleField in compatibleFields)
				{
					DecideBestSelection(field.Name, ref selection, compatibleField.Name);
					slaveCell.Items.Add(compatibleField.Name);
				}
				
				if (!string.IsNullOrEmpty(selection))
					slaveCell.Value = selection;

				row.Cells.Add(slaveCell);
				grdRelationShip.Rows.Add(row);
			}
		}

		// auristica sul nome più simile rispetto al campo del master
		//-----------------------------------------------------------------------------
		private void DecideBestSelection(string masterFieldName, ref string actualSelection, string candidate)
		{
			//  già trovato il migliore
			if (string.Compare(actualSelection, masterFieldName, true) == 0)
				return;

			// il primo trovato oppure il migliore con lo stesso nome oppure contiene il nome e l'attuale selezione no
			if	(
					string.IsNullOrEmpty(actualSelection) || 
					string.Compare(candidate, masterFieldName, true) == 0 ||
					!actualSelection.ContainsNoCase(masterFieldName) && candidate.ContainsNoCase(masterFieldName)
				)
			{
				actualSelection = candidate;
				return;
			}

			// singolare => vari plurali
			// a parità di nome contenuto provo a vedere prima i plurali
			foreach (string plural in plurals)
			{
				if	(
						// l'attuale selezione non contiene il campo ma il candidato contiene il plurale
						!actualSelection.ContainsNoCase(masterFieldName) && candidate.ContainsNoCase(masterFieldName + plural) ||
						// l'attuale selezione contiene il plurale ma il candidato contiene il singolare
						actualSelection.ContainsNoCase(masterFieldName + plural) && candidate.ContainsNoCase(masterFieldName)
					)
				{
					actualSelection = candidate;
					return;
				}
			}	
		}

		
	}
}
