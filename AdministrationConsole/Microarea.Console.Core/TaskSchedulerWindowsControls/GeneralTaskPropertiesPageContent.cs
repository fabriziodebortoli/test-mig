using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	public delegate void TaskCodeValidationEventHandler(object sender, bool valid);
	//============================================================================================
	/// <summary>
	/// Summary description for GeneralTaskPropertiesPageContent.
	/// </summary>
	public partial class GeneralTaskPropertiesPageContent : System.Windows.Forms.UserControl
	{
		private WTEScheduledTaskObj	task = null;
		private string			currentConnectionString = String.Empty;

		private bool showingAdvancedDetails = false;

		public event TaskCodeValidationEventHandler OnValidatedCode;

		//--------------------------------------------------------------------------------------------------------------------------------
		public GeneralTaskPropertiesPageContent()
		{
			InitializeComponent();

			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.GenericTask.bmp");
			if (imageStream != null)
				TaskPictureBox.Image = Image.FromStream(imageStream);

			RetryAttemptsNumberNumericUpDown.Maximum = WTEScheduledTask.RetryAttemptsMaxNumber;
			RetryDelayNumericUpDown.Maximum = WTEScheduledTask.RetryDelayMaximum;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string CodeText
		{
			get { return CodeTextBox.Text; }
			set { CodeTextBox.Text = value;}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string CurrentConnectionString
		{
			get { return currentConnectionString; }
			set { currentConnectionString = value;}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj Task
		{
			get { return task; }
			set 
			{
				task = value;

				CodeTextBox.Text = (task != null) ? task.Code : String.Empty;
				CodeTextBox.Enabled = (task != null && (task.Code == null || task.Code == String.Empty));
			
				EnabledCheckBox.Checked = (task != null && task.Enabled);

				DescriptionTextBox.Text = (task != null) ? task.Description : String.Empty;

				RetryAttemptsNumberNumericUpDown.Value = (task != null) ? task.RetryAttempts : 0;
				RetryDelayNumericUpDown.Value = (task != null) ? task.RetryDelay : 0;
			}
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetFocusToCodeTextBox()
		{
			CodeTextBox.Focus();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool ValidateCodeText()
		{
            return true;
			//return !ScheduledTask.Exists(CodeTextBox.Text, 
			//							task.CompanyId, 
			//							task.LoginId, 
			//							currentConnectionString);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			showingAdvancedDetails = this.DesignMode;
			RetryAttemptsGroupBox.Visible = showingAdvancedDetails;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnValidated(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnValidated(e);

			if (task == null)
				return;

			if
				(
				(String.Compare(CodeTextBox.Text, task.Code, true, CultureInfo.InvariantCulture) != 0) &&
				(!ValidateCodeText() || !task.SetCode(CodeTextBox.Text, currentConnectionString))
				)
			{	
				if (OnValidatedCode != null)
					OnValidatedCode(this, false);
				return;
			}

			task.Enabled = EnabledCheckBox.Checked;
			task.Description = DescriptionTextBox.Text;
			task.RetryAttempts = Decimal.ToInt32(RetryAttemptsNumberNumericUpDown.Value);
			task.RetryDelay = Decimal.ToInt32(RetryDelayNumericUpDown.Value);

			if (OnValidatedCode != null)
				OnValidatedCode(this, true);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AdvancedDetailsButton_Click(object sender, System.EventArgs e)
		{
			showingAdvancedDetails = !showingAdvancedDetails;
			
			RetryAttemptsGroupBox.Visible = showingAdvancedDetails;

			if (showingAdvancedDetails)
				AdvancedDetailsButton.Text = TaskSchedulerWindowsControlsStrings.ShowingAdvancedDetailsButtonText;
			else
				AdvancedDetailsButton.Text = TaskSchedulerWindowsControlsStrings.NotShowingAdvancedDetailsButtonText;
		}
	}
}
