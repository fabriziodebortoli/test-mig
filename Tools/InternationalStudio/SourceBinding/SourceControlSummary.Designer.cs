namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	partial class SourceControlSummary
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
			this.lwItems = new System.Windows.Forms.ListView();
			this.chCheck = new System.Windows.Forms.ColumnHeader();
			this.chName = new System.Windows.Forms.ColumnHeader();
			this.chAction = new System.Windows.Forms.ColumnHeader();
			this.chPath = new System.Windows.Forms.ColumnHeader();
			this.label1 = new System.Windows.Forms.Label();
			this.txtComment = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.OK = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lwItems
			// 
			this.lwItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lwItems.CheckBoxes = true;
			this.lwItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chCheck,
            this.chName,
            this.chAction,
            this.chPath});
			this.lwItems.Location = new System.Drawing.Point(12, 127);
			this.lwItems.Name = "lwItems";
			this.lwItems.Size = new System.Drawing.Size(619, 217);
			this.lwItems.TabIndex = 3;
			this.lwItems.UseCompatibleStateImageBehavior = false;
			this.lwItems.View = System.Windows.Forms.View.Details;
			this.lwItems.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lwItems_ItemChecked);
			// 
			// chCheck
			// 
			this.chCheck.Text = "";
			// 
			// chName
			// 
			this.chName.Text = "Name";
			this.chName.Width = 147;
			// 
			// chAction
			// 
			this.chAction.Text = "Action";
			// 
			// chPath
			// 
			this.chPath.Text = "Path";
			this.chPath.Width = 348;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 110);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Items";
			// 
			// txtComment
			// 
			this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtComment.Location = new System.Drawing.Point(12, 40);
			this.txtComment.Multiline = true;
			this.txtComment.Name = "txtComment";
			this.txtComment.Size = new System.Drawing.Size(619, 60);
			this.txtComment.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Comment";
			// 
			// OK
			// 
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.OK.Location = new System.Drawing.Point(426, 350);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(100, 25);
			this.OK.TabIndex = 4;
			this.OK.Text = "OK";
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Cancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.Cancel.Location = new System.Drawing.Point(531, 350);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(100, 25);
			this.Cancel.TabIndex = 5;
			this.Cancel.Text = "Cancel";
			// 
			// SourceControlSummary
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(643, 384);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.txtComment);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lwItems);
			this.Name = "SourceControlSummary";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Text = "Source Control Operations Summary";
			this.Load += new System.EventHandler(this.SourceControlSummary_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SourceControlSummary_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView lwItems;
		private System.Windows.Forms.ColumnHeader chName;
		private System.Windows.Forms.ColumnHeader chAction;
		private System.Windows.Forms.ColumnHeader chCheck;
		private System.Windows.Forms.ColumnHeader chPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtComment;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button Cancel;
	}
}