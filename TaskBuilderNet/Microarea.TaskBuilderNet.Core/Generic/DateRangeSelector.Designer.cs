namespace Microarea.TaskBuilderNet.Core.Generic
{
	partial class DateRangeSelector
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listViewOption = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// listViewOption
			// 
			this.listViewOption.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listViewOption.Location = new System.Drawing.Point(1, 0);
			this.listViewOption.Name = "listViewOption";
			this.listViewOption.Size = new System.Drawing.Size(291, 169);
			this.listViewOption.TabIndex = 0;
			this.listViewOption.UseCompatibleStateImageBehavior = false;
			this.listViewOption.View = System.Windows.Forms.View.List;
			this.listViewOption.SelectedIndexChanged += new System.EventHandler(this.listViewOption_SelectedIndexChanged);
			this.listViewOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewOption_KeyDown);
			// 
			// DateRangeSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(295, 172);
			this.Controls.Add(this.listViewOption);
			this.Name = "DateRangeSelector";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listViewOption;

	}
}