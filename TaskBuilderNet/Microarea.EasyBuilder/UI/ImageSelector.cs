using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// Control used to import images in module files folder
	/// </summary>
	public partial class ImageSelector : UserControl
	{
		internal event EventHandler Closed;
		string easyBuilderCurrentPath;
		string bkgImage;

		//---------------------------------------------------------------------------------
		private void OnClosed()
		{
			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}
		/// <summary>
		/// 
		/// </summary>
		//---------------------------------------------------------------------------------
		public string BkgImage { get { return bkgImage; } set { bkgImage = value; } }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing )
			{
				if (components != null)
					components.Dispose();

				EventHandlers.RemoveEventHandlers(ref Closed);
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// 
		/// </summary>
		//---------------------------------------------------------------------------------
		public ImageSelector(string image = null)
		{
			InitializeComponent();
			this.bkgImage = image == null ? string.Empty : image;
		}

		/// <summary>
		/// 
		/// </summary>
		//---------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			easyBuilderCurrentPath = EBStaticFunctions.GetCurrentEasyBuilderAppPath();
			LoadImages();
		}

		//---------------------------------------------------------------------------------
		private void LoadImages()
		{
			listViewModuleImages.Items.Clear();

			ListViewItem lvItem;
			foreach (FileInfo item in EBStaticFunctions.GetImagesFiles(easyBuilderCurrentPath))
			{
				lvItem = AddListViewItem(item.Name);

				if (lvItem.Tag.ToString() == bkgImage)
					lvItem.Selected = true;
			}
		}

		//---------------------------------------------------------------------------------
		private ListViewItem AddListViewItem(string filename)
		{
			ListViewItem lvItem = new ListViewItem();
			listViewModuleImages.Items.Add(lvItem);
			lvItem.Text = filename;
			string ns = PathFinderWrapper.GetImageNamespace(
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName, 
				filename);
			lvItem.Tag = lvItem.ToolTipText = ns;
			return lvItem;
		}

		//---------------------------------------------------------------------------------
		private void listViewModuleImages_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (listViewModuleImages.SelectedItems == null || listViewModuleImages.SelectedItems.Count <= 0)
				return;

			ListViewItem lvItem = listViewModuleImages.SelectedItems[0];
			if (lvItem == null)
				return;

			bkgImage = lvItem.Tag.ToString();

			OnClosed();
		}

		private void tsbImport_Click(object sender, EventArgs e)
		{
			List<string> impImages = EBStaticFunctions.ImportImages(this, easyBuilderCurrentPath);
			foreach (string item in impImages)
			{
				FileInfo fi = new FileInfo(item);

				ListViewItem lvItem = listViewModuleImages.Items[fi.Name];
				if (lvItem == null)
					lvItem = AddListViewItem(fi.Name);

				//aggiungo alla custom list
				BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(item);
			}
		}
	}
}
