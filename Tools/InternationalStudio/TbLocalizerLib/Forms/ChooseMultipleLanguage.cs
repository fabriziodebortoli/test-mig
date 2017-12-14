using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	//=========================================================================
	public class ChooseMultipleLanguage : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label Lbl;
		private System.Windows.Forms.Button BtnOK;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.CheckedListBox CheckedList;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button BtnSelectAll;

		private bool allChecked = false;
		public ArrayList CheckedItems = null;

		//---------------------------------------------------------------------
		public ChooseMultipleLanguage(ArrayList list)
		{
			InitializeComponent();
			try
			{

				foreach (string s in list)
				{
					MyCultureInfo ci = new MyCultureInfo(s);
					CheckedList.Items.Add(ci);				
				}
			}
			catch (Exception exc)
			{Debug.WriteLine(exc.Message);}
			ManageCheck();
		}
		
		//---------------------------------------------------------------------
		private void GetCheckedItems()
		{
			CheckedItems = new ArrayList();
			for (int i = 0 ; i< CheckedList.Items.Count; i++)
				if (CheckedList.GetItemCheckState(i) == CheckState.Checked)
					CheckedItems.Add(((CultureInfo)CheckedList.Items[i]).Name);
		}

		//---------------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			GetCheckedItems();
			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnSelectAll_Click(object sender, System.EventArgs e)
		{
			ManageCheck();
		}

		//---------------------------------------------------------------------
		private void ManageCheck()
		{
			BtnSelectAll.Text = allChecked ? Strings.CheckAll : Strings.UnCheckAll; 	
			allChecked = !allChecked;
			for (int i = 0 ; i< CheckedList.Items.Count; i++)
				CheckedList.SetItemChecked(i, allChecked);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CheckedList = new System.Windows.Forms.CheckedListBox();
			this.Lbl = new System.Windows.Forms.Label();
			this.BtnOK = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnSelectAll = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// CheckedList
			// 
			this.CheckedList.CheckOnClick = true;
			this.CheckedList.Location = new System.Drawing.Point(16, 64);
			this.CheckedList.Name = "CheckedList";
			this.CheckedList.Size = new System.Drawing.Size(256, 184);
			this.CheckedList.Sorted = true;
			this.CheckedList.TabIndex = 1;
			// 
			// Lbl
			// 
			this.Lbl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Lbl.Location = new System.Drawing.Point(16, 8);
			this.Lbl.Name = "Lbl";
			this.Lbl.Size = new System.Drawing.Size(264, 48);
			this.Lbl.TabIndex = 0;
			this.Lbl.Text = "Choose dictionaries that you want to convert.";
			// 
			// BtnOK
			// 
			this.BtnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnOK.Location = new System.Drawing.Point(109, 272);
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.Size = new System.Drawing.Size(75, 48);
			this.BtnOK.TabIndex = 3;
			this.BtnOK.Text = "OK";
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnCancel.Location = new System.Drawing.Point(205, 272);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = new System.Drawing.Size(75, 48);
			this.BtnCancel.TabIndex = 4;
			this.BtnCancel.Text = "Cancel";
			// 
			// BtnSelectAll
			// 
			this.BtnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnSelectAll.Location = new System.Drawing.Point(13, 272);
			this.BtnSelectAll.Name = "BtnSelectAll";
			this.BtnSelectAll.Size = new System.Drawing.Size(75, 48);
			this.BtnSelectAll.TabIndex = 2;
			this.BtnSelectAll.Text = "Select All";
			this.BtnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
			// 
			// ChooseMultipleLanguage
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
			this.ClientSize = new System.Drawing.Size(292, 326);
			this.Controls.Add(this.BtnSelectAll);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.Lbl);
			this.Controls.Add(this.CheckedList);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximumSize = new System.Drawing.Size(298, 352);
			this.MinimumSize = new System.Drawing.Size(298, 352);
			this.Name = "ChooseMultipleLanguage";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ChooseMultipleLanguage";
			this.ResumeLayout(false);

		}
		#endregion

	}

	//=========================================================================
	public class MyCultureInfo : CultureInfo
	{
		public MyCultureInfo(string cultureString) : base(cultureString)
		{}

		public override string ToString()
		{
			return EnglishName;
		}

	}
}
