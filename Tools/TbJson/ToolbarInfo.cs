using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microarea.TbJson
{
    class ToolbarInfo 
    {
        static private JObject GetItemByTag(JArray items, string name, string sClass)
        {
            foreach (JObject jItem in items)
                if (jItem.GetFlatString(Constants.ngTag) == name && jItem.GetFlatString(Constants.ngClass) == sClass)
                    return jItem;
            JObject jNewItem = new JObject();
            jNewItem[Constants.type] = WndObjType.Toolbar.ToString();
            jNewItem[Constants.ngTag] = name;
            jNewItem[Constants.ngClass] = sClass;
            items.Add(jNewItem);
            return jNewItem;
        }
        //-----------------------------------------------------------------------------
        internal static void ToolbarToMap(JArray jToolbars, JObject toolbar)
        {
            foreach (JObject btn in toolbar.GetItems())
            {
                AddButton(jToolbars, btn);
            }
        }

        static private void AddButton(JArray items, JObject btn)
        {
            JObject jOuter = GetItemByTag(items, GetToolbarOuterSection(btn), "");
            JObject jInner = GetItemByTag(jOuter.GetItems(true), Constants.div, GetToolbarInnerSectionClass(btn.GetCommandCategory()));
            JObject jCat = GetItemByTag(jInner.GetItems(true), Constants.div, GetToolbarCategoryClass(btn.GetCommandCategory()));
            string group = GetToolbarButtonGroup(btn.GetCommandCategory());
			string cssClass = GetToolbarButtonCssClass(btn.GetCommandCategory());
			JObject jButton = string.IsNullOrEmpty(group) ? jCat : GetItemByTag(jCat.GetItems(true), group, "");
            btn[Constants.category] = (int)btn.GetCommandCategory();
            JArray jItems = jButton.GetItems(true);
			if (jItems.Find(btn.GetId()) == null)
			{
				if (!string.IsNullOrWhiteSpace(cssClass))
					btn[Constants.ngClass] = cssClass;
                if (btn.GetBool(Constants.isDropdown))
                {
                    foreach(JObject jMenuItem in btn.GetItems())
                        jMenuItem[Constants.ngClass] = Constants.dropDownButton;
                }
				jItems.Add(btn);
			}
        }

        private static string GetToolbarButtonGroup(CommandCategory cat)
        {
            switch (cat)
            {
                case CommandCategory.Advanced:
                    return Constants.tbToolbarTopDropdown;
				case CommandCategory.Print:
					return Constants.tbToolbarBottomDropup;
				default:
                    return "";
            }
        }

		private static string GetToolbarButtonCssClass(CommandCategory cat)
		{
			switch (cat)
			{
				case CommandCategory.Advanced:
					return Constants.dropDownButton;
				case CommandCategory.Print:
					return Constants.dropUpButton;
				default:
					return "";
			}
		}

		static private string GetToolbarOuterSection(JObject btn)
        {
            CommandCategory cat = btn.GetCommandCategory();
            switch (cat)
            {
                
                case CommandCategory.Print:
				case CommandCategory.File:
                case CommandCategory.Bottom:
                    return Constants.tbToolbarBottom;
                case CommandCategory.Undefined:
                case CommandCategory.Search:
                case CommandCategory.Radar:
                case CommandCategory.Navigation:
                case CommandCategory.Advanced:
                case CommandCategory.Tools:
                case CommandCategory.Edit:
                case CommandCategory.Exit:
                case CommandCategory.Fab:
                default:
                    return Constants.tbToolbarTop;
                
                //return Constants.tbFloatingActionMenu;
            }
        }

        static private string GetToolbarInnerSectionClass(CommandCategory cat)
        {
            switch (cat)
            {
                case CommandCategory.Search:
                case CommandCategory.Navigation:
                case CommandCategory.Edit:
                case CommandCategory.Fab:
                case CommandCategory.Radar:
                case CommandCategory.Tools:
                case CommandCategory.Print:
                case CommandCategory.File:
                case CommandCategory.Undefined:
                    return "toolbar-menu";
                case CommandCategory.Exit:
                case CommandCategory.Advanced:
                    return "toolbar-right";
                
                default:
                    return "toolbar-undefined";
            }
        }
        static private string GetToolbarCategoryClass(CommandCategory cat)
        {
            switch (cat)
            {
                case CommandCategory.Search:
                    return "menu-category search";
                case CommandCategory.Navigation:
                    return "menu-category navigation";
                case CommandCategory.Edit:
                    return "menu-category edit";
                case CommandCategory.Exit:
                    return "menu-category exit";
                case CommandCategory.Fab:
                case CommandCategory.Advanced:
                    return "menu-category advanced";
                case CommandCategory.Radar:
                case CommandCategory.Tools:
                case CommandCategory.Print:
				case CommandCategory.File:
				case CommandCategory.Undefined:
                default:
                    return "menu-category undefined";
            }
        }
    }

   

    
}
