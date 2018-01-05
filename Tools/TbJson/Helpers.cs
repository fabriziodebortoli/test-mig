using System;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;
using SharedCode;

namespace Microarea.TbJson
{
    public static class Helpers
    {
        /// <summary>
        /// Racchiude un tag tra parentesi quadre
        /// </summary>
        public static string Square(string tag) => $"[{tag}]";

        public static JObject FindAnchoredObjectInSiblings(JArray jItems, JObject currentObject)
        {
            string anchorName = string.Empty;
            if (currentObject["id"] != null)
            {
                anchorName = currentObject["id"].ToString();
            }

            foreach (JObject current in jItems.Children<JObject>())
            {
                if (current["anchor"] != null)
                {
                    string anchor = current["anchor"].ToString();
                    if (string.Compare(anchor, "col1", true) == 0 || string.Compare(anchor, "col2", true) == 0)
                        continue;

                    if (string.Compare(anchor, anchorName, true) == 0)
                        return current;
                }
            }

            return null;
        }

        public static string ResolveGetParentNameFunction(string ds, JObject jObj)
        {
            if (ds.IndexOf(Constants.getParentNameFunction) != -1)
            {
                string parentName = jObj.Parent?.Parent?.Parent?.GetFlatString(Constants.name);
                ds = ds.Replace(Constants.getParentNameFunction, parentName);
                ds = Regex.Replace(ds, "\\(|\\)|\\{|\\}|\\+|\\'|\\s", string.Empty);
            }
            return ds;
        }

        public static WebControl GetDefaultWebControl(WndObjType type)
        {
            switch (type)
            {
                case WndObjType.Button:
                    return new WebControl(Constants.tbButton);
                case WndObjType.Check:
                    return new WebControl(Constants.tbCheckBox);
                case WndObjType.Edit:
                    return new WebControl(Constants.tbText);
                case WndObjType.Label:
                    return new WebControl(Constants.tbCaption);
                case WndObjType.Combo:
                    return new WebControl(Constants.tbCombo);
                case WndObjType.Radio:
                    return new WebControl(Constants.tbRadio);
                case WndObjType.TreeAdv:
                    return new WebControl(Constants.tbTreeView);
                case WndObjType.BodyEdit:
                    return new WebControl(Constants.tbBodyEdit);
                case WndObjType.ColTitle:
                    return new WebControl(Constants.tbBodyEditColumn);
                case WndObjType.PropertyGrid:
                    return new WebControl(Constants.tbPropertyGrid);
                case WndObjType.PropertyGridItem:
                    return new WebControl(Constants.tbPropertyGridItem);
                default:
                    throw new Exception($"Unsupported web control type: {type}");
            }
        }

        public static WebControl GetWebControl(JObject jObj)
        {
            string ctrlClass = jObj.GetFlatString(Constants.controlClass);

            if (String.IsNullOrEmpty(ctrlClass))
                return GetDefaultWebControl(jObj.GetWndObjType());

            ControlClassMap map = CacheConnector.GetControlClasses();
            if (map.TryGetValue(ctrlClass, out WebControl ctrlType))
            {
                return ctrlType;
            }
            else
            {
                return GetDefaultWebControl(jObj.GetWndObjType());
            }
        }
    }
}
