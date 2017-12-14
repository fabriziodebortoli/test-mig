using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// DocumentListItem
	/// Item di visualizzazione di un documento estratto dalla ricerca e relativi dettagli
	///</summary>
	//================================================================================
	public partial class DocumentListItem : UserControl, IExtendedListItem
	{
		private AttachmentsForErpDocument searchResult = null;
		private Color oldBackColor;
		private string tbDocNamespace = string.Empty;
		private string documentKey = string.Empty;
		
		// Properties
		//--------------------------------------------------------------------------------
		public string DocumentName { get { return documentName.Text; } set { documentName.Text = value; } }
		public string KeyDescription { get { return keyDescription.Text; } set { keyDescription.Text = value; } }

		// Events
		//--------------------------------------------------------------------------------
		public event EventHandler ListItemResizing;
		public event EventHandler DocumentListItemCollapsed;

		public event EventHandler<AttachmentInfoEventArgs> DocumentOpening;
		public event EventHandler<AttachmentInfoEventArgs> DocumentShowingPreview;

		//---------------------------------------------------------------------
		public DocumentListItem(DataRow erpDocument)
		{
			InitializeComponent();			
			Height = 40;
			expandBtn.Visible = false;

			if (erpDocument != null)
			{
				tbDocNamespace = erpDocument[CommonStrings.DocNamespace].ToString();
				DocumentName = CUtility.GetDocumentTitle(tbDocNamespace);
				string keyDescription = erpDocument[CommonStrings.DocKeyDescription].ToString();
				documentKey = erpDocument[CommonStrings.TBPrimaryKey].ToString();
				KeyDescription = (!string.IsNullOrEmpty(keyDescription) || !string.IsNullOrWhiteSpace(keyDescription))
								? keyDescription
								: documentKey;
			}
		}

		//---------------------------------------------------------------------
		public DocumentListItem(string docNamespace, string docKey, string keyDescription)
		{
			InitializeComponent();
			Height = 40;
			expandBtn.Visible = false;

			tbDocNamespace = docNamespace;
			DocumentName = CUtility.GetDocumentTitle(tbDocNamespace);
			documentKey = docKey;
			KeyDescription = (!string.IsNullOrEmpty(keyDescription) || !string.IsNullOrWhiteSpace(keyDescription))
							? keyDescription
							: documentKey;
		}

		//---------------------------------------------------------------------
		public DocumentListItem(string docNamespace, string docKey, string keyDescription, AttachmentsForErpDocument dt, bool bShowAttachment)
		{
			InitializeComponent();

			searchResult = dt;
			Height = 40;

			if (searchResult != null && searchResult.Rows.Count > 0)
			{
				tbDocNamespace = docNamespace;
				documentKey = docKey;
				DocumentName = CUtility.GetDocumentTitle(tbDocNamespace);
				KeyDescription = (!string.IsNullOrEmpty(keyDescription) || !string.IsNullOrWhiteSpace(keyDescription))
								? keyDescription
								: documentKey; 		
			
				// assegno il DataSource con i risultati della ricerca al DataGrid per visualizzare gli attachment
				attachmentGridView.DataSource = searchResult;
				// aggancio gli eventi ruotati sulla selezione del documento
				attachmentGridView.DocumentOpening += new EventHandler<AttachmentInfoEventArgs>(attachmentGridView_DocumentOpening);			
				attachmentGridView.DocumentShowingPreview += new EventHandler<AttachmentInfoEventArgs>(attachmentGridView_DocumentShowingPreview);

				attachmentGridView.Height = attachmentGridView.TotalHeight;
			}
		}

		///<summary>
		/// Ruoto l'evento di richiesta di visualizzazione del file nella preview con il click del mouse
		///</summary>
		//---------------------------------------------------------------------
		private void attachmentGridView_DocumentShowingPreview(object sender, AttachmentInfoEventArgs e)
		{
			if (DocumentShowingPreview != null)
				DocumentShowingPreview(sender, e);
		}

		///<summary>
		/// Ruoto l'evento di richiesta di apertura del file con il doppio click del mouse
		///</summary>
		//---------------------------------------------------------------------
		private void attachmentGridView_DocumentOpening(object sender, AttachmentInfoEventArgs e)
		{
			if (DocumentOpening != null)
				DocumentOpening(sender, e);
		}

		#region IExtendedListItem Members
		//---------------------------------------------------------------------
		public void SelectedChanged(bool isSelected)
		{
		}

		//---------------------------------------------------------------------
		public void PositionChanged(int index)
		{
			if ((index & 1) == 0)
			{
				BackColor = SystemColors.GradientInactiveCaption;
				attachmentGridView.DefaultCellStyle.SelectionBackColor = SystemColors.GradientActiveCaption;
			}
			else
			{
				BackColor = SystemColors.GradientActiveCaption;
				attachmentGridView.DefaultCellStyle.SelectionBackColor = SystemColors.GradientInactiveCaption;
			}

			attachmentGridView.BackgroundColor = BackColor;
			attachmentGridView.GridColor = BackColor;
			oldBackColor = BackColor;
		}
		#endregion

		//---------------------------------------------------------------------
		private void expandBtn_Click(object sender, EventArgs e)
		{
			if (!expandBtn.Visible)
				return;

			if (attachmentGridView.Visible)
			{
				expandBtn.Image = Microarea.EasyAttachment.Properties.Resources.arrowdown16x16;
				documentName.BackColor = oldBackColor;
				Height = 40;
				attachmentGridView.Visible = false;
				// ruoto l'evento di collapse del DocumentListItem
				if (DocumentListItemCollapsed != null)
					DocumentListItemCollapsed(sender, e);
			}
			else
			{
				expandBtn.Image = Microarea.EasyAttachment.Properties.Resources.arrowup16x16;
				Height = Height + attachmentGridView.Height + 2;
				documentName.BackColor = Color.Azure;
				attachmentGridView.Visible = true;
			}

			if (ListItemResizing != null)
				ListItemResizing(this, new EventArgs());
		}

		//--------------------------------------------------------------------------------
		private void documentName_DoubleClick(object sender, EventArgs e)
		{
			expandBtn_Click(sender, e);
		}

		//--------------------------------------------------------------------------------
		private void keyDescription_DoubleClick(object sender, EventArgs e)
		{
			expandBtn_Click(sender, e);
		}

		//--------------------------------------------------------------------------------		
		private void expandBtn_MouseMove(object sender, MouseEventArgs e)
		{
			if (string.Compare(BtnToolTip.GetToolTip(expandBtn), Strings.Expand, StringComparison.InvariantCultureIgnoreCase) != 0)
				BtnToolTip.SetToolTip(expandBtn, Strings.Expand);
		}

		//--------------------------------------------------------------------------------		
		private void OpenMagoDocBtn_MouseMove(object sender, MouseEventArgs e)
		{
			if (string.Compare(BtnToolTip.GetToolTip(OpenMagoDocBtn), Strings.OpenMagoDoc, StringComparison.InvariantCultureIgnoreCase) != 0)
				BtnToolTip.SetToolTip(OpenMagoDocBtn, Strings.OpenMagoDoc);
		}

		//--------------------------------------------------------------------------------		
		private void OpenMagoDocBtn_Click(object sender, EventArgs e)
		{
			Utils.OpenERPDocument(tbDocNamespace, documentKey);
		}	

        //--------------------------------------------------------------------------------		
        public List<AttachmentInfo> GetAttachments()
        {
            List<AttachmentInfo> list = new List<AttachmentInfo>();
            if (searchResult != null && searchResult.Rows.Count > 0)
                foreach (DataRow r in searchResult.Rows)
                    list.Add(r[CommonStrings.AttachmentInfo] as AttachmentInfo);

            return list;//non mi preoccupo di doppi
        }
	    
	}
}