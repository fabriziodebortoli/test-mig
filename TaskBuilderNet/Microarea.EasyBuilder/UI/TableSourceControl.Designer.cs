using Microarea.TaskBuilderNet.Core.Generic;
namespace Microarea.EasyBuilder.UI
{
	partial class TableSourceControl
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
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				EventHandlers.RemoveEventHandlers(ref NoSelection);
				EventHandlers.RemoveEventHandlers(ref SelectedTableChanged);
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableSourceControl));
            this.cbxTableName = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDBTCreated = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbxTableName
            // 
            resources.ApplyResources(this.cbxTableName, "cbxTableName");
            this.cbxTableName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbxTableName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxTableName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxTableName.FormattingEnabled = true;
            this.cbxTableName.Name = "cbxTableName";
            this.cbxTableName.Sorted = true;
            this.cbxTableName.SelectedIndexChanged += new System.EventHandler(this.cbxTableName_SelectedIndexChanged);
            this.cbxTableName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cbxTableName_KeyPress);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lblDBTCreated
            // 
            resources.ApplyResources(this.lblDBTCreated, "lblDBTCreated");
            this.lblDBTCreated.Name = "lblDBTCreated";
            // 
            // TableSourceControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbxTableName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDBTCreated);
            this.Name = "TableSourceControl";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cbxTableName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblDBTCreated;
	}
}
