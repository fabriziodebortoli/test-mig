using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.TBWebFormControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Microarea.EasyBuilder.UI
{
	/// <remarks/>
	//================================================================================
	class JsonPropertiesParser
	{
		public string FileLocation { get; private set; }
		public FormEditor EditorForm { get; private set; }

		//-----------------------------------------------------------------------------
		public JsonPropertiesParser(string fileLocation, FormEditor editor)
		{
			this.FileLocation = fileLocation;
			this.EditorForm = editor;
		}

		//-----------------------------------------------------------------------------
		public DocOutlineProperties Parse(string jsonPath = null)
		{
			if (jsonPath == null)
				jsonPath = FileLocation;
			if (!File.Exists(jsonPath))
			{
				MessageBox.Show(string.Format(Resources.FileNotFound, Path.GetFileName(jsonPath)), Resources.FileNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
			string json = File.ReadAllText(jsonPath);
			JObject jObj = JObject.Parse(json);
			return CreateWrapper(jObj);
		}

		//-----------------------------------------------------------------------------
		internal DocOutlineProperties CreateWrapper(JObject jObj)
		{
			JToken jHref = jObj["href"];
			DocOutlineProperties root = null;
			if (jHref != null)
				root = ParseHRef(jObj, jHref);
			else
			{
				root = FromType(jObj);
			}
			var jItems = jObj["items"];
			if (jItems != null && root != null && root.Type != AllowedTypes.Tile)
				root.Items = ParseItems(jItems);
			return root;
		}

		//-----------------------------------------------------------------------------
		private List<DocOutlineProperties> ParseItems(JToken jItems)
		{
			if (jItems == null) return null;
			List<DocOutlineProperties> items = new List<DocOutlineProperties>();
			try
			{
				foreach (JObject item in ((JArray)jItems).Children())
				{
					var itemparsed = CreateWrapper(item);
					if (itemparsed != null)
						items.Add(itemparsed);
				}
			}
			catch (InvalidCastException)
			{
				MessageBox.Show(@"'Items' sintax incorrect.\nDid you forgot the [ ] brackets?\n Anyway, this token ( and the relatives) will be ignored");
			}
			return items;
		}

		//-----------------------------------------------------------------------------
		private DocOutlineProperties ParseHRef(JObject jObj, JToken jHref)
		{
			string hrefFile = FileFromHref(jHref);
			DocOutlineProperties doc = Parse(hrefFile);
			if (doc == null)
			{//ho riscontrato un errore nel parse ricorsivo
				var node = new DocOutlineProperties(jObj, EditorForm);
				node.Type = AllowedTypes.Error;
				return node;
			}
			DocOutlineProperties outer = (DocOutlineProperties)Activator.CreateInstance(doc.GetType(), jObj, EditorForm);
			outer.HrefObject = doc;
			return outer;
		}

		//-----------------------------------------------------------------------------
		private string FileFromHref(JToken jHref)
		{
			string jsonFileId = jHref.ToString();
			var number = jsonFileId.Contains(".");
			if(jsonFileId.Contains("."))
			//if (jsonFileId.StartsWith("M.") || jsonFileId.StartsWith("D."))
				return StaticFunctions.GetFileFromJsonFileId(jsonFileId);
			string jsonFile = Path.GetDirectoryName(FileLocation);
			return Path.Combine(jsonFile, jsonFileId + NameSolverStrings.TbjsonExtension);
		}

		//-----------------------------------------------------------------------------
		internal DocOutlineProperties FromType(JObject jObj)
		{
			JToken jType = jObj["type"];
			if (jType == null)
				return null;
			AllowedTypes type;
			if (Enum.TryParse<AllowedTypes>(jType.ToString(), out type))
				return GetPropertyFromType(type, jObj);
			return GetPropertyFromType(AllowedTypes.Generic, jObj);
		}

		//-----------------------------------------------------------------------------
		internal DocOutlineProperties GetPropertyFromType(AllowedTypes type, JObject jObj)
		{
			switch (type)
			{
				case AllowedTypes.Undefined:
				case AllowedTypes.Tile:
					return new TileDialogProperties(jObj, EditorForm);
				case AllowedTypes.TileGroup:
				case AllowedTypes.Tab:
					return new TileGroupProperties(jObj, EditorForm);
				case AllowedTypes.TileManager:
				case AllowedTypes.Tabber:
					return new TileManagerProperties(jObj, EditorForm);
				case AllowedTypes.View:
				case AllowedTypes.Panel:
					return new ViewProperties(jObj, EditorForm);
				case AllowedTypes.HeaderStrip:
					return new HeaderStripProperties(jObj, EditorForm);
				case AllowedTypes.LayoutContainer:
					return new LayoutContainerProperties(jObj, EditorForm);
				case AllowedTypes.Frame:
					return new FrameProperties(jObj, EditorForm);
				case AllowedTypes.Toolbar:
					return new ToolbarProperties(jObj, EditorForm);
				case AllowedTypes.ToolbarButton:
					return new ToolbarButtonProperties(jObj, EditorForm);
				default:
					return new DocOutlineProperties(jObj, EditorForm);
			}
		}
	}

	/// <remarks/>
	//================================================================================
	public class DocOutlineProperties : EasyBuilderComponent
	{
		#region properties
		/// <remarks/>
		public virtual bool Border { get { return GetBool("border"); } set { SetBool("border", value); } }
		/// <remarks/>
		public virtual bool HScroll { get { return GetBool("hScroll"); } set { SetBool("hScroll", value); } }
		/// <remarks/>
		public virtual bool VScroll { get { return GetBool("vScroll"); } set { SetBool("vScroll", value); } }
		/// <remarks/>
		public virtual bool AcceptFiles { get { return GetBool("acceptFiles"); } set { SetBool("acceptFiles", value); } }
		/// <remarks/>
		public virtual string Activation { get { return GetString("activation"); } set { SetString("activation", value); } }
		/// <remarks/>
		public virtual bool Enabled { get { return GetBool("enabled"); } set { SetBool("enabled", value); } }
		/// <remarks/>
		public virtual bool Group { get { return GetBool("group"); } set { SetBool("group", value); } }
		/// <remarks/>
		public virtual string Text { get { return GetString("text"); } set { SetString("text", value); } }
		/// <remarks/>
		[DisplayName("Location")]
		public virtual System.Drawing.Point LocationLU
		{
			get { return new System.Drawing.Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}
		/// <remarks/>
		[DisplayName("Size")]
		public virtual System.Drawing.Size SizeLU
		{
			get { return new System.Drawing.Size(Width, Height); }
			set { Width = value.Width; Height = value.Height; }
		}

		/// <remarks/>
		[Browsable(false)]
		public int Width { get { return GetInt("width"); } set { SetInt("width", value); } }
		/// <remarks/>
		[Browsable(false)]
		public int Height { get { return GetInt("height"); } set { SetInt("height", value); } }
		/*	/// <remarks/>
			public virtual int TabOrder {		get { return GetInt("tabOrder"); }		set { SetInt("tabOrder", value); } }
			/// <remarks/>
			public virtual bool TabStop {		get { return GetBool("tabStop"); }		set { SetBool("tabStop", value); } }*/
		/// <remarks/>
		public virtual bool Visible { get { return GetBool("visible"); } set { SetBool("visible", value); } }
		/// <remarks/>
		public virtual int Flex { get { return GetInt("flex"); } set { SetInt("flex", value); } }
		/// <remarks/>
		public virtual string Id { get { return GetString("id"); } set { SetString("id", value); } }

		/// <remarks/>
		[ReadOnly(true)]
		public override string Name { get { return GetString("name"); } }

		/// <remarks/>
		[ReadOnly(true)]
		public INameSpace Namespace { get; }

		/// <remarks/>
		[Browsable(false)]
		public virtual int X { get { return GetInt("x"); } set { SetInt("x", value); } }
		/// <remarks/>
		[Browsable(false)]
		public virtual int Y { get { return GetInt("y"); } set { SetInt("y", value); } }

		/// <remarks/>
		public virtual AllowedTypes Type { get { return (AllowedTypes)GetEnumValue("type"); } set { SetEnumValue("type", value); } }

		/// <remarks/>
		public string Href { get { return GetString("href"); } set { SetString("href", value); } }

		/// <remarks/>
		[Browsable(false)]
		public JObject InnerJObject { get { return jObject; } }
		/// <remarks/>
		[Browsable(false)]
		public DocOutlineProperties HrefObject { get { return hrefObject; } set { hrefObject = value; } }

		private List<DocOutlineProperties> items;
		/// <remarks/>
		public List<DocOutlineProperties> Items
		{
			get
			{
				return items ?? new List<DocOutlineProperties>();
			}
			set { items = value; }
		}
		#endregion

		#region Getters/Setters
		//-----------------------------------------------------------------------------
		internal string GetString(string name)
		{
			return jObject[name]?.ToString();
		}
		//-----------------------------------------------------------------------------
		internal void SetString(string name, string value)
		{
			InnerJObject[name] = value;
			formEditor.SetDirty(true);
		}

		//-----------------------------------------------------------------------------
		internal int GetInt(string name)
		{
			JValue val = jObject[name] as JValue;
			return val == null ? 0 : String.Compare(val.Type.ToString(), "string", true) == 0 ?
				 (int)Enum.Parse(typeof(AllowedTypes), GetString(name)) : val.Value<int>();
		}
		//-----------------------------------------------------------------------------
		internal void SetInt(string name, int value)
		{
			InnerJObject[name] = value;
			formEditor.SetDirty(true);
		}

		//-----------------------------------------------------------------------------
		internal int GetEnumValue(string name)
		{
			JValue val = jObject[name] as JValue;

			if (val == null)
			{ //è un href oppure non ha type specificato
				return hrefObject != null ?
					hrefObject.GetEnumValue(name) : (int)AllowedTypes.Undefined;
			}
			
			if (String.Compare(val.Type.ToString(), "string", true) != 0)  //se non è di tipo stringa, estraggo l'int
				return val.Value<int>();

			AllowedTypes type;
			if (Enum.TryParse<AllowedTypes>(val.ToString(), out type))
				return (int)Enum.Parse(typeof(AllowedTypes), type.ToString());
			else
				return (int)AllowedTypes.Generic; // è un tipo non nullo, ma non gestito
		}
		//-----------------------------------------------------------------------------
		internal void SetEnumValue(string name, Object value)
		{
			InnerJObject[name] = value.ToString();
			formEditor.SetDirty(true);
		}

		//-----------------------------------------------------------------------------
		internal bool GetBool(string name)
		{
			JValue val = jObject[name] as JValue;
			return val == null ? false : val.Value<bool>();
		}
		//-----------------------------------------------------------------------------
		internal void SetBool(string name, bool value)
		{
			InnerJObject[name] = value;
			formEditor.SetDirty(true);
		}
		#endregion

		JObject jObject;
		DocOutlineProperties hrefObject;
		internal FormEditor formEditor;

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public DocOutlineProperties(JObject jObject, FormEditor editor)
		{
			this.jObject = jObject;
			this.Site = new TBSite(this, null, editor, NameSolverStrings.EasyStudioDesigner);
			formEditor = editor;
		}

		//-----------------------------------------------------------------------------
		internal void AddToItems(DocOutlineProperties newItem)
		{
			if (this.Items == null)
				Items = new List<DocOutlineProperties>();
			Items.Add(newItem);
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return this.Type.ToString();
		}
	}

	/// <remarks/>
	//================================================================================
	public class TileDialogProperties : DocOutlineProperties
	{
		/// <remarks/>
		public TileDialogProperties(JObject jObject, FormEditor editor) : base(jObject, editor) { }

		/// <remarks/>
		public bool Collapsible { get { return GetBool("collapsible"); } set { SetBool("collapsible", value); } }
		/// <remarks/>
		public bool Collapsed { get { return GetBool("collapsed"); } set { SetBool("collapsed", value); } }
		/// <remarks/>
		public bool Pinnable { get { return GetBool("pinnable"); } set { SetBool("pinnable", value); } }
		/// <remarks/>
		public bool Pinned { get { return GetBool("pinned"); } set { SetBool("pinned", value); } }

		/// <remarks/>
		public virtual ETileDialogStyle TileDialogStyle { get { return (ETileDialogStyle)GetInt("tileStyle"); } set { SetInt("tileStyle", (int)value); } }

		/*/// <remarks/>			non ha la proprietà		TODOROBY
		public virtual ETileDialogSize TileDialogType { get { return (ETileDialogSize)GetInt("tileStyle"); } set { SetInt("tileStyle", (int)value); } }*/

		//	/// <remarks/>
		//[Browsable(true)] public override string Href {	get { return GetString("href"); }		set { SetString("href", value); } }

		/// <remarks/>
		[Browsable(false)]
		public override System.Drawing.Point LocationLU { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override string Id { get; set; }

	}

	/// <remarks/>
	//================================================================================
	public class TileGroupProperties : DocOutlineProperties
	{
		/// <remarks/>
		public TileGroupProperties(JObject jObject, FormEditor editor) : base(jObject, editor) { }
		/// <remarks/>
		public bool OwnsPane { get { return GetBool("ownsPane"); } set { SetBool("ownsPane", value); } }
		/// <remarks/>
		public bool HasPinnableTiles { get { return GetBool("hasPinnableTiles"); } set { SetBool("hasPinnableTiles", value); } }
		/// <remarks/>
		public string Icon { get { return GetString("icon"); } set { SetString("icon", value); } }
		/// <remarks/>
		public string Hint { get { return GetString("hint"); } set { SetString("hint", value); } }
		/// <remarks/>
		public LayoutType LayoutType { get { return (LayoutType)GetInt("layoutType"); } set { SetInt("layoutType", (int)value); } }
		/// <remarks/>
		/// 
		[Browsable(false)]
		public override System.Drawing.Point LocationLU { get; set; }
		/*/// <remarks/>
		[Browsable(false)]		public override bool TabStop { get; set; }*/

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.TileGroup, "IDC_TILE_GROUP"); }
	}

	/// <remarks/>
	//================================================================================
	public class TileManagerProperties : DocOutlineProperties
	{
		/// <remarks/>
		public TileManagerProperties(JObject jObject, FormEditor editor) : base(jObject, editor) { }

		/// <remarks/>	non ha la proprietà		TODOROBY	public bool AutoStretch { get { return GetBool("collapsed"); } set { SetBool("collapsed", value); } }

		/// <remarks/>
		public string Anchor { get { return GetString("anchor"); } set { SetString("anchor", value); } }

		/// <remarks/>
		[Browsable(false)]
		public override bool Visible { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override string Text { get; set; }

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.TileManager, "IDC_TILE_MANAGER"); }

	}

	/// <remarks/>
	//================================================================================
	public class LayoutContainerProperties : DocOutlineProperties
	{
		//TODOROBY
		/// <remarks/>
		public LayoutContainerProperties(JObject jObject, FormEditor editor) : base(jObject, editor) { }

		/// <remarks/>
		public LayoutType LayoutType { get { return (LayoutType)GetInt("layoutType"); } set { SetInt("layoutType", (int)value); } }

		/// <remarks/>
		public override string ToString()
		{
			return this.Type.ToString() + " : " + this.LayoutType.ToString();
		}
	}


	/// <remarks/>
	//================================================================================
	public class ToolbarProperties : DocOutlineProperties
	{
		/// <remarks/>
		public ToolbarProperties(JObject jObject, FormEditor editor) : base(jObject, editor)
		{
		}

	/*	/// <remarks/>
		public bool Foo1 { get; set; }
		/// <remarks/>
		public bool Foo2 { get; set; }*/

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.Toolbar, "IDD_TOOLBAR_DEFAULT"); }
	}

	/// <remarks/>
	//================================================================================
	public class ToolbarButtonProperties : DocOutlineProperties
	{
		/// <remarks/>
		public ToolbarButtonProperties(JObject jObject, FormEditor editor) : base(jObject, editor)
		{
		}

		/// <remarks/>
		public string Icon { get { return GetString("icon"); } set { SetString("icon", value); } }
		/// <remarks/>
		[Browsable(false)]
		public virtual IconTypes IconType { get { return (IconTypes)GetEnumValue("iconType"); } set { SetEnumValue("iconType", value); } }

		/// <remarks/>
		public bool AlignRight { get { return GetBool("alignRight"); } set { SetBool("alignRight", value); } }

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.ToolbarButton, "ID_TOOLBAR_BUTTON"); }
	}

	/// <remarks/>
	//================================================================================
	public class ToolbarSeparatorProperties : ToolbarButtonProperties
	{
		/// <remarks/>
		public ToolbarSeparatorProperties(JObject jObject, FormEditor editor) : base(jObject, editor)
		{
		}

		/// <remarks/>
		public bool IsSeparator { get; set; }

		/// <remarks/>
		public static new DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.ToolbarButton, "ID_TOOLBAR_SEPARATOR"); }
	}


	/// <remarks/>
	//================================================================================
	public class HeaderStripProperties : DocOutlineProperties
	{
		Binding bindings;
		//TODOROBY
		/// <remarks/>
		public HeaderStripProperties(JObject jObject, FormEditor editor) : base(jObject, editor)
		{
			bindings = new Binding();
		}

		/*	/// <remarks/>
			public string Datasource
			{
				get { return bindings.datasource; }
				set {
					InnerJObject["binding"] = InnerJObject["datasource"] + value +"1";
					//InnerJObject["datasource"] = value;
					bindings.datasource = value;
					formEditor.SetDirty(true);}
			}*/

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.HeaderStrip, "IDC_HEADERSTRIP"); }
	}

	/// <remarks/>
	//================================================================================
	public class Binding
	{
		/// <remarks/>
		public string datasource { get; set; }
	}

	/// <remarks/>
	//================================================================================
	public class FrameProperties : DocOutlineProperties
	{
		/// <remarks/>
		public FrameProperties(JObject jObject, FormEditor editor) : base(jObject, editor)
		{
		}

		/// <remarks/>
		public bool Wizard { get { return GetBool("wizard"); } set { SetBool("wizard", value); } }
		/// <remarks/>
		public bool Stepper { get { return GetBool("stepper"); } set { SetBool("stepper", value); } }
		/// <remarks/>
		public Object[] Accelerators { get; set; }
	}

	/// <remarks/>
	//================================================================================
	public class ViewProperties : DocOutlineProperties
	{
		//TODOROBY
		/// <remarks/>
		public ViewProperties(JObject jObject, FormEditor editor) : base(jObject, editor) { }

		/// <remarks/>
		[Browsable(false)]
		public override bool Border { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override bool AcceptFiles { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override string Activation { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override bool Enabled { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override bool Group { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override string Text { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override System.Drawing.Point LocationLU { get; set; }
		/// <remarks/>
		[Browsable(false)]
		public override System.Drawing.Size SizeLU { get; set; }
		/*	/// <remarks/>
			[Browsable(false)] 		public override int TabOrder { get; set; }
			/// <remarks/>
			[Browsable(false)] 		public override bool TabStop { get; set; }*/
		/// <remarks/>	    
		[Browsable(false)]
		public override bool Visible { get; set; }
		/// <remarks/>	   
		[Browsable(false)]
		public override int Flex { get; set; }
		/// <remarks/>	   
		[Browsable(false)]
		public override int X { get; set; }
		/// <remarks/>	   
		[Browsable(false)]
		public override int Y { get; set; }

		/// <remarks/>
		public static DocOutObj GetDocOutObj() { return new DocOutObj(AllowedTypes.View, "IDD_VIEW"); }
	}

	/// <remarks/>
	[Flags]
	//================================================================================
	public enum AllowedTypes
	{
		/// <remarks/>	
		Generic = -1,
		/// <remarks/>	
		Error = -2,
		/// <remarks/>		
		Undefined = WndObjDescription.WndObjType.Undefined,
		/// <remarks/>
		View = WndObjDescription.WndObjType.View,
		/// <remarks/>
		Toolbar = WndObjDescription.WndObjType.Toolbar,
		/// <remarks/>
		ToolbarButton = WndObjDescription.WndObjType.ToolbarButton,
		/// <remarks/>		
		Frame = WndObjDescription.WndObjType.Frame,
		/// <remarks/>		
		Panel = WndObjDescription.WndObjType.Panel,
		/// <remarks/>
		Tile = WndObjDescription.WndObjType.Tile,
		/// <remarks/>
		TileGroup = WndObjDescription.WndObjType.TileGroup,
		/// <remarks/>
		TileManager = WndObjDescription.WndObjType.TileManager,
		/// <remarks/>
		TilePanel = WndObjDescription.WndObjType.TilePanel,
		/// <remarks/>
		LayoutContainer = WndObjDescription.WndObjType.LayoutContainer,
		/// <remarks/>
		HeaderStrip = WndObjDescription.WndObjType.HeaderStrip,
		/// <remarks/>
		Tabber = WndObjDescription.WndObjType.Tabber,
		/// <remarks/>
		Tab = WndObjDescription.WndObjType.Tab
	};
	/// <remarks/>
	[Flags]
	//================================================================================
	public enum IconTypes
	{
		/// <remarks/>		
		IMG = 0
	}
	/// <remarks/>
	//================================================================================
	public enum LayoutType
	{
		/// <remarks/>
		STRIPE = EContainerLayout.Stripe,
		/// <remarks/>
		COLUMN = EContainerLayout.Column,
		/// <remarks/>
		HBOX = EContainerLayout.Hbox,
		/// <remarks/>
		VBOX = EContainerLayout.Vbox,
		/// <remarks/>
		NONE = -1
	};

	/// <remarks/>
	//================================================================================
	public class DocOutObj
	{
		/// <remarks/>
		public AllowedTypes allowedType { get; set; }
		/// <remarks/>
		public string defaultname { get; set; }

		/// <remarks/>
		public DocOutObj(AllowedTypes a, string dn)
		{
			allowedType = a;
			defaultname = dn;
		}

	}
}
