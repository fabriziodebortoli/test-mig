namespace Microarea.EasyAttachment.UI.Forms
{
	partial class MonthCalendarForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonthCalendarForm));
			this.DateLabel = new System.Windows.Forms.Label();
			this.OptionsListView = new System.Windows.Forms.ListView();
			this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.MCalendar = new System.Windows.Forms.MonthCalendar();
			this.BorderPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// DateLabel
			// 
			resources.ApplyResources(this.DateLabel, "DateLabel");
			this.DateLabel.Name = "DateLabel";
			// 
			// OptionsListView
			// 
			this.OptionsListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.OptionsListView.BackColor = System.Drawing.Color.Lavender;
			this.OptionsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.OptionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1});
			this.OptionsListView.FullRowSelect = true;
			this.OptionsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.OptionsListView.HoverSelection = true;
			resources.ApplyResources(this.OptionsListView, "OptionsListView");
			this.OptionsListView.MultiSelect = false;
			this.OptionsListView.Name = "OptionsListView";
			this.OptionsListView.UseCompatibleStateImageBehavior = false;
			this.OptionsListView.View = System.Windows.Forms.View.Details;
			this.OptionsListView.Click += new System.EventHandler(this.OptionsListView_Click);
			// 
			// ColumnHeader1
			// 
			resources.ApplyResources(this.ColumnHeader1, "ColumnHeader1");
			// 
			// MCalendar
			// 
			this.MCalendar.BackColor = System.Drawing.SystemColors.Window;
			resources.ApplyResources(this.MCalendar, "MCalendar");
			this.MCalendar.MaxSelectionCount = 365;
			this.MCalendar.Name = "MCalendar";
			this.MCalendar.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.MCalendar_DateChanged);
			this.MCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.MCalendar_DateSelected);
			// 
			// BorderPanel
			// 
			this.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.BorderPanel, "BorderPanel");
			this.BorderPanel.Name = "BorderPanel";
			// 
			// MonthCalendarForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.OptionsListView);
			this.Controls.Add(this.MCalendar);
			this.Controls.Add(this.DateLabel);
			this.Controls.Add(this.BorderPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "MonthCalendarForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Deactivate += new System.EventHandler(this.MonthCalendarForm_Deactivate);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MonthCalendarForm_FormClosed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label DateLabel;
		private System.Windows.Forms.ListView OptionsListView;
		private System.Windows.Forms.ColumnHeader ColumnHeader1;
		private System.Windows.Forms.MonthCalendar MCalendar;
		private System.Windows.Forms.Panel BorderPanel;
	}
}