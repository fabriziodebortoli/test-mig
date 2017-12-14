using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Microarea.EasyAttachment.UI.Controls
{
	[System.ComponentModel.DesignerCategory("code")]
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	//================================================================================
	public partial class ToolStripExtensionImageComboBox : ToolStripControlHost
	{
		//--------------------------------------------------------------------------------
		public ToolStripExtensionImageComboBox() : base(new ExtensionImageComboBox()) { }

		// espone all'esterno la combobox
		public ExtensionImageComboBox ExtensionImageComboBoxControl { get { return Control as ExtensionImageComboBox; } }

		public delegate void ExtensionImageComboBoxSelectedIndexDelegate(string extension);
		public event ExtensionImageComboBoxSelectedIndexDelegate ExtensionImageComboBoxSelectedIndexChanged;
		
		/// <summary>
		/// Attach to events we want to re-wrap
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnSubscribeControlEvents(Control control)
		{
			base.OnSubscribeControlEvents(control);

			// Cast the control to a ImageComboBox control.
			ExtensionImageComboBox icb = (ExtensionImageComboBox)control;
			// Add the event.
			icb.ImageComboBoxSelectedIndex += new ExtensionImageComboBox.ImageComboBoxDelegate(ImageComboBox_ImageComboBoxSelectedIndex);
		}

		// ruoto l'evento alla form contenitore
		//--------------------------------------------------------------------------------
		private void ImageComboBox_ImageComboBoxSelectedIndex(EventArgs e)
		{
			// estensione del file selezionata nella combobox
			string searchExtension = ExtensionImageComboBoxControl.SelectedItem.ToString();

			if (ExtensionImageComboBoxSelectedIndexChanged != null)
				ExtensionImageComboBoxSelectedIndexChanged(searchExtension);
		}

		/// <summary>
		/// Detach from events.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnUnsubscribeControlEvents(Control control)
		{
			base.OnUnsubscribeControlEvents(control);

			ExtensionImageComboBox icb = (ExtensionImageComboBox)control;
			icb.ImageComboBoxSelectedIndex -= new ExtensionImageComboBox.ImageComboBoxDelegate(ImageComboBox_ImageComboBoxSelectedIndex);
		}

		// set other defaults that are interesting
		//--------------------------------------------------------------------------------
		protected override Size DefaultSize { get { return new Size(200, 100); } }
	}
}