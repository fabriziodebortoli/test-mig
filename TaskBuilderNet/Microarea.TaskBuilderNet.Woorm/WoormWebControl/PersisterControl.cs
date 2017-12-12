using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	
	/// <summary>
	/// Descrizione di riepilogo per WebForm.
	/// </summary>
	/// ================================================================================
	public class PersisterControl : Control, INamingContainer
	{

		private short			tabIndex = 1;
		private TextBox			descriptionTextBox;
		private CheckBox		allUsersCheckBox;
		private	System.Web.UI.WebControls.Table			formTable;

		WoormDocument woormViewer;
		WoormWebControl woormWebControl;
		
		//--------------------------------------------------------------------------
		WoormWebControl WoormWebControl { get { return woormWebControl; } }
		RSEngine	StateMachine	{ get { return WoormWebControl == null ? null : WoormWebControl.StateMachine; }}
		TbReportSession Session			{ get { return woormViewer.ReportSession; }}

		//--------------------------------------------------------------------------
		public PersisterControl(WoormDocument woormViewer, WoormWebControl woormWebControl)
		{
			this.woormViewer = woormViewer;
			this.woormWebControl = woormWebControl;
		}

		
		//--------------------------------------------------------------------------
		private void SubmitClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			string description = descriptionTextBox.Text;
			if (allUsersCheckBox.Checked)
				woormViewer.SaveForUser(NameSolverStrings.AllUsers, description);
			else
				woormViewer.SaveForUser(Session.UserInfo.LoginManager.UserName, description);

			CloseForm();
		}
		
		//--------------------------------------------------------------------------
		private void CancelClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;
			
			CloseForm();
		}

		//--------------------------------------------------------------------------------
		private void CloseForm()
		{
			StateMachine.CurrentState = State.EndPersister;
			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}

		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			// esterno a tutto
			Panel bigPanel = new Panel();
			Controls.Add(bigPanel);

			formTable = new System.Web.UI.WebControls.Table();
			formTable.Attributes["align"] = "center";
			formTable.Attributes["valign"] = "center";
			formTable.CssClass = "PersistentControlTable";
			bigPanel.Controls.Add(formTable);

			descriptionTextBox = TextBox(WoormWebControlStrings.InsertDescription);
			allUsersCheckBox = CheckBox(WoormWebControlStrings.AllUsers);	

			if (woormViewer.CanSaveForAllUsers && woormViewer.CanSaveForUser)
			{
				allUsersCheckBox.Checked = false;
				allUsersCheckBox.Enabled = true;
			}
			else
			if (!woormViewer.CanSaveForAllUsers && woormViewer.CanSaveForUser)
			{
				allUsersCheckBox.Checked = false;
				allUsersCheckBox.Visible = false;
			}

			OkCancelButton();
		}

		//--------------------------------------------------------------------------
		private TextBox TextBox(string text)
		{
			TableRow titleRow = new TableRow();
			titleRow.CssClass = "PersistentControlTitle";
			formTable.Controls.Add(titleRow);
		
			TableCell titleCell = new TableCell();
			titleCell.CssClass = "PersistentControlLabel";
			titleCell.Attributes["align"] = "center";
			titleRow.Controls.Add(titleCell);

			System.Web.UI.WebControls.Label titleLabel = new System.Web.UI.WebControls.Label();
			titleLabel.Text = text;
			titleCell.Controls.Add(titleLabel);

			TableRow inputRow = new TableRow();
			inputRow.CssClass = "ControlClass";
			inputRow.Font.Bold = true;
			formTable.Controls.Add(inputRow);
		
			TableCell inputCell = new TableCell();
			inputCell.CssClass = "ControlClass";
			inputCell.Attributes["align"] = "center";
			inputRow.Controls.Add(inputCell);

			TextBox textBox = new TextBox();
			textBox.TabIndex = tabIndex++;
			textBox.Columns = 50;
			textBox.MaxLength = 100;
			textBox.TextMode = TextBoxMode.MultiLine;
			textBox.Rows = 5;
			textBox.Wrap = true;
			textBox.ID = "Description";

			inputCell.Controls.Add(textBox);
			return textBox;
		}

		//--------------------------------------------------------------------------
		private CheckBox CheckBox(string text)
		{
			TableRow checkRow = new TableRow();
			checkRow.CssClass = "ControlClass";
			formTable.Controls.Add(checkRow);
		
			TableCell checkCell = new TableCell();
			checkCell.Font.Name = "Verdana";
			checkCell.Attributes["align"] = "center";
			checkRow.Controls.Add(checkCell);

			CheckBox checkBox = new CheckBox();
			checkBox.TabIndex = tabIndex++;
			checkBox.Text = text;
			checkBox.ID = "AllUsers";

			checkCell.Controls.Add(checkBox);

			return checkBox;
		}

		//--------------------------------------------------------------------------
		private void OkCancelButton()
		{
			TableRow buttonRow = new TableRow();
			formTable.Controls.Add(buttonRow);

			TableCell buttonCell = new TableCell();
			buttonCell.Attributes["align"] = "center";
			buttonRow.Controls.Add(buttonCell);

			Button Submit = new Button();
			Submit.Text = WoormWebControlStrings.Ok;
			Submit.TabIndex = tabIndex++;
			Submit.Width = Unit.Pixel(100);
			Submit.Click += new EventHandler(this.SubmitClick);
			buttonCell.Controls.Add(Submit);

			Button Cancel = new Button();
			Cancel.Text = WoormWebControlStrings.Cancel;
			Cancel.TabIndex = tabIndex++;
			Cancel.Width = Unit.Pixel(100);
			Cancel.Click += new EventHandler(this.CancelClick);
			buttonCell.Controls.Add(Cancel);
		}		
	}
}
