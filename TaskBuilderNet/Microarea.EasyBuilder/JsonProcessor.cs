using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.UI.TBWebFormControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.EasyBuilder.UI;

namespace Microarea.EasyBuilder
{
	internal class JsonProcessor
	{
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static bool PromoteGenericControls(ref string json, GenericWindowWrapper[] controls)
		{
			if (string.IsNullOrEmpty(json))
				return false;

			JObject root = JObject.Parse(json);

			var modified = false;
			foreach (var wrapper in controls)
			{
				string ctrlClass = GetControlClass(wrapper);
				if (string.IsNullOrEmpty(ctrlClass))
					continue;
				JObject obj = Find(root, wrapper.Id);
				if (obj == null)
					return false;
				obj["controlClass"] = ctrlClass;
				//obj["name"] = wrapper.Id;
				AppendDefaults(obj, wrapper);

				modified = true;
			}
			if (modified)
				json = root.ToString();
			return modified;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static string PasteControls(string json, string parentId)
		{
			if (!Clipboard.ContainsText(TextDataFormat.UnicodeText))
				return null;
			if (string.IsNullOrEmpty(json))
				return null;

			try
			{

				JObject root = JObject.Parse(json);
				JObject parentObj = Find(root, parentId);
				if (parentObj == null)
					return null;
				string copiedJson = Clipboard.GetText(TextDataFormat.UnicodeText);
				JArray ar = JArray.Parse(copiedJson);
				foreach (JObject itemToCopy in ar)
				{
					CopyItem(root, parentObj, itemToCopy, Point.Empty);
				}
				return root.ToString();
			}
			catch
			{
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static void CopyItem(JObject root, JObject parentObj, JObject itemToCopy, Point location)
		{
			string[] nonCopyAbleProps = { "name" };

			foreach (var prop in nonCopyAbleProps)
			{
				itemToCopy.Remove(prop);
			}
			if (location.IsEmpty)
			{
				JValue x = itemToCopy["x"] as JValue;
				if (x == null)
					x = new JValue(0);
				else
					x.Value = ((long)x.Value) + 5;

				JValue y = itemToCopy["y"] as JValue;
				if (y == null)
					y = new JValue(0);
				else
					y.Value = ((long)y.Value) + 5;

			}
			else
			{
				itemToCopy["x"] = location.X;
				itemToCopy["y"] = location.Y;
			}
			itemToCopy["id"] = CreateUniqueId(root, (string)itemToCopy["id"]);
			JArray items = parentObj["items"] as JArray;
			if (items == null)
			{
				items = new JArray();
				parentObj["items"] = items;
			}
			items.Add(itemToCopy);
		}

		//--------------------------------------------------------------------------------
		private static string CreateUniqueId(JObject root, string id)
		{
			StringBuilder suffix = new StringBuilder();
			string prefix = "";
			for (int i = id.Length - 1; i >= 0; i--)
			{
				if (Char.IsDigit(id[i]))
					suffix.Append(id[i]);
				else
				{
					prefix = id.Substring(0, i + 1);
					break;
				}
			}
			int nSuffix = suffix.Length > 0 ? int.Parse(suffix.ToString()) : 0;
			while (Find(root, id) != null)
			{
				id = prefix + (++nSuffix).ToString();
			}
			return id;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static bool CopyControls(string json, IWindowWrapper[] controls)
		{
			JArray ar = ExtractJsonControls(json, controls);
			if (ar.Count == 0)
				return false;

			Clipboard.Clear();
			Clipboard.SetText(ar.ToString(), TextDataFormat.UnicodeText);
			return true;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static JArray ExtractJsonControls(string json, IWindowWrapper[] controls)
		{
			JArray ar = new JArray();
			if (string.IsNullOrEmpty(json))
				return ar;
			JObject root = JObject.Parse(json);
			foreach (var wrapper in controls)
			{
				JObject obj = Find(root, wrapper.Id);
				if (obj == null)
					continue;
				ar.Add(obj);
			}
			return ar;
		}
		//--------------------------------------------------------------------------------
		internal static JObject Find(JObject obj, string id)
		{
			JValue idObj = obj["id"] as JValue;
			if (idObj == null)
				return null;
			if (idObj.Value.Equals(id))
				return obj;
			JArray jItems = obj["items"] as JArray;
			if (jItems == null)
				return null;
			foreach (JObject jItem in jItems)
			{
				JObject jInnerId = Find(jItem, id);
				if (jInnerId != null)
					return jInnerId;
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		private static void AppendDefaults(JObject obj, GenericWindowWrapper wrapper)
		{
			switch (GetControlClass(wrapper))
			{
				case ("RadioButton"):
					obj["tabStop"] = true;
					break;

				default: break;
			}

		}

		//--------------------------------------------------------------------------------
		private static string GetControlClass(GenericWindowWrapper wrapper)
		{
			if (wrapper is GenericEdit)
				return "StringEdit";
			if (wrapper is GenericComboBox)
				return "StringComboDropDown";
			if (wrapper is MLabel)
				return "StringStatic";
			if (wrapper is GenericListBox)
				return "StringListBox";
			if (wrapper is GenericPushButton)
				return "Button";
			if (wrapper is GenericCheckBox)
				return "CheckBox";
			if (wrapper is GenericRadioButton)
				return "RadioButton";
			if (wrapper is GenericGroupBox)
				return "EnumButton";
			return "";
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static bool ChangePanelType(ref string json, EPanelType newType)
		{
			if (string.IsNullOrEmpty(json))
				return false;
			JObject root = JObject.Parse(json);
			root["type"] = (int)newType;
			json = root.ToString();
			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static bool ChangeIdName(ref string jsonToChange, string newName)
		{
			if (string.IsNullOrEmpty(jsonToChange))
				return false;
			JObject root = JObject.Parse(jsonToChange);
			root["id"] = newName;
			jsonToChange = root.ToString();
			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		internal static string CreateJsonForm(string id, UI.AddItem.ItemType /*WndObjDescription.WndObjType*/ type, AdditemComboOption aico = null)
		{
			if (type == UI.AddItem.ItemType.Href && aico != null)
				return CreateNewHref(aico.Namespace).ToString();

			JObject obj = new JObject();
			obj["id"] = id;

			if (type == UI.AddItem.ItemType.Generic_Element)
			{
				return obj.ToString();
			}

			WndObjDescription.WndObjType newType;
			if (type == UI.AddItem.ItemType.Panel || type == UI.AddItem.ItemType.View)
				newType = (WndObjDescription.WndObjType)type;
			else
				newType = WndObjDescription.WndObjType.Tile;

			Size idealSize = Size.Empty;
			switch (type)
			{
				case UI.AddItem.ItemType.Panel:
				case UI.AddItem.ItemType.Tile_Standard:
					idealSize = CUtility.GetIdealTileSizeLU(ETileDialogSize.Standard);
					obj["size"] = (int)ETileDialogSize.Standard;
					if (type == AddItem.ItemType.Tile_Standard)
						obj["hasStaticArea"] = true;
					break;

				case UI.AddItem.ItemType.Tile_Mini:
					idealSize = CUtility.GetIdealTileSizeLU(ETileDialogSize.Mini);
					obj["size"] = (int)ETileDialogSize.Mini;
					break;

				case UI.AddItem.ItemType.Tile_Wide:
					idealSize = CUtility.GetIdealTileSizeLU(ETileDialogSize.Wide);
					obj["hasStaticArea"] = true;
					obj["size"] = (int)ETileDialogSize.Wide;
					break;

				case UI.AddItem.ItemType.Frame:
					newType = WndObjDescription.WndObjType.Frame;
					obj["accelerators"] = new JArray();
					idealSize = CUtility.GetIdealViewSize(); break;

				case UI.AddItem.ItemType.View:
					idealSize = CUtility.GetIdealViewSize(); break;
				case UI.AddItem.ItemType.Folder:
				default:
					break;
			}

			obj["type"] = newType.ToString();
			obj["width"] = idealSize.Width;
			obj["height"] = idealSize.Height;
			return obj.ToString();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		internal static JObject CreateNewDocOutItem(DocOutObj doo, string nameUnique)
		{
			JObject obj = new JObject();
			obj["type"] = doo.allowedType.ToString();
			obj["id"] = nameUnique.ToUpper();
			Size idealSize = Size.Empty;
			switch (doo.allowedType)
			{
				case AllowedTypes.Tile: return CreateNewHref(nameUnique);
				case AllowedTypes.ToolbarButton: return CreateNewToolbarButton(ref obj, doo.defaultname);

				case AllowedTypes.View:
					idealSize = CUtility.GetIdealViewSize();
					obj["items"] = new JArray();
					break;
				case AllowedTypes.HeaderStrip:
					idealSize = CUtility.GetIdealHeaderStripSize();
					obj["items"] = new JArray();
					break;

				case AllowedTypes.Toolbar:
					idealSize = CUtility.GetIdealToolbarSize(false);
					obj = CreateNewToolbar(ref obj);
					break;
				case AllowedTypes.TileGroup:
				case AllowedTypes.TileManager:
					// per ora li creiamo uguali
					idealSize = CUtility.GetIdealTileGroupSize();
					obj = CreateNewTileGroup(ref obj);
					break;
				default: break;
			}
			obj["width"] = idealSize.Width;
			obj["height"] = idealSize.Height;
			return obj;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		internal static JObject CreateNewTileGroup(ref JObject obj)
		{
			obj["flex"] = -1;
			obj["items"] = new JArray();
			return obj;
		}

		//--------------------------------------------------------------------------------
		internal static JObject CreateNewToolbar(ref JObject obj)
		{
			string id = obj["id"].ToString().ToUpper();
			obj["name"] = obj["text"] = id.ToUpper();
			obj["items"] = new JArray();
			return obj;
		}

		//--------------------------------------------------------------------------------
		internal static JObject CreateNewToolbarButton(ref JObject obj, string defaultname)
		{
			string id = obj["id"].ToString().ToUpper();
			if (defaultname == ToolbarSeparatorProperties.GetDocOutObj().defaultname)
				obj["isSeparator"] = true;
			else
			{
				obj["name"] = obj["text"] = id.ToUpper();
				obj["icon"] = "";
			}
			return obj;
		}

		//--------------------------------------------------------------------------------
		internal static JObject CreateNewHref(string newTileName)
		{
			JObject obj = new JObject();
			obj["href"] = newTileName;
			return obj;
		}

	}

	//=========================================================================
	internal class JsonUpdateSession
	{
		JObject root;

		public JsonUpdateSession(string json)
		{
			root = JObject.Parse(json);
		}

		internal void Clone(string windowId, string parentId, Point location)
		{
			JObject windowObj = JsonProcessor.Find(root, windowId);
			JObject parentWindowObj = JsonProcessor.Find(root, parentId);
			JsonProcessor.CopyItem(root, parentWindowObj, (JObject)windowObj.DeepClone(), location);
		}

		public string Json { get { return root.ToString(); } }
	}

}
