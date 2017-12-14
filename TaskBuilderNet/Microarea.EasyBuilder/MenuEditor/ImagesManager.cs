using System;
using System.IO;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;


namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	class ImagesManager : IDisposable
	{
		private MenuModel menuModel;
		private string workingMenuFilePath;

		//---------------------------------------------------------------------
		public void SubscribeToMenuModelPropertyChanges(MenuModel menuModel, string workingMenuFilePath)
		{
			if (menuModel == null)
				return;

			if (workingMenuFilePath.IsNullOrEmpty())
				return;

			UnsubscribeToMenuModelPropertyChanges();

			this.menuModel = menuModel;
			this.workingMenuFilePath = workingMenuFilePath;

			this.menuModel.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(MenuModel_PropertyValueChanged);
		}

		//---------------------------------------------------------------------
		public void UnsubscribeToMenuModelPropertyChanges()
		{
			if (menuModel == null)
				return;

			menuModel.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(MenuModel_PropertyValueChanged);
		}

		//---------------------------------------------------------------------
		void MenuModel_PropertyValueChanged(
			object sender,
			MenuItemPropertyValueChangedEventArgs e
			)
		{
			if (e == null)
				return;

			if (e.PreviousValue == null)
				return;

			MenuGroup aMenuGroup = e.ChangedItem as MenuGroup;
			if (aMenuGroup != null)
			{
				ManageImage(e, aMenuGroup);
				return;
			}
			MenuApplication aMenuApplication = e.ChangedItem as MenuApplication;
			if (aMenuApplication != null)
			{
				ManageImage(e, aMenuApplication);
				return;
			}
		}

		//---------------------------------------------------------------------
		private void ManageImage(
			MenuItemPropertyValueChangedEventArgs e,
			BaseMenuItem aBaseMenuItem
			)
		{
			//Se non è cambiato il nome del MenuGroup allora ritorno.
			if (e.Property == null || String.Compare(e.Property.Name, MenuXmlNode.XML_ATTRIBUTE_NAME, StringComparison.InvariantCultureIgnoreCase) != 0)
				return;

			string menuFolderPath = Path.GetDirectoryName(workingMenuFilePath);
			DirectoryInfo menuFolderDirInfo = new DirectoryInfo(menuFolderPath);
			if (!menuFolderDirInfo.Exists)
				return;

			string oldMenuGroupName = e.PreviousValue.ToString();
			FileInfo[] oldGroupImageFileInfos = menuFolderDirInfo.GetFiles(
				String.Format("{0}.*", oldMenuGroupName),
				SearchOption.TopDirectoryOnly
				);

			if (oldGroupImageFileInfos == null || oldGroupImageFileInfos.Length == 0)
				return;

			string oldFileFullName = null;
			string newFileFullName = null;
			//Faccio il ciclo anche se dovrebbe esserci una immagine sola.
			foreach (FileInfo oldGroupImageFileInfo in oldGroupImageFileInfos)
			{
				oldFileFullName = oldGroupImageFileInfo.FullName;
				newFileFullName = oldGroupImageFileInfo.FullName.Replace(oldMenuGroupName, aBaseMenuItem.Name);

				oldGroupImageFileInfo.CopyTo(newFileFullName, true);

				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(oldFileFullName);
				BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(newFileFullName);
			}
		}


		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		private void Dispose(bool disposing)
		{
			if (disposing)
				UnsubscribeToMenuModelPropertyChanges();
		}

		#endregion
	}
}
