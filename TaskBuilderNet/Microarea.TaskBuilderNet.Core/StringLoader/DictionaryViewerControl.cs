using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;


namespace Microarea.TaskBuilderNet.Core.StringLoader
{
	//================================================================================
	public partial class DictionaryViewerControl : System.Windows.Forms.UserControl
	{

		//--------------------------------------------------------------------------------
		DataRow CurrentHeadRow
		{
			get
			{
				BindingManagerBase bmb = (BindingManagerBase)BindingContext[dgDictionaryItems.DataSource, dgDictionaryItems.DataMember];
				if (bmb.Position == -1)
					return null;

				DataRowView drv = bmb.Current as DataRowView;
				if (drv == null)
					return null;
				return drv.Row;
			}
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------------------
		public DictionaryViewerControl ()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		//--------------------------------------------------------------------------------
		private void btnOpenDictionary_Click (object sender, System.EventArgs e)
		{
			if (openFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				dgDictionaryItems.SuspendLayout();

				string extension = Path.GetExtension(openFileDialog.FileName);

				if (String.Compare(extension, ".bin", true, CultureInfo.InvariantCulture) == 0)
				{
					FillDataSetDictionary();
				}
				else if (
					String.Compare(extension, ".dll", true, CultureInfo.InvariantCulture) == 0
					||
					String.Compare(extension, ".exe", true, CultureInfo.InvariantCulture) == 0
					)
				{
					FillDataSetSatellite();
				}

				BindingManagerBase bmb = BindingContext[dgDictionaryItems.DataSource, dgDictionaryItems.DataMember];
				bmb.CurrentChanged += new EventHandler(CurrentRowChanged);

			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
			}
			finally
			{
				dgDictionaryItems.ResumeLayout();
				Cursor.Current = Cursors.Default;

			}
		}


		//--------------------------------------------------------------------------------
		private void checkedListBoxFilter_ItemCheck (object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			BeginInvoke(new CreateFilterDelegate(CreateFilterDictHead));
		}


		//--------------------------------------------------------------------------------
		private void DictionaryViewerControl_Load (object sender, System.EventArgs e)
		{
			string[] typesFilter = DictionaryBinaryIndexItem.Types;

			checkedListBoxFilter.BeginUpdate();
			for (int i = 0; i < typesFilter.Length; i++)
			{
				string item = typesFilter[i];

				if (item != null && item.Length > 0)
					this.checkedListBoxFilter.Items.Add(item, CheckState.Checked);
			}
			checkedListBoxFilter.Sorted = true;
			checkedListBoxFilter.EndUpdate();

		}


		private delegate void CreateFilterDelegate ();
		//--------------------------------------------------------------------------------
		private void CreateFilterDictHead ()
		{
			CheckedListBox.CheckedItemCollection checkedTypes = this.checkedListBoxFilter.CheckedItems;

			StringBuilder filter = new StringBuilder();

			if (checkedTypes.Count != 0)
			{
				filter.Append("(");
				foreach (object type in checkedTypes)
				{
					if (filter.Length > 1)
						filter.Append(" OR ");
					filter.Append(dictionaryDataSet.DictionaryHead.typeColumn.ColumnName);
					filter.Append(" = '");
					filter.Append(DictionaryBinaryIndexItem.TypeToUInt((string)type));
					filter.Append("'");
				}
				filter.Append(")");
			}
			else
				filter.Append("false");

			filter.Append(" AND ");
			filter.Append(dictionaryDataSet.DictionaryHead.nameColumn.ColumnName);
			filter.Append(" LIKE '*");
			filter.Append(this.txtBoxName.Text);
			filter.Append("*'");

			filter.Append(" AND ");

			filter.Append(dictionaryDataSet.DictionaryHead.idColumn.ColumnName);
			filter.Append(" LIKE '*");
			filter.Append(this.txtBoxId.Text);
			filter.Append("*'");

			this.dictionaryHeadDataView.RowFilter = filter.ToString();
		}




		//--------------------------------------------------------------------------------
		private void CurrentRowChanged (object sender, EventArgs e)
		{
			DataRow curr = CurrentHeadRow;
			if (curr is DictionaryDataSet.DictionaryHeadRow)
				CreateFilterDicRows();
			else if (curr is SatelliteAssemblyDataSet.SatelliteAssemblyRow)
				CreateFilterSatRows();
		}

		//--------------------------------------------------------------------------------
		private void CreateFilterDicRows ()
		{
			DataRow r = CurrentHeadRow;
			StringBuilder filter = new StringBuilder();
			if (r == null)
			{
				filter.Append("false");
			}
			else
			{
				filter.Append(dictionaryDataSet.DictionaryRows.itemIdColumn.ColumnName);
				filter.Append("='");
				filter.Append(r[dictionaryDataSet.DictionaryRows.itemIdColumn.ColumnName]);
				filter.Append("'");
			}
			this.dictionaryRowDataView.RowFilter = filter.ToString();
		}

		//--------------------------------------------------------------------------------
		private void CreateFilterSatRows ()
		{
			long typeColumn = 0;
			DataRow r = CurrentHeadRow;
			if (r != null)
				typeColumn = System.Convert.ToInt64(r[satelliteAssemblyDataSet.SatelliteAssembly.resourceIDColumn.ColumnName]);

			StringBuilder filter = new StringBuilder();
			filter.Append(satelliteAssemblyDataSet.Resource.typeColumn.ColumnName);
			filter.Append(" LIKE '*");
			filter.Append(this.textBoxType.Text);
			filter.Append("*' AND ");
			filter.Append(satelliteAssemblyDataSet.Resource.resourceIDColumn.ColumnName);
			filter.Append("='");
			filter.Append(typeColumn);
			filter.Append("'");

			this.satelliteRowDataView.RowFilter = filter.ToString();

		}

		//--------------------------------------------------------------------------------
		private void FillDataSetDictionary ()
		{
			//visualizzo i filtri per file binari
			this.txtBoxId.Text="";
			this.txtBoxName.Text="";
			this.panelDictionary.Enabled=true;
			//nascondo quelli per dll
			this.textBoxType.Text="";
			this.panelSatellite.Enabled=false;
	
			DictionaryBinaryFile d = new DictionaryBinaryFile();
			using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
			{
				using (DictionaryBinaryParser parser = new DictionaryBinaryParser(fs))
				{
					d.Parse(parser);
				}
			}
			dictionaryDataSet.DictionaryRows.Clear();
			dictionaryDataSet.DictionaryHead.Clear();
		
			DictionaryDataSet.DictionaryHeadRow rowHead = null; 
			DictionaryDataSet.DictionaryRowsRow rowRows = null;
		    uint itemId = 0;
			foreach (DictionaryEntry de in d)
			{
				DictionaryBinaryIndexItem item = de.Key as DictionaryBinaryIndexItem;
				DictionaryStringBlock block = de.Value as DictionaryStringBlock;
				itemId++;   	
				rowHead = dictionaryDataSet.DictionaryHead.NewDictionaryHeadRow();
				rowHead.itemId = itemId; 
				rowHead.type = item.Type;
				rowHead.typeDescription = item.TypeDescription;
				rowHead.id = item.Id;
				rowHead.name = item.Name;
				dictionaryDataSet.DictionaryHead.Rows.Add(rowHead );

				foreach (DictionaryEntry sde in block)
				{
					string _base = sde.Key as string;
					DictionaryStringItem sItem = sde.Value as DictionaryStringItem;
					rowRows = dictionaryDataSet.DictionaryRows.NewDictionaryRowsRow();
					rowRows.itemId = itemId;
					rowRows._base = _base;
					rowRows.target = sItem.Target;
					dictionaryDataSet.DictionaryRows.Rows.Add(rowRows);	
				}
			}
			this.dgDictionaryItems.DataSource = this.dictionaryHeadDataView;
			this.dgDictionaryItemRows.DataSource = this.dictionaryRowDataView;

		}

		//--------------------------------------------------------------------------------
		private void FillDataSetSatellite ()
		{
			Assembly a = Assembly.LoadFile(openFileDialog.FileName);
			satelliteAssemblyDataSet.Resource.Clear();
			satelliteAssemblyDataSet.SatelliteAssembly.Clear();
			string[] resources = a.GetManifestResourceNames();
			uint i = 0;
			foreach (string resource in resources)
			{
				i++;
				SatelliteAssemblyDataSet.SatelliteAssemblyRow rowMaster = satelliteAssemblyDataSet.SatelliteAssembly.NewSatelliteAssemblyRow();
				using (Stream str = a.GetManifestResourceStream(resource))
				{

					rowMaster.resourceName = resource;
					rowMaster.resourceID = i;

					this.satelliteAssemblyDataSet.SatelliteAssembly.Rows.Add(rowMaster);
					ResourceReader rr = new ResourceReader(str);
					IDictionaryEnumerator en = rr.GetEnumerator();

					SatelliteAssemblyDataSet.ResourceRow rowSlave = null;
					while (en.MoveNext())
					{
						rowSlave = satelliteAssemblyDataSet.Resource.NewResourceRow();
						rowSlave.resourceID = i;
						rowSlave.name = en.Key.ToString();
						rowSlave.value = (en.Value == null) ? "(null)" : en.Value.ToString();
						rowSlave.type = (en.Value == null) ? "(null)" : en.Value.GetType().ToString();

						this.satelliteAssemblyDataSet.Resource.Rows.Add(rowSlave);
					}
					rr.Close();
				}
				//nascondo i filtri per file binari
				this.txtBoxId.Text = "";
				this.txtBoxName.Text = "";
				this.panelDictionary.Enabled = false;

				//visualizzo quelli per dll
				this.textBoxType.Text = "";
				this.panelSatellite.Enabled = true;


				this.dgDictionaryItems.DataSource = this.satelliteHeadDataView;
				this.dgDictionaryItemRows.DataSource = this.satelliteRowDataView;
			}
		}

		private void textBoxType_TextChanged (object sender, System.EventArgs e)
		{
			CreateFilterSatRows();
		}

		private void txtBoxId_TextChanged (object sender, System.EventArgs e)
		{
			CreateFilterDictHead();
			CreateFilterDicRows();
		}

		private void txtBoxName_TextChanged (object sender, System.EventArgs e)
		{
			CreateFilterDictHead();
			CreateFilterDicRows();
		}

	}



	//================================================================================
	internal class AutoSizeTextColumnStyle : DataGridTextBoxColumn
	{
		//--------------------------------------------------------------------------------
		protected override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			DataRow row = ((DataRowView)source.List[rowNum]).Row;

			string text = row[MappingName] as string;
			SizeF size = g.MeasureString(text, this.DataGridTableStyle.DataGrid.Font);
			int actualWidth = Convert.ToInt32(size.Width) + 5;
			if (actualWidth > Width)
				Width = actualWidth;

			base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}
	}
}
