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
            JObject jOuter = GetItemByTag(items, GetToolbarOuterSection(btn.GetCommandCategory()), "");
            JObject jInner = GetItemByTag(jOuter.GetItems(true), Constants.div, GetToolbarInnerSectionClass(btn.GetCommandCategory()));
            JObject jCat = GetItemByTag(jInner.GetItems(true), Constants.div, GetToolbarCategoryClass(btn.GetCommandCategory()));
            JArray jItems = jCat.GetItems(true);
            if (jItems.Find(btn.GetId()) == null)
                jItems.Add(btn);
        }
        static private string GetToolbarOuterSection(CommandCategory cat)
        {
            switch (cat)
            {
                case CommandCategory.Search:
                case CommandCategory.Radar:
                case CommandCategory.Navigation:
                case CommandCategory.Advanced:
                case CommandCategory.Tools:
                case CommandCategory.Edit:
                case CommandCategory.Exit:
                    return Constants.tbToolbarTop;
                case CommandCategory.Print:
                case CommandCategory.Undefined:
                default:
                    return Constants.tbToolbarBottom;
            }
        }

        static private string GetToolbarInnerSectionClass(CommandCategory cat)
        {
            switch (cat)
            {
                case CommandCategory.Search:
                case CommandCategory.Navigation:
                case CommandCategory.Edit:
                    return "toolbar-menu";
                case CommandCategory.Exit:
                    return "toolbar-right";
                case CommandCategory.Radar:
                case CommandCategory.Advanced:
                case CommandCategory.Tools:
                case CommandCategory.Print:
                case CommandCategory.Undefined:
                default:
                    return "";
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
                case CommandCategory.Radar:
                case CommandCategory.Advanced:
                case CommandCategory.Tools:
                case CommandCategory.Print:
                case CommandCategory.Undefined:
                default:
                    return "";
            }
        }
    }

   

    
}
