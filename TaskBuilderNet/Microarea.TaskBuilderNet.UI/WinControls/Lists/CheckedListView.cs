using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Lists
{
	/// <summary>
	/// Summary description for CheckedListView.
	/// </summary>
	public class CheckedListView : System.Windows.Forms.ListView
	{
		#region CheckedListView private fields
		
		private System.Windows.Forms.ColumnHeader uniqueColumnHeader = null;

		#endregion

		#region CheckedListView public properties
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new bool CheckBoxes
		{
			get { return true; }
			set { base.CheckBoxes = true; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new bool AutoArrange
		{
			get { return false; }
			set { base.CheckBoxes = false; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Windows.Forms.ListViewAlignment Alignment
		{
			get { return System.Windows.Forms.ListViewAlignment.Left; }
			set { base.Alignment = System.Windows.Forms.ListViewAlignment.Left; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Windows.Forms.ColumnHeaderStyle HeaderStyle
		{
			get { return System.Windows.Forms.ColumnHeaderStyle.None; }
			set { base.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None; }
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new System.Windows.Forms.View View
		{
			get { return System.Windows.Forms.View.Details; }
			set { base.View = System.Windows.Forms.View.Details; }
		}

		//---------------------------------------------------------------------------
		[Localizable(false)]
		[Browsable(false)]
		public new System.Windows.Forms.ListView.ColumnHeaderCollection Columns
		{
			get { return base.Columns; }
		}

		#endregion

		#region CheckedListView Constructor
		
		//---------------------------------------------------------------------------
		public CheckedListView()
		{
			CreateCheckedStateImageList();
			
			if (!this.DesignMode)
				CreateUniqueColumnHeader();
		}
		
		#endregion

		#region CheckedListView protected overriden methods

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			// Invoke base class implementation
			base.OnResize(e);
			
			if (this.Columns.Count == 1 && this.Columns[0] != null)
				this.Columns[0].Width = this.DisplayRectangle.Width;

			this.PerformLayout();
		}
		
		#endregion

		#region CheckedListView private methods
		
		//---------------------------------------------------------------------------
		private bool IsToUseXPStyle()
		{
			// Makes sure Windows XP is running and a .manifest file exists for the current Application.
			return (Environment.OSVersion.Version.Major > 4 &&
					Environment.OSVersion.Version.Minor > 0 &&
					System.IO.File.Exists(Application.ExecutablePath + ".manifest"));
		}

		//---------------------------------------------------------------------------
		private void CreateCheckedStateImageList()
		{
			this.StateImageList = new System.Windows.Forms.ImageList();

			this.StateImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.StateImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.StateImageList.TransparentColor = System.Drawing.Color.Transparent;

			Stream imageStream = null;
			bool isToUseXPStyle = IsToUseXPStyle();

			if (isToUseXPStyle)
				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images.UncheckedItemXP.bmp");
			else
				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images.UncheckedItem.bmp");
			if (imageStream != null)
				this.StateImageList.Images.Add(Image.FromStream(imageStream));

			if (isToUseXPStyle)
				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images.CheckedItemXP.bmp");
			else
				imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images.CheckedItem.bmp");
			if (imageStream != null)
				this.StateImageList.Images.Add(Image.FromStream(imageStream));
		}
		
		//---------------------------------------------------------------------------
		private void CreateUniqueColumnHeader()
		{
			this.Columns.Clear();

			uniqueColumnHeader = new System.Windows.Forms.ColumnHeader();
			uniqueColumnHeader.Text = String.Empty;
			uniqueColumnHeader.Width = this.DisplayRectangle.Width;
			uniqueColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;

			this.Columns.Add(uniqueColumnHeader);
		}
		
		#endregion
	}
}
