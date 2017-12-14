using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.SecurityAdminPlugIn.WizardForms
{
	public class SecurityWizardForm : WizardForm
	{

		private System.ComponentModel.IContainer components = null;

		private WizardParameters wizardParameters = null;



		public SecurityWizardForm(WizardParameters wizardParameters)
		{
			this.wizardParameters = wizardParameters;
			DataForWizard.Add(wizardParameters);
			InitializeComponent();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		//---------------------------------------------------------------------
		public WizardParameters GetImportSelections()
		{
			return wizardParameters;
		}
		//---------------------------------------------------------------------

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SecurityWizardForm));
			this.SuspendLayout();
			// 
			// m_separator
			// 
			this.m_separator.Location = ((System.Drawing.Point)(resources.GetObject("m_separator.Location")));
			this.m_separator.Name = "m_separator";
			this.m_separator.Size = ((System.Drawing.Size)(resources.GetObject("m_separator.Size")));
			// 
			// m_finishButton
			// 
			this.m_finishButton.Font = ((System.Drawing.Font)(resources.GetObject("m_finishButton.Font")));
			this.m_finishButton.Location = ((System.Drawing.Point)(resources.GetObject("m_finishButton.Location")));
			this.m_finishButton.Name = "m_finishButton";
			this.m_finishButton.Size = ((System.Drawing.Size)(resources.GetObject("m_finishButton.Size")));
			// 
			// m_backButton
			// 
			this.m_backButton.Font = ((System.Drawing.Font)(resources.GetObject("m_backButton.Font")));
			this.m_backButton.Location = ((System.Drawing.Point)(resources.GetObject("m_backButton.Location")));
			this.m_backButton.Name = "m_backButton";
			this.m_backButton.Size = ((System.Drawing.Size)(resources.GetObject("m_backButton.Size")));
			// 
			// m_nextButton
			// 
			this.m_nextButton.Font = ((System.Drawing.Font)(resources.GetObject("m_nextButton.Font")));
			this.m_nextButton.Location = ((System.Drawing.Point)(resources.GetObject("m_nextButton.Location")));
			this.m_nextButton.Name = "m_nextButton";
			this.m_nextButton.Size = ((System.Drawing.Size)(resources.GetObject("m_nextButton.Size")));
			// 
			// m_cancelButton
			// 
			this.m_cancelButton.Font = ((System.Drawing.Font)(resources.GetObject("m_cancelButton.Font")));
			this.m_cancelButton.Location = ((System.Drawing.Point)(resources.GetObject("m_cancelButton.Location")));
			this.m_cancelButton.Name = "m_cancelButton";
			this.m_cancelButton.Size = ((System.Drawing.Size)(resources.GetObject("m_cancelButton.Size")));
			// 
			// m_helpButton
			// 
			this.m_helpButton.Font = ((System.Drawing.Font)(resources.GetObject("m_helpButton.Font")));
			this.m_helpButton.Location = ((System.Drawing.Point)(resources.GetObject("m_helpButton.Location")));
			this.m_helpButton.Name = "m_helpButton";
			this.m_helpButton.Size = ((System.Drawing.Size)(resources.GetObject("m_helpButton.Size")));
			// 
			// SecurityWizardForm
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SecurityWizardForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion
	}
}

