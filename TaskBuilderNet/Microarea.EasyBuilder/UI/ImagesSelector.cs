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
	public partial class ImagesSelector : UserControl
	{
		internal event EventHandler Closed;
		string easyBuilderCurrentPath;
		List<string> images;

		/// <summary>
		/// 
		/// </summary>
		//---------------------------------------------------------------------------------
		public List<string> Images { get { return images; } set { images = value; } }

		private void OnClosed()
		{
			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//---------------------------------------------------------------------------------
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
		public ImagesSelector(List<string> images = null)
		{
			InitializeComponent();
			this.images = images == null ? new List<string>() : images;
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
			
				bool found = false;
				foreach (string current in images)
				{
					if (lvItem.Tag.ToString() == current)
					{
						found = true;
						break;
					}
				}
				lvItem.Checked = found;
			}
		}

		//---------------------------------------------------------------------------------
		private ListViewItem AddListViewItem(string filename)
		{
			ListViewItem lvItem = new ListViewItem();
			listViewModuleImages.Items.Add(lvItem);
			lvItem.Text = filename;
			string ns = BasePathFinder.BasePathFinderInstance.GetEasyBuilderImageNamespace(
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName,
				filename);
			lvItem.Tag = lvItem.ToolTipText = ns;
			return lvItem;
		}

		//---------------------------------------------------------------------------------
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

		//---------------------------------------------------------------------------------
		private void tsbOk_Click(object sender, EventArgs e)
		{
			images.Clear();
			foreach (ListViewItem item in listViewModuleImages.Items)
			{
				if (!item.Checked)
					continue;

				images.Add(item.Tag.ToString());
			}

			OnClosed();
		}

		//---------------------------------------------------------------------------------
		private void tsbCancel_Click(object sender, EventArgs e)
		{
			OnClosed();
		
		}
	}
}
