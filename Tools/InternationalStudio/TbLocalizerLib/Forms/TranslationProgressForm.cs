using System;
using System.Globalization;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public class TranslationProgressForm : Form
	{
		#region Controls
		private Button ButtonOk;
		private Button ButtonCopy;
		private Label LabelFooter;
		private ProgressBar ProgressBarTranslationProgress;
		private RadioButton RadioButtonVerbose;
		private RadioButton RadioButtonSynthetic;
		private RichTextBox RichTextBoxOutput;

		private System.ComponentModel.Container components = null;
		#endregion

		#region Private members
		private CultureInfo cultureInfo;
		private int totalWords;
		private int translatedWords;
		private float percentage;
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public TranslationProgressForm()
		{
			InitializeComponent();
			this.Text = Strings.TranslationProgress;
		}
		#endregion

		#region Public methods
		//---------------------------------------------------------------------
		public void Init(CultureInfo cultureInfo, int translatedWords, int totalWords, bool filtered, string[] filters)
		{
			this.cultureInfo = cultureInfo;
			this.translatedWords = translatedWords;
			this.totalWords = totalWords;
			this.Text =
				Strings.TranslationProgress + " (" +
				cultureInfo.Name + ")";

			percentage = 0F;
			if (translatedWords != 0 && totalWords != 0)
				percentage = ((float)translatedWords / (float)totalWords * 100F);
			if (totalWords == 0)
				percentage = 100F;

			string message;
			if (filtered)
			{
				message = Strings.CategoryFilter;
				message += Environment.NewLine;

				foreach (string filter in filters)
				{
					if (filter != filters[0])
						message += ", ";
					message += filter;
				}

				LabelFooter.Text = message;
			}
			else
				message = Strings.NoFilterSelected;

			LabelFooter.Text = message;
			ProgressBarTranslationProgress.Maximum = totalWords;
			ProgressBarTranslationProgress.Value = translatedWords;
			RichTextBoxOutput.Text = GetMessage();

			bool enabled = RichTextBoxOutput.Text.Length > 0;
			RadioButtonVerbose.Enabled = enabled;
			RadioButtonSynthetic.Enabled = enabled;
			ButtonCopy.Enabled = enabled;
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private string GetMessage()
		{
			string message;

			if (RadioButtonVerbose.Checked)
				message =
					string.Format
					(
						Strings.TranslationProgressExplication,
						translatedWords,
						totalWords,
						cultureInfo.EnglishName,
						percentage
					);
			else
				message =
					string.Format
					(
						"{2}: {0}/{1} ({3:0.0}%)",
						translatedWords,
						totalWords,
						cultureInfo.Name,
						percentage
					);

			return message;
		}

		//---------------------------------------------------------------------
		private void OnModeCheckedChanged(object sender, EventArgs e)
		{
			RichTextBoxOutput.Text = GetMessage();
		}

		//---------------------------------------------------------------------
		private void OnCopyClick(object sender, EventArgs e)
		{
			Clipboard.SetDataObject(RichTextBoxOutput.Text, true);
		}
		#endregion

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TranslationProgressForm));
			this.ButtonOk = new System.Windows.Forms.Button();
			this.RichTextBoxOutput = new System.Windows.Forms.RichTextBox();
			this.ProgressBarTranslationProgress = new System.Windows.Forms.ProgressBar();
			this.LabelFooter = new System.Windows.Forms.Label();
			this.RadioButtonVerbose = new System.Windows.Forms.RadioButton();
			this.RadioButtonSynthetic = new System.Windows.Forms.RadioButton();
			this.ButtonCopy = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ButtonOk
			// 
			this.ButtonOk.AccessibleDescription = resources.GetString("ButtonOk.AccessibleDescription");
			this.ButtonOk.AccessibleName = resources.GetString("ButtonOk.AccessibleName");
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ButtonOk.Anchor")));
			this.ButtonOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonOk.BackgroundImage")));
			this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ButtonOk.Dock")));
			this.ButtonOk.Enabled = ((bool)(resources.GetObject("ButtonOk.Enabled")));
			this.ButtonOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ButtonOk.FlatStyle")));
			this.ButtonOk.Font = ((System.Drawing.Font)(resources.GetObject("ButtonOk.Font")));
			this.ButtonOk.Image = ((System.Drawing.Image)(resources.GetObject("ButtonOk.Image")));
			this.ButtonOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ButtonOk.ImageAlign")));
			this.ButtonOk.ImageIndex = ((int)(resources.GetObject("ButtonOk.ImageIndex")));
			this.ButtonOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ButtonOk.ImeMode")));
			this.ButtonOk.Location = ((System.Drawing.Point)(resources.GetObject("ButtonOk.Location")));
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ButtonOk.RightToLeft")));
			this.ButtonOk.Size = ((System.Drawing.Size)(resources.GetObject("ButtonOk.Size")));
			this.ButtonOk.TabIndex = ((int)(resources.GetObject("ButtonOk.TabIndex")));
			this.ButtonOk.Text = resources.GetString("ButtonOk.Text");
			this.ButtonOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ButtonOk.TextAlign")));
			this.ButtonOk.Visible = ((bool)(resources.GetObject("ButtonOk.Visible")));
			// 
			// RichTextBoxOutput
			// 
			this.RichTextBoxOutput.AccessibleDescription = resources.GetString("RichTextBoxOutput.AccessibleDescription");
			this.RichTextBoxOutput.AccessibleName = resources.GetString("RichTextBoxOutput.AccessibleName");
			this.RichTextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RichTextBoxOutput.Anchor")));
			this.RichTextBoxOutput.AutoSize = ((bool)(resources.GetObject("RichTextBoxOutput.AutoSize")));
			this.RichTextBoxOutput.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RichTextBoxOutput.BackgroundImage")));
			this.RichTextBoxOutput.BulletIndent = ((int)(resources.GetObject("RichTextBoxOutput.BulletIndent")));
			this.RichTextBoxOutput.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RichTextBoxOutput.Dock")));
			this.RichTextBoxOutput.Enabled = ((bool)(resources.GetObject("RichTextBoxOutput.Enabled")));
			this.RichTextBoxOutput.Font = ((System.Drawing.Font)(resources.GetObject("RichTextBoxOutput.Font")));
			this.RichTextBoxOutput.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RichTextBoxOutput.ImeMode")));
			this.RichTextBoxOutput.Location = ((System.Drawing.Point)(resources.GetObject("RichTextBoxOutput.Location")));
			this.RichTextBoxOutput.MaxLength = ((int)(resources.GetObject("RichTextBoxOutput.MaxLength")));
			this.RichTextBoxOutput.Multiline = ((bool)(resources.GetObject("RichTextBoxOutput.Multiline")));
			this.RichTextBoxOutput.Name = "RichTextBoxOutput";
			this.RichTextBoxOutput.ReadOnly = true;
			this.RichTextBoxOutput.RightMargin = ((int)(resources.GetObject("RichTextBoxOutput.RightMargin")));
			this.RichTextBoxOutput.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RichTextBoxOutput.RightToLeft")));
			this.RichTextBoxOutput.ScrollBars = ((System.Windows.Forms.RichTextBoxScrollBars)(resources.GetObject("RichTextBoxOutput.ScrollBars")));
			this.RichTextBoxOutput.Size = ((System.Drawing.Size)(resources.GetObject("RichTextBoxOutput.Size")));
			this.RichTextBoxOutput.TabIndex = ((int)(resources.GetObject("RichTextBoxOutput.TabIndex")));
			this.RichTextBoxOutput.Text = resources.GetString("RichTextBoxOutput.Text");
			this.RichTextBoxOutput.Visible = ((bool)(resources.GetObject("RichTextBoxOutput.Visible")));
			this.RichTextBoxOutput.WordWrap = ((bool)(resources.GetObject("RichTextBoxOutput.WordWrap")));
			this.RichTextBoxOutput.ZoomFactor = ((System.Single)(resources.GetObject("RichTextBoxOutput.ZoomFactor")));
			// 
			// ProgressBarTranslationProgress
			// 
			this.ProgressBarTranslationProgress.AccessibleDescription = resources.GetString("ProgressBarTranslationProgress.AccessibleDescription");
			this.ProgressBarTranslationProgress.AccessibleName = resources.GetString("ProgressBarTranslationProgress.AccessibleName");
			this.ProgressBarTranslationProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ProgressBarTranslationProgress.Anchor")));
			this.ProgressBarTranslationProgress.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ProgressBarTranslationProgress.BackgroundImage")));
			this.ProgressBarTranslationProgress.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ProgressBarTranslationProgress.Dock")));
			this.ProgressBarTranslationProgress.Enabled = ((bool)(resources.GetObject("ProgressBarTranslationProgress.Enabled")));
			this.ProgressBarTranslationProgress.Font = ((System.Drawing.Font)(resources.GetObject("ProgressBarTranslationProgress.Font")));
			this.ProgressBarTranslationProgress.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ProgressBarTranslationProgress.ImeMode")));
			this.ProgressBarTranslationProgress.Location = ((System.Drawing.Point)(resources.GetObject("ProgressBarTranslationProgress.Location")));
			this.ProgressBarTranslationProgress.Name = "ProgressBarTranslationProgress";
			this.ProgressBarTranslationProgress.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ProgressBarTranslationProgress.RightToLeft")));
			this.ProgressBarTranslationProgress.Size = ((System.Drawing.Size)(resources.GetObject("ProgressBarTranslationProgress.Size")));
			this.ProgressBarTranslationProgress.TabIndex = ((int)(resources.GetObject("ProgressBarTranslationProgress.TabIndex")));
			this.ProgressBarTranslationProgress.Text = resources.GetString("ProgressBarTranslationProgress.Text");
			this.ProgressBarTranslationProgress.Visible = ((bool)(resources.GetObject("ProgressBarTranslationProgress.Visible")));
			// 
			// LabelFooter
			// 
			this.LabelFooter.AccessibleDescription = resources.GetString("LabelFooter.AccessibleDescription");
			this.LabelFooter.AccessibleName = resources.GetString("LabelFooter.AccessibleName");
			this.LabelFooter.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LabelFooter.Anchor")));
			this.LabelFooter.AutoSize = ((bool)(resources.GetObject("LabelFooter.AutoSize")));
			this.LabelFooter.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LabelFooter.Dock")));
			this.LabelFooter.Enabled = ((bool)(resources.GetObject("LabelFooter.Enabled")));
			this.LabelFooter.Font = ((System.Drawing.Font)(resources.GetObject("LabelFooter.Font")));
			this.LabelFooter.Image = ((System.Drawing.Image)(resources.GetObject("LabelFooter.Image")));
			this.LabelFooter.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelFooter.ImageAlign")));
			this.LabelFooter.ImageIndex = ((int)(resources.GetObject("LabelFooter.ImageIndex")));
			this.LabelFooter.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LabelFooter.ImeMode")));
			this.LabelFooter.Location = ((System.Drawing.Point)(resources.GetObject("LabelFooter.Location")));
			this.LabelFooter.Name = "LabelFooter";
			this.LabelFooter.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LabelFooter.RightToLeft")));
			this.LabelFooter.Size = ((System.Drawing.Size)(resources.GetObject("LabelFooter.Size")));
			this.LabelFooter.TabIndex = ((int)(resources.GetObject("LabelFooter.TabIndex")));
			this.LabelFooter.Text = resources.GetString("LabelFooter.Text");
			this.LabelFooter.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelFooter.TextAlign")));
			this.LabelFooter.Visible = ((bool)(resources.GetObject("LabelFooter.Visible")));
			// 
			// RadioButtonVerbose
			// 
			this.RadioButtonVerbose.AccessibleDescription = resources.GetString("RadioButtonVerbose.AccessibleDescription");
			this.RadioButtonVerbose.AccessibleName = resources.GetString("RadioButtonVerbose.AccessibleName");
			this.RadioButtonVerbose.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioButtonVerbose.Anchor")));
			this.RadioButtonVerbose.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioButtonVerbose.Appearance")));
			this.RadioButtonVerbose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioButtonVerbose.BackgroundImage")));
			this.RadioButtonVerbose.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonVerbose.CheckAlign")));
			this.RadioButtonVerbose.Checked = true;
			this.RadioButtonVerbose.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioButtonVerbose.Dock")));
			this.RadioButtonVerbose.Enabled = ((bool)(resources.GetObject("RadioButtonVerbose.Enabled")));
			this.RadioButtonVerbose.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioButtonVerbose.FlatStyle")));
			this.RadioButtonVerbose.Font = ((System.Drawing.Font)(resources.GetObject("RadioButtonVerbose.Font")));
			this.RadioButtonVerbose.Image = ((System.Drawing.Image)(resources.GetObject("RadioButtonVerbose.Image")));
			this.RadioButtonVerbose.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonVerbose.ImageAlign")));
			this.RadioButtonVerbose.ImageIndex = ((int)(resources.GetObject("RadioButtonVerbose.ImageIndex")));
			this.RadioButtonVerbose.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioButtonVerbose.ImeMode")));
			this.RadioButtonVerbose.Location = ((System.Drawing.Point)(resources.GetObject("RadioButtonVerbose.Location")));
			this.RadioButtonVerbose.Name = "RadioButtonVerbose";
			this.RadioButtonVerbose.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioButtonVerbose.RightToLeft")));
			this.RadioButtonVerbose.Size = ((System.Drawing.Size)(resources.GetObject("RadioButtonVerbose.Size")));
			this.RadioButtonVerbose.TabIndex = ((int)(resources.GetObject("RadioButtonVerbose.TabIndex")));
			this.RadioButtonVerbose.TabStop = true;
			this.RadioButtonVerbose.Text = resources.GetString("RadioButtonVerbose.Text");
			this.RadioButtonVerbose.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonVerbose.TextAlign")));
			this.RadioButtonVerbose.Visible = ((bool)(resources.GetObject("RadioButtonVerbose.Visible")));
			this.RadioButtonVerbose.CheckedChanged += new System.EventHandler(this.OnModeCheckedChanged);
			// 
			// RadioButtonSynthetic
			// 
			this.RadioButtonSynthetic.AccessibleDescription = resources.GetString("RadioButtonSynthetic.AccessibleDescription");
			this.RadioButtonSynthetic.AccessibleName = resources.GetString("RadioButtonSynthetic.AccessibleName");
			this.RadioButtonSynthetic.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioButtonSynthetic.Anchor")));
			this.RadioButtonSynthetic.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioButtonSynthetic.Appearance")));
			this.RadioButtonSynthetic.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioButtonSynthetic.BackgroundImage")));
			this.RadioButtonSynthetic.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonSynthetic.CheckAlign")));
			this.RadioButtonSynthetic.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioButtonSynthetic.Dock")));
			this.RadioButtonSynthetic.Enabled = ((bool)(resources.GetObject("RadioButtonSynthetic.Enabled")));
			this.RadioButtonSynthetic.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioButtonSynthetic.FlatStyle")));
			this.RadioButtonSynthetic.Font = ((System.Drawing.Font)(resources.GetObject("RadioButtonSynthetic.Font")));
			this.RadioButtonSynthetic.Image = ((System.Drawing.Image)(resources.GetObject("RadioButtonSynthetic.Image")));
			this.RadioButtonSynthetic.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonSynthetic.ImageAlign")));
			this.RadioButtonSynthetic.ImageIndex = ((int)(resources.GetObject("RadioButtonSynthetic.ImageIndex")));
			this.RadioButtonSynthetic.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioButtonSynthetic.ImeMode")));
			this.RadioButtonSynthetic.Location = ((System.Drawing.Point)(resources.GetObject("RadioButtonSynthetic.Location")));
			this.RadioButtonSynthetic.Name = "RadioButtonSynthetic";
			this.RadioButtonSynthetic.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioButtonSynthetic.RightToLeft")));
			this.RadioButtonSynthetic.Size = ((System.Drawing.Size)(resources.GetObject("RadioButtonSynthetic.Size")));
			this.RadioButtonSynthetic.TabIndex = ((int)(resources.GetObject("RadioButtonSynthetic.TabIndex")));
			this.RadioButtonSynthetic.Text = resources.GetString("RadioButtonSynthetic.Text");
			this.RadioButtonSynthetic.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioButtonSynthetic.TextAlign")));
			this.RadioButtonSynthetic.Visible = ((bool)(resources.GetObject("RadioButtonSynthetic.Visible")));
			this.RadioButtonSynthetic.CheckedChanged += new System.EventHandler(this.OnModeCheckedChanged);
			// 
			// ButtonCopy
			// 
			this.ButtonCopy.AccessibleDescription = resources.GetString("ButtonCopy.AccessibleDescription");
			this.ButtonCopy.AccessibleName = resources.GetString("ButtonCopy.AccessibleName");
			this.ButtonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ButtonCopy.Anchor")));
			this.ButtonCopy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ButtonCopy.BackgroundImage")));
			this.ButtonCopy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ButtonCopy.Dock")));
			this.ButtonCopy.Enabled = ((bool)(resources.GetObject("ButtonCopy.Enabled")));
			this.ButtonCopy.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ButtonCopy.FlatStyle")));
			this.ButtonCopy.Font = ((System.Drawing.Font)(resources.GetObject("ButtonCopy.Font")));
			this.ButtonCopy.Image = ((System.Drawing.Image)(resources.GetObject("ButtonCopy.Image")));
			this.ButtonCopy.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ButtonCopy.ImageAlign")));
			this.ButtonCopy.ImageIndex = ((int)(resources.GetObject("ButtonCopy.ImageIndex")));
			this.ButtonCopy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ButtonCopy.ImeMode")));
			this.ButtonCopy.Location = ((System.Drawing.Point)(resources.GetObject("ButtonCopy.Location")));
			this.ButtonCopy.Name = "ButtonCopy";
			this.ButtonCopy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ButtonCopy.RightToLeft")));
			this.ButtonCopy.Size = ((System.Drawing.Size)(resources.GetObject("ButtonCopy.Size")));
			this.ButtonCopy.TabIndex = ((int)(resources.GetObject("ButtonCopy.TabIndex")));
			this.ButtonCopy.Text = resources.GetString("ButtonCopy.Text");
			this.ButtonCopy.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ButtonCopy.TextAlign")));
			this.ButtonCopy.Visible = ((bool)(resources.GetObject("ButtonCopy.Visible")));
			this.ButtonCopy.Click += new System.EventHandler(this.OnCopyClick);
			// 
			// TranslationProgressForm
			// 
			this.AcceptButton = this.ButtonOk;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.ButtonCopy);
			this.Controls.Add(this.RadioButtonSynthetic);
			this.Controls.Add(this.RadioButtonVerbose);
			this.Controls.Add(this.LabelFooter);
			this.Controls.Add(this.ProgressBarTranslationProgress);
			this.Controls.Add(this.RichTextBoxOutput);
			this.Controls.Add(this.ButtonOk);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "TranslationProgressForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion
	}
}
