using System.Xml;


namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal interface IMenuDomUpdater
	{
		//---------------------------------------------------------------------
		void AddMenuItemNode(MenuItemEventArgs args);
		
		//---------------------------------------------------------------------
		void MoveMenuItemNode(MenuItemEventArgs args);
		
		//---------------------------------------------------------------------
		XmlNode RemoveMenuItemNode(MenuItemEventArgs args);
		
		//---------------------------------------------------------------------
		void ChangeMenuItemNodePropertyValue(MenuItemPropertyValueChangedEventArgs aPropertyValueChangedEventArgs);
	}
}
