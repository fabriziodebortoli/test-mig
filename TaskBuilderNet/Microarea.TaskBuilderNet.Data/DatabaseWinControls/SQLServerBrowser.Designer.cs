namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	partial class SQLServerBrowser
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
			this.ServersTabControl = new System.Windows.Forms.TabControl();
			this.LocalServersTab = new System.Windows.Forms.TabPage();
			this.LocalServersListBox = new System.Windows.Forms.ListBox();
			this.LocalLabel = new System.Windows.Forms.Label();
			this.NetworkServersTab = new System.Windows.Forms.TabPage();
			this.NetworkServersListBox = new System.Windows.Forms.ListBox();
			this.NetworkLabel = new System.Windows.Forms.Label();
			this.OKButton = new System.Windows.Forms.Button();
			this.CancelButton1 = new System.Windows.Forms.Button();
			this.ServersTabControl.SuspendLayout();
			this.LocalServersTab.SuspendLayout();
			this.NetworkServersTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// ServersTabControl
			// 
			this.ServersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ServersTabControl.Controls.Add(this.LocalServersTab);
			this.ServersTabControl.Controls.Add(this.NetworkServersTab);
			this.ServersTabControl.Location = new System.Drawing.Point(8, 8);
			this.ServersTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.ServersTabControl.Name = "ServersTabControl";
			this.ServersTabControl.SelectedIndex = 0;
			this.ServersTabControl.Size = new System.Drawing.Size(335, 317);
			this.ServersTabControl.TabIndex = 0;
			// 
			// LocalServersTab
			// 
			this.LocalServersTab.BackColor = System.Drawing.SystemColors.Control;
			this.LocalServersTab.Controls.Add(this.LocalServersListBox);
			this.LocalServersTab.Controls.Add(this.LocalLabel);
			this.LocalServersTab.Location = new System.Drawing.Point(4, 23);
			this.LocalServersTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.LocalServersTab.Name = "LocalServersTab";
			this.LocalServersTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.LocalServersTab.Size = new System.Drawing.Size(327, 290);
			this.LocalServersTab.TabIndex = 0;
			this.LocalServersTab.Text = "Local Servers";
			// 
			// LocalServersListBox
			// 
			this.LocalServersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LocalServersListBox.FormattingEnabled = true;
			this.LocalServersListBox.ItemHeight = 14;
			this.LocalServersListBox.Location = new System.Drawing.Point(7, 47);
			this.LocalServersListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.LocalServersListBox.Name = "LocalServersListBox";
			this.LocalServersListBox.Size = new System.Drawing.Size(334, 214);
			this.LocalServersListBox.TabIndex = 1;
			// 
			// LocalLabel
			// 
			this.LocalLabel.AutoSize = true;
			this.LocalLabel.Location = new System.Drawing.Point(8, 11);
			this.LocalLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.LocalLabel.Name = "LocalLabel";
			this.LocalLabel.Size = new System.Drawing.Size(206, 14);
			this.LocalLabel.TabIndex = 0;
			this.LocalLabel.Text = "Select the server to connect to:";
			// 
			// NetworkServersTab
			// 
			this.NetworkServersTab.BackColor = System.Drawing.SystemColors.Control;
			this.NetworkServersTab.Controls.Add(this.NetworkServersListBox);
			this.NetworkServersTab.Controls.Add(this.NetworkLabel);
			this.NetworkServersTab.Location = new System.Drawing.Point(4, 23);
			this.NetworkServersTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.NetworkServersTab.Name = "NetworkServersTab";
			this.NetworkServersTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.NetworkServersTab.Size = new System.Drawing.Size(327, 290);
			this.NetworkServersTab.TabIndex = 1;
			this.NetworkServersTab.Text = "Network Servers";
			this.NetworkServersTab.Enter += new System.EventHandler(this.NetworkServersTab_Enter);
			// 
			// NetworkServersListBox
			// 
			this.NetworkServersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NetworkServersListBox.FormattingEnabled = true;
			this.NetworkServersListBox.ItemHeight = 14;
			this.NetworkServersListBox.Location = new System.Drawing.Point(7, 47);
			this.NetworkServersListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.NetworkServersListBox.Name = "NetworkServersListBox";
			this.NetworkServersListBox.Size = new System.Drawing.Size(315, 214);
			this.NetworkServersListBox.TabIndex = 2;
			// 
			// NetworkLabel
			// 
			this.NetworkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NetworkLabel.Location = new System.Drawing.Point(8, 11);
			this.NetworkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.NetworkLabel.Name = "NetworkLabel";
			this.NetworkLabel.Size = new System.Drawing.Size(314, 33);
			this.NetworkLabel.TabIndex = 1;
			this.NetworkLabel.Text = "Select a SQL Server instance in the network for your connection:";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = new System.Drawing.Point(131, 326);
			this.OKButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(100, 25);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton1
			// 
			this.CancelButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton1.Location = new System.Drawing.Point(239, 326);
			this.CancelButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.Size = new System.Drawing.Size(100, 25);
			this.CancelButton1.TabIndex = 2;
			this.CancelButton1.Text = "Cancel";
			this.CancelButton1.UseVisualStyleBackColor = true;
			this.CancelButton1.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SQLServerBrowser
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelButton1;
			this.ClientSize = new System.Drawing.Size(349, 353);
			this.Controls.Add(this.CancelButton1);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.ServersTabControl);
			this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(357, 387);
			this.Name = "SQLServerBrowser";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Browse for Servers";
			this.Load += new System.EventHandler(this.SQLServerBrowser_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SQLServerBrowser_FormClosing);
			this.ServersTabControl.ResumeLayout(false);
			this.LocalServersTab.ResumeLayout(false);
			this.LocalServersTab.PerformLayout();
			this.NetworkServersTab.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl ServersTabControl;
		private System.Windows.Forms.TabPage LocalServersTab;
		private System.Windows.Forms.Label LocalLabel;
		private System.Windows.Forms.TabPage NetworkServersTab;
		private System.Windows.Forms.Label NetworkLabel;
		private System.Windows.Forms.ListBox LocalServersListBox;
		private System.Windows.Forms.ListBox NetworkServersListBox;
		private System.Windows.Forms.Button OKButton;
		private System.Windows.Forms.Button CancelButton1;
	}
}