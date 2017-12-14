using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Controls
{
    //--------------------------------------------------------------------------------
    public partial class MassiveAttachDetailsListItem : UserControl, IExtendedListItem
	{
		private ERPDocumentBarcode erpDocumentBarcode;
		private string tbDocNamespace = string.Empty;
		private string documentKey = string.Empty;

		private Color oldBackColor;

		// Events
		//--------------------------------------------------------------------------------
		public event EventHandler ListItemResizing;
	
		
		// Properties
		//--------------------------------------------------------------------------------
		public string DocumentName { get { return documentName.Text; } set { documentName.Text = value; } }
		public string KeyDescription { get { return keyDescription.Text; } set { keyDescription.Text = value; } }


		//-----------------------------------------------------------------------------------------
		public MassiveAttachDetailsListItem(ERPDocumentBarcode document)
		{
			InitializeComponent();
			erpDocumentBarcode = document;
			if (erpDocumentBarcode.ErpDocDiagnostic.Error)
			{
				resultLabel.Text = Strings.ErrorCreatingAttachment; ;
				imageBtn.Image = Microarea.EasyAttachment.Properties.Resources.KO;
				showErrorBtn.Visible = true;
			}
            
			tbDocNamespace = erpDocumentBarcode.Namespace;
			DocumentName = CUtility.GetDocumentTitle(tbDocNamespace);
			documentKey = erpDocumentBarcode.PK;
			KeyDescription = documentKey;

            if (ListItemResizing != null) 
				ListItemResizing(this, new EventArgs());
		}


		//---------------------------------------------------------------------
		public void SelectedChanged(bool isSelected)
		{
		}

		//---------------------------------------------------------------------
		public void PositionChanged(int index)
		{
			if ((index & 1) == 0)
				BackColor = SystemColors.GradientInactiveCaption;
			else
				BackColor = SystemColors.GradientActiveCaption;
			
			oldBackColor = BackColor;
		}

		//-----------------------------------------------------------------------------------------
		private void OpenMagoDocBtn_Click(object sender, EventArgs e)
		{
			Utils.OpenERPDocument(tbDocNamespace, documentKey);			
		}		

		//-----------------------------------------------------------------------------------------
		private void showErrorBtn_Click(object sender, EventArgs e)
		{
			 using (SafeThreadCallContext context = new SafeThreadCallContext())
				 DiagnosticViewer.ShowDiagnostic(erpDocumentBarcode.ErpDocDiagnostic);			
		}

		//-----------------------------------------------------------------------------------------
		private void imageBtn_Click(object sender, EventArgs e)
		{
			OpenMagoDocBtn_Click(sender, e);
		}
	}
}
