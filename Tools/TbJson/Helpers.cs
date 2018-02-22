using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SharedCode;

namespace Microarea.TbJson
{
    public static class Helpers
    {
        private static readonly Regex exp = new Regex("{{2}([a-zA-Z0-9]*)}{2}", RegexOptions.Compiled);

        /// <summary>
        /// Racchiude un tag tra parentesi quadre
        /// </summary>
        public static string Square(string tag) => $"[{tag}]";
        public static bool AdjustExpression(ref string text)
        {
            if (text == null)
                return false;
            string s = exp.Replace(text, "eventData?.model?.$1");
            if (s == text)
                return false;
            text = s;
            return true;
        }

        /// <summary>
        /// Risolve le interplazioni delle stringhe (es. "{{campo}}" diventa "eventData?.model?.campo"). Supporta il Not, l'And e l'Or (es. {{!campo && campo2 || !campo3}})
        /// </summary>
        public static string ResolveInterplation(this string str) => //https://stackoverflow.com/a/48271222/1538384
            Regex.Replace(str, @"{{(!?\w+(?:\s*(?:&&|\|\|)\s*!?\w+)*)}}", m =>
              Regex.Replace(
                  Regex.Replace(m.Groups[1].Value, @"\s*(&&|\|\|)\s*", " $1 "),
                   @"\w+",
                   "eventData?.model?.$&"
                  )
             );



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

        public static WebControl GetDefaultWebControl(WndObjType type, string controlClass = "")
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
                    if (!string.IsNullOrEmpty(controlClass))
                        Console.Out.WriteLineAsync(controlClass + " Invalid column type, or control class not specified in webControls.xml file");
                    
                    //non devo più tornare colonne, ma gli oggetti contenuti
                    return new WebControl(Constants.tbText);
                case WndObjType.PropertyGrid:
                    return new WebControl(Constants.tbPropertyGrid);
                case WndObjType.PropertyGridItem:
                    return new WebControl(Constants.tbPropertyGridItem);
                case WndObjType.List:
                    return new WebControl(Constants.tbCheckListBox);
                default:
                    Debug.Fail(type.ToString() + " Unsupported web control type");
                    return null;
                    //throw new Exception($"Unsupported web control type: {type}");
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
                return GetDefaultWebControl(jObj.GetWndObjType(), ctrlClass);
            }
        }
    }
}
