using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

//TODO RSWEB using Microarea.TaskBuilderNet.Core.WebServicesWrapper; // -> Country, ActivationExpression

using Microarea.Common.StringLoader;
using Microarea.Common.CoreTypes;
using Microarea.Common.NameSolver;
using Microarea.Common.Lexan;
using TaskBuilderNetCore.Interfaces;
using System.Xml;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microarea.Common.Applications
{

    // Definizioni di ELEMENT e ATTRIBUTE dei files enums.xml
    //=============================================================================        
    public class EnumsXml
	{
		//================================================================================
		public class Element
		{
			public const string Enums			= "Enums";
			public const string Tag				= "Tag";
			public const string Item			= "Item";
			public const string DefaultValue	= "DefaultValue";
		}
		//================================================================================
		public class Attribute
		{
			public const string Value			= "value";
			public const string Name			= "name";
			public const string DefaultValue	= "defaultValue";
			public const string Stored			= "stored";
			public const string AllowISO		= "allowISO";
			public const string DenyISO			= "denyISO";
			public const string ISOSeparator	= ",";
            public const string Activation      = "activation";
            public const string Localized       = "localized";
            public const string ExternalValue   = "externalValue";
        }
	}

	//=============================================================================
	public class EnumsException : Exception
	{
		//--------------------------------------------------------------------------------
		public EnumsException(string message) : base(message) {}
	}

	//=============================================================================        
	public class EnumItem : IComponent, /*ICustomTypeDescriptor, */INotifyPropertyChanged, INotifyPropertyChanging
	{
		public readonly static ushort ItemError	= 0xFFFF;

		public event EventHandler Disposed;
		private ISite site;

		private string name;
		private ushort value;
		private string description="";
		private bool hidden;
		private int stored;
        private string externalValue;
	
		private NameSolver.ModuleInfo moduleInfo;
		private EnumTag owner;

		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public EnumTag Owner
		{
			get { return owner; }
		}
		//-----------------------------------------------------------------------------
		public string Name
		{
			get { return name; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Name"));
				name = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Name"));

				if (this.site != null)
					this.site.Name = name;
			}
		}
		//-----------------------------------------------------------------------------
		public string Description
		{
			get { return description; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Description"));
				description = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Description"));
			}
		}
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public bool Hidden
		{
			get { return hidden; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Hidden"));
				hidden = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Hidden"));
			}
		}
		//-----------------------------------------------------------------------------
		[ReadOnly(true)]
		public int Stored
		{
			get { return stored; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Stored"));
				stored = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Stored"));
			}
		}

        //-----------------------------------------------------------------------------
        [ReadOnly(true)]
        public string ExternalValue
        {
            get { return externalValue; }
            set { externalValue = value; }
        }

		//-----------------------------------------------------------------------------
		public EnumItem(EnumTag owner)
		{
			this.owner = owner;
		}

		//-----------------------------------------------------------------------------
		public EnumItem(EnumTag owner, string	aName, ushort aValue)
		{
			this.Name	= aName;
			this.value = aValue;
			this.owner	= owner;
		}

		//-----------------------------------------------------------------------------
		public EnumItem(EnumTag owner, string	aName, ushort aValue, ModuleInfo moduleInfo)
		{
			this.Name	= aName;
			this.value = aValue;
			this.owner = owner;
			
			// di default punta al parent tag
			this.moduleInfo = null;

			if (this.owner.OwnerModule != moduleInfo)
				this.moduleInfo = moduleInfo;
		}

		//-----------------------------------------------------------------------------
		public EnumItem(EnumItem enumItem)
		{
			this.description = enumItem.description;
			this.hidden = enumItem.hidden;
			this.name = enumItem.name;

			this.moduleInfo = enumItem.moduleInfo;
			this.owner = enumItem.owner;
			this.site = enumItem.site;

			this.stored = enumItem.stored;
			this.value = enumItem.value;
		}

		//-----------------------------------------------------------------------------
		public ushort Value
		{
			get { return this.value; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Value"));
				this.value = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Value"));

				UpdateStored();
			}
		}
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public string FullValue { get { return string.Format("{{{0}:{1}}}", owner.Value, value); } }
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ModuleInfo	OwnerModule { get { return moduleInfo == null ? owner.OwnerModule : moduleInfo; } }
		
		//-----------------------------------------------------------------------------
		public string LocalizedName { get { return owner.Localizer.Translate(name); } }
		
		//-----------------------------------------------------------------------------
		public string LocalizedDescription { get { return owner.Localizer.Translate(description); } }

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
		
		//-----------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			EnumItem enumItem = item as EnumItem;
			
			if (enumItem == null)
				return false;

			return
				name == enumItem.Name &&
				value == enumItem.Value;
		}

		//-----------------------------------------------------------------------------
		public void SetItem(string aName, ushort aValue)
		{
			this.name = aName;
			this.value = aValue;
		}

		//-----------------------------------------------------------------------------
		public void Unparse(XmlNode aNode)
		{
		
		}

		#region IComponent Members

		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ISite Site
		{
			get
			{
				return this.site;
			}
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Site"));
				this.site = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Site"));
			}
		}

		//-----------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//-----------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		#endregion

		#region ICustomTypeDescriptor Members

		//-----------------------------------------------------------------------------
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(GetType()); 
		}

		//-----------------------------------------------------------------------------
		public string GetClassName()
		{
			return GetType().ToString(); 
		}

		//-----------------------------------------------------------------------------
		public string GetComponentName()
		{
			return Site == null ? String.Empty : Site.Name; 
		}

		//-----------------------------------------------------------------------------
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(GetType()); 
		}

		//-----------------------------------------------------------------------------
		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(GetType()); 
		}

		//-----------------------------------------------------------------------------
		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(GetType()); 
		}

		//-----------------------------------------------------------------------------
		public object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(GetType(), editorBaseType); 
		}

		//-----------------------------------------------------------------------------
		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(GetType(), attributes); 
		}

		//-----------------------------------------------------------------------------
		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(GetType()); 
		}

		//-----------------------------------------------------------------------------
		//public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		//{
		//	return readOnlyPropertiesManager.GetProperties(attributes);
		//}

		//-----------------------------------------------------------------------------
		//public PropertyDescriptorCollection GetProperties()
		//{
		//	return GetProperties(null); 
		//}

		//-----------------------------------------------------------------------------
		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this; 
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		//-----------------------------------------------------------------------------
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}

		#endregion

		#region INotifyPropertyChanging Members

		public event PropertyChangingEventHandler PropertyChanging;

		//-----------------------------------------------------------------------------
		protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, e);
		}

		#endregion

		#region ICloneable Members

		//-----------------------------------------------------------------------------
		public object Clone()
		{
			return new EnumItem(this);
		}

		#endregion

		//-----------------------------------------------------------------------------
		//public void SetAllPropertiesReadOnly(bool readOnly)
		//{
		//	readOnlyPropertiesManager.SetAllPropertiesReadOnly(readOnly);
		//}

		//-----------------------------------------------------------------------------
		private void UpdateStored()
		{
			DataEnum de = new DataEnum(Owner.Value, Value);
			int stored = 0;
			Int32.TryParse(de.ToString(), out stored);

			Stored = stored;
		}
	}
	                      
	//=============================================================================        
	public class EnumItems : List<EnumItem>
	{
		//-----------------------------------------------------------------------------
		new public EnumItem this[int i] { get { return base[i] as EnumItem; } set { base[i] = value; }}

		//-----------------------------------------------------------------------------
		public EnumItem GetItem (string aName)
		{
			foreach (EnumItem ei in this)
				if (string.Compare(ei.Name, aName, StringComparison.OrdinalIgnoreCase) == 0)
					return ei;

			return null;
		}

		//-----------------------------------------------------------------------------
		public EnumItem GetItem (ushort aValue)
		{
			foreach (EnumItem ei in this)
				if (ei.Value == aValue)
					return ei;

			return null;
		}

        //-----------------------------------------------------------------------------
        public EnumItem GetItemByStoredValue(int storedValue)
        {
            foreach (EnumItem ei in this)
                if (ei.Stored == storedValue)
                    return ei;

            return null;
        }
		//-----------------------------------------------------------------------------
		public ushort GetValue (string aName)
		{
			EnumItem item = GetItem(aName);
			return item != null ? item.Value : EnumItem.ItemError;
		}

		//-----------------------------------------------------------------------------
		public string GetName (ushort aValue)
		{
			EnumItem item = GetItem(aValue);
			if (item != null)
				return item.Name;

			return aValue.ToString();
		}

		//-----------------------------------------------------------------------------
		public string GetLocalizedName (ushort aValue)
		{
			EnumItem item = GetItem(aValue);
			if (item != null)
				return item.LocalizedName;

			return aValue.ToString();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			EnumItems given = obj as EnumItems;
			if (given == null)
				return false;

			if (this.Count != given.Count)
				return false;

			bool areEqual = true;
			for (int i = 0; i < this.Count; i++)
			{
				areEqual |= this[i].Equals(given[i]);
				if (!areEqual)
					break;
			}
			return areEqual;
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			int hashCode = Environment.TickCount;
			foreach (var item in this)
				hashCode ^= item.GetHashCode();

			return hashCode;
		}
	}                                        


	//=============================================================================        
	public class EnumTag : IComponent, IContainer, /*ICustomTypeDescriptor, */INotifyPropertyChanged, INotifyPropertyChanging
	{
		public readonly static ushort TagError = 0xFFFF;
		public readonly static ushort MaxValue =0xFFF0; // alcuni sono riservati
		public readonly static ushort ReportStatusTag = 0xFFFE; // alcuni sono riservati

		[Browsable(false)]
		public event EventHandler Disposed;
		private ISite site;

		private string name;
		private ushort value;
		private bool hidden;
		private ushort defaultValue;
		private string description = "";
		public EnumItems EnumItems	=  new EnumItems();
		
		private ModuleInfo owner;

		private Dictionary<string, ILocalizer> localizers = new Dictionary<string, ILocalizer>();	//different localizers depending on thread culture (multithreading support)

		private int longerItemIdx = -1;	// give EnumItem idx with most wide string in Enum Names

		//-----------------------------------------------------------------------------
		public EnumTag
			(
			ModuleInfo		owner,
			string				name,
			ushort				aValue,
			ushort				defaultValue			
			)
		{
			this.name			= name;
			this.value			= aValue;
			this.defaultValue	= defaultValue;
			this.owner			= owner;
		}

		//-----------------------------------------------------------------------------
		public EnumTag(EnumTag enumTag)
		{
			this.defaultValue = enumTag.defaultValue;
			this.description = enumTag.description;

			this.EnumItems = new EnumItems();
			foreach (EnumItem enumItem in enumTag.EnumItems)
				EnumItems.Add(enumItem.Clone() as EnumItem);

			this.hidden = enumTag.hidden;
			this.localizers = enumTag.localizers;

			this.longerItemIdx = enumTag.longerItemIdx;
			this.name = enumTag.name;
			this.owner = enumTag.owner;

			this.site = enumTag.site;

			this.value = enumTag.value;
		}

		//-----------------------------------------------------------------------------
		public string Name
		{
			get { return name; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Name"));
				name = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Name"));

				if (this.site != null)
					this.site.Name = name;
			}
		}
		//-----------------------------------------------------------------------------
		public ushort Value
		{
			get { return this.value; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Value"));
				this.value = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Value"));
			}
		}
		//-----------------------------------------------------------------------------
		public ushort DefaultValue
		{
			get { return defaultValue; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("DefaultValue"));
				defaultValue = value;
				OnPropertyChanged(new PropertyChangedEventArgs("DefaultValue"));
			}
		}
		//-----------------------------------------------------------------------------
		public string Description
		{
			get { return description; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Description"));
				description = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Description"));
			}
		}
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public bool Hidden
		{
			get { return hidden; }
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Hidden"));
				hidden = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Hidden"));
			}
		}
		
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ModuleInfo OwnerModule { get { return owner; } }

		//-----------------------------------------------------------------------------
		public string LocalizedName { get { return Localizer.Translate(name); } }
		
		//-----------------------------------------------------------------------------
		public string LocalizedDescription { get { return Localizer.Translate(description); } }

		/// <summary>
		/// Restituisce l'oggetto che si occupa di localizzare il tag ed i 
		/// suoi items
		/// </summary>
		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ILocalizer Localizer 
		{ 
			get 
			{
                ILocalizer localizer = null;
                if (!localizers.TryGetValue(Helper.Culture, out localizer))
				{
					localizer = new EnumLocalizer(name, owner.GetDictionaryPath());
					localizers[Helper.Culture] = localizer;
				}
				return localizer;
			}
		}
		
		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object item)
		{
			EnumTag enumTag = item as EnumTag;

			if (enumTag == null)
				return false;

			return
				name			== enumTag.Name				&&
				value			== enumTag.Value;
		}

		//-----------------------------------------------------------------------------
		public EnumItem AddItem(string aName, ushort aValue, string aDescription, ModuleInfo moduleInfo)
		{
			if (ExistItem(aName, aValue))
				return null;

			EnumItem item = new EnumItem(this, aName, aValue, moduleInfo);
			item.Description = aDescription;
			EnumItems.Add(item);
            int nIdx = EnumItems.Count - 1;


            if	(
				longerItemIdx < 0 ||
				EnumItems[longerItemIdx].Name.Length < aName.Length
				)
				longerItemIdx = nIdx;

			return item;
		}

        //--------------------------------------------------------------------------------
        public bool ChangeTagDefaultValue(string moduleNamespace, ushort defaultValue)
        {
            if (string.Compare(OwnerModule.NameSpace.ToString(), moduleNamespace, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.defaultValue = defaultValue;
                return true;
            }
            
            return false;
        }

        //-----------------------------------------------------------------------------
        public bool DeleteItem(string moduleNamespace, string aName)
		{
			foreach (EnumItem ei in EnumItems)
				if 
                    (
                        string.Compare(ei.Name, aName, StringComparison.OrdinalIgnoreCase) == 0 &&
                        string.Compare(ei.OwnerModule.NameSpace.ToString(), moduleNamespace, StringComparison.OrdinalIgnoreCase) == 0
                    )
				{
                    if (DefaultValue != ei.Value)
                    {
                        EnumItems.Remove(ei);
                        return true;
                    }
                    else
                        return false;
				}

            return false;
		}

		//-----------------------------------------------------------------------------
		public bool ExistItem(string aName)
		{                       
			foreach (EnumItem ei in EnumItems)
				if (string.Compare(ei.Name, aName, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			
			return false;
		}

		//-----------------------------------------------------------------------------
		public bool ExistItem(ushort aValue)
		{                       		
			return GetItem(aValue) != null;
		}

		//-----------------------------------------------------------------------------
		public bool ExistItem(string aName, ushort aValue)
		{                       
			foreach (EnumItem ei in EnumItems)
				if (
					(string.Compare(ei.Name, aName, StringComparison.OrdinalIgnoreCase) == 0) ||
					(ei.Value == aValue)
					)
					return true;
			
			return false;
		}

		//-----------------------------------------------------------------------------
		public void SetTag(string aName, ushort aValue)
		{
			name	= aName;
			value	= aValue;
		}

		//-----------------------------------------------------------------------------
		public ushort GetLongerItemValue()
		{
			Debug.Assert(longerItemIdx >= 0 && longerItemIdx < EnumItems.Count);
			return EnumItems[longerItemIdx].Value;
		}

		//-----------------------------------------------------------------------------
		public EnumItem GetItemByName(string name)
		{
			foreach (EnumItem ei in EnumItems)
				if (string.Compare(ei.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					return ei;
			return null;
		}

        //-----------------------------------------------------------------------------
        public EnumItem GetItem(ushort aValue)
        {
            foreach (EnumItem ei in EnumItems)
                if (ei.Value == aValue)
                    return ei;
            return null;
        }

        //-----------------------------------------------------------------------------
        public EnumItem GetItemByStoredValue(int storedValue)
        {
            return EnumItems.GetItemByStoredValue(storedValue);
        }
        


		#region IComponent Members

		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ISite Site
		{
			get
			{
				return this.site;
			}
			set
			{
				OnPropertyChanging(new PropertyChangingEventArgs("Site"));
				this.site = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Site"));
			}
		}

		//-----------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//-----------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		#endregion

		#region IContainer Members

		//-----------------------------------------------------------------------------
		public void Add(IComponent component, string name)
		{
			EnumItem item = component as EnumItem;
			if (item == null)
				return;

			AddItem(item.Name, item.Value, item.Description, item.OwnerModule);
		}

		//-----------------------------------------------------------------------------
		public void Add(IComponent component)
		{
			Add(component, String.Empty);
		}

		//-----------------------------------------------------------------------------
		[Browsable(false)]
		public ComponentCollection Components
		{
			get
            {    // TODO RSWeb       ComponentCollection vuoto;
                 //return new ComponentCollection(EnumItems.ToArray(typeof(IComponent)) as IComponent[]);
                return null;
            }
		}

		//-----------------------------------------------------------------------------
		public void Remove(IComponent component)
		{
			EnumItem item = component as EnumItem;
			if (item == null)
				return;

			EnumItems.Remove(item);
		}

		#endregion

		#region ICustomTypeDescriptor Members

		//-----------------------------------------------------------------------------
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(GetType());
		}

		//-----------------------------------------------------------------------------
		public string GetClassName()
		{
			return GetType().ToString();
		}

		//-----------------------------------------------------------------------------
		public string GetComponentName()
		{
			return Site == null ? String.Empty : Site.Name;
		}

		//-----------------------------------------------------------------------------
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(GetType());
		}

		//-----------------------------------------------------------------------------
		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(GetType());
		}

		//-----------------------------------------------------------------------------
		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(GetType());
		}

		//-----------------------------------------------------------------------------
		public object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(GetType(), editorBaseType);
		}

		//-----------------------------------------------------------------------------
		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(GetType(), attributes);
		}

		//-----------------------------------------------------------------------------
		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(GetType());
		}

		//-----------------------------------------------------------------------------
		//public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		//{
		//	return readOnlyPropertiesManager.GetProperties(attributes);
		//}

		//-----------------------------------------------------------------------------
		//public PropertyDescriptorCollection GetProperties()
		//{
		//	return GetProperties(null);
		//}

		//-----------------------------------------------------------------------------
		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		//-----------------------------------------------------------------------------
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}

		#endregion

		#region INotifyPropertyChanging Members

		public event PropertyChangingEventHandler PropertyChanging;

		//-----------------------------------------------------------------------------
		protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, e);
		}

		#endregion

		#region ICloneable Members

		//-----------------------------------------------------------------------------
		public object Clone()
		{
			return new EnumTag(this);
		}

		#endregion

		//-----------------------------------------------------------------------------
		//public void SetAllPropertiesReadOnly(bool readOnly)
		//{
		//	readOnlyPropertiesManager.SetAllPropertiesReadOnly(readOnly);
		//}
	}       

	//=============================================================================        
	public class EnumTags : List<EnumTag>
	{
        private string country = string.Empty;
        private string applicationName = string.Empty;
        private string moduleName = string.Empty;

        //-----------------------------------------------------------------------------
        //TODO RSWEB 
        private ILoginManager loginMng = null;

        public string ModuleName { get { return moduleName; } set { moduleName = value; } }
        public string ApplicationName { get { return applicationName; } set  { applicationName = value; }}
        public ILoginManager LoginManager
        {
            get
            {
				return loginMng;
            }
            set
            {
                loginMng = value;
            }
        }

        //-----------------------------------------------------------------------------
        protected string Country
        {
            get
            {
                if (country == string.Empty && loginMng != null)
                {
                    country = loginMng.GetCountry();
                }
                return country;
            }
        }

        protected bool CheckActivationExpression(string parentApplicationName, string activationExpr)
        {
            return loginMng == null ? true : loginMng.CheckActivationExpression(parentApplicationName, activationExpr);
        }

		//-----------------------------------------------------------------------------
		new public EnumTag this[int i] { get { return base[i] as EnumTag; } set { base[i] = value; }}

		//-----------------------------------------------------------------------------
		public bool	ExistName	(string aName)	{ return GetTag(aName) != null; }
		//--------------------------------------------------------------------------------
		public bool	ExistValue	(ushort aValue)	{ return GetTag(aValue) != null; }

        //--------------------------------------------------------------------------------
        public string GetExternalValue(uint aStoredValue) 
        {
            EnumTag et = GetTag((ushort)(aStoredValue >> 16));
            if (et == null)
                return string.Empty;

            EnumItem ei = et.GetItem((ushort)(aStoredValue & 0xFFFF));
            if (ei == null)
                return string.Empty;

             return ei.ExternalValue; 
        }

		//-----------------------------------------------------------------------------
		public EnumTags()
		{
		}

		//-----------------------------------------------------------------------------
		public EnumItems EnumItems(string aName)
		{
			foreach (EnumTag tag in this)
				if (string.Compare(tag.Name, aName, StringComparison.OrdinalIgnoreCase) == 0)
					return tag.EnumItems;
			
			return null;
		}

		//-----------------------------------------------------------------------------
		public EnumItems EnumItems (ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag.EnumItems;

			return null;
		}

		//-----------------------------------------------------------------------------
		public ushort GetLongerItemValue (ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag.GetLongerItemValue();

			return EnumTag.TagError;
		}
			
		//-----------------------------------------------------------------------------
		public ushort GetValue (string	aName)
		{
			foreach (EnumTag tag in this)
				if (string.Compare(tag.Name, aName, StringComparison.OrdinalIgnoreCase) == 0)
					return tag.Value;

			return EnumTag.TagError;
		}

		//-----------------------------------------------------------------------------
		public string GetName (ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag.Name;

			return aValue.ToString();
		}

		//-----------------------------------------------------------------------------
		public string GetLocalizedName (ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag.LocalizedName;

			return aValue.ToString();
		}

		//-----------------------------------------------------------------------------
		public ushort GetDefaultItemValue (ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag.DefaultValue;

			return 0;
		}

		//-----------------------------------------------------------------------------
		public ushort GetValue (string tagName, string itemName)
		{
			EnumItems enumTags = EnumItems(itemName);
			return (enumTags == null ? EnumTag.TagError : enumTags.GetValue(tagName));
		}

		//-----------------------------------------------------------------------------
		public EnumTag AddTag (ModuleInfo owner, string aName, ushort aValue) { return AddTag(owner, aName, aValue, 0); }
		//--------------------------------------------------------------------------------
		public EnumTag AddTag 
			(
			ModuleInfo	owner,
			string			aName, 
			ushort			aValue, 
			ushort			defaultItemValue
			)
		{                       
			if (ExistTag(aName, aValue))
				return null;
				
			EnumTag enumTag = new EnumTag (owner, aName, aValue, defaultItemValue);
			Add(enumTag);

			return enumTag;
		}

        //-----------------------------------------------------------------------------
        public bool DeleteTag(string moduleNamespace, string aName)
		{
			foreach (EnumTag tag in this)
				if 
                    (
                        string.Compare(tag.Name, aName, StringComparison.OrdinalIgnoreCase) == 0 &&
                        string.Compare(tag.OwnerModule.NameSpace.ToString(), moduleNamespace, StringComparison.OrdinalIgnoreCase) == 0 
                    )
				{
					Remove(tag);
					return true;
				}

            return false;
		}

        //-----------------------------------------------------------------------------
        public EnumTag GetTag(string aName)
		{
			foreach (EnumTag tag in this)
				if (string.Compare(tag.Name, aName, StringComparison.OrdinalIgnoreCase) == 0)
					return tag;
			
			return null;
		}

		//-----------------------------------------------------------------------------
		public EnumTag GetTag(ushort aValue)
		{
			foreach (EnumTag tag in this)
				if (tag.Value == aValue)
					return tag;
			
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool ExistTag(string aName, ushort aValue)
		{                       
			foreach (EnumTag tag in this)
				if	(
					(string.Compare(tag.Name, aName, StringComparison.OrdinalIgnoreCase) == 0) ||
					(tag.Value == aValue)
					)
					return true;

			return false;
		}

		//-----------------------------------------------------------------------------
		public bool ExistTag(ushort aValue)
		{                       
			return GetTag(aValue) != null;
		}

        //dato lo stored value di un item restituisce il tag di appartenenza
        //-----------------------------------------------------------------------------
        public EnumTag GetTagByStoredValue(int storedValue)
        {
            foreach (EnumTag tag in this)
            {
                if (tag.GetItemByStoredValue(storedValue) != null)
                    return tag;
            }

            return null;
        }

		//-----------------------------------------------------------------------------
		//	Syntax :
		//		ENUM COLORE_ENUM = 10 (ROSSO, BIANCO = 5, VERDE)
		//		SET COLORE_SET = 10 (ROSSO, BIANCO = 2, VERDE)	// 2 = secondo bit alzato
		//
		//-----------------------------------------------------------------------------
		internal bool Parse (Parser lex, ModuleInfo owner)
		{
			string		itemName;
			ushort		itemValue = 0;
			string		tagName;
			ushort		tagValue = 0;
				
			ushort		defaultItemValue = 0;
			bool		isDefault = false;

			ushort		nextTagValue = 0;
			ushort		nextItemValue = 0;

			lex.SkipToken(); // Skip ENUM

			if (!lex.ParseString(out tagName))
				return false;
			
			// controlla che non esista nel linguaggio o che non sia già stato definito
			if	(Language.ExistToken(tagName))
			{
				lex.SetError(string.Format(ApplicationsStrings.TagNameNotValid, tagName));
				return false;
			}
			EnumTag tag = GetTag(tagName);
			if (tag != null)
			{
				lex.SetError(string.Format(ApplicationsStrings.TagNameExist, tagName));
				return false;
			}
	 
			// la sintassi di INSERT (vedi enterprise) non è più supportata
			if (lex.Matched(Token.INSERT)) 
			{
				lex.SetError(ApplicationsStrings.InsertUnsupported);
				return false;
			}
			if (lex.Matched(Token.ASSIGN))
			{
				int dummy = 0;
				if (!lex.ParseInt(out dummy))
					return false;

				// sequential token MUST be sequntially numbered			
				nextTagValue = tagValue = (ushort) dummy;
				nextTagValue++;

				if (tagValue > EnumTag.MaxValue)
				{
					lex.SetError(string.Format(ApplicationsStrings.TagValueExist, tagValue, tagName));
					return false;
				}
			}
			else
				tagValue = nextTagValue++;
		
			// adesso sono considerati tutti readonly (non c'è più l'interfaccia per la modifica)
			lex.Matched(Token.READ_ONLY);
			
			if (!lex.ParseOpen())
				return false;

			// Loop on All Items
			while (!lex.Matched(Token.ROUNDCLOSE) && !lex.Error)
			{
				if (!lex.ParseString (out itemName))
					return false;
				
				int dummy = 0;
				if (lex.Matched(Token.ASSIGN))
				{
					if (!lex.ParseInt(out dummy))
						return false;
					
					// sequential token MUST be sequentially numbered			
					nextItemValue = itemValue = (ushort)dummy;
					nextItemValue++;
				}
				else
					itemValue = nextItemValue++;

				// Try to see if this value is also default
				if (lex.Matched(Token.DEFAULT))
				{                                                      
					if (isDefault)
					{
						lex.SetError(ApplicationsStrings.OnlyOneDefault);
						return false;
					}	
					isDefault = true;
					defaultItemValue = itemValue; // for SET MUST do bitwhise OR
				}
				
				// MUST be separate by comma
				if (!lex.LookAhead(Token.ROUNDCLOSE) && !lex.ParseComma())
					return false;
			    
				// add tag only if present are good
				if (tag == null)
					if ((tag = AddTag (owner, tagName, tagValue)) == null)
					{
						lex.SetError(string.Format(ApplicationsStrings.TagValueExist, tagValue, tagName));
						return false;
					}

				// Parola chiave del linguaggio
				if	(Language.ExistToken(itemName))
				{
					lex.SetError(string.Format(ApplicationsStrings.ItemNameNotValid, itemName));
					return false;
				}

				// check for duplicate Name for all items of current tag and all global tokens
				if	(tag.ExistItem(itemName))
				{
					lex.SetError(string.Format(ApplicationsStrings.ItemNameExist, itemName, tagName, lex.Filename));
					return false;
				}

				// Add correct item
				if (tag.AddItem(itemName, itemValue, "", owner) == null)
				{
					lex.SetError(ApplicationsStrings.ItemValueExist);
					return false;
				}
			}

            // Add default value if any (otherwise add first item value)
            if (isDefault)
                tag.DefaultValue = defaultItemValue;
            else if (tag.EnumItems.Count > 0)
                tag.DefaultValue = tag.EnumItems[0].Value;

			return !lex.Error;
		}

		/// <summary>
		/// Attenzione!!!!!
		/// Bisogna tenere conto che gli enumerativi possono essere dichiarati in più moduli.
		/// Ovvero stesso enumerativo (nome e valore) ma item diversi
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		public bool LoadXml(string filename, string appName, ModuleInfo owner = null, bool checkActivation = false)
		{
            if (String.IsNullOrEmpty(filename) || !PathFinder.PathFinderInstance.ExistFile(filename))
                return true;

			ushort		itemValue = 0;
			ushort		tagValue = 0;
			ushort		nextItemValue = 0;

            this.ApplicationName = appName;
            this.ModuleName = owner.Name;

            XmlDocument dom = null;
            dom = PathFinder.PathFinderInstance.LoadXmlDocument(dom, filename);

			if (!dom.DocumentElement.HasChildNodes)
				return true;

			XmlNode enumsNode = dom.DocumentElement.FirstChild;
			
			if (enumsNode == null)
                return false;

			XmlNodeList enums = enumsNode.SelectNodes("/" + EnumsXml.Element.Enums + "/" + EnumsXml.Element.Tag);
			foreach (XmlElement tagNode in enums)
			{
				string tagName = tagNode.GetAttribute(EnumsXml.Attribute.Name);

				// se esiste già allora corrisponde alla vecchia sintassi di insert. Posso 
				// aggiungere solo item nuovi altrimenti ignoro i nuovi e tengo i vecchi
				EnumTag tag = GetTag(tagName);

				// Deve esistere l'attributo VALUE
				string tagValueString = tagNode.GetAttribute(EnumsXml.Attribute.Value);
				if (tagValueString == null || tagValueString == String.Empty)
					throw (new EnumsException(ApplicationsStrings.TagValueMustBePresent));
				
				try
				{
					tagValue = UInt16.Parse(tagValueString);
				}
				catch(FormatException)
				{
					throw (new EnumsException(String.Format(ApplicationsStrings.TagValueMustBeValidUInt16Formatted, tagValueString)));
				}
				catch(OverflowException)
				{
					throw (new EnumsException(String.Format(ApplicationsStrings.TagValueMustBeValidUInt16Range, tagValueString, UInt16.MinValue.ToString(), UInt16.MaxValue.ToString())));
				}

				// l'attributo VALUE non deve andare sopra quelli riservati
				if (tagValue > EnumTag.MaxValue)
					throw (new EnumsException(String.Format(ApplicationsStrings.TagValueReserved, tagValue)));

				EnumTag tagWithSameValue = GetTag(tagValue);

				if (tagWithSameValue != null && tag == null)
				{
					Debug.Fail("Enum with tag value " + tagValue.ToString() + " is already present.");
					if (tag == null)
						tag = tagWithSameValue;
					else
						break;
				}

				bool tagHidden = false;
				string allowISOTag	= tagNode.GetAttribute(EnumsXml.Attribute.AllowISO);
				string denyISOTag	= tagNode.GetAttribute(EnumsXml.Attribute.DenyISO);
				if (owner != null && checkActivation && !IsValidCountry(allowISOTag, denyISOTag, Country))
					tagHidden = true;

                if (checkActivation && owner != null)
                {
                    string activationExpr = tagNode.GetAttribute(EnumsXml.Attribute.Activation);
                    if (!CheckActivationExpression(owner.ParentApplicationName, activationExpr))
                        tagHidden = true;
                }
              
				// parse <Item> nodes
				XmlNodeList items = tagNode.SelectNodes(EnumsXml.Element.Item);
				foreach(XmlElement itemNode in items)
				{
					string itemName = itemNode.GetAttribute(EnumsXml.Attribute.Name);
					if (itemName == "")
						throw (new EnumsException(string.Format(ApplicationsStrings.ItemNameMustExist, tagName)));
				
					// se non esiste l'attributo Value allora prendo il precedente incrementato di uno
					string itemValueString = itemNode.GetAttribute(EnumsXml.Attribute.Value);

					string allowISOAtt	= itemNode.GetAttribute(EnumsXml.Attribute.AllowISO);
					string denyISOAtt	= itemNode.GetAttribute(EnumsXml.Attribute.DenyISO);

					bool itemHidden = false;
					if (checkActivation && !IsValidCountry(allowISOAtt, denyISOAtt, Country))
						itemHidden = true;

                    if (checkActivation && owner != null)
                    {
                        string activationExpr = itemNode.GetAttribute(EnumsXml.Attribute.Activation);
                        if (!CheckActivationExpression(owner.ParentApplicationName, activationExpr))
                            itemHidden = true;
                    }

					if (itemValueString != "")
					{
						nextItemValue = itemValue = ushort.Parse(itemValueString);
						nextItemValue++;
					}
					else
						itemValue = nextItemValue++;

					// Se non esisteva già aggiungo il nuovo tag
					if (tag == null)
					{
						tag = AddTag (owner, tagName, tagValue);
						if (tag == null)
						{
							Debug.Fail("Enum with tag value " + tagValue.ToString() + " cannot be loaded.");
							break;
						}
						tag.Description = GetTextDescription(tagNode);
					}
					tag.Hidden = tagHidden;

					// non deve esistere già per il tag associato
					if	(tag.ExistItem(itemName))
						throw (new EnumsException(string.Format(ApplicationsStrings.ItemNameExist, itemName, tagName, filename)));

					// Aggiunge l'item alll'enumeratico corrente
					EnumItem newItem =  tag.AddItem(itemName, itemValue, GetTextDescription(itemNode), owner);
					if (newItem == null)
						throw (new EnumsException(ApplicationsStrings.ItemValueExist));

					newItem.Hidden = itemHidden;
					string stored = itemNode.GetAttribute(EnumsXml.Attribute.Stored);
					try
					{
						newItem.Stored = Int32.Parse(stored);
					}
					catch(Exception)
					{
 						//throw (new EnumsException(string.Format("Exception retrieving value of: {0}", itemName)));
                        Debug.Assert(false, string.Format("Exception retrieving stored value of: {0}", itemName));
                        newItem.Stored = tagValue << 16 + itemValue;
					}

                    newItem.ExternalValue = itemNode.GetAttribute(EnumsXml.Attribute.ExternalValue);
                }

				// parse <DefaultValue> nodes
				string defValueString = string.Empty; // mi serve per valorizzare il valore del <Tag>

				XmlNodeList defValues = tagNode.SelectNodes(EnumsXml.Element.DefaultValue);
				foreach(XmlElement defValueNode in defValues)
				{
					string allowISOAtt	= defValueNode.GetAttribute(EnumsXml.Attribute.AllowISO);
					string denyISOAtt	= defValueNode.GetAttribute(EnumsXml.Attribute.DenyISO);
					if (checkActivation && !IsValidCountry(allowISOAtt, denyISOAtt, Country))
						continue;

                    if (checkActivation && owner != null)
                    {
                        string activationExpr = defValueNode.GetAttribute(EnumsXml.Attribute.Activation);
                        if (!CheckActivationExpression(owner.ParentApplicationName, activationExpr))
                            continue;
                    }

					string foundDefault = defValueNode.GetAttribute(EnumsXml.Attribute.Value);
					EnumItem defaultItem = tag.EnumItems.GetItem(ushort.Parse(foundDefault));

					if (defaultItem != null && !defaultItem.Hidden)
						defValueString = foundDefault;
				}

				if (tag.EnumItems.Count == 0)
					throw (new EnumsException(string.Format(ApplicationsStrings.ItemsMustBePresent, tagName)));

				if (defValueString == string.Empty ||
					tag.EnumItems.GetItem(ushort.Parse(defValueString)) == null)
				{
					// gestisce la sintassi default. Se non c'è prende il valore del primo item
					string tagDefault = tagNode.GetAttribute(EnumsXml.Attribute.DefaultValue);

					if (tagDefault != string.Empty)
					{
						EnumItem tagDefaultItem = tag.EnumItems.GetItem(ushort.Parse(tagDefault));
						if (tagDefaultItem != null && !tagDefaultItem.Hidden)
						{
							tag.DefaultValue = ushort.Parse(tagDefault);
							continue;
						}
					}
					
					EnumItem zeroValueItem	= tag.EnumItems.GetItem(0);
					if ((zeroValueItem != null && !zeroValueItem.Hidden))
					{
						tag.DefaultValue = zeroValueItem.Value;
						continue;
					}

					foreach (EnumItem firstTagItem in tag.EnumItems)
					{
						if (!firstTagItem.Hidden)
						{
							tag.DefaultValue = firstTagItem.Value;
							break;
						}
					}
				}
				else
					tag.DefaultValue = ushort.Parse(defValueString);			
			}
			return true;
		}

        //-----------------------------------------------------------------------------
        public bool SaveXml(string filename, bool localizedVersion)
		{	
			filename = Path.ChangeExtension(filename, NameSolverStrings.XmlExtension);
			return SaveXml(filename, localizedVersion, false);
		}
		
		//-----------------------------------------------------------------------------
		public bool SaveXml(string fileName, bool localizedVersion, bool useLocalizeAttribute, ModuleInfo info = null)
		{	
			XmlDocument dom = new XmlDocument();
			// root node
			XmlDeclaration declaration = dom.CreateXmlDeclaration("1.0", "utf-8", "yes");
			dom.RemoveAll();

			dom.AppendChild(declaration);
			XmlNode enums = dom.CreateElement(EnumsXml.Element.Enums);
			dom.AppendChild(enums);

			// aggiunge tutti i tag che trova
			foreach (EnumTag enumTag in this)
			{
                if (info != null && enumTag.OwnerModule != info)
                    continue;

				XmlElement tagElement = dom.CreateElement(EnumsXml.Element.Tag);

                AddAttribute(dom, tagElement, EnumsXml.Attribute.Name, localizedVersion && !useLocalizeAttribute ? enumTag.LocalizedName : enumTag.Name);
				AddAttribute(dom, tagElement, EnumsXml.Attribute.Value, enumTag.Value.ToString());
                if (localizedVersion && useLocalizeAttribute)
                    AddAttribute(dom, tagElement, EnumsXml.Attribute.Localized, enumTag.LocalizedName);

				//gli aggiungo l'Attributo Default se è diverso dal primo
				if (enumTag.EnumItems.Count > 0 && enumTag.DefaultValue != enumTag.EnumItems[0].Value)
				{
					AddAttribute(dom, tagElement, EnumsXml.Attribute.DefaultValue, enumTag.DefaultValue.ToString());
				}
				
				string description = localizedVersion ? enumTag.LocalizedDescription : enumTag.Description;
				if (description != null && description.Length > 0)
					tagElement.AppendChild(dom.CreateTextNode(description));

				// aggiunge tutti gli elementi
				foreach (EnumItem enumItem in enumTag.EnumItems)
				{
                    if (info != null && enumItem.OwnerModule != info)
                        continue;

                    DataEnum de = new DataEnum(enumTag.Value, enumItem.Value);
					XmlElement itemElement = dom.CreateElement(EnumsXml.Element.Item);

					AddAttribute(dom, itemElement, EnumsXml.Attribute.Name, localizedVersion && !useLocalizeAttribute ? enumItem.LocalizedName : enumItem.Name);
					AddAttribute(dom, itemElement, EnumsXml.Attribute.Value, enumItem.Value.ToString());
					AddAttribute(dom, itemElement, EnumsXml.Attribute.Stored, de.ToString());
                    if (localizedVersion && useLocalizeAttribute)
                        AddAttribute(dom, itemElement, EnumsXml.Attribute.Localized, enumItem.LocalizedName);

					description = localizedVersion ? enumItem.LocalizedDescription : enumItem.Description;
					if (description != null && description.Length > 0)
						itemElement.AppendChild(dom.CreateTextNode(description));

					tagElement.AppendChild(itemElement);
				}
				enums.AppendChild(tagElement);
			}

            return PathFinder.PathFinderInstance.SaveTextFileFromXml(fileName, dom);
		}

		//-----------------------------------------------------------------------------
		public bool SaveXml(string filename)
		{
			return SaveXml(filename, false);
		}

		//--------------------------------------------------------------------------------
		private string GetTextDescription(XmlNode node)
		{
			XmlNode textNode = node.SelectSingleNode("text()");
			return (textNode == null) ? string.Empty : textNode.Value.Trim();
		}

		//-----------------------------------------------------------------------------
		internal void AddAttribute(XmlDocument dom, XmlElement element, string attributeName, string data)
		{	
			XmlAttribute attribute = dom.CreateAttribute(attributeName);
			attribute.Value = data;
			element.Attributes.Append(attribute);
		}

		//-----------------------------------------------------------------------------
		private bool IsValidCountry(string allowISO, string denyISO, string country)
		{
			if (allowISO == string.Empty && denyISO == string.Empty)
				return true;
			
			string temp = string.Empty;
			string[] array = null;

			// la priorità è sull'attributo allowISO
			if (allowISO != string.Empty)
			{
				temp = "," + allowISO + ","; // metto una virgola in cima e in fondo al contenuto dell'attributo
				array = temp.Split(new Char[] {','});	// splitto i codici ISO tra le virgole

				for (int i = 0; i < array.Length; i++)
				{
					// faccio il Trim per togliere i blank (per pararci)
					if (string.Compare(country, array[i].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
						return true;
				}

				return false;
			}

			temp = "," + denyISO + ",";
			array = temp.Split(new Char[] {','});

			for (int i = 0; i < array.Length; i++)
			{
				// faccio il Trim per togliere i blank (per pararci)
				if (string.Compare(country, array[i].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
					return false;
			}

			return true;
		}
	}
	 
	// Sitassi del file enums.XML
	// <Enums>
	//		<Tag name="colore" value="1"  defaultValue="23">
	//			<Item name="rosso" value="22" stored="65559"\>
	//			<Item name="rosso" value="23" stored="65560"\>
	//			<Item name="rosso" value="24" stored="65561"\>
	//		</Tag>
	// </Enums>
	//=============================================================================        
	public class Enums
	{
		public readonly int RELEASE = 2;

		private bool		loaded = false;
		private EnumTags	enumTags;
		//-----------------------------------------------------------------------------
		public bool		Loaded	{ get { return loaded; }}
		//--------------------------------------------------------------------------------
		public EnumTags	Tags	{ get { return enumTags; }}
		
		//-----------------------------------------------------------------------------
		public EnumItems EnumItems(string aName)
		{
			return enumTags.EnumItems(aName);
		}

        public EnumItems EnumItems(ushort tag)
        {
            return enumTags.EnumItems(tag);
        }

        //-----------------------------------------------------------------------------
        public Enums()
		{
			// carica tutti gli enums che fanno parte della applicazione
			enumTags = new EnumTags();
		}

		//-----------------------------------------------------------------------------
		public void LoadIni()
		{
			loaded = true;
			foreach (ApplicationInfo ai in PathFinder.PathFinderInstance.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
				{
					string filename = mi.GetEnumsIniPath();
					if (!LoadIni(filename, mi)) loaded = false;
				}
		}

		//-----------------------------------------------------------------------------
		public void LoadXml ()
		{
			LoadXml(true);
		}
		//-----------------------------------------------------------------------------
		public void LoadXml (bool checkActivation)
		{
			this.loaded = true;

			//Le applicazioni EasyStudio che vivono nella custom appaiono comunque
			//in PathFinder.PathFinderInstance.ApplicationInfos...
			foreach (ApplicationInfo ai in PathFinder.PathFinderInstance.ApplicationInfos)
			{
				//...quindi, per ogni modulo...
				foreach (ModuleInfo mi in ai.Modules)
				{
					//...GetEnumsPath() può ritornare il percorso al file degli
					//enumerativi nella custom (per una customizzazione EasyStudio)...
					string standardFilename = mi.GetEnumsPath();

					//...comunque per il percorso ritenuto "dalla standard" carico gli enumerativi...
					if (!LoadXml(standardFilename, mi, checkActivation, ai.Name))
					{
						this.loaded = false;
					}
					

                    //TODO LARA BRUNA EASYBUILDER
					////...poi vado a valutare il percorso ritenuto "dalla custom":
					////se non coincide con quello "della standard", allora lo carico
					////altrimenti no perchè lo ho già caricato.
					//string customFilename = mi.GetCustomEnumsPath(subscription);
					//if (String.Compare(standardFilename, customFilename, StringComparison.OrdinalIgnoreCase) != 0)
					//{
					//	if (!LoadXml(customFilename, mi, checkActivation, ai.Name))
					//	{
					//		this.loaded = false;
					//	}
					//}
				}
			}
		}

		//-----------------------------------------------------------------------------
		public string ConvertAllIniIntoXml()
		{
			foreach (ApplicationInfo ai in PathFinder.PathFinderInstance.ApplicationInfos)
				foreach (ModuleInfo mi in ai.Modules)
				{
					string filename = mi.GetEnumsIniPath();
					if (!FromIniToXml(filename, mi)) 
						return filename;
				}

			return null;
		}

		//-----------------------------------------------------------------------------
		public EnumTag AddEnumTag(ModuleInfo owner, string aName, ushort aValue)	{ return AddEnumTag(owner, aName, aValue, 0);}
		
		//--------------------------------------------------------------------------------
		public EnumTag AddEnumTag
			(
			ModuleInfo owner,
			string		aName,
			ushort		aValue,
			ushort		defaultItemValue
			)	
		{
			return enumTags.AddTag(owner, aName, aValue, defaultItemValue); 
		}

		//-----------------------------------------------------------------------------
		public bool AddEnumValue (EnumTag tag, string aName, ushort aValue, ModuleInfo moduleInfo)	
		{ 
			return tag.AddItem(aName, aValue, "", moduleInfo) != null; 
		}

		//-----------------------------------------------------------------------------
		private bool Parse(Parser lex, ModuleInfo owner)
		{
			int	nRelease = 1;
		    
			if (!lex.ParseTag (Token.RELEASE) || !lex.ParseInt (out nRelease))
				return false;

			if (nRelease != RELEASE)
			{
				lex.SetError(ApplicationsStrings.BadEnumsRelease);
				return false;
			}
	
			// no TYPEDEF section present
			if (!lex.LookAhead(Token.TYPEDEF))
				return true;

			// Start to Parse TYPEDEF (ENUMS) definition
			lex.SkipToken();
		
			bool bSingle = !lex.Matched(Token.BEGIN);	
			while(!lex.Error)
			{
				// to accept empty Begin .. End blocks
				if (lex.Matched(Token.END))
				{
					if (bSingle)
					{
						lex.SetError(ApplicationsStrings.SyntaxError);
						return false;
					}
					break;
				}
				switch (lex.LookAhead())
				{
					case Token.ENUM:
						if (!enumTags.Parse(lex, owner))
							return false;
						break;
						
					default :
						lex.SetError(ApplicationsStrings.BadType);
						return false;
				}
			
				if (bSingle) return true;
			}

			return !lex.Error;
		}

		//-----------------------------------------------------------------------------
		public bool LoadIni(string filename, ModuleInfo owner)
		{
			Parser lex = new Parser(Parser.SourceType.FromFile);


            if (!PathFinder.PathFinderInstance.ExistFile(filename))
                return true;
				
			if (lex.Open(filename))
			{
				bool ok = Parse(lex, owner);
				lex.Close();
				return ok;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
        public bool LoadXml(string filename, ModuleInfo owner, bool checkActivation, string appName)
		{
			try
			{
                return enumTags.LoadXml(filename, appName, owner, checkActivation);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}
		}

		
		// converte il singolo file da formato .ini in formato .xml
		//-----------------------------------------------------------------------------
		public bool FromIniToXml(string filename, ModuleInfo owner)
		{
      
            if (!PathFinder.PathFinderInstance.ExistFile(filename))
                return true;

			// pulisce tutto quello che era stato caricato
			enumTags = new EnumTags();

			// carica il singolo file
			if (!LoadIni(filename, owner))
				return false;

			// salva quello appena caricato
			return enumTags.SaveXml(filename);
		}
	
		/// <summary>
		///  ritorna il nome della sola parte tag come pura sintassi string
		/// </summary>
		/// <param name="de">Il DataEnum del quale si vuole il TagName</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public string TagName(DataEnum de)
		{
			return enumTags.GetName(de.Tag);
		}

		/// <summary>
		///  ritorna il nome della sola parte tag come pura sintassi string
		/// </summary>
		/// <param name="de">Il DataEnum del quale si vuole il TagName</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public string TagName(ushort val)
		{
			return enumTags.GetName(val);
		}

		/// <summary>
		///  ritorna il nome della sola parte item come pura sintassi string
		/// </summary>
		/// <param name="de">Il DataEnum del quale si vuole l'ItemName</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public string ItemName(DataEnum de)
		{
			EnumTag enumTag = enumTags.GetTag(de.Tag);
			return enumTag == null ? "" : enumTag.EnumItems.GetName(de.Item);
		}

		//------------------------------------------------------------------------------
		public string LocalizedTagName(DataEnum de)
		{
			return enumTags.GetLocalizedName(de.Tag);
		}

		//------------------------------------------------------------------------------
		public string LocalizedItemName(DataEnum de)
		{
			EnumTag enumTag = enumTags.GetTag(de.Tag);
			return enumTag == null ? "" : enumTag.EnumItems.GetLocalizedName(de.Item);
		}

		//------------------------------------------------------------------------------
		public bool ExistTag(string tagName)
		{
			return enumTags.GetTag(tagName) != null;
		}

		//--------------------------------------------------------------------------------
		public bool ExistTag(ushort tagValue)
		{
			return enumTags.GetTag(tagValue) != null;
		}

		//------------------------------------------------------------------------------
		public ushort TagValue(string tagName)
		{
			EnumTag enumTag = enumTags.GetTag(tagName);
			if (enumTag == null) 
				return 0;

			return enumTag.Value;
		}

		//------------------------------------------------------------------------------
		public ushort ItemValue(string tagName, string itemName)
		{
			EnumTag enumTag = enumTags.GetTag(tagName);
			if (enumTag == null) 
				return 0;
			
			return enumTag.EnumItems.GetValue(itemName);
		}

		// contiene solo il nome dell'item e prende il tag dal template passato
		//-----------------------------------------------------------------------------
		public DataEnum Parse(string itemName, DataEnum template)
		{
			string tagName = TagName(template);
			ushort itemValue = ItemValue(tagName, itemName);

			DataEnum op = new DataEnum(template.Tag, itemValue);
			return op;
		}

		// Costruisce un DataEnum a parire dalla conoscenza del Tag (numerico) e del
		// suo ItemName localizzato. (usato ad esempio nelle combo di EasyLook)
		//-----------------------------------------------------------------------------
		public DataEnum LocalizedParse(string itemName, DataEnum template)
		{
			string tagName = TagName(template);
			EnumTag enumTag = enumTags.GetTag(tagName);
			if (enumTag == null) 
				return template;
			
			foreach (EnumItem ei in enumTag.EnumItems)
				if (string.Compare(ei.LocalizedName, itemName, StringComparison.OrdinalIgnoreCase) == 0)
					return new DataEnum(template.Tag, ei.Value);

			return template;
		}

		//-----------------------------------------------------------------------------
		public DataEnum DataEnumCreate(string tagName, string itemName)
		{
			ushort tagValue = TagValue(tagName);
			ushort itemValue = ItemValue(tagName, itemName);

			return new DataEnum(tagValue, itemValue);
		}

		/// <summary>
		/// ritorna il nome dell'intero DataEnum nel formato:
		///		{"NomeDelTag":"NomeDellItem"} (es: {"Colore":"Rosso"}
		/// </summary>
		/// <param name="de">Il DataEnum del quale si vuole l'intero nome esteso</param>
		/// <returns></returns>
		//------------------------------------------------------------------------------
		public string FullName(DataEnum de)
		{
			string tag = TagName(de);
			string item = ItemName(de);

			string full = "{" + tag + ":" + item + "}";
			return full;
		}
        
        //dato lo stored value di un item restituisce il tag di appartenenza
        //-----------------------------------------------------------------------------
        public EnumTag GetTagByStoredValue(int storedValue)
        {
            return enumTags.GetTagByStoredValue(storedValue);
        }

		//-----------------------------------------------------------------------------
		public static string GetJsonEnumsTable()
		{
			Enums enums = new Enums(); //TODOLUCA, manca localizzazione
			enums.LoadXml(true);

			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				JsonWriter jsonWriter = new JsonTextWriter(sw);

				try
				{
					jsonWriter.WriteStartObject();
					jsonWriter.WritePropertyName("enums");

					jsonWriter.WriteStartObject();
					jsonWriter.WritePropertyName("tags");
					jsonWriter.WriteStartArray();

					foreach (EnumTag tag in enums.Tags)
					{
						jsonWriter.WriteStartObject();

						jsonWriter.WritePropertyName("name");
						jsonWriter.WriteValue(tag.LocalizedName);
						jsonWriter.WritePropertyName("value");
						jsonWriter.WriteValue(tag.Value);

						//jsonWriter.WriteStartObject();
						jsonWriter.WritePropertyName("items");
						jsonWriter.WriteStartArray();
						foreach (EnumItem item in tag.EnumItems)
						{
							jsonWriter.WriteStartObject();
							jsonWriter.WritePropertyName("name");
							jsonWriter.WriteValue(item.LocalizedName);
							jsonWriter.WritePropertyName("value");
							jsonWriter.WriteValue(item.Value);
							jsonWriter.WritePropertyName("stored");
							jsonWriter.WriteValue(item.Stored);
							jsonWriter.WriteEndObject();
						}
						//jsonWriter.WriteEndObject();
						jsonWriter.WriteEndArray();
						jsonWriter.WriteEndObject();

					}

					jsonWriter.WriteEndArray();
					jsonWriter.WriteEndObject();
					jsonWriter.WriteEndObject();
				}
				catch (Exception)
				{
				}

				return sb.ToString();
			}
		}
	}
}
