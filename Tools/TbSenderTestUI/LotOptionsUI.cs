using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;

namespace TbSenderTestUI
{
	public partial class LotOptionsUI : UserControl
	{
		//-------------------------------------------------------------------------------
		public LotOptionsUI()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		private void LotOptionsUI_Load(object sender, EventArgs e)
		{
			this.cmbDeliveryType.DataSource = (Delivery[])Enum.GetValues(typeof(Delivery));
			this.cmbPrintingType.DataSource = (Printing[])Enum.GetValues(typeof(Printing));
		}
	}
}
