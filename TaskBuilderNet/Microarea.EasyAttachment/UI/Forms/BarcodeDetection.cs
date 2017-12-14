using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;

namespace Microarea.EasyAttachment.UI.Forms
{
	//================================================================================
	public partial class BarcodeDetection : Form
	{
		private Font verdanaFont = new Font("Verdana", 8.25F);
		private Font verdanaBoldFont = new Font("Verdana", 8.25F, FontStyle.Bold);

		private List<Barcode> barcodeList = new List<Barcode>();

		private DTBarcodes dtBarcodes = new DTBarcodes();

		private Barcode selectedBarcode = new Barcode();

		//--------------------------------------------------------------------------------
		public Barcode SelectedBarcode { get { return selectedBarcode; } }

		//--------------------------------------------------------------------------------
		public BarcodeDetection(List<Barcode> bcCodes)
		{
			InitializeComponent();

			barcodeList = bcCodes;

			this.ValDataGridView.AutoGenerateColumns = false;

			if (!DesignMode)
				DesignTableStyle();

			CreateDTBarcodes();
		}

		//--------------------------------------------------------------------------------
		private void CreateDTBarcodes()
		{
			foreach (Barcode bc in barcodeList)
			{
				// aggiungo al datatable solo i barcode validi
				if (bc.Status == BarcodeStatus.OK)
				{
					DataRow dr = dtBarcodes.NewRow();
					dr[CommonStrings.Value] = bc.Value;
					dr[CommonStrings.BarcodeTag] = bc;
					dtBarcodes.Rows.Add(dr);
				}
			}

			this.ValDataGridView.DataSource = dtBarcodes;
		}

		/// <summary>
		/// Define columns style
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void DesignTableStyle()
		{
			// imposto un font piu' piccolo allo stile delle celle delle DataGridView
			DataGridViewCellStyle verdanaCellStyle = new DataGridViewCellStyle();
			verdanaCellStyle.Font = verdanaFont;
			// anche negli header di colonna
			ValDataGridView.ColumnHeadersDefaultCellStyle.Font = verdanaFont;

			// Checkbox se la colonna e' utilizzata come bookmark (colonna nascosta)
			DataGridViewCheckBoxColumn inUseColumn = new DataGridViewCheckBoxColumn();
			inUseColumn.Name = CommonStrings.InUse;
			inUseColumn.DataPropertyName = CommonStrings.InUse;
			inUseColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			inUseColumn.Visible = false;
			ValDataGridView.Columns.Add(inUseColumn);

			// Pulsante con immagine per il valore di default
			DataGridViewCheckImageButtonColumn selectedColumn = new DataGridViewCheckImageButtonColumn();
			selectedColumn.DataPropertyName = CommonStrings.Selected;
			selectedColumn.ValueType = typeof(bool);
			selectedColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			selectedColumn.DefaultCellStyle = verdanaCellStyle;
			ValDataGridView.Columns.Add(selectedColumn);

			// Valore della categoria
			DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn();
			valueColumn.Name = CommonStrings.Value;
			valueColumn.DataPropertyName = CommonStrings.Value;
			valueColumn.HeaderText = Strings.FieldValue;
			valueColumn.Width = 150;
			valueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			valueColumn.ReadOnly = true;
			valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			valueColumn.DefaultCellStyle = verdanaCellStyle;
			ValDataGridView.Columns.Add(valueColumn);
		}

		# region Eventi sul DataGridView
		//--------------------------------------------------------------------------------
		private void ValDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				// click col pulsante su immagine
				if (ValDataGridView.CurrentCell.ColumnIndex == 1)
				{
					if (!(ValDataGridView.CurrentCell is DataGridViewCheckImageButtonCell))
						return;

					// inizializzo l'immagine sul pulsante tramite il ButtonState
					PushButtonState currState = ((DataGridViewCheckImageButtonCell)(ValDataGridView.CurrentRow.Cells[1])).ButtonState;
					if (currState == PushButtonState.Hot)
						((DataGridViewCheckImageButtonCell)(ValDataGridView.CurrentRow.Cells[1])).ButtonState = PushButtonState.Normal;
					if (currState == PushButtonState.Normal)
						((DataGridViewCheckImageButtonCell)(ValDataGridView.CurrentRow.Cells[1])).ButtonState = PushButtonState.Hot;

					// cambio il backcolor della riga se PushButtonState.Hot
					if (((DataGridViewCheckImageButtonCell)(ValDataGridView.CurrentRow.Cells[1])).ButtonState == PushButtonState.Hot)
					{
						this.ValDataGridView.Rows[e.RowIndex].DefaultCellStyle.Font = verdanaBoldFont;
						ValDataGridView.CurrentCell.Value = true;
					}
					else
					{
						this.ValDataGridView.Rows[e.RowIndex].DefaultCellStyle.Font = verdanaFont;
						ValDataGridView.CurrentCell.Value = false;
					}

					// imposto tutte le altre righe con lo sfondo di default, ovvero bianco
					foreach (DataGridViewRow dgvr in ValDataGridView.Rows)
					{
						if (dgvr.Index == ValDataGridView.CurrentCell.RowIndex)
							continue;

						dgvr.DefaultCellStyle.Font = verdanaFont;
						((DataGridViewCheckImageButtonCell)(dgvr.Cells[1])).ButtonState = PushButtonState.Normal;
						dgvr.Cells[1].Value = false;
					}
				}
			}
			catch (ArgumentOutOfRangeException)
			{ }
		}

		//--------------------------------------------------------------------------------
		private void ValDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
		}
		# endregion

		//--------------------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		//--------------------------------------------------------------------------------
		private void BarcodeDetection_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool atLeastOneDefaultRow = false;

			foreach (DataRow dr in dtBarcodes.Rows)
			{
				if (!atLeastOneDefaultRow)
				{
					atLeastOneDefaultRow = ((bool)(dr[CommonStrings.Selected])); // solo la prima volta!!

					if (atLeastOneDefaultRow)
						selectedBarcode = ((Barcode)(dr[CommonStrings.BarcodeTag]));
				}
			}

			// se non ho selezionato un barcode visualizzo un msg e chiedo all'utente se vuole procedere
			if (!atLeastOneDefaultRow)
			{
				DialogResult dr = MessageBox.Show(Strings.BarcodeNotSelected, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.No)
					e.Cancel = true;
			}
		}
	}
}
