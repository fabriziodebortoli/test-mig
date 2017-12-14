using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// DataGridView che mostra l'elenco degli attachment di un documento di Mago
	/// Visualizzato nella finestra di Search
	///</summary>
	//================================================================================
	public partial class ResultDataGridView : DataGridView
	{
		private int height = 0;

		// properties
		//--------------------------------------------------------------------------------
		public int TotalHeight { get { return height; } }

		// Events
		//--------------------------------------------------------------------------------
		public event EventHandler<AttachmentInfoEventArgs> DocumentOpening;
		public event EventHandler<AttachmentInfoEventArgs> DocumentShowingPreview;

		/// <summary>
		/// ResultDataGridView constructor
		/// </summary>
		//--------------------------------------------------------------------------------------
		public ResultDataGridView()
		{
			InitializeComponent();

			AutoGenerateColumns = false;

			if (!DesignMode)
				DesignTableStyle();

			CellFormatting += new DataGridViewCellFormattingEventHandler(FieldsDataGridView_CellFormatting);
		}


        Font f = new Font("Verdana", 6.75F);
		/// <summary>
		/// Define columns style
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void DesignTableStyle()
		{
			DataGridViewCellStyle verdanaStyle = new DataGridViewCellStyle();
			verdanaStyle.Font =f;

			//type field image column
			DataGridViewImageColumn typeColumn = new DataGridViewImageColumn();
			typeColumn.Name = CommonStrings.ExtensionType;
			typeColumn.DataPropertyName = CommonStrings.ExtensionType;
			//typeColumn.DefaultCellStyle.NullValue = null;
			typeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			typeColumn.Width = 25;
			typeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
			typeColumn.Resizable = DataGridViewTriState.True;
			Columns.Add(typeColumn);

			//description field column
			DataGridViewTextBoxColumn descriColumn = new DataGridViewTextBoxColumn();
			descriColumn.Name = CommonStrings.Name;
			descriColumn.DataPropertyName = CommonStrings.Name;
			descriColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			descriColumn.Width = 230;
			descriColumn.MinimumWidth = 50;
			descriColumn.SortMode = DataGridViewColumnSortMode.Automatic;
			descriColumn.DefaultCellStyle = verdanaStyle;
			descriColumn.Resizable = DataGridViewTriState.True;
			Columns.Add(descriColumn);

			//date field column
			DataGridViewTextBoxColumn dateColumn = new DataGridViewTextBoxColumn();
			dateColumn.Name = CommonStrings.AttachmentDate;
			dateColumn.DataPropertyName = CommonStrings.AttachmentDate;
			dateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			dateColumn.Width = 110;
			dateColumn.SortMode = DataGridViewColumnSortMode.Automatic;
			dateColumn.DefaultCellStyle = verdanaStyle;
			dateColumn.Resizable = DataGridViewTriState.True;
			Columns.Add(dateColumn);
		}

		/// <summary>
		/// Show a different bitmap for each file type
		/// </summary>
		//---------------------------------------------------------------------
		private void FieldsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (Columns[e.ColumnIndex].Name.Equals(CommonStrings.ExtensionType))
			{
				if (e.Value == null)
					return;

				DataGridViewCell cell = this[e.ColumnIndex, e.RowIndex];
				string cellValue = e.Value.ToString();
				cell.ToolTipText = cellValue;
				e.Value = Utils.GetSmallImage(cellValue);
			}
		}

		//---------------------------------------------------------------------
		private void ResultDataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			DataGridViewRow addedRow = Rows[e.RowIndex];
			height += addedRow.Height;
		}

		///<summary>
		/// Intercetto il doppio click del mouse sul datagrid, individuo la riga selezionata e 
		/// ruoto le informazioni del file da visualizzare all'esterno
		///</summary>
		//---------------------------------------------------------------------
		private void ResultDataGridView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) 
				return;

			if (DataMember == null || DataSource == null)
				return;

			AttachmentInfo ai = LoadCurrentAttachmentInfo();
			if (ai == null)
				return;

			AttachmentInfoEventArgs arg = new AttachmentInfoEventArgs();
			arg.CurrentAttachment = ai;

			// ruoto l'evento alla finestra di Search
			if (DocumentOpening != null)
				DocumentOpening(sender, arg);
		}

		///<summary>
		/// Intercetto il click del mouse sul datagrid, individuo la riga selezionata e 
		/// ruoto le informazioni del file da visualizzare dentro il GdViewer
		///</summary>
		//---------------------------------------------------------------------
		private void ResultDataGridView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				return;

			if (DataMember == null || DataSource == null)
				return;

			AttachmentInfo ai = LoadCurrentAttachmentInfo();
			if (ai == null)
				return;

			AttachmentInfoEventArgs arg = new AttachmentInfoEventArgs();
			arg.CurrentAttachment = ai;

			// ruoto l'evento alla finestra di Search
			if (DocumentShowingPreview != null)
				DocumentShowingPreview(sender, arg);
		}

		///<summary>
		/// Individuo la riga corrente nel DataGrid, estrapolo l'oggetto AttachmentInfo 
		/// in essa memorizzato e lo ruoto esternamente ai componenti esterni che si occuperanno di
		/// visualizzarlo
		///</summary>
		//---------------------------------------------------------------------
		private AttachmentInfo LoadCurrentAttachmentInfo()
		{
			Point pt = PointToClient(Control.MousePosition);
			DataGridView.HitTestInfo hti = HitTest(pt.X, pt.Y);
			if (hti.ColumnIndex == -1 || hti.RowIndex == -1)
				return null;

			BindingManagerBase bmb = this.BindingContext[DataSource, DataMember];
			this.CurrentCell = this[hti.ColumnIndex, hti.RowIndex];

			AttachmentInfo ai = null; 

			try
			{
				DataRow dr = ((DataRowView)bmb.Current).Row;
				if (dr == null)
					return null;

				ai = dr[CommonStrings.AttachmentInfo] as AttachmentInfo;
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.ToString());
			}

			return ai;
		}
	}
}