using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Microarea.TbJson
{
    enum ValueType { NOT_FOUND, PLAIN, EXPRESSION, CONSTANT }
    internal static class Extensions
    {
        //-----------------------------------------------------------------------------
        internal static void WriteIndent(this HtmlTextWriter htmlWriter, int indent)
        {
            for (int i = 0; i < indent; i++)
                htmlWriter.Write(' ');
        }

        //-----------------------------------------------------------------------------
        internal static JArray GetItems(this JToken jObj, bool createIfNone = false)
        {
            JArray ar = jObj[Constants.items] as JArray;
            if (ar == null)
            {
                ar = new JArray();
                if (createIfNone)
                    jObj[Constants.items] = ar;
            }
            return ar;
        }
        //-----------------------------------------------------------------------------
        internal static string GetId(this JToken jObj)
        {
            return jObj.GetFlatString(Constants.id);
        }

        //-----------------------------------------------------------------------------
        public static string GetDataSource(this JObject jBinding)
        {
            return jBinding[Constants.datasource]?.ToString();
        }
        internal static bool TryGetId(this JToken jObj, out string id) =>
            (id = jObj.GetFlatString(Constants.id)) != null;

        internal static string GetClick(this JToken jObj)
        {
            if (jObj.GetFlatString(Constants.id) == "ID_EXTDOC_RADAR") // TODOPD
                return "openRadar";
            return jObj.GetFlatString(Constants.buttonClick);
        }

        //-----------------------------------------------------------------------------
        internal static WndObjType GetWndObjType(this JToken jObj)
        {
            if (!(jObj is JObject))
                return WndObjType.Undefined;
            JToken t = jObj[Constants.type];
            if (t == null)
                return WndObjType.Undefined;
            if (t.Type == JTokenType.Integer)
                return (WndObjType)t.Value<int>();
            if (t.Type == JTokenType.String)
                return (WndObjType)Enum.Parse(typeof(WndObjType), t.Value<string>(), true);
            throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
        }
        //-----------------------------------------------------------------------------
        internal static TileDialogSize GetTileDialogSize(this JToken jObj)
        {
            JToken t = jObj[Constants.size];
            if (t == null)
                return TileDialogSize.Standard;
            if (t.Type == JTokenType.Integer)
                return (TileDialogSize)t.Value<int>();
            if (t.Type == JTokenType.String)
                return (TileDialogSize)Enum.Parse(typeof(TileDialogSize), t.Value<string>(), true);
            throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
        }

        //-----------------------------------------------------------------------------
        internal static LayoutType GetLayoutType(this JToken jObj)
        {
            JToken t = jObj[Constants.layoutType];
            if (t == null)
                return LayoutType.None;
            if (t.Type == JTokenType.Integer)
                return (LayoutType)t.Value<int>();
            if (t.Type == JTokenType.String)
                return (LayoutType)Enum.Parse(typeof(LayoutType), t.Value<string>(), true);
            throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
        }

        //-----------------------------------------------------------------------------
        internal static string GetTextAlign(this JToken jObj)
        {
            JToken t = jObj[Constants.textAlign];
            if (t == null)
                return null;

            TextAlign align = TextAlign.Left;
            Enum.TryParse<TextAlign>(t.Value<string>(), out align);

            switch (align)
            {
                case TextAlign.Left:
                    return null;
                case TextAlign.Center:
                    return "center";
                case TextAlign.Right:
                    return "right";
                default:
                    return null;
            }
                       
        }

        //-----------------------------------------------------------------------------
        internal static TileDialogStyle GetDialogStyle(this JToken jObj)
        {
            JToken t = jObj[Constants.tileStyle];
            if (t == null)
                return TileDialogStyle.None;
            if (t.Type == JTokenType.Integer)
                return (TileDialogStyle)t.Value<int>();
            if (t.Type == JTokenType.String)
                return (TileDialogStyle)Enum.Parse(typeof(TileDialogStyle), t.Value<string>(), true);
            throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
        }

        //-----------------------------------------------------------------------------
        internal static CommandCategory GetCommandCategory(this JToken jObj)
        {
            if (jObj.GetFlatString(Constants.id) == "ID_EXTDOC_RADAR")
                return CommandCategory.Search; // TODOPD
            JToken cat = jObj[Constants.category];
            if (cat != null)
                return (CommandCategory)cat.Value<int>();
            if (jObj.GetParentItem().GetBool(Constants.bottom))
                return CommandCategory.Bottom;
            return CommandCategory.Undefined;
        }
        //-----------------------------------------------------------------------------
        internal static ViewCategory GetViewCategory(this JToken jObj)
        {
            JToken t = jObj[Constants.category];
            if (t == null)
                return ViewCategory.DataEntry;
            if (t.Type == JTokenType.Integer)
                return (ViewCategory)t.Value<int>();
            if (t.Type == JTokenType.String)
                return (ViewCategory)Enum.Parse(typeof(ViewCategory), t.Value<string>(), true);
            throw new Exception(string.Format("Invalid type: {0}", t.ToString()));
        }
        //-----------------------------------------------------------------------------
        internal static JObject GetObject(this JToken jObj, string name)
        {
            return jObj is JObject
            ? jObj[name] as JObject
            : null;
        }
        /// <summary>
        /// Prepara una stringa alla localizzazione con _TB()
        /// </summary>
        //-----------------------------------------------------------------------------
        internal static string GetLocalizableString(this JToken jObj, string name)
        {
            if (string.IsNullOrEmpty(jObj.GetFlatString(name)))
                return null;

            if (!(jObj is JObject))
                return null;

            var result = jObj[name];

            if (result == null || !(result is JValue))
                return null;
            string text = result.Value<string>();
            if (text == null)
                return null;
            if (Helpers.AdjustExpression(ref text))
            {
                return text;
            }
            //la rimozione di '&' va fatta lato client nell a_TB, altrimenti non trova le traduzioni
            text = Regex.Replace(text, "'|\\\"|\r|\n|\t", new MatchEvaluator(ReplaceInLocalizableString));
            //HttpUtility.HtmlEncode(text.Replace("'", "\\'"));

            return string.Concat("_TB('", text, "')");
        }

        //-----------------------------------------------------------------------------
        private static string ReplaceInLocalizableString(Match match)
        {
            if (match.Value == "'")
                return "\\'";
            if (match.Value == "\r")
                return "\\r";
            if (match.Value == "\n")
                return "\\n";
            if (match.Value == "\t")
                return "\\t";
            return HttpUtility.HtmlEncode(match.Value);
        }

        //-----------------------------------------------------------------------------
        internal static bool GetBool(this JToken jObj, string name)
        {
            if (!(jObj is JObject))
                return false;

            return Convert.ToBoolean(jObj[name]);
        }

        //-----------------------------------------------------------------------------
        internal static JObject GetParentItem(this JToken jObj)
        {
            if (!(jObj is JObject))
                return null;

            return jObj.Parent?.Parent?.Parent as JObject;
        }
        //-----------------------------------------------------------------------------
        internal static void SortItems(this JToken jObj)
        {
            JArray jAr = jObj.GetItems();
            if (jAr == null)
                return;
            //toolbar
            JArray sorted = new JArray(jAr.OrderBy(obj =>
            {
                //la toolbar top ma in testa, poi gli altri oggetti, infine la toolbar bottom
                WndObjType type = obj.GetWndObjType();
                if (type == WndObjType.Toolbar)
                {
                    string ngTag = obj.GetFlatString(Constants.ngTag);
                    if (ngTag == Constants.tbToolbarTop)
                        return 1;
                    else
                        return 20;
                }

                return 10;//vista o altri oggetti analoghi
            }));
            jObj[Constants.items] = sorted;
            foreach (JObject jButton in sorted)
            {
                jAr = jButton.GetItems();
                if (jAr == null)
                    continue;
                JArray buttonsSorted = new JArray(jAr.OrderBy(obj => obj.GetButtonOrdinal()));
                jButton[Constants.items] = buttonsSorted;
            }
        }
        //-----------------------------------------------------------------------------
        internal static void ReplaceEnums(this JObject jObj)
        {
            var type = jObj.GetWndObjType();
            if (type != WndObjType.Undefined)
                jObj[Constants.type] = type.ToString();
            JArray jAr = jObj.GetItems();
            if (jAr == null)
                return;
            foreach (JObject obj in jAr)
                obj.ReplaceEnums();
        }
        //-----------------------------------------------------------------------------
        internal static string GetToolbarButtonTag(this JObject jObj)
        {
            JObject obj = jObj?.GetParentItem()?.GetParentItem();
            if (obj != null)
            {
                if (obj.GetWndObjType() == WndObjType.BodyEdit)
                    return Constants.tbBodyEditToolbarButton;
            }
            if (jObj.GetBool(Constants.isDropdown))
                return Constants.tbToolbarTopButtonDropdown;

            switch (jObj.GetCommandCategory())
            {
                case CommandCategory.Search:
                case CommandCategory.Radar:
                case CommandCategory.Navigation:
                case CommandCategory.Tools:
                case CommandCategory.Edit:
                case CommandCategory.Exit:
                case CommandCategory.Advanced:
                case CommandCategory.Fab:
                case CommandCategory.Undefined:
                    return Constants.tbToolbarTopButton;

                case CommandCategory.Print:
                case CommandCategory.File:
                case CommandCategory.Bottom:
                    return Constants.tbToolbarBottomButton;

                default:
                    return Constants.tbFloatingActionButton;
            }
        }



        //-----------------------------------------------------------------------------
        internal static bool MatchId(this JObject jObj, string id)
        {
            if (jObj.GetId() == id)
                return true;
            JArray hier = jObj[Constants.idHierarchy] as JArray;
            if (hier != null)
            {
                foreach (var item in hier)
                {
                    if (item.Value<string>() == id)
                        return true;
                }
            }
            return false;
        }
        //-----------------------------------------------------------------------------
        internal static JToken Find(this JArray ar, string id)
        {
            foreach (JObject obj in ar.Children<JObject>())
            {
                if (obj.MatchId(id))
                    return obj;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        internal static bool Find(this JArray jItems, JObject obj)
        {
            return Find(jItems, obj.GetId()) != null;
        }

        //-----------------------------------------------------------------------------
        internal static JObject Find(this JObject jRoot, string id)
        {
            if (jRoot.MatchId(id))
                return jRoot;

            JArray items = jRoot.GetItems();
            if (items == null)
                return null;
            foreach (JObject child in items)
            {
                JObject found = Find(child, id);
                if (found != null)
                    return found;
            }
            return null;
        }
        //-----------------------------------------------------------------------------
        internal static void FindAll(this JObject jRoot, string id, List<JObject> list)
        {
            if (jRoot.MatchId(id))
                list.Add(jRoot);

            JArray items = jRoot.GetItems();
            if (items == null)
                return;
            foreach (JObject child in items)
            {
                FindAll(child, id, list);
            }
        }
        //-----------------------------------------------------------------------------
        internal static string GetFlatString(this JToken jObj, string name)
        {
            if (!(jObj is JObject))
                return null;
            var result = jObj[name];

            if (result == null || !(result is JValue))
                return null;
            return result.Value<string>();
        }
        //-----------------------------------------------------------------------------
        internal static string GetSafeJsonString(this JToken jObj)
        {
            switch (jObj.Type)
            {
                case JTokenType.Float:
                    double f = jObj.Value<double>();
                    return f.ToString(CultureInfo.InvariantCulture);
                case JTokenType.Boolean:
                    return jObj.ToString().ToLowerInvariant();
                case JTokenType.None:
                case JTokenType.Object:
                case JTokenType.Array:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Integer:
                case JTokenType.String:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    break;
            }

            return jObj.ToString();
        }
        //-----------------------------------------------------------------------------
        internal static ValueType GetString(this JToken jObj, string name, out string val)
        {
            if (!(jObj is JObject))
            {
                val = "";
                return ValueType.NOT_FOUND;
            }
            var result = jObj[name];

            if (result == null)
            {
                val = "";
                return ValueType.NOT_FOUND;
            }
            if (result.Type == JTokenType.Property || result.Type == JTokenType.String)
            {
                val = result.ToString();
                if (Helpers.AdjustExpression(ref val))
                    return ValueType.EXPRESSION;

                return ValueType.PLAIN;
            }
            if (result.Type == JTokenType.Object)
            {
                string c = result[Constants.Const]?.ToString();
                if (string.IsNullOrEmpty(c))
                {
                    val = "";
                    return ValueType.NOT_FOUND;
                }

                val = c;
                return ValueType.CONSTANT;
            }

            val = result.GetSafeJsonString();
            return ValueType.PLAIN;
        }
        static IList buttonOrder = new string[]{
                "ID_EXTDOC_FIND",
                "ID_EXTDOC_RADAR",

                "ID_EXTDOC_FIRST",
                "ID_EXTDOC_PREV",
                "ID_EXTDOC_NEXT",
                "ID_EXTDOC_LAST",

                "ID_EXTDOC_NEW",
                "ID_EXTDOC_EDIT",
                "ID_EXTDOC_DELETE",

                "ID_EXTDOC_REFRESH_ROWSET",
				//"_BUTTONGROUPADVANCED",
                "ID_EXTDOC_EXIT"
            }.ToList();
        /// <summary>
        /// Serve per ordinare vista e toolbar in base alle rispettive categorie; alcune toolbar vanno prima della vista, altre dopo
        /// </summary>
        internal static int GetButtonOrdinal(this JToken jObj)
        {

            WndObjType type = jObj.GetWndObjType();
            if (type == WndObjType.ToolbarButton)
            {
                string id = jObj.GetId();
                int idx = buttonOrder.IndexOf(id);
                return idx;
            }

            return 100;//bottoni residuali
        }


        /// <summary>
        /// Appende una stringa allo StringBuilder controllando che non sia già presente
        /// </summary>
        internal static StringBuilder AppendIfNotExist(this StringBuilder sb, string value)
        {
            if (!sb.ToString().Contains(value))
                sb.Append(value);
            return sb;
        }
    }
}
