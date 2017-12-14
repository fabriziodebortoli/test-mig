using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.UI
{
	//=============================================================================
	/// <remarks/>
	internal partial class DataManagerEditor : ThemedForm
	{
		//-----------------------------------------------------------------------------
		Dictionary<string, IRecord> dbObjects = null;
		private IRecord currentTable;
		private string dataManagerName = "";
		private string initialTableName = string.Empty;
		public delegate bool ExistNameFunction(string name);

		public ExistNameFunction ExistName { get; set; }
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string DataManagerName { get { return dataManagerName; } }
		public string TableName { get { return dataManagerControl.TableName; } }
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public DataManagerEditor(string initialTableName = "")
		{
			InitializeComponent();
			this.initialTableName = initialTableName;

			btnOk.Enabled = false;
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
				dataManagerControl.SetMessage(string.Empty);
				dataManagerControl.SetMessageColor(oldColor);
			}
		}

		//-----------------------------------------------------------------------------
		private void FillCatalogCombo ()
		{
			MSqlCatalog catalog = new MSqlCatalog();

			// non avendo un tag di un item a disposizione mi tengo
			// una lista di corrispondenza alle tabelle della combo
			dbObjects = new Dictionary<string,IRecord>();

			// una view non può essere un master
			foreach (IRecord record in (catalog.Tables))
			{
				if (!record.IsRegistered)
					continue;

				dbObjects.Add(record.Name, record);
			}

			dataManagerControl.FillCatalogCombo(dbObjects);
		}


		//-----------------------------------------------------------------------------
		void dataManagerControl_NoSelection(object sender, System.EventArgs e)
		{
			btnOk.Enabled = false;
		}

		//-----------------------------------------------------------------------------
		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (!dataManagerControl.ValidateData())
				return;
			dataManagerName = dataManagerControl.ObjectName;
			if (ExistName(dataManagerName))
			{
				MessageBox.Show(this,Resources.DuplicateResourceName);
				return;
			}
			
			DialogResult = DialogResult.OK;
			this.Close();
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

			int nPos = tableName.IndexOf("_");
			if (nPos > 0)
				tableName = tableName.Substring(nPos + 1);
			dataManagerControl.ObjectName = GetUniqueName(tableName);
			btnOk.Enabled = true;
		}
		
		//-----------------------------------------------------------------------------
		private string GetUniqueName(string seedName)
		{
			string objName = seedName;
			int nr = 0;
			while (ExistName(objName))
			{
				objName = string.Format("{0}{1}", seedName, ++nr);
			}
			return objName;
		}
	}
}
