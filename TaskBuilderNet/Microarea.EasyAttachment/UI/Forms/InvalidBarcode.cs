using System.Windows.Forms;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form con la visualizzazione messaggio in caso di errore nel valore del barcode
	///</summary>
	//================================================================================
	public partial class InvalidBarcode : Form
	{
		// proprietà che consente la visualizzazione di visualizzare un messaggio ad-hoc
		// sostituendo quello presente by design nella form
		//--------------------------------------------------------------------------------
		public string SetMessageText 
		{ 
			set
			{ 
				this.LblMessage2.Text = value;
				this.LblMessage2.Location = this.LblMessage1.Location;
				this.LblMessage2.Height = this.LblMessage2.Height + this.LblMessage1.Height;
				this.LblMessage1.Visible = false;
			} 
		}

		//--------------------------------------------------------------------------------
		public InvalidBarcode()
		{
			InitializeComponent();
            this.LblMessage2.Text = string.Format(LblMessage2.Text,Utils.BarcodePrefix);
			this.Text = CommonStrings.EasyAttachmentTitle;
		}

		//--------------------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
