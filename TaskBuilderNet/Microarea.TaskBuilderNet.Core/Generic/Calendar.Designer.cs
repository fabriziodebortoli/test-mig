﻿namespace Microarea.TaskBuilderNet.Core.Generic
{
	partial class Calendar
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
			this.monthCalendar = new System.Windows.Forms.MonthCalendar();
			this.SuspendLayout();
			// 
			// monthCalendar
			// 
			this.monthCalendar.Location = new System.Drawing.Point(0, 0);
			this.monthCalendar.Name = "monthCalendar";
			this.monthCalendar.ShowWeekNumbers = true;
			this.monthCalendar.TabIndex = 0;
			this.monthCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar_DateSelected);
			// 
			// Calendar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 161);
			this.Controls.Add(this.monthCalendar);
			this.Name = "Calendar";
			this.Text = "Calendar";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.MonthCalendar monthCalendar;
	}
}