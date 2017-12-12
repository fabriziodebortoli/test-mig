using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.MenuManager
{
	//================================================================================
	public partial class AskClosingMenu : Form
	{
		DocumentStates docStates;
		ITheme theme = null;

		//--------------------------------------------------------------------------------
		public AskClosingMenu(string message, DocumentStates docStates)
		{
			InitializeComponent();
			this.docStates = docStates;
			this.labelMessage.Text = message;
			theme = DefaultTheme.GetTheme();
	
			if (docStates != null && docStates.OpenDocumentCount > 0)
			{
				this.checkBoxSaveOpenDocuments.Visible = true;
				this.checkBoxSaveOpenDocuments.Checked = docStates.RestoreOpenDocuments;
			}
			else
			{
				this.checkBoxSaveOpenDocuments.Visible = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//codice commentato per lasciare il default (punto interrogativo) in ogni caso
			//Image imgMenuLoginPanelSmall = theme.GetThemeElementImage("MenuLoginPanelSmall");
			//if (imgMenuLoginPanelSmall != null)
			//{
			//	pictureBox1.Image = imgMenuLoginPanelSmall;
			//}

			try
			{
				Image imgMenuLoginOk = theme.GetThemeElementImage("MenuLoginOk");
				if (imgMenuLoginOk != null)
				{

					btnYes.Image = imgMenuLoginOk;
					btnYes.Text = string.Empty;
				}

				Image imgMenuLoginCancel = theme.GetThemeElementImage("MenuLoginCancel");
				if (imgMenuLoginCancel != null)
				{
					btnNo.Image = imgMenuLoginCancel;
					btnNo.Text = string.Empty;
				}
			}
			catch (Exception)
			{
			}
		}

		//--------------------------------------------------------------------------------
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (DialogResult == System.Windows.Forms.DialogResult.Yes && docStates != null)
				docStates.RestoreOpenDocuments = checkBoxSaveOpenDocuments.Checked;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			pictureBox1.Location = new Point(pictureBox1.Location.X, (this.ClientSize.Height - pictureBox1.Height) / 2);
		}
	}
}