namespace Microarea.EasyAttachment.UI.Controls
{
	partial class SOSUserCtrl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SOSUserCtrl));
			this.SOSInfoGBox = new System.Windows.Forms.GroupBox();
			this.PictBoxDocStatus = new System.Windows.Forms.PictureBox();
			this.LblRegDateValue = new System.Windows.Forms.Label();
			this.LblRegDate = new System.Windows.Forms.Label();
			this.LblDocStatus = new System.Windows.Forms.Label();
			this.LblLotIDValue = new System.Windows.Forms.Label();
			this.LblLotID = new System.Windows.Forms.Label();
			this.LblAbsoluteCodeValue = new System.Windows.Forms.Label();
			this.LblAbsoluteCode = new System.Windows.Forms.Label();
			this.BtnSendToSOS = new System.Windows.Forms.Button();
			this.LblInfo = new System.Windows.Forms.Label();
			this.SOSInfoGBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxDocStatus)).BeginInit();
			this.SuspendLayout();
			// 
			// SOSInfoGBox
			// 
			resources.ApplyResources(this.SOSInfoGBox, "SOSInfoGBox");
			this.SOSInfoGBox.Controls.Add(this.PictBoxDocStatus);
			this.SOSInfoGBox.Controls.Add(this.LblRegDateValue);
			this.SOSInfoGBox.Controls.Add(this.LblRegDate);
			this.SOSInfoGBox.Controls.Add(this.LblDocStatus);
			this.SOSInfoGBox.Controls.Add(this.LblLotIDValue);
			this.SOSInfoGBox.Controls.Add(this.LblLotID);
			this.SOSInfoGBox.Controls.Add(this.LblAbsoluteCodeValue);
			this.SOSInfoGBox.Controls.Add(this.LblAbsoluteCode);
			this.SOSInfoGBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SOSInfoGBox.Name = "SOSInfoGBox";
			this.SOSInfoGBox.TabStop = false;
			// 
			// PictBoxDocStatus
			// 
			resources.ApplyResources(this.PictBoxDocStatus, "PictBoxDocStatus");
			this.PictBoxDocStatus.Name = "PictBoxDocStatus";
			this.PictBoxDocStatus.TabStop = false;
			// 
			// LblRegDateValue
			// 
			resources.ApplyResources(this.LblRegDateValue, "LblRegDateValue");
			this.LblRegDateValue.Name = "LblRegDateValue";
			// 
			// LblRegDate
			// 
			resources.ApplyResources(this.LblRegDate, "LblRegDate");
			this.LblRegDate.Name = "LblRegDate";
			// 
			// LblDocStatus
			// 
			resources.ApplyResources(this.LblDocStatus, "LblDocStatus");
			this.LblDocStatus.Name = "LblDocStatus";
			// 
			// LblLotIDValue
			// 
			resources.ApplyResources(this.LblLotIDValue, "LblLotIDValue");
			this.LblLotIDValue.Name = "LblLotIDValue";
			// 
			// LblLotID
			// 
			resources.ApplyResources(this.LblLotID, "LblLotID");
			this.LblLotID.Name = "LblLotID";
			// 
			// LblAbsoluteCodeValue
			// 
			resources.ApplyResources(this.LblAbsoluteCodeValue, "LblAbsoluteCodeValue");
			this.LblAbsoluteCodeValue.Name = "LblAbsoluteCodeValue";
			// 
			// LblAbsoluteCode
			// 
			resources.ApplyResources(this.LblAbsoluteCode, "LblAbsoluteCode");
			this.LblAbsoluteCode.Name = "LblAbsoluteCode";
			// 
			// BtnSendToSOS
			// 
			resources.ApplyResources(this.BtnSendToSOS, "BtnSendToSOS");
			this.BtnSendToSOS.Name = "BtnSendToSOS";
			this.BtnSendToSOS.UseVisualStyleBackColor = true;
			this.BtnSendToSOS.Click += new System.EventHandler(this.BtnSendToSOS_Click);
			// 
			// LblInfo
			// 
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.Name = "LblInfo";
			// 
			// SOSUserCtrl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.LblInfo);
			this.Controls.Add(this.BtnSendToSOS);
			this.Controls.Add(this.SOSInfoGBox);
			this.Name = "SOSUserCtrl";
			this.SOSInfoGBox.ResumeLayout(false);
			this.SOSInfoGBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictBoxDocStatus)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox SOSInfoGBox;
		private System.Windows.Forms.Label LblRegDateValue;
		private System.Windows.Forms.Label LblRegDate;
		private System.Windows.Forms.Label LblLotIDValue;
		private System.Windows.Forms.Label LblLotID;
		private System.Windows.Forms.Label LblAbsoluteCodeValue;
		private System.Windows.Forms.Label LblAbsoluteCode;
		private System.Windows.Forms.Label LblDocStatus;
		private System.Windows.Forms.Button BtnSendToSOS;
		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.PictureBox PictBoxDocStatus;
	}
}
