using System;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.Packager
{
	//================================================================================
	internal enum OverwriteResult
	{
		None,
		Yes,
		No,
		YesToAll,
		NoToAll
	}

	//================================================================================
	internal partial class OverwriteFileWindow : ThemedForm
	{
		private OverwriteResult result;

		/// <remarks/>
		public OverwriteResult Result { get { return result; } }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public OverwriteFileWindow(string fileName)
		{
			InitializeComponent();
			lblOverwrite.Text = string.Format(Resources.FileAlreadyExist, fileName, Environment.NewLine);
		}

		//-----------------------------------------------------------------------------
		private void btnYes_Click(object sender, EventArgs e)
		{
			result = OverwriteResult.Yes;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void btnNo_Click(object sender, EventArgs e)
		{
			result = OverwriteResult.No;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void btnYesToAll_Click(object sender, EventArgs e)
		{
			result = OverwriteResult.YesToAll;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void btnNoToAll_Click(object sender, EventArgs e)
		{
			result = OverwriteResult.NoToAll;
			this.Close();
		}

		private const int CP_NOCLOSE_BUTTON = 0x200;
		/// <summary>
		/// Trucco per rendere il bottone di close della finestra disabilitato
		/// </summary>
		//-----------------------------------------------------------------------------
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
				return myCp;
			}
		}
	}
}
