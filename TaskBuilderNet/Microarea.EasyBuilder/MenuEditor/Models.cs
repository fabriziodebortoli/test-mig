using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.EasyBuilder.MenuEditor 
{
    //=========================================================================
    [Serializable]
	internal abstract class BaseMenuItem : EasyBuilderTypeDescriptor, IComparable, IComponent
	{
		private string insertAfter = String.Empty;
		private string insertBefore = String.Empty;
		private MenuItemTitle title;
		private string name = String.Empty;
		private string activationExpression = String.Empty;
		private string imageFilePath;
		private ISite site;
		private string owner = String.Empty;

		/// <remarks/>
		public event EventHandler Disposed;
		/// <remarks/>
		public event EventHandler<MenuItemEventArgs> MenuItemAdded;
		/// <remarks/>
		public event EventHandler<MenuItemEventArgs> MenuItemMoved;
		/// <remarks/>
		public event EventHandler<MenuItemEventArgs> MenuItemRemoved;
		/// <remarks/>
		public event EventHandler<MenuItemPropertyValueChangedEventArgs> PropertyValueChanged;

		//---------------------------------------------------------------------
		[Browsable(false)]
		public string ImageFilePath
		{
			get { return imageFilePath; }
			set
			{
				if (String.Compare(imageFilePath, value) == 0)
					return;

				string currentValue = imageFilePath;
				imageFilePath = value;
				OnPropertyValueChanged("ImageFilePath", currentValue);
			}
		}

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuItemNamePropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Design"),
		AffectsAppearanceAttribute(true),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME)]
		public virtual string Name 
        {
            get { return name; } 
            set 
            {
                if (String.Compare(name, value) == 0)
                    return;

                string currentValue = name;
                name = value;
                OnPropertyValueChanged("Name", currentValue);
            }
        }
        //---------------------------------------------------------------------
        [TypeConverterAttribute(typeof(MenuItemTitleConverter)),
		LocalizedDescriptionAttribute("MenuItemTitlePropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Appearance"),
		AffectsAppearanceAttribute(true),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_TITLE)]
		public MenuItemTitle Title 
        {
            get { return title; }
            set 
            {
                if (title == value)
                    return;

				if (title != null)
				{
					title.PropertyChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(Title_PropertyChanged);
					if (title.ContainerComponent != null)
					{
						title.ContainerComponent = null;
					}
				}

                MenuItemTitle currentValue = title;
                
                title = value;

				if (title != null)
				{
					title.PropertyChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(Title_PropertyChanged);
					title.ContainerComponent = this;
				}

                OnPropertyValueChanged("Title", currentValue);
            }
        }

        //---------------------------------------------------------------------
        [LocalizedDescriptionAttribute("MenuItemActivationPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_ACTIVATION),
		Browsable(false)]
        public string Activation 
        {
            get { return activationExpression; } 
            set 
            {
                if (String.Compare(activationExpression, value) == 0)
                    return;

                string currentValue = activationExpression;
                
                activationExpression = value;

                OnPropertyValueChanged("Activation", currentValue); 
            }
        }

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuCommandInsertAfterPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER),
		Browsable(false)]
		public string InsertAfter
		{
			get { return insertAfter; }
			set
			{
				if (String.Compare(insertAfter, value) == 0)
					return; 
				
				string currentValue = insertAfter;

				insertAfter = value;

				OnPropertyValueChanged("InsertAfter", currentValue);
			}
		}

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuCommandInsertBeforePropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE),
		Browsable(false)]
		public string InsertBefore
		{
			get { return insertBefore; }
			set
			{
				if (String.Compare(insertBefore, value) == 0)
					return;

				string currentValue = insertBefore;

				insertBefore = value;

				OnPropertyValueChanged("InsertBefore", currentValue);
			}
		}

		//---------------------------------------------------------------------
		[MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_OWNER),
		Browsable(false)]
		public string Owner
		{
			get { return owner; }
			set
			{
				if (owner == value)
					return;

				string currentValue = owner;

				owner = value;

				OnPropertyValueChanged("Owner", currentValue);
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		internal bool CanBeCustomized
		{
			get
			{
				return this.Owner == MenuEditorEngine.GetMyOwnerString();
			}
		}

		/// <remarks/>
		[Browsable(false)]
		//---------------------------------------------------------------------
		public ISite Site { get { return this.site; } set { this.site = value; } }

		//---------------------------------------------------------------------
		/// <remarks/>
		protected BaseMenuItem(MenuItemTitle aMenuItemTitle, string activation)
		{
			title = aMenuItemTitle;
			activationExpression = activation;
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public BaseMenuItem()
		{
			imageFilePath = string.Empty;
			name = string.Empty;
			title = null;
			activationExpression = string.Empty;
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public BaseMenuItem(MenuXmlNode aMenuXmlNode, IContainer container)
		{
			if (aMenuXmlNode == null)
				return;

			this.Name = aMenuXmlNode.GetNameAttribute();
			this.ImageFilePath = aMenuXmlNode.GetImageFileName();
			this.Title = new MenuItemTitle(aMenuXmlNode.Title, aMenuXmlNode.IsTitleLocalizable);
			this.Activation = aMenuXmlNode.GetActivationAttribute();

			this.InsertBefore = aMenuXmlNode.GetInsertBeforeAttribute();
			this.InsertAfter = aMenuXmlNode.GetInsertAfterAttribute();

			this.Site = new TBSite(this, container, null, Name);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public BaseMenuItem(string aName, string aImageFilePath, MenuItemTitle aTitle, string aActivation)
		{
			imageFilePath = aImageFilePath;
			name = aName;
			title = aTitle;
			activationExpression = aActivation;
			this.Site = new TBSite(this, null, null, name);
		}

        ////-----------------------------------------------------------------------------
        //public override bool Equals(object obj)
        //{
        //    BaseMenuItem aBaseMenuItem = obj as BaseMenuItem;
        //    if (aBaseMenuItem == null)
        //        return false;

        //    return String.Compare(this.name, aBaseMenuItem.name, StringComparison.InvariantCultureIgnoreCase) == 0;
        //}

        ////-----------------------------------------------------------------------------
        //public override int GetHashCode()
        //{
        //    return this.name.GetHashCode();
        //}

		//-----------------------------------------------------------------------------
		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			//Uso il TypeDescriptor sul tipo e non sull'istanza perchè usandolo sull'istanza non funziona:
			//infatti viene usato il DictionaryService ottenuto tramite il Site del Component in causa per memorizzare una cache per la collezione delle proprietà.
			//Questa cache è una per ogni istanza, e a seconda del controllo viene ritornata sempre la stessa causando problemi alla PropertyGrid
			//Ogni qualvolta si selezionavano oggetti di tipo diverso (per esempio combo box e tab)
			//Lavorando sui tipi invece viene utilizzata una cache per ogni tipo e non per ogni istanza, cosa che, oltre ad essere più efficiente, non causa
			//malfunzionamenti alla PropertyGrid nel nostro caso.
			PropertyDescriptorCollection defaultProperties;
			if (attributes == null)
				defaultProperties = TypeDescriptor.GetProperties(GetType());
			else
				defaultProperties = TypeDescriptor.GetProperties(GetType(), attributes);

			PropertyDescriptor[] props = new PropertyDescriptor[defaultProperties.Count];
			for (int i = 0; i < props.Length; i++)
				props[i] = new EasyBuilderPropertyDescriptor(defaultProperties[i]);

			PropertyDescriptorCollection properties = new PropertyDescriptorCollection(props);
			for (int i = properties.Count - 1; i >= 0; i--)
			{
				PropertyDescriptor descriptor = properties[i];

				// instance readonly 
				EasyBuilderPropertyDescriptor tbDescriptor = descriptor as EasyBuilderPropertyDescriptor;

				if (tbDescriptor != null)
					tbDescriptor.IsReadOnlyForContext = String.IsNullOrWhiteSpace(owner);
			}

			return properties;
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public virtual BaseMenuItem Clone()
        {
            BaseMenuItem clonedMenuItem = GetType().Assembly.CreateInstance(GetType().FullName) as BaseMenuItem;

            clonedMenuItem.Name = this.Name;
            clonedMenuItem.Title = (this.Title != null) ? new MenuItemTitle(this.Title.Text, this.Title.Localizable) : null;
            clonedMenuItem.Activation = this.Activation;
			clonedMenuItem.Owner = MenuEditorEngine.GetMyOwnerString();

            PropertyInfo eventsPropertyInfo = this.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
            if (eventsPropertyInfo == null)
				return clonedMenuItem;

            System.ComponentModel.EventHandlerList eventHandlerList = eventsPropertyInfo.GetValue(this, null) as System.ComponentModel.EventHandlerList;
            if (eventHandlerList == null)
				return clonedMenuItem;

            System.Reflection.EventInfo[] events = this.GetType().GetEvents();
            foreach (System.Reflection.EventInfo aEventInfo in events)
            {
                FieldInfo eventKeyField = this.GetType().GetField("Event" + aEventInfo.Name, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);

				if (eventKeyField == null)
					continue;

                object eventKeyValue = eventKeyField.GetValue(this);
				if (eventKeyValue == null)
					continue;

                System.EventHandler eventHandler = eventHandlerList[eventKeyValue] as System.EventHandler;
                if (eventHandler != null)
                    aEventInfo.AddEventHandler(clonedMenuItem, new System.EventHandler(eventHandler));
            }

            return clonedMenuItem;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public virtual void Clear()
		{
			name					= string.Empty;
			title					= null;
			activationExpression	= String.Empty;
			insertAfter				= String.Empty;
			insertBefore			= String.Empty;
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public virtual int CompareTo(object obj)
		{
			return CompareTo(obj, true, CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------
        protected virtual void OnPropertyValueChanged(string propertyName, object previousValue)
        {
            PropertyInfo changedProperty = this.GetType().GetProperty(propertyName);
            if (changedProperty == null)
                return;

            if (PropertyValueChanged != null)
                PropertyValueChanged(
					this,
					new MenuItemPropertyValueChangedEventArgs(
						this,
						changedProperty,
                        previousValue
						)
					);

        }

        //---------------------------------------------------------------------
		protected virtual void OnMenuItemAdded(MenuItemEventArgs e)
        {
            if (MenuItemAdded != null)
                MenuItemAdded(this, e);
		}

        //---------------------------------------------------------------------
        protected virtual void OnMenuItemMoved(MenuItemEventArgs e)
        {
            if (MenuItemMoved != null)
                MenuItemMoved(this, e);
        }

        //---------------------------------------------------------------------
        protected virtual void OnMenuItemRemoved(MenuItemEventArgs e)
        {
            if (MenuItemRemoved != null)
                MenuItemRemoved(this, e);
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public virtual int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1; // This instance is greater than obj.

			BaseMenuItem aGenericItemModel = obj as BaseMenuItem;

			if (aGenericItemModel == null)
				throw new ArgumentException("The 'obj' argument is not a BaseMenuItem object.");

			int result = String.Compare(name, aGenericItemModel.name, ignoreCase, culture);

			if (result != 0)
				return result;
		
			result = title.CompareTo(aGenericItemModel.title, true, CultureInfo.InvariantCulture);

			if (result != 0)
				return result;

			return String.Compare(activationExpression, aGenericItemModel.activationExpression, ignoreCase, culture);
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public override string ToString()
        {
            return name;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public virtual string GetMenuItemTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.GenericMenuItemTypeDescription;
        }

        //---------------------------------------------------------------------
        public virtual string GetXmlTag()
        {
            return null;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public virtual string GetNodePathHierarchyElement()
		{
			string xmlTag = GetXmlTag();
			if (xmlTag == null || xmlTag.Length == 0)
				return string.Empty;

			if (name != null && name.Length > 0)
				return String.Format(
				"{0}[@{1}='{2}']",
				xmlTag,
				MenuXmlNode.XML_ATTRIBUTE_NAME,
				name
				);

			return String.Format(
				"{0}[{1}='{2}']",
				xmlTag,
				MenuXmlNode.XML_TAG_TITLE,
				this.title != null ? this.title.Text : string.Empty
				);
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public virtual XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            string menuItemXmlTag = GetXmlTag();
            if (String.IsNullOrEmpty(menuItemXmlTag))
                return null;

            if (aMenuXmlDocument == null)
            {
                aMenuXmlDocument = new XmlDocument();
                // if the PreserveWhitespace property is set to false, 
                // XmlDocument auto-indents the output.
                aMenuXmlDocument.PreserveWhitespace = false;
            }

            XmlElement menuItemNode = aMenuXmlDocument.CreateElement(menuItemXmlTag);
            if (menuItemNode == null)
                return null;

            menuItemNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME, name);

            if (!String.IsNullOrEmpty(activationExpression))
                menuItemNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_ACTIVATION, activationExpression);

			if (!String.IsNullOrEmpty(Owner))
				menuItemNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_OWNER, Owner);

            if (title != null)
            {
                XmlElement titleElement = aMenuXmlDocument.CreateElement(MenuXmlNode.XML_TAG_TITLE);
                if (titleElement != null)
                {
                    titleElement.InnerText = title.Text;
                    titleElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_LOCALIZABLE, title.Localizable.ToString(CultureInfo.InvariantCulture));

                    menuItemNode.AppendChild(titleElement);
                }
            }

            menuItemNode.Normalize();

            return menuItemNode;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public string GetXmlElementText(bool indented)
        {
            XmlElement xmlNode = GetXmlElement(null);
            if (xmlNode == null)
                return String.Empty;

            if (!indented)
                return xmlNode.OuterXml;

            MemoryStream memStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(memStream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            xmlNode.WriteTo(writer);

            writer.Flush();
            memStream.Flush();
            
            memStream.Position = 0;
            StreamReader sReader = new StreamReader(memStream);
            string result = sReader.ReadToEnd();

            sReader.Dispose();
            memStream.Dispose();

            return result;
        }

        //---------------------------------------------------------------------
		protected virtual void SubItem_PropertyValueChanged(
			object sender,
			MenuItemPropertyValueChangedEventArgs e
			)
		{
			if (PropertyValueChanged != null)
				PropertyValueChanged(this, e);
		}

		//---------------------------------------------------------------------
		protected virtual void MenuItem_SubMenuItemAdded(object sender, MenuItemEventArgs e)
		{
			if (MenuItemAdded != null)
				MenuItemAdded(this, e);
		}

        //---------------------------------------------------------------------
        protected virtual void MenuItem_SubMenuItemMoved(object sender, MenuItemEventArgs e)
        {
            if (MenuItemMoved != null)
                MenuItemMoved(this, e);
        }
        
        //---------------------------------------------------------------------
		protected virtual void MenuItem_SubMenuItemRemoved(object sender, MenuItemEventArgs e)
		{
			if (MenuItemRemoved != null)
				MenuItemRemoved(this, e);
		}

        //---------------------------------------------------------------------
        private void Title_PropertyChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
        {
            MenuItemTitle previousValue = null;
            if (e.Property.Name == "Text")
                previousValue = new MenuItemTitle(e.PreviousValue as string, title.Localizable);
            else if (e.Property.Name == "Localizable")
                previousValue = new MenuItemTitle(title.Text, (bool)e.PreviousValue);

            OnPropertyValueChanged("Title", previousValue);
        }

		//---------------------------------------------------------------------
		protected void OnDisposed()
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}
	}

	//=========================================================================
	internal class MenuModel : BaseMenuItem, IContainer
    {

        protected int m_iMaxDepth = 2;

		//---------------------------------------------------------------------
		public static BaseMenuItem GetMenuItemFromClipboard()
		{
			IDataObject data = Clipboard.GetDataObject();
			if (data == null)
				return null;

			string format = null;
			if (data.GetDataPresent((format = typeof(MenuApplication).FullName)))
				return data.GetData(format) as MenuApplication;

			if (data.GetDataPresent((format = typeof(MenuGroup).FullName)))
				return data.GetData(format) as MenuGroup;

			if (data.GetDataPresent((format = typeof(MenuBranch).FullName)))
				return data.GetData(format) as MenuBranch;

			if (data.GetDataPresent((format = typeof(DocumentMenuCommand).FullName)))
				return data.GetData(format) as DocumentMenuCommand;

			if (data.GetDataPresent((format = typeof(ReportMenuCommand).FullName)))
				return data.GetData(format) as ReportMenuCommand;

			if (data.GetDataPresent((format = typeof(BatchMenuCommand).FullName)))
				return data.GetData(format) as BatchMenuCommand;

			if (data.GetDataPresent((format = typeof(FunctionMenuCommand).FullName)))
				return data.GetData(format) as FunctionMenuCommand;

			if (data.GetDataPresent((format = typeof(ExeMenuCommand).FullName)))
				return data.GetData(format) as ExeMenuCommand;

			if (data.GetDataPresent((format = typeof(TextMenuCommand).FullName)))
				return data.GetData(format) as TextMenuCommand;

			if (data.GetDataPresent((format = typeof(OfficeItemMenuCommand).FullName)))
				return data.GetData(format) as OfficeItemMenuCommand;

			return null;
		}

		private List<MenuApplication> applications = new List<MenuApplication>();
		private bool canRaiseMenuItemEvents = true;
		internal event EventHandler<EventArgs> MenuModelCleared;

		/// <remarks/>
		//---------------------------------------------------------------------
		public bool CanRaiseMenuItemEvents { get { return canRaiseMenuItemEvents; } set { canRaiseMenuItemEvents = value; } }

		/// <remarks/>
		//---------------------------------------------------------------------
		public List<MenuApplication> Applications { get { return applications; } }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuModel()
        {
		}

        //---------------------------------------------------------------------
        protected override void OnMenuItemAdded(MenuItemEventArgs e)
        {
			if (canRaiseMenuItemEvents)
               base.OnMenuItemAdded(e);
        }

        //---------------------------------------------------------------------
		protected override void OnMenuItemMoved(MenuItemEventArgs e)
        {
			if (canRaiseMenuItemEvents)
                base.OnMenuItemMoved(e);
        }

        //---------------------------------------------------------------------
		protected override void OnMenuItemRemoved(MenuItemEventArgs e)
        {
			if (canRaiseMenuItemEvents)
                base.OnMenuItemRemoved(e);

        }

		//---------------------------------------------------------------------
		protected override void OnPropertyValueChanged(string propertyName, object previousValue)
		{
			if (canRaiseMenuItemEvents)
				base.OnPropertyValueChanged(propertyName, previousValue);
		}

        //---------------------------------------------------------------------
		protected override void SubItem_PropertyValueChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
        {
			if (canRaiseMenuItemEvents)
				base.SubItem_PropertyValueChanged(sender, e);
        }

        //---------------------------------------------------------------------
        private void MenuApplication_SubMenuItemAdded(object sender, MenuItemEventArgs e)
        {
			OnMenuItemAdded(e);
        }

        //---------------------------------------------------------------------
        private void MenuApplication_SubMenuItemMoved(object sender, MenuItemEventArgs e)
        {
			OnMenuItemMoved(e);
        }

        //---------------------------------------------------------------------
        private void MenuApplication_SubMenuItemRemoved(object sender, MenuItemEventArgs e)
        {
			OnMenuItemRemoved(e);
        }

		//---------------------------------------------------------------------
		private void OnMenuModelCleared(EventArgs e)
		{
			if (MenuModelCleared != null)
				MenuModelCleared(this, e);
		}

        //---------------------------------------------------------------------
        private bool Contains(object aMenuItemToSearch)
        {
            if (aMenuItemToSearch == null)
                return false;

            if (aMenuItemToSearch is MenuApplication)
                return (applications != null && applications.Contains((MenuApplication)aMenuItemToSearch));

            if (aMenuItemToSearch is MenuGroup)
                return (GetGroupApplication((MenuGroup)aMenuItemToSearch) != null);

            if (aMenuItemToSearch is MenuBranch)
                return (GetMenuBranchParent((MenuBranch)aMenuItemToSearch) != null);

            if (aMenuItemToSearch is MenuCommand)
                return (GetCommandMenuBranch((MenuCommand)aMenuItemToSearch) != null);

            return false;
        }

        //---------------------------------------------------------------------
        private MenuApplication GetGroupApplication(MenuGroup aMenuGroupToSearch)
        {
            if (aMenuGroupToSearch == null || applications == null || applications.Count == 0)
                return null;

            foreach (MenuApplication aMenuApplication in applications)
            {
                if
                    (
                    aMenuApplication != null &&
                    aMenuApplication.Groups != null &&
                    aMenuApplication.Groups.Contains(aMenuGroupToSearch)
                    )
                    return aMenuApplication;
            }

            return null;
        }

        //---------------------------------------------------------------------
        public BaseMenuItem GetMenuBranchParent(MenuBranch aMenuBranch)
        {
            if (applications == null || aMenuBranch == null)
                return null;

            foreach (MenuApplication aMenuApplication in applications)
            {
				if (aMenuApplication == null || aMenuApplication.Groups == null && aMenuApplication.Groups.Count <= 0)
					continue;

                foreach (MenuGroup aMenuGroup in aMenuApplication.Groups)
                {
					if (aMenuGroup == null || aMenuGroup.Menus == null || aMenuGroup.Menus.Count <= 0)
						continue;

                    if (aMenuGroup.Menus.Contains(aMenuBranch))
                        return aMenuGroup;

                    foreach (MenuBranch aGroupMenuBranch in aMenuGroup.Menus)
                    {
                        MenuBranch parentMenuBranch = GetMenuBranchParentMenuBranch(aMenuBranch, aGroupMenuBranch);
                        if (parentMenuBranch != null)
                            return parentMenuBranch;
                    }
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
        private MenuBranch GetMenuBranchParentMenuBranch(MenuBranch aMenuBranch, MenuBranch aMenuBranchToSearch)
        {
            if
                (
                aMenuBranch == null ||
                aMenuBranchToSearch == null ||
                aMenuBranchToSearch.Menus == null ||
                aMenuBranchToSearch.Menus.Count == 0
                )
                return null;

            if (aMenuBranchToSearch.Menus.Contains(aMenuBranch))
                return aMenuBranchToSearch;

            foreach (MenuBranch aSubMenuBranch in aMenuBranchToSearch.Menus)
            {
                MenuBranch parentMenuBranch = GetMenuBranchParentMenuBranch(aMenuBranch, aSubMenuBranch);
                if (parentMenuBranch != null)
                    return parentMenuBranch;
            }

            return null;
        }

        //---------------------------------------------------------------------
        public MenuBranch GetCommandMenuBranch(MenuCommand aMenuCommandToSearch)
        {
            if (applications == null || aMenuCommandToSearch == null)
                return null;

            foreach (MenuApplication aMenuApplication in applications)
            {
				if (aMenuApplication == null || aMenuApplication.Groups == null && aMenuApplication.Groups.Count <= 0)
					continue;
             
				foreach (MenuGroup aMenuGroup in aMenuApplication.Groups)
                {
					if (aMenuGroup == null || aMenuGroup.Menus == null || aMenuGroup.Menus.Count <= 0)
						continue;

					MenuBranch foundBranch = aMenuGroup.Menus.GetCommandMenuBranch(aMenuCommandToSearch);
                    if (foundBranch != null)
                        return foundBranch;
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
		public override int CompareTo(object obj)
        {
            return CompareTo(obj, true, CultureInfo.InvariantCulture);
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
        {
            if (obj == null)
                return 1;

            MenuModel aMenuModel = obj as MenuModel;
            if (aMenuModel == null)
                throw new ArgumentException("'obj' is not a MenuModel object.");

            if (applications == null)
                return (aMenuModel.Applications == null) ? 0 : -1;

            return applications.CompareTo(aMenuModel.Applications, ignoreCase, culture);
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void Insert(int index, MenuApplication aMenuApplication)
        {
            if (aMenuApplication == null)
                return;

            if (applications == null)
				applications = new List<MenuApplication>();
            else if (applications.Contains(aMenuApplication))
                return;

            if (index < 0 || index > applications.Count)
				throw new ArgumentOutOfRangeException("index", "'index' was outside of range");

            applications.Insert(index, aMenuApplication);
	
            aMenuApplication.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
			aMenuApplication.MenuItemAdded += new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemAdded);
            aMenuApplication.MenuItemMoved += new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemMoved);
            aMenuApplication.MenuItemRemoved += new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemRemoved);

            OnMenuItemAdded(new MenuItemEventArgs(aMenuApplication, null, (index > 0) ? applications[index - 1] : null));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void AddApplication(MenuApplication aMenuApplication)
        {
            if (aMenuApplication == null)
                return;

            Insert((applications != null) ? applications.Count : 0, aMenuApplication);
        }

		/// <remarks/>
		//---------------------------------------------------------------------
        public void Move(int index, MenuApplication aMenuApplication)
        {
            if (aMenuApplication == null || applications == null || !applications.Contains(aMenuApplication))
                return;

            if (index < 0 || index > applications.Count)
                throw new ArgumentOutOfRangeException();

            int currentIdx = applications.IndexOf(aMenuApplication);
            if (currentIdx == -1 || currentIdx == index)
                return;
            MenuApplication previousBeforeApplication = (currentIdx > 0) ? applications[currentIdx - 1] : null;

            MenuApplication beforeApplication = (index > 0) ? applications[index - 1] : null;

            applications.Insert(index, aMenuApplication);

            if (currentIdx > index)
                currentIdx++;

            applications.RemoveAt(currentIdx);

            OnMenuItemMoved(new MenuItemEventArgs(aMenuApplication, null, beforeApplication, null, previousBeforeApplication));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public bool Remove(MenuApplication aMenuApplication)
        {
            if (applications == null || applications.Count == 0)
                return false;

            int menuItemIdx = applications.IndexOf(aMenuApplication);
            if (menuItemIdx == -1)
                return false;

            if (!applications.Remove(aMenuApplication))
                return false;

            aMenuApplication.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
            aMenuApplication.MenuItemAdded -= new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemAdded);
            aMenuApplication.MenuItemMoved -= new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemMoved);
            aMenuApplication.MenuItemRemoved -= new EventHandler<MenuItemEventArgs>(MenuApplication_SubMenuItemRemoved);

            OnMenuItemRemoved(new MenuItemEventArgs(aMenuApplication, null, (menuItemIdx > 0) ? applications[menuItemIdx - 1] : null));

            return true;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuApplication GetApplication(string applicationName)
        {
            if (applications == null || applications.Count == 0 || applicationName == null || applicationName.Length == 0)
                return null;

            foreach (MenuApplication aMenuApplication in applications)
            {
                if (String.Compare(applicationName, aMenuApplication.Name, true, CultureInfo.InvariantCulture) == 0)
                    return aMenuApplication;
            }

            return null;
        }


        //---------------------------------------------------------------------
		/// <remarks/>
		public bool AddMenuItem(object aMenuItemToAdd, BaseMenuItem targetMenuItem, object menuItemBefore, bool createNewName)
        {
            if
                (
                aMenuItemToAdd == null ||
                (menuItemBefore != null && !(aMenuItemToAdd is MenuCommand) && menuItemBefore.GetType() != aMenuItemToAdd.GetType()) ||
                (menuItemBefore != null && aMenuItemToAdd is MenuCommand && !(menuItemBefore is MenuCommand))
                )
                return false;

			BaseMenuItem aBaseMenuItemToAdd = aMenuItemToAdd as BaseMenuItem;

            if (targetMenuItem == null)
            {
				MenuApplication aMenuApplicationToAdd = aMenuItemToAdd as MenuApplication;
				if (aMenuApplicationToAdd == null)
                    return false;

				aMenuApplicationToAdd.Name = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;
				aMenuApplicationToAdd.Title = new MenuItemTitle(aMenuApplicationToAdd.Name, true);

				if (applications != null && applications.Contains(aMenuApplicationToAdd))
                    return false;

				MenuApplication menuItemBeforeApplication = menuItemBefore as MenuApplication;
                int appIdx = -1;
				if (menuItemBeforeApplication != null)
					appIdx = applications.IndexOf(menuItemBeforeApplication);

				Insert(appIdx + 1, aMenuApplicationToAdd);

				aBaseMenuItemToAdd.Site = new TBSite(aBaseMenuItemToAdd, this, null, aBaseMenuItemToAdd.Name);

				aBaseMenuItemToAdd.Owner = MenuEditorEngine.GetMyOwnerString();
                return true;
            }

            if (targetMenuItem is MenuApplication)
            {
				MenuGroup aMenuItemToAddGroup = aMenuItemToAdd as MenuGroup;
				if (applications == null || aMenuItemToAddGroup == null)
                    return false;

				MenuApplication targetMenuItemApp = targetMenuItem as MenuApplication;
				int appIdx = applications.IndexOf(targetMenuItemApp);
                if (appIdx == -1)
                    return false;

                int groupIdx = -1;
				MenuGroup menuItemBeforeGroup = menuItemBefore as MenuGroup;
                if
                    (
					menuItemBeforeGroup != null &&
					targetMenuItemApp.Groups != null &&
					targetMenuItemApp.Groups.Count > 0
                    )
					groupIdx = targetMenuItemApp.Groups.IndexOf(menuItemBeforeGroup);

				if (targetMenuItemApp.Owner != MenuEditorEngine.GetMyOwnerString())
				{
					if (menuItemBeforeGroup != null)
					{
						aMenuItemToAddGroup.InsertAfter = menuItemBeforeGroup.Name;
						aMenuItemToAddGroup.InsertBefore = String.Empty;
					}
					else if (targetMenuItemApp.Groups.Count > 0)
					{
						aMenuItemToAddGroup.InsertBefore = targetMenuItemApp.Groups[0].Name;
						aMenuItemToAddGroup.InsertAfter = String.Empty;
					}
				}

				CalculateGroupNameAndTitle(
					aMenuItemToAddGroup,
					targetMenuItemApp.Name,
					targetMenuItemApp.Groups
					);

				targetMenuItemApp.Insert(groupIdx + 1, aMenuItemToAddGroup);

				aBaseMenuItemToAdd.Site = new TBSite(aBaseMenuItemToAdd, targetMenuItemApp, null, aBaseMenuItemToAdd.Name);

                
				aBaseMenuItemToAdd.Owner = MenuEditorEngine.GetMyOwnerString();
                return true;

            }

            if (targetMenuItem is MenuGroup)
            {
				MenuBranch aMenuItemToAddBranch = aMenuItemToAdd as MenuBranch;
				if (applications == null || aMenuItemToAddBranch == null)
                    return false;

				MenuGroup targetMenuItemGroup = targetMenuItem as MenuGroup;
				MenuApplication targetMenuApplication = GetGroupApplication(targetMenuItemGroup);

                if (targetMenuApplication == null)
                    return false;

				MenuBranch menuItemBeforeBranch = menuItemBefore as MenuBranch;
                int menuBranchIdx = -1;
				if (menuItemBeforeBranch != null && targetMenuItemGroup.Menus != null)
					menuBranchIdx = targetMenuItemGroup.Menus.IndexOf(menuItemBeforeBranch);

				if (targetMenuItemGroup.Owner != MenuEditorEngine.GetMyOwnerString())
				{
					if (menuItemBeforeBranch != null)
					{
						aMenuItemToAddBranch.InsertAfter = menuItemBeforeBranch.Name;
						aMenuItemToAddBranch.InsertBefore = String.Empty;
					}
					else if (targetMenuItemGroup.Menus.Count > 0)
					{
						aMenuItemToAddBranch.InsertBefore = targetMenuItemGroup.Menus[0].Name;
						aMenuItemToAddBranch.InsertAfter = String.Empty;
					}
				}

				int counter = 1;
				if (createNewName || aMenuItemToAddBranch.Name.IsNullOrEmpty())
				{
					aMenuItemToAddBranch.Name = GetNewMenuBranchDefaultNameCounter(
						targetMenuItemGroup.Name + "." + MenuEditorStrings.MenuBranchNamePrefix + "{0}",
						targetMenuItemGroup.Menus,
						ref counter
						);
					aMenuItemToAddBranch.Title = new MenuItemTitle(
						aMenuItemToAddBranch.Name.Substring(aMenuItemToAddBranch.Name.LastIndexOf(".") + 1),
						true
						);
				}

				targetMenuItemGroup.Insert(menuBranchIdx + 1, aMenuItemToAddBranch);

				aBaseMenuItemToAdd.Site = new TBSite(aBaseMenuItemToAdd, targetMenuItemGroup, null, aBaseMenuItemToAdd.Name);
                MenuBranch oBranchToAdd = aBaseMenuItemToAdd as MenuBranch;
                if (oBranchToAdd != null)
                {
                    foreach (MenuBranch oItem in oBranchToAdd.Menus)
                    {
                        if (oItem.Site == null)
                        {
                            // set the site for the added node children.
                            oItem.Site = new TBSite(oItem, oBranchToAdd, null, oItem.Name);
                        }

                    }
                }

				aBaseMenuItemToAdd.Owner = MenuEditorEngine.GetMyOwnerString();
				return true;

            }

            if (targetMenuItem is MenuBranch)
            {
				MenuBranch aMenuItemToAddBranch = aMenuItemToAdd as MenuBranch;
				MenuCommand aMenuItemToAddCommand = aMenuItemToAdd as MenuCommand;
				if (applications == null || (aMenuItemToAddBranch == null && aMenuItemToAddCommand == null))
                    return false;

				MenuBranch targetMenuItemBranch = targetMenuItem as MenuBranch;
				BaseMenuItem parentMenuItem = GetMenuBranchParent(targetMenuItemBranch);
                if (parentMenuItem == null)
                    return false;

                int iNodeDepth = GetCurrentDepth(targetMenuItemBranch);
                if (iNodeDepth >= m_iMaxDepth && aMenuItemToAddCommand == null) 
                {
                    return false;
                }
				if (aMenuItemToAddBranch != null)
                {
					MenuBranch menuItemBeforeBranch = menuItemBefore as MenuBranch;
                    int menuBranchIdx = -1;
					if (menuItemBeforeBranch != null && targetMenuItemBranch.Menus != null)
						menuBranchIdx = targetMenuItemBranch.Menus.IndexOf(menuItemBeforeBranch);

					if (targetMenuItemBranch.Owner != MenuEditorEngine.GetMyOwnerString())
					{
						if (menuItemBeforeBranch != null)
						{
							aMenuItemToAddBranch.InsertAfter = menuItemBeforeBranch.Name;
							aMenuItemToAddBranch.InsertBefore = String.Empty;
						}
						else if (targetMenuItemBranch.Menus.Count > 0)
						{
							aMenuItemToAddBranch.InsertBefore = targetMenuItemBranch.Menus[0].Name;
							aMenuItemToAddBranch.InsertAfter = String.Empty;
						}
					}

					int counter = 1;
					if (createNewName || aMenuItemToAddBranch.Name.IsNullOrEmpty())
					{
						aMenuItemToAddBranch.Name = GetNewMenuBranchDefaultNameCounter(
							targetMenuItemBranch.Name + "." + MenuEditorStrings.MenuBranchNamePrefix + "{0}",
							targetMenuItemBranch.Menus,
							ref counter
							);
						aMenuItemToAddBranch.Title = new MenuItemTitle(
							aMenuItemToAddBranch.Name.Substring(aMenuItemToAddBranch.Name.LastIndexOf(".") + 1),
							true
							);
					}

					targetMenuItemBranch.Insert(menuBranchIdx + 1, aMenuItemToAddBranch);

					aBaseMenuItemToAdd.Site = new TBSite(aBaseMenuItemToAdd, targetMenuItemBranch, null, aBaseMenuItemToAdd.Name);

                    
					aBaseMenuItemToAdd.Owner = MenuEditorEngine.GetMyOwnerString();
					return true;
                }
				else if (aMenuItemToAddCommand != null)
                {
					MenuCommand menuItemBeforeCommand = menuItemBefore as MenuCommand;
					int commandIdx = -1;
					if (menuItemBeforeCommand != null && targetMenuItemBranch.Commands != null)
						commandIdx = targetMenuItemBranch.Commands.IndexOf(menuItemBeforeCommand);

					if (targetMenuItemBranch.Owner != MenuEditorEngine.GetMyOwnerString())
					{
						if (menuItemBeforeCommand != null)
						{
							aMenuItemToAddCommand.InsertAfter = menuItemBeforeCommand.Name;
							aMenuItemToAddCommand.InsertBefore = String.Empty;
						}
						else if (targetMenuItemBranch.Commands.Count > 0)
						{
							aMenuItemToAddCommand.InsertBefore = targetMenuItemBranch.Commands[0].Name;
							aMenuItemToAddCommand.InsertAfter = String.Empty;
						}
					}

					if (createNewName ||  aMenuItemToAddCommand.Title == null)
					{
						aMenuItemToAddCommand.Title = new MenuItemTitle(
							 MenuModel.GetNewMenuCommandDefaultTitle(
								aMenuItemToAddCommand.GetMenuCommandDefaultTitleFormat(),
								targetMenuItemBranch.Commands
								),
								true
							);
					}

					targetMenuItemBranch.Insert(commandIdx + 1, aMenuItemToAddCommand);

					aBaseMenuItemToAdd.Site = new TBSite(aBaseMenuItemToAdd, targetMenuItemBranch, null, aBaseMenuItemToAdd.Name);

					aBaseMenuItemToAdd.Owner = MenuEditorEngine.GetMyOwnerString();
					return true;
                }
            }

            return false;
        }

        private int GetCurrentDepth(MenuBranch targetMenuItemBranch)
        {
            if (targetMenuItemBranch == null) 
            {
                return m_iMaxDepth + 1;
            }
            int iDepth = 0;
            BaseMenuItem parentMenuItem = GetMenuBranchParent(targetMenuItemBranch);
            while (parentMenuItem != null) 
            {
                iDepth++;
                MenuBranch oBranch = parentMenuItem as MenuBranch;
                parentMenuItem = GetMenuBranchParent(oBranch);
            }

            return iDepth;
        }

		//---------------------------------------------------------------------
		private static void CalculateGroupNameAndTitle(
			MenuGroup aGroupToInsert,
			string applicationName,
			List<MenuGroup> groups
			)
		{
			string newGroupName = applicationName + "." + MenuEditorStrings.GroupNamePrefix;
			int groupCounter = 0;

			int found = 0;
			do
			{
				groupCounter++;
				found = (from g in groups 
						where g.Name == (newGroupName + groupCounter.ToString(CultureInfo.InvariantCulture))
						select g).Count();

			} while (found != 0);

			newGroupName += groupCounter.ToString(CultureInfo.InvariantCulture);
			aGroupToInsert.Name = newGroupName;
			if (aGroupToInsert.Title == null || aGroupToInsert.Title.Text == null || aGroupToInsert.Title.Text.Length == 0)
				aGroupToInsert.Title = new MenuItemTitle(MenuEditorStrings.GroupNamePrefix + " " + groupCounter.ToString(CultureInfo.InvariantCulture), true);
		}

		//---------------------------------------------------------------------
		private string GetNewMenuBranchDefaultNameCounter(
			string menuBranchToDragNameFormat,
			List<MenuBranch> branchesToSearch,
			ref int menuBranchCounter
			)
		{
			if (menuBranchToDragNameFormat == null || menuBranchToDragNameFormat.Trim().Length == 0)
				return String.Empty;

			if (branchesToSearch == null || branchesToSearch.Count == 0)
				return String.Format(menuBranchToDragNameFormat, menuBranchCounter.ToString(CultureInfo.InvariantCulture));

			string menuBranchNumberExpr = @"(?<branchNumber>[0-9]*)";
			Regex menuBranchNameRegex = new Regex
				(
				String.Format(menuBranchToDragNameFormat.Replace(".", "\\."), menuBranchNumberExpr) + "$",
				RegexOptions.Singleline | RegexOptions.IgnoreCase
				);

			foreach (MenuBranch aMenuBranch in branchesToSearch)
			{
				Match nameMatch = menuBranchNameRegex.Match(aMenuBranch.Name);
				if (nameMatch != null && nameMatch.Success && !nameMatch.NextMatch().Success)
				{
					try
					{
						int currentBranchNumber = Int32.Parse(nameMatch.Groups["branchNumber"].Value);
						if (menuBranchCounter <= currentBranchNumber)
							menuBranchCounter = ++currentBranchNumber;
					}
					catch
					{
					}
				}

				GetNewMenuBranchDefaultNameCounter(menuBranchToDragNameFormat, aMenuBranch.Menus, ref menuBranchCounter);
			}

			return String.Format(menuBranchToDragNameFormat, menuBranchCounter.ToString(CultureInfo.InvariantCulture));
		}


        //---------------------------------------------------------------------
		/// <remarks/>
		public bool MoveMenuItem(
			object aMenuItemToMove,
			BaseMenuItem targetMenuItem,
			object menuItemBefore
			)
        {
            if
                (
                aMenuItemToMove == null ||
                (menuItemBefore != null && !(aMenuItemToMove is MenuCommand) && menuItemBefore.GetType() != aMenuItemToMove.GetType()) ||
                (menuItemBefore != null && aMenuItemToMove is MenuCommand && !(menuItemBefore is MenuCommand))
                )
                return false;

            if (targetMenuItem == null)
            {
				MenuApplication aMenuApplication = aMenuItemToMove as MenuApplication;
                if (aMenuApplication == null)
                    return false;

				MenuApplication aMenuApplicationBefore = menuItemBefore as MenuApplication;

                if (applications == null || !applications.Contains(aMenuApplication))
                    return false;

				int appIdx = (menuItemBefore != null) ? applications.IndexOf(aMenuApplicationBefore) : -1;

				Move(appIdx + 1, aMenuApplication);

                return true;
            }

            if (targetMenuItem is MenuApplication)
            {
				MenuGroup aMenuGroup = aMenuItemToMove as MenuGroup;
				if (applications == null || aMenuGroup == null)
                    return false;

				MenuApplication aMenuApplicationTarget = targetMenuItem as MenuApplication;
				int appIdx = applications.IndexOf(aMenuApplicationTarget);
                if (appIdx == -1)
                    return false;

				MenuGroup aMenuItemBeforeGroup = menuItemBefore as MenuGroup;
                int groupIdx = -1;
                if
                    (
                    menuItemBefore != null &&
					aMenuApplicationTarget.Groups != null &&
					aMenuApplicationTarget.Groups.Count > 0
                    )
					groupIdx = aMenuApplicationTarget.Groups.IndexOf(aMenuItemBeforeGroup);

				MenuApplication parentApplication = GetGroupApplication(aMenuGroup);
                if (parentApplication == null)
                    return false;

				if (parentApplication.Owner != MenuEditorEngine.GetMyOwnerString())
				{
					if (aMenuItemBeforeGroup != null)
					{
						aMenuGroup.InsertAfter = aMenuItemBeforeGroup.Name;
						aMenuGroup.InsertBefore = String.Empty;
					}
					else if (aMenuApplicationTarget.Groups.Count > 0)
					{
						aMenuGroup.InsertBefore = aMenuApplicationTarget.Groups[0].Name;
						aMenuGroup.InsertAfter = String.Empty;
					}
				}

				aMenuApplicationTarget.Move(groupIdx + 1, aMenuGroup, parentApplication);

                return true;
            }

            if (targetMenuItem is MenuGroup)
            {
				MenuBranch aMenuBranchToMove = aMenuItemToMove as MenuBranch;
				if (applications == null || aMenuBranchToMove == null)
                    return false;

				MenuGroup targetMenuItemGroup = targetMenuItem as MenuGroup;
				MenuApplication targetMenuApplication = GetGroupApplication(targetMenuItemGroup);

                if (targetMenuApplication == null)
                    return false;

                int menuBranchIdx = -1;
				MenuBranch menuItemBeforeBranch = menuItemBefore as MenuBranch;
				if (menuItemBefore != null && targetMenuItemGroup.Menus != null)
					menuBranchIdx = targetMenuItemGroup.Menus.IndexOf(menuItemBeforeBranch);

				MenuBranch aMenuItemToMoveBranch = aMenuItemToMove as MenuBranch;
				BaseMenuItem parentMenuItem = GetMenuBranchParent(aMenuItemToMoveBranch);
                if (parentMenuItem == null)
                    return false;

				if (parentMenuItem.Owner != MenuEditorEngine.GetMyOwnerString())
				{
					if (menuItemBeforeBranch != null)
					{
						aMenuBranchToMove.InsertAfter = menuItemBeforeBranch.Name;
						aMenuBranchToMove.InsertBefore = String.Empty;
					}
					else if (targetMenuItemGroup.Menus.Count > 0)
					{
						aMenuBranchToMove.InsertBefore = targetMenuItemGroup.Menus[0].Name;
						aMenuBranchToMove.InsertAfter = String.Empty;
					}
				}

				targetMenuItemGroup.Move(menuBranchIdx + 1, aMenuItemToMoveBranch, parentMenuItem);

                return true;
            }

            if (targetMenuItem is MenuBranch)
            {
				MenuBranch aMenuItemToMoveBranch = aMenuItemToMove as MenuBranch;
				MenuCommand aMenuItemToMoveCommand = aMenuItemToMove as MenuCommand;
				if (applications == null || (aMenuItemToMoveBranch == null && aMenuItemToMoveCommand == null))
                    return false;

				MenuBranch targetMenuItemBranch = targetMenuItem as MenuBranch;
				if (aMenuItemToMoveBranch != null)
                {
                    int menuBranchIdx = -1;
					MenuBranch menuItemBeforeBranch = menuItemBefore as MenuBranch;
					if (menuItemBefore != null && targetMenuItemBranch.Menus != null)
						menuBranchIdx = targetMenuItemBranch.Menus.IndexOf(menuItemBeforeBranch);

					BaseMenuItem parentMenuItem = GetMenuBranchParent(aMenuItemToMoveBranch);
                    if (parentMenuItem == null)
                        return false;

					if (parentMenuItem.Owner != MenuEditorEngine.GetMyOwnerString())
					{
						if (menuItemBeforeBranch != null)
						{
							aMenuItemToMoveBranch.InsertAfter = menuItemBeforeBranch.Name;
							aMenuItemToMoveBranch.InsertBefore = String.Empty;
						}
						else if (targetMenuItemBranch.Menus.Count > 0)
						{
							aMenuItemToMoveBranch.InsertBefore = targetMenuItemBranch.Menus[0].Name;
							aMenuItemToMoveBranch.InsertAfter = String.Empty;
						}
					}

					targetMenuItemBranch.Move(menuBranchIdx + 1, aMenuItemToMoveBranch, parentMenuItem);

                    return true;
                }
				else if (aMenuItemToMoveCommand != null)
                {
                    int commandIdx = -1;
					MenuCommand menuItemBeforeCommand = menuItemBefore as MenuCommand;
					if (menuItemBefore != null && targetMenuItemBranch.Commands != null)
						commandIdx = targetMenuItemBranch.Commands.IndexOf(menuItemBeforeCommand);

					MenuBranch parentMenuBranch = GetCommandMenuBranch(aMenuItemToMoveCommand);
                    if (parentMenuBranch == null)
                        return false;

					if (parentMenuBranch.Owner != MenuEditorEngine.GetMyOwnerString())
					{
						if (menuItemBeforeCommand != null)
						{
							aMenuItemToMoveCommand.InsertAfter = menuItemBeforeCommand.Name;
							aMenuItemToMoveCommand.InsertBefore = String.Empty;
						}
						else if (targetMenuItemBranch.Commands.Count > 0)
						{
							aMenuItemToMoveCommand.InsertBefore = targetMenuItemBranch.Commands[0].Name;
							aMenuItemToMoveCommand.InsertAfter = String.Empty;
						}
					}

					targetMenuItemBranch.Move(commandIdx + 1, aMenuItemToMoveCommand, parentMenuBranch);

					return true;
                }
            }

            return false;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public bool RemoveMenuItem(object aMenuItemToRemove)
        {
            if (aMenuItemToRemove == null || applications == null)
                return false;

            if (aMenuItemToRemove is MenuApplication)
            {
                if (applications != null && !applications.Contains((MenuApplication)aMenuItemToRemove))
                    return false;
                
                return Remove((MenuApplication)aMenuItemToRemove);
            }

            if (aMenuItemToRemove is MenuGroup)
            {
                foreach (MenuApplication aMenuApplication in applications)
                {
                    if
                        (
                        aMenuApplication != null &&
                        aMenuApplication.Groups != null &&
                        aMenuApplication.Groups.Count > 0 &&
                        aMenuApplication.Groups.Contains((MenuGroup)aMenuItemToRemove)
                        )
                    {
                        return aMenuApplication.Remove((MenuGroup)aMenuItemToRemove);
                    }
                }

                return false;
            }

            if (aMenuItemToRemove is MenuBranch)
            {
                BaseMenuItem parentMenuItem = GetMenuBranchParent((MenuBranch)aMenuItemToRemove);
                if (parentMenuItem == null)
                    return false;

                if (parentMenuItem is MenuGroup)
                   return ((MenuGroup)parentMenuItem).Remove((MenuBranch)aMenuItemToRemove);

                if (parentMenuItem is MenuBranch)
                    return ((MenuBranch)parentMenuItem).Remove((MenuBranch)aMenuItemToRemove);
            }

            if (aMenuItemToRemove is MenuCommand)
            {
                MenuBranch commandMenuBranch = GetCommandMenuBranch((MenuCommand)aMenuItemToRemove);
                if (commandMenuBranch == null)
                    return false;

                return commandMenuBranch.Remove((MenuCommand)aMenuItemToRemove);
            }

            return false;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public bool ChangeMenuItemPropertyValue(object aMenuItemToChange, PropertyDescriptor propertyDescr, object propertyValue)
        {
            if (aMenuItemToChange == null || propertyDescr == null || applications == null)
                return false;

			propertyDescr.SetValue(aMenuItemToChange, propertyValue);

			return true;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public override void Clear()
		{
			foreach (MenuApplication menuApp in applications)
				menuApp.Clear();
			
			applications.Clear();

			OnMenuModelCleared(EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		internal static string GetModelFullPath(BaseMenuItem baseMenuItem)
		{
			if (baseMenuItem == null || baseMenuItem.Site == null)
				return String.Empty;

			if (baseMenuItem.Site.Container == null)
				return baseMenuItem.Site.Name;

			return String.Concat(GetModelFullPath(baseMenuItem.Site.Container as BaseMenuItem), "/", baseMenuItem.GetNodePathHierarchyElement());
		}

		#region IContainer Members

		/// <remarks/>
		//---------------------------------------------------------------------
		public void Add(IComponent component, string name)
		{
			MenuApplication application = component as MenuApplication;
			if (application == null)
				return;

			if (!String.IsNullOrWhiteSpace(name))
				application.Site = new TBSite(application, this, null, name);

			AddApplication(application);
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public void Add(IComponent component)
		{
			string name = component.Site != null ? component.Site.Name : string.Empty;
			Add(component, name);
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		[Browsable(false)]
		public ComponentCollection Components
		{
			get { return new ComponentCollection(applications.ToArray()); }
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public void Remove(IComponent component)
		{
			MenuApplication application = component as MenuApplication;
			if (application == null)
				return;

			Remove(application);
		}

		#endregion

		//---------------------------------------------------------------------
		public static string GetNewMenuCommandDefaultTitle(string newCommandTitleFormat, List<MenuCommand> menuItems)
		{
			if (newCommandTitleFormat == null || newCommandTitleFormat.Length == 0)
				return String.Empty;

			string cmdNumberExpr = @".(?<cmdNumber>[0-9]*)";
			Regex cmdTitleRegex = new Regex
				(
				String.Format(newCommandTitleFormat, cmdNumberExpr) + "$",
				RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
				);
			int commandCounter = 1;
			foreach (MenuCommand menuItem in menuItems)
			{
				if (menuItem.Title == null)
					continue;

				Match titleMatch = cmdTitleRegex.Match(menuItem.Title.Text);
				if (titleMatch != null && titleMatch.Success && !titleMatch.NextMatch().Success)
				{
					try
					{
						int currentCmdNumber = Int32.Parse(titleMatch.Groups["cmdNumber"].Value);
						if (commandCounter <= currentCmdNumber)
							commandCounter = ++currentCmdNumber;
					}
					catch
					{
					}
				}
			}

			return String.Format(newCommandTitleFormat, commandCounter.ToString(CultureInfo.InvariantCulture));
		}

		//---------------------------------------------------------------------
		internal void ApplyMenuEditorAttributes(XmlDocument menuXmlDocument)
		{
			if (menuXmlDocument == null)
				return;

			//dal xml document troviamo tutti i nodi con customizable a true;
			string xPath = String.Format("//*[@{0}='{1}']", MenuXmlNode.XML_ATTRIBUTE_OWNER, MenuEditorEngine.GetMyOwnerString());
			XmlNodeList customizableNodes = menuXmlDocument.DocumentElement.SelectNodes(xPath);
			foreach (XmlNode xmlNode in customizableNodes)
			{
				//per ogni nodo calcoliamo l'oggetto di menu
				BaseMenuItem menuItem = GetMenuItem(xmlNode);
				if (menuItem != null)
					//set-iamo l'attributo a true;
					menuItem.Owner = MenuEditorEngine.GetMyOwnerString();
			}
		}

		//---------------------------------------------------------------------
		private BaseMenuItem GetMenuItem(XmlNode xmlNode)
		{
			if (xmlNode == null)
				return null;

			Stack<ModelPathItem> modelPath = new Stack<ModelPathItem>();
			while (xmlNode.Name != MenuXmlNode.XML_TAG_MENU_ROOT)
			{
				XmlAttribute nameAttr = xmlNode.Attributes[MenuXmlNode.XML_ATTRIBUTE_NAME];
				string token = null;
				if (nameAttr == null)
				{
					XmlNode titleChild = xmlNode.SelectSingleNode(String.Format("./{0}", MenuXmlNode.XML_TAG_TITLE));
					if (titleChild == null)
						return null;

					token = titleChild.InnerText;
				}
				else
					token = nameAttr.InnerText;

				modelPath.Push(new ModelPathItem(xmlNode.Name, token));

				xmlNode = xmlNode.ParentNode;
			}
			// /[Application]Application1/[Group]Application1.Group1/[Menu]Application1.Group1.Menu1/[OfficeItem]OfficeItem 1
			IComponent component = null;
			IContainer container = this;
			while (modelPath.Count > 0)
			{
				ModelPathItem current = modelPath.Pop();
				component = GetChildComponent(current, container);

				container = component as IContainer;
				if (container == null)
					return component as BaseMenuItem;
			}

			return component as BaseMenuItem;
		}

		//---------------------------------------------------------------------
		private static IComponent GetChildComponent(ModelPathItem current, IContainer container)
		{
			if (container == null || current == null)
				return null;

			foreach (BaseMenuItem item in container.Components)
			{
				if (
					String.Compare(item.Name, current.ItemName, StringComparison.InvariantCulture) == 0 &&
					String.Compare(item.GetXmlTag(), current.ItemType, StringComparison.InvariantCulture) == 0
					)
					return item;
			}

			return null;
		}

		private class ModelPathItem
		{
			public ModelPathItem(string itemType, string itemName)
			{
				this.ItemType = itemType;
				this.ItemName = itemName;
			}

			public string ItemName { get; set; }

			public string ItemType { get; set; }
		}

        internal bool CanAddChild(BaseMenuItem oBaseMenuItem)
        {
            if (oBaseMenuItem == null)             
            {
                return false;
            }

            if (GetCurrentDepth(oBaseMenuItem as MenuBranch) >= m_iMaxDepth)
            {
                return false;
            }
            return true;
            
        }

        internal bool CanAddCommand(BaseMenuItem oBaseMenuItem)
        {
            if (oBaseMenuItem == null)
            {
                return false;
            }

            if (GetCurrentDepth(oBaseMenuItem as MenuBranch) <= 1)
            {
                return false;
            }
            return true;

        }
    }

    //=========================================================================
    [Serializable]
	internal class MenuApplication : BaseMenuItem, IComponent, IContainer
    {
		private List<MenuGroup> groups = new List<MenuGroup>();

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuItemNamePropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Design"),
		AffectsAppearanceAttribute(true),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME),
		ReadOnly(true)]
		public override string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public List<MenuGroup> Groups { get { return groups; } }

		//---------------------------------------------------------------------
		[Browsable(false)]
		public new string InsertAfter
		{
			get { return null; }
			set
			{
				throw new NotImplementedException();
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public new string InsertBefore
		{
			get { return null; }
			set { }
		}

        //---------------------------------------------------------------------
		public MenuApplication(
			string aName,
			string aImageFilePath,
			MenuItemTitle aTitle,
			List<MenuGroup> aGroupCollection,
			string aActivation
			)
			: base(aName, aImageFilePath, aTitle, aActivation)
        {
			if (aGroupCollection != null)
				this.groups = aGroupCollection;
        }

        //---------------------------------------------------------------------
		public MenuApplication(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsApplication)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the MenuApplication constructor.");

            ArrayList groupNodes = aMenuXmlNode.GroupItems;
            if (groupNodes != null && groupNodes.Count > 0)
            {
                foreach (MenuXmlNode aGroupNode in groupNodes)
                {
					if (aGroupNode != null && aGroupNode.IsGroup)
					{
						MenuGroup aMenuGroup = new MenuGroup(aGroupNode, this);
						AddGroup(aMenuGroup);
					}
                }
            }
        }

		//---------------------------------------------------------------------
		public MenuApplication()
		{}

        //---------------------------------------------------------------------
		/// <remarks/>
		public new MenuApplication Clone()
        {
            MenuApplication clonedMenuApplication = base.Clone() as MenuApplication;

            if (this.Groups != null && this.Groups.Count > 0)
            {
                foreach (MenuGroup aMenuGroupToClone in this.Groups)
                    clonedMenuApplication.AddGroup(aMenuGroupToClone.Clone());
            }

            return clonedMenuApplication;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public void AddGroup(MenuGroup aGroup)
		{
			if (aGroup == null)
				return;

			Insert((groups != null) ? groups.Count : 0, aGroup);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Insert(int index, MenuGroup aGroup)
		{
			if (aGroup == null)
				return;

			if (groups == null)
				groups = new List<MenuGroup>();
            else if (groups.Contains(aGroup))
                return;

			if (index < 0 || index > groups.Count)
				throw new ArgumentOutOfRangeException("index", "'index' was outside of range");
			groups.Insert(index, aGroup);

			aGroup.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
			aGroup.MenuItemAdded += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aGroup.MenuItemMoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aGroup.MenuItemRemoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemAdded(new MenuItemEventArgs(aGroup, this, (index > 0) ? groups[index - 1] : null));
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public void Move(int index, MenuGroup aGroup, MenuApplication parentMenuApplication)
        {
            if 
                (
                aGroup == null || 
                parentMenuApplication == null || 
                parentMenuApplication.Groups == null ||
                !parentMenuApplication.Groups.Contains(aGroup))
                return;

            if 
                (
                index < 0 || 
                (groups == null && index != 0) ||
                (groups != null && index > groups.Count)
                )
                throw new ArgumentOutOfRangeException();

            if (groups == null)
				groups = new List<MenuGroup>();

            int currentIdx = parentMenuApplication.Groups.IndexOf(aGroup);
            if (currentIdx == -1 || (object.ReferenceEquals(parentMenuApplication, this) && currentIdx == index))
                return;

            MenuGroup previousBeforeGroup = (currentIdx > 0) ? parentMenuApplication.Groups[currentIdx - 1] : null;

            MenuGroup beforeGroup = (index > 0) ? groups[index - 1] : null;

            groups.Insert(index, aGroup);

            if (object.ReferenceEquals(parentMenuApplication, this) && currentIdx > index)
                currentIdx++;
            parentMenuApplication.Groups.RemoveAt(currentIdx);

            OnMenuItemMoved(new MenuItemEventArgs(aGroup, this, beforeGroup, parentMenuApplication, previousBeforeGroup));
        }
        
        //---------------------------------------------------------------------
		/// <remarks/>
		public bool Remove(MenuGroup aGroup)
        {
            if (groups == null || groups.Count == 0)
                return false;

            int menuItemIdx = groups.IndexOf(aGroup);
            if (menuItemIdx == -1)
                return false;
            
            if (!groups.Remove(aGroup))
                return false;

			aGroup.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
			aGroup.MenuItemAdded -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aGroup.MenuItemMoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aGroup.MenuItemRemoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemRemoved(new MenuItemEventArgs(aGroup, this, (menuItemIdx > 0) ? groups[menuItemIdx - 1] : null));

            return true;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public override void Clear()
		{
			base.Clear();

			foreach (MenuGroup menuGrp in groups)
				menuGrp.Clear();

			groups.Clear();
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
        {
            if (obj == null)
                return 1;

            MenuApplication aApplication = obj as MenuApplication;

            if (aApplication == null)
                throw new ArgumentException("'obj' is not a MenuApplication object.");

            int result = base.CompareTo(obj, ignoreCase, culture);

            if (result != 0)
                return result;

            if (groups == null)
                return (aApplication.Groups == null || aApplication.Groups.Count == 0) ? 0 : -1;

            return groups.CompareTo(aApplication.Groups, ignoreCase, culture);
        }


        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuGroup GetGroup(string groupName)
        {
            if (groups == null || groups.Count == 0 || groupName == null || groupName.Length == 0)
                return null;

            foreach (MenuGroup aMenuGroup in groups)
            {
                if (String.Compare(groupName, aMenuGroup.Name, true, CultureInfo.InvariantCulture) == 0)
                    return aMenuGroup;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public override string GetMenuItemTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.ApplicationItemTypeDescription;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_APPLICATION;
        }
        
        //---------------------------------------------------------------------
		/// <remarks/>
		public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement applicationNode = base.GetXmlElement(aMenuXmlDocument);
            if (applicationNode == null || applicationNode.OwnerDocument == null)
                return null;

            if (groups != null && groups.Count > 0)
            {
                foreach (MenuGroup aMenuGroup in groups)
                {
                    XmlElement groupNode = aMenuGroup.GetXmlElement(applicationNode.OwnerDocument);
                    if (groupNode != null)
                        applicationNode.AppendChild(groupNode);
                }
            }
            
            applicationNode.Normalize();

            return applicationNode;
        }

		#region IContainer Members

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component, string name)
		{
			MenuGroup group = component as MenuGroup;
			if (group == null)
				return;

			if (!String.IsNullOrWhiteSpace(name))
				group.Site = new TBSite(group, this, null, name);

			AddGroup(group);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component)
		{
			string name = component.Site != null ? component.Site.Name : string.Empty;
			Add(component, name);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public ComponentCollection Components
		{
			get { return new ComponentCollection(groups.ToArray()); }
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Remove(IComponent component)
		{
			MenuGroup group = component as MenuGroup;
			if (group == null)
				return;

			groups.Remove(group);
		}

		#endregion
	}

    //=========================================================================
    [Serializable]
	internal class MenuGroup : BaseMenuItem, IComponent, IContainer
    {
        private List<MenuBranch> menus = new List<MenuBranch>();

		[Browsable(false)]
		public List<MenuBranch> Menus { get { return menus; } }

		/// <remarks/>
		//---------------------------------------------------------------------
        public MenuGroup()
        {
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuGroup(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsGroup)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the MenuGroup constructor.");

            ArrayList menuNodes = aMenuXmlNode.MenuItems;
            if (menuNodes != null && menuNodes.Count > 0)
            {
                foreach (MenuXmlNode aMenuNode in menuNodes)
                {
					if (aMenuNode != null && aMenuNode.IsMenu)
					{
						MenuBranch aMenuBranch = new MenuBranch(aMenuNode, this);
						AddBranch(aMenuBranch);
					}
                }
            }
        }
       
        //---------------------------------------------------------------------
		/// <remarks/>
		public new MenuGroup Clone()
        {
            MenuGroup clonedMenuGroup = base.Clone() as MenuGroup;

            clonedMenuGroup.InsertAfter = this.InsertAfter;
            clonedMenuGroup.InsertBefore = this.InsertBefore;

            if (this.Menus != null && this.Menus.Count > 0)
            {
                foreach (MenuBranch aMenuBranchToClone in this.Menus)
                    clonedMenuGroup.AddBranch(aMenuBranchToClone.Clone());
            }

            return clonedMenuGroup;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public void AddBranch(MenuBranch aMenuBranch)
		{
			if (aMenuBranch == null)
				return;

			if (menus == null)
				menus = new List<MenuBranch>();

			Insert(menus.Count, aMenuBranch);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Insert(int index, MenuBranch aMenuBranch)
		{
			if (aMenuBranch == null)
				return;

			if (menus == null)
				menus = new List<MenuBranch>();
            else if (menus.Contains(aMenuBranch))
                return;

			if (index < 0 || index > menus.Count)
				throw new ArgumentOutOfRangeException();

			menus.Insert(index, aMenuBranch);

			aMenuBranch.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
            aMenuBranch.MenuItemAdded += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aMenuBranch.MenuItemMoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aMenuBranch.MenuItemRemoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemAdded(new MenuItemEventArgs(aMenuBranch, this, (index > 0) ? menus[index - 1] : null));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void Move(int index, MenuBranch aMenuBranch, object parentMenuItem)
        {
            if (aMenuBranch == null || parentMenuItem == null)
                return;

			List<MenuBranch> parentMenuBranches = null;
            if (parentMenuItem is MenuGroup)
                parentMenuBranches = ((MenuGroup)parentMenuItem).Menus;
            else if (parentMenuItem is MenuBranch)
                parentMenuBranches = ((MenuBranch)parentMenuItem).Menus;
            if (parentMenuBranches == null || !parentMenuBranches.Contains(aMenuBranch))
                return;

            if
               (
               index < 0 ||
               (menus == null && index != 0) ||
               (menus != null && index > menus.Count)
               )
                throw new ArgumentOutOfRangeException();

            if (menus == null)
				menus = new List<MenuBranch>();

            int currentIdx = parentMenuBranches.IndexOf(aMenuBranch);
            if (currentIdx == -1 || (object.ReferenceEquals(parentMenuBranches, menus) && currentIdx == index))
                return;

            MenuBranch previousBeforeMenuBranch = (currentIdx > 0) ? parentMenuBranches[currentIdx - 1] : null;

            MenuBranch beforeMenuBranch = (index > 0) ? menus[index - 1] : null;

            menus.Insert(index, aMenuBranch);

            if (object.ReferenceEquals(parentMenuBranches, menus) && currentIdx > index)
                currentIdx++;
            parentMenuBranches.RemoveAt(currentIdx);

            OnMenuItemMoved(new MenuItemEventArgs(aMenuBranch, this, beforeMenuBranch, parentMenuItem, previousBeforeMenuBranch));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public bool Remove(MenuBranch aMenuBranch)
        {
            if (menus == null || menus.Count == 0)
                return false;

            int menuItemIdx = menus.IndexOf(aMenuBranch);
            if (menuItemIdx == -1)
                return false;

            if (!menus.Remove(aMenuBranch))
                return false;

			aMenuBranch.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
            aMenuBranch.MenuItemAdded -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aMenuBranch.MenuItemMoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aMenuBranch.MenuItemRemoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemRemoved(new MenuItemEventArgs(aMenuBranch, this, (menuItemIdx > 0) ? menus[menuItemIdx - 1] : null));

            return true;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public override void Clear()
		{
			base.Clear();

			foreach (MenuBranch menuBranch in menus)
				menuBranch.Clear();

			menus.Clear();
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
        {
            if (obj == null)
                return 1;

            MenuGroup aGroup = obj as MenuGroup;

            if (aGroup == null)
                throw new ArgumentException("'obj' is not a MenuGroup object.");

            int result = base.CompareTo(obj, ignoreCase, culture);

            if (result != 0)
                return result;

            if (menus == null)
                result = (aGroup.Menus == null || aGroup.Menus.Count == 0) ? 0 : -1;
            else
                result = menus.CompareTo(aGroup.Menus, true, CultureInfo.InvariantCulture);

            if (result != 0)
                return result;

            result = String.Compare(InsertAfter, aGroup.InsertAfter, ignoreCase, culture);

            if (result != 0)
                return result;

            return String.Compare(InsertBefore, aGroup.InsertBefore, ignoreCase, culture);
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuBranch GetMenuBranch(string menuBranchName)
        {
            if (menus == null || menus.Count == 0 || menuBranchName == null || menuBranchName.Length == 0)
                return null;

            foreach (MenuBranch aMenuBranch in menus)
            {
                if (String.Compare(menuBranchName, aMenuBranch.Name, true, CultureInfo.InvariantCulture) == 0)
                    return aMenuBranch;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuBranch GetMenuBranchByTitle(string menuBranchTitle)
        {
            if (menus == null || menus.Count == 0 || menuBranchTitle == null || menuBranchTitle.Length == 0)
                return null;

            foreach (MenuBranch aMenuBranch in menus)
            {
                if (String.Compare(menuBranchTitle, aMenuBranch.Title.ToString(), true, CultureInfo.InvariantCulture) == 0)
                    return aMenuBranch;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public override string GetMenuItemTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.GroupItemTypeDescription;
        }

         //---------------------------------------------------------------------
		/// <remarks/>
		public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_GROUP;
        }
        
        //---------------------------------------------------------------------
		/// <remarks/>
		public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement groupNode = base.GetXmlElement(aMenuXmlDocument);
            if (groupNode == null || groupNode.OwnerDocument == null)
                return null;

            if (!String.IsNullOrEmpty(InsertBefore))
                groupNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE, InsertBefore);
            if (!String.IsNullOrEmpty(InsertAfter))
                groupNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER, InsertAfter);

            if (menus != null && menus.Count > 0)
            {
                foreach (MenuBranch aMenuBranch in menus)
                {
                    XmlElement menuBranchNode = aMenuBranch.GetXmlElement(groupNode.OwnerDocument);
                    if (menuBranchNode != null)
                        groupNode.AppendChild(menuBranchNode);
                }
            }

            groupNode.Normalize();

            return groupNode;
        }

		#region IContainer Members

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component, string name)
		{
			MenuBranch branch = component as MenuBranch;
			if (branch == null)
				return;

			if (!String.IsNullOrWhiteSpace(name))
				branch.Site = new TBSite(branch, this, null, name);

			AddBranch(branch);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component)
		{
			string name = component.Site != null ? component.Site.Name : string.Empty;
			Add(component, name);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public ComponentCollection Components
		{
			get { return new ComponentCollection(menus.ToArray()); }
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Remove(IComponent component)
		{
			MenuBranch branch = component as MenuBranch;
			if (branch == null)
				return;

			menus.Remove(branch);
		}

        //---------------------------------------------------------------------
        /// <remarks/>
        public bool ValidateStructure() 
        {
            bool bValidate = true;
            // validate each child.
            foreach (MenuBranch oComponent in this.Components)
            {
                bValidate &= oComponent.ValidateStructure();
                
                
                //if (oComponent.Components.Count == 0)
                //{
                //    bValidate = false;
                //}
            }
            return bValidate;
        }
		#endregion
	}

	//=========================================================================
    [Serializable]
	internal class MenuBranch : BaseMenuItem, IComponent, IContainer
	{
		private List<MenuBranch>  menus		= new List<MenuBranch>();
		private List<MenuCommand> commands	= new List<MenuCommand>();

        //---------------------------------------------------------------------
		[Browsable(false)]
		public List<MenuBranch> Menus { get { return menus; } }
		//---------------------------------------------------------------------
		[Browsable(false)]
		public List<MenuCommand> Commands { get { return commands; } }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuBranch()
        {
        }

		/// <remarks/>
		//---------------------------------------------------------------------
		public MenuBranch(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsMenu)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the MenuBranch constructor.");

			ArrayList menuNodes = aMenuXmlNode.MenuItems;
            if (menuNodes != null && menuNodes.Count > 0)
            {
                foreach (MenuXmlNode aMenuNode in menuNodes)
                {
					if (aMenuNode != null && aMenuNode.IsMenu)
					{
						MenuBranch aMenuBranch = new MenuBranch(aMenuNode, this);
						AddBranch(aMenuBranch);
					}
                }
            }

            ArrayList commandNodes = aMenuXmlNode.CommandItems;
			MenuCommand aMenuCommand = null;
			if (commandNodes != null && commandNodes.Count > 0)
            {
                foreach (MenuXmlNode aCommandNode in commandNodes)
                {
                    if (aCommandNode != null)
                    {
						aMenuCommand = MenuEditorEngine.GetMenuObjectFromXmlNode(aCommandNode, this) as MenuCommand;

						if (
							IsContainedInUserGroup(aMenuCommand, MenuUserGroups.DotUserReportsGroup) ||
							IsContainedInUserGroup(aMenuCommand, MenuUserGroups.DotUserOfficeFilesGroup)
							)
							aMenuCommand.Owner = MenuEditorEngine.GetMyOwnerString();

						AddCommand(aMenuCommand);
                    }
                }
            }
        }

		//---------------------------------------------------------------------
		private static bool IsContainedInUserGroup(MenuCommand aMenuCommand, string userGroup)
		{
			if (aMenuCommand.Site == null || aMenuCommand.Site.Container == null)
				return false;

			IComponent component = aMenuCommand.Site.Container as IComponent;
			while (component != null)
			{
				if (component.Site == null || component.Site.Container == null)
					return false;

				MenuGroup group = component.Site.Container as MenuGroup;
				if (group != null && group.Name.Contains(userGroup))
					return true;

				component = component.Site.Container as IComponent; 
			}

			return false;
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public new MenuBranch Clone()
        {
            MenuBranch clonedMenuBranch = base.Clone() as MenuBranch;

            clonedMenuBranch.InsertAfter = this.InsertAfter;
            clonedMenuBranch.InsertBefore = this.InsertBefore;

            if (this.Menus != null && this.Menus.Count > 0)
            {
                foreach (MenuBranch aSubMenuBranchToClone in this.Menus)
                    clonedMenuBranch.AddBranch(aSubMenuBranchToClone.Clone());
            }
            if (this.Commands != null && this.Commands.Count > 0)
            {
                foreach (MenuCommand aCommandToClone in this.Commands)
                    clonedMenuBranch.AddCommand(aCommandToClone.Clone() as MenuCommand);
            }

            
            return clonedMenuBranch;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void AddBranch(MenuBranch aMenuBranch)
		{
            if (aMenuBranch == null)
				return;

			if (menus == null)
				menus = new List<MenuBranch>();

			Insert(menus.Count, aMenuBranch);
		}

		/// <remarks/>
		//---------------------------------------------------------------------
		public void Insert(int index, MenuBranch aMenuBranch)
		{
			if (aMenuBranch == null)
				return;

			if (menus == null)
				menus = new List<MenuBranch>();
            else if (menus.Contains(aMenuBranch))
                return;

			if (index < 0 || index > menus.Count)
				throw new ArgumentOutOfRangeException();

			menus.Insert(index, aMenuBranch);

			aMenuBranch.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
            aMenuBranch.MenuItemAdded += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aMenuBranch.MenuItemMoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aMenuBranch.MenuItemRemoved += new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemAdded(new MenuItemEventArgs(aMenuBranch, this, (index > 0) ? menus[index - 1] : null));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void Move(int index, MenuBranch aMenuBranch, object parentMenuItem)
        {
            if (aMenuBranch == null || parentMenuItem == null)
                return;

			List<MenuBranch> parentMenuBranches = null;
            if (parentMenuItem is MenuGroup)
                parentMenuBranches = ((MenuGroup)parentMenuItem).Menus;
            else if (parentMenuItem is MenuBranch)
                parentMenuBranches = ((MenuBranch)parentMenuItem).Menus;
            if (parentMenuBranches == null || !parentMenuBranches.Contains(aMenuBranch))
                return;

            if
               (
               index < 0 ||
               (menus == null && index != 0) ||
               (menus != null && index > menus.Count)
               )
                throw new ArgumentOutOfRangeException();

            if (menus == null)
				menus = new List<MenuBranch>();

            int currentIdx = parentMenuBranches.IndexOf(aMenuBranch);
            if (currentIdx == -1 || (object.ReferenceEquals(parentMenuBranches, menus) && currentIdx == index))
                return;

            MenuBranch previousBeforeMenuBranch = (currentIdx > 0) ? parentMenuBranches[currentIdx - 1] : null;

            MenuBranch beforeMenuBranch = (index > 0) ? menus[index - 1] : null;

            menus.Insert(index, aMenuBranch);

            if (object.ReferenceEquals(parentMenuBranches, menus) && currentIdx > index)
                currentIdx++;
            parentMenuBranches.RemoveAt(currentIdx);

            OnMenuItemMoved(new MenuItemEventArgs(aMenuBranch, this, beforeMenuBranch, parentMenuItem, previousBeforeMenuBranch));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public bool Remove(MenuBranch aMenuBranch)
        {
            if (menus == null || menus.Count == 0)
                return false;

            int menuItemIdx = menus.IndexOf(aMenuBranch);
            if (menuItemIdx == -1)
                return false;

            if (!menus.Remove(aMenuBranch))
                return false;

            aMenuBranch.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);
            aMenuBranch.MenuItemAdded -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemAdded);
            aMenuBranch.MenuItemMoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemMoved);
            aMenuBranch.MenuItemRemoved -= new EventHandler<MenuItemEventArgs>(MenuItem_SubMenuItemRemoved);

            OnMenuItemRemoved(new MenuItemEventArgs(aMenuBranch, this, (menuItemIdx > 0) ? menus[menuItemIdx - 1] : null));

            return true;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void AddCommand(MenuCommand aCommand)
		{
			if (aCommand == null)
				return;

			if (commands == null)
				commands = new List<MenuCommand>();

			Insert(commands.Count, aCommand);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Insert(int index, MenuCommand aCommand)
		{
			if (aCommand == null)
				return;

			if (commands == null)
				commands = new List<MenuCommand>();
            else if (commands.Contains(aCommand))
                return;

			if (index < 0 || index > commands.Count)
				throw new ArgumentOutOfRangeException();

			commands.Insert(index, aCommand);
			aCommand.PropertyValueChanged += new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);

            OnMenuItemAdded(new MenuItemEventArgs(aCommand, this, (index > 0) ? commands[index - 1] : null));
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public void Move(int index, MenuCommand aCommand, MenuBranch parentMenuBranch)
        {
            if 
                (
                aCommand == null || 
                parentMenuBranch == null || 
                parentMenuBranch.Commands == null ||
                !parentMenuBranch.Commands.Contains(aCommand))
                return;

            if 
                (
                index < 0 || 
                (commands == null && index != 0) ||
                (commands != null && index > commands.Count)
                )
                throw new ArgumentOutOfRangeException();

            if (commands == null)
				commands = new List<MenuCommand>();

            int currentIdx = parentMenuBranch.Commands.IndexOf(aCommand);
            if (currentIdx == -1 || (object.ReferenceEquals(parentMenuBranch, this) && currentIdx == index))
                return;

            MenuCommand previousBeforeCommand = (currentIdx > 0) ? parentMenuBranch.Commands[currentIdx - 1] : null;
            
            MenuCommand beforeCommand = (index > 0) ? commands[index - 1] : null;

            commands.Insert(index, aCommand);

            if (object.ReferenceEquals(parentMenuBranch, this) && currentIdx > index)
                currentIdx++;
            parentMenuBranch.Commands.RemoveAt(currentIdx);

            OnMenuItemMoved(new MenuItemEventArgs(aCommand, this, beforeCommand, parentMenuBranch, previousBeforeCommand));
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public bool Remove(MenuCommand aCommand)
		{
			if (commands == null || commands.Count == 0)
				return false;

            int menuItemIdx = commands.IndexOf(aCommand);
            if (menuItemIdx == -1)
                return false;

            if (!commands.Remove(aCommand))
                return false;

			aCommand.PropertyValueChanged -= new EventHandler<MenuItemPropertyValueChangedEventArgs>(SubItem_PropertyValueChanged);

            OnMenuItemRemoved(new MenuItemEventArgs(aCommand, this, (menuItemIdx > 0) ? commands[menuItemIdx - 1] : null));

            return true;
        }

		//---------------------------------------------------------------------
		/// <remarks/>
		public override void Clear()
		{
			base.Clear();

			foreach (MenuCommand menuCmd in commands)
				menuCmd.Clear();

			commands.Clear();

			foreach (MenuBranch menuBranch in menus)
				menuBranch.Clear();

			menus.Clear();
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

            MenuBranch aMenuBranch = obj as MenuBranch;

            if (aMenuBranch == null)
                throw new ArgumentException("'obj' is not a MenuBranch object");

			int result = base.CompareTo(obj, ignoreCase, culture);

			if (result != 0)
				return result;

			if (menus == null)
                result = (aMenuBranch.Menus == null || aMenuBranch.Menus.Count == 0) ? 0 : -1;
			else
                result = menus.CompareTo(aMenuBranch.menus);

			if (result != 0)
				return result;

			if (commands == null)
                result = (aMenuBranch.Commands == null || aMenuBranch.Commands.Count == 0) ? 0 : -1;
			else
                result = commands.CompareTo(aMenuBranch.Commands);

			if (result != 0)
				return result;

            result = String.Compare(InsertAfter, aMenuBranch.InsertAfter, ignoreCase, culture);

			if (result != 0)
				return result;

            return String.Compare(InsertBefore, aMenuBranch.InsertBefore, ignoreCase, culture);
		}

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuBranch GetMenuBranch(string menuBranchName)
        {
            if (menus == null || menus.Count == 0 || menuBranchName == null || menuBranchName.Length == 0)
                return null;

            foreach (MenuBranch aMenuBranch in menus)
            {
                if (String.Compare(menuBranchName, aMenuBranch.Name, true, CultureInfo.InvariantCulture) == 0)
                    return aMenuBranch;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuBranch GetMenuBranchByTitle(string menuBranchTitle)
        {
            if (menus == null || menus.Count == 0 || menuBranchTitle == null || menuBranchTitle.Length == 0)
                return null;

            foreach (MenuBranch aMenuBranch in menus)
            {
                if (String.Compare(menuBranchTitle, aMenuBranch.Title.ToString(), true, CultureInfo.InvariantCulture) == 0)
                    return aMenuBranch;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public MenuCommand GetCommandByTitle(string commandTitle)
        {
            if (commands == null || commands.Count == 0 || commandTitle == null || commandTitle.Length == 0)
                return null;

            foreach (MenuCommand aMenuCommand in commands)
            {
                if (String.Compare(commandTitle, aMenuCommand.Title.ToString(), true, CultureInfo.InvariantCulture) == 0)
                    return aMenuCommand;
            }

            return null;
        }

        //---------------------------------------------------------------------
		/// <remarks/>
		public override string GetMenuItemTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.MenuBranchItemTypeDescription;
        }

		/// <remarks/>
		//---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_MENU;
        }
        
        //---------------------------------------------------------------------
		/// <remarks/>
		public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement menuBranchNode = base.GetXmlElement(aMenuXmlDocument);
            if (menuBranchNode == null || menuBranchNode.OwnerDocument == null)
                return null;

            if (!String.IsNullOrEmpty(InsertBefore))
                menuBranchNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE, InsertBefore);
            if (!String.IsNullOrEmpty(InsertAfter))
                menuBranchNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER, InsertAfter);

            if (menus != null && menus.Count > 0)
            {
                foreach (MenuBranch aMenuBranch in menus)
                {
                    XmlElement subMenuBranchNode = aMenuBranch.GetXmlElement(menuBranchNode.OwnerDocument);
                    if (subMenuBranchNode != null)
                        menuBranchNode.AppendChild(subMenuBranchNode);
                }
            }

            if (commands != null && commands.Count > 0)
            {
                foreach (MenuCommand aMenuCommand in commands)
                {
                    XmlElement commandNode = aMenuCommand.GetXmlElement(menuBranchNode.OwnerDocument);
                    if (commandNode != null)
                        menuBranchNode.AppendChild(commandNode);
                }
            }
            return menuBranchNode;
        }

		#region IContainer Members

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component, string name)
		{
			MenuBranch branch = component as MenuBranch;
			MenuCommand command = component as MenuCommand;
			if (branch == null && command == null)
				return;

			if (!String.IsNullOrWhiteSpace(name))
				command.Site = new TBSite(command, this, null, name);

			if (branch != null)
				AddBranch(branch);

			if (command != null)
				AddCommand(command);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component)
		{
			string name = component.Site != null ? component.Site.Name : string.Empty;
			Add(component, name);
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public ComponentCollection Components
		{
			get
			{
				List<BaseMenuItem> children = new List<BaseMenuItem>(this.menus);
				children.AddRange(this.commands);

				return new ComponentCollection(children.ToArray());
			}
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public void Remove(IComponent component)
		{
			MenuBranch branch = component as MenuBranch;
			MenuCommand command = component as MenuCommand;
			if (branch == null && command == null)
				return;

			if (branch != null)
				Remove(branch);

			if (command != null)
				Remove(command);
		}

		#endregion

        internal bool ValidateStructure()
        {
            bool bValidate = true;


            if (this.Menus.Count > 0 && (this.commands.Count > 0))
            {
                bValidate = false;
            }

            return bValidate;
        }
    }

	//=========================================================================
    [Serializable]

	internal abstract class MenuCommand : BaseMenuItem
	{
		private string			commandObject = String.Empty;
		private string			description	= String.Empty;
		private bool			searchCommandImage;

        //---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
        CategoryAttribute("Behavior"),
        MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT)]
		public virtual string CommandObject
        {
            get { return commandObject; } 
            set 
            {
                string currentValue = commandObject;
                
                commandObject = value;

                OnPropertyValueChanged("CommandObject", currentValue); 
            }
        }

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuCommandDescriptionPropertyDescription", typeof(MenuEditorStrings)),
        CategoryAttribute("Appearance"),
		AffectsAppearanceAttribute(true),
        MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_DESCRIPTION)]
        public string Description 
        {
            get { return description; } 
            set 
            {
                string currentValue = description;
                
                description = value;

                OnPropertyValueChanged("Description", currentValue);
            }
        }

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuCommandSearchCommandImagePropertyDescription", typeof(MenuEditorStrings)),
        CategoryAttribute("Appearance"),
		AffectsAppearanceAttribute(true),
        DefaultValueAttribute(false),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE)]
        public bool SearchCommandImage 
        {
            get { return searchCommandImage; } 
            set
            {
                bool currentValue = searchCommandImage;

                searchCommandImage = value;

                OnPropertyValueChanged("SearchCommandImage", currentValue); 
            }
        }

		//---------------------------------------------------------------------
		[LocalizedDescriptionAttribute("MenuItemNamePropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Design"),
		AffectsAppearanceAttribute(true),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_NAME),
		Browsable(false)]
		public override string Name
		{
			get
			{
				return this.Title.Text;
			}
			set
			{
				//Do nothing;
			}
		}

		//---------------------------------------------------------------------
		public MenuCommand()
		{
		}

		//---------------------------------------------------------------------
		public MenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
		{
			this.CommandObject = aMenuXmlNode.ItemObject;
			this.Description = aMenuXmlNode.Description;
			this.SearchCommandImage = aMenuXmlNode.IsCommandImageToSearch;
			this.Site = new TBSite(this, container, null, this.Name);
		}

        ////-----------------------------------------------------------------------------
        //public override bool Equals(object obj)
        //{
        //    MenuCommand aMenuCommand = obj as MenuCommand;
        //    if (aMenuCommand == null)
        //        return false;

        //    if (this.Title == null && aMenuCommand.Title == null)
        //        return true;

        //    if (this.Title == null && aMenuCommand.Title != null)
        //        return false;

        //    if (this.Title != null && aMenuCommand.Title == null)
        //        return false;

        //    return String.Compare(this.Title.Text, aMenuCommand.Title.Text, StringComparison.InvariantCultureIgnoreCase) == 0;
        //}

        ////-----------------------------------------------------------------------------
        //public override int GetHashCode()
        //{
        //    if (this.Title == null)
        //        return base.GetHashCode();

        //    return this.Title.GetHashCode();
        //}
       
        //---------------------------------------------------------------------
        public override BaseMenuItem Clone()
        {
            MenuCommand clonedMenuCommand = GetType().Assembly.CreateInstance(GetType().FullName) as MenuCommand;

            clonedMenuCommand.Title = (this.Title != null) ? new MenuItemTitle(this.Title.Text, this.Title.Localizable) : null;
            clonedMenuCommand.Activation = this.Activation;
            clonedMenuCommand.CommandObject = this.CommandObject;
            clonedMenuCommand.InsertAfter = this.InsertAfter;
            clonedMenuCommand.InsertBefore = this.InsertBefore;
            clonedMenuCommand.Description = this.Description;
            clonedMenuCommand.SearchCommandImage = this.SearchCommandImage;
			clonedMenuCommand.Owner = MenuEditorEngine.GetMyOwnerString();
			
            PropertyInfo eventsPropertyInfo = this.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
            if (eventsPropertyInfo == null)
				return clonedMenuCommand;

            System.ComponentModel.EventHandlerList eventHandlerList = eventsPropertyInfo.GetValue(this, null) as System.ComponentModel.EventHandlerList;
            if (eventHandlerList == null)
				return clonedMenuCommand;

            System.Reflection.EventInfo[] events = this.GetType().GetEvents();
            foreach (System.Reflection.EventInfo aEventInfo in events)
            {
                FieldInfo eventKeyField = this.GetType().GetField("Event" + aEventInfo.Name, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);

				if (eventKeyField == null)
					continue;
                
				object eventKeyValue = eventKeyField.GetValue(this);
				if (eventKeyValue == null)
					continue;

                System.EventHandler eventHandler = eventHandlerList[eventKeyValue] as System.EventHandler;
                if (eventHandler != null)
                    aEventInfo.AddEventHandler(clonedMenuCommand, new System.EventHandler(eventHandler));
            }
            
            return clonedMenuCommand;
        }

		//---------------------------------------------------------------------
		public override void Clear()
		{
			base.Clear();

			commandObject			= string.Empty;
			description				= string.Empty;
			searchCommandImage		= false;
		}
        
        //---------------------------------------------------------------------
        public virtual string GetMenuCommandTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.GenericMenuCommandTypeDescription;
        }

		//---------------------------------------------------------------------
		public override int CompareTo(object obj)
		{
			return CompareTo(obj, true, CultureInfo.InvariantCulture);
		}
		
		//---------------------------------------------------------------------
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

			MenuCommand aCommand = obj as MenuCommand;

			if (aCommand == null)
				throw new ArgumentException("'obj' is not a MenuCommand object.");

			int result = String.Compare(commandObject, aCommand.commandObject, ignoreCase, culture);

			if (result != 0)
				return result;

			result = Title.CompareTo(aCommand.Title, ignoreCase, culture);

			if (result != 0)
				return result;

			result = String.Compare(Activation, aCommand.Activation, ignoreCase, culture);

			if (result != 0)
				return result;

			result = String.Compare(InsertAfter, aCommand.InsertAfter, ignoreCase, culture);

			if (result != 0)
				return result;

			result = String.Compare(InsertBefore, aCommand.InsertBefore, ignoreCase, culture);

			if (result != 0)
				return result;

			if (searchCommandImage == aCommand.SearchCommandImage)
				return 0;

			return searchCommandImage ? 1 : -1;
		}

        //---------------------------------------------------------------------
        public override string ToString()
        {
			return (Title != null) ? Title.ToString() : String.Empty;
        }

        //---------------------------------------------------------------------
        private void Title_PropertyChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
        {
            MenuItemTitle previousValue = null;
            if (e.Property.Name == "Text")
				previousValue = new MenuItemTitle(e.PreviousValue as string, Title.Localizable);
            else if (e.Property.Name == "Localizable")
				previousValue = new MenuItemTitle(Title.Text, (bool)e.PreviousValue);

                OnPropertyValueChanged("Title", previousValue);
        }

		//---------------------------------------------------------------------
		public override string GetNodePathHierarchyElement()
		{
			string xmlTag = GetXmlTag();
			if (xmlTag == null || xmlTag.Length == 0)
				return string.Empty;

			return String.Format(
				"{0}[{1}='{2}']",
				xmlTag,
				MenuXmlNode.XML_TAG_TITLE,
				this.Title != null ? this.Title.Text : string.Empty
				);
		}

        //---------------------------------------------------------------------
        public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            string commandXmlTag = GetXmlTag();
            if (String.IsNullOrEmpty(commandXmlTag))
                return null;

            if (aMenuXmlDocument == null)
            {
                aMenuXmlDocument = new XmlDocument();
                // if the PreserveWhitespace property is set to false, 
                // XmlDocument auto-indents the output.
                aMenuXmlDocument.PreserveWhitespace = false;
            }

            XmlElement commandNode = aMenuXmlDocument.CreateElement(commandXmlTag);
            if (commandNode == null)
                return null;

            if (!String.IsNullOrEmpty(InsertBefore))
                commandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_BEFORE, InsertBefore);
            if (!String.IsNullOrEmpty(InsertAfter))
                commandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_INSERT_AFTER, InsertAfter);

            if (!String.IsNullOrEmpty(Activation))
				commandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_ACTIVATION, Activation);

			if (!String.IsNullOrEmpty(Owner))
				commandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_OWNER, Owner);

            if (searchCommandImage)
                commandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_SEARCH_COMMAND_IMAGE, true.ToString(CultureInfo.InvariantCulture));

            if (Title != null)
            {
                XmlElement titleElement = aMenuXmlDocument.CreateElement(MenuXmlNode.XML_TAG_TITLE);
                if (titleElement != null)
                {
                    titleElement.InnerText = Title.Text;
                    titleElement.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_LOCALIZABLE, Title.Localizable.ToString(CultureInfo.InvariantCulture));

                    commandNode.AppendChild(titleElement);
                }
            }

            if (!String.IsNullOrEmpty(commandObject))
            {
                XmlElement cmdObjElement = aMenuXmlDocument.CreateElement(MenuXmlNode.XML_TAG_OBJECT);
                if (cmdObjElement != null)
                {
                    cmdObjElement.InnerText = commandObject;

                    commandNode.AppendChild(cmdObjElement);
                }
            }
            
            if (!String.IsNullOrEmpty(description))
            {
                XmlElement descriptionElement = aMenuXmlDocument.CreateElement(MenuXmlNode.XML_TAG_DESCRIPTION);
                if (descriptionElement != null)
                {
                    descriptionElement.InnerText = description;

                    commandNode.AppendChild(descriptionElement);
                }
            }

            commandNode.Normalize();

            return commandNode;
        }

		//---------------------------------------------------------------------
		public abstract string GetMenuCommandDefaultTitleFormat();
	}
	
	//=========================================================================
    [Serializable]
	internal class DocumentMenuCommand : MenuCommand, IComponent
	{
		//---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return MenuEditorStrings.DocumentCommandTypeDescription;
        }
		
		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(DocumentBatchMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public DocumentMenuCommand()
        {
        }
	
        //---------------------------------------------------------------------
        public DocumentMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_DOCUMENT;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.DocumentMenuCommandDefaultTitleFormat;
		}
    }

    //=========================================================================
    [Serializable]
	internal class ReportMenuCommand : MenuCommand, IComponent
    {
		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(ReportMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

        //---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return MenuEditorStrings.ReportCommandTypeDescription;
        }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public ReportMenuCommand()
        {
        }

        //---------------------------------------------------------------------
        public ReportMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_REPORT;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.ReportMenuCommandDefaultTitleFormat;
		}
    }

    //=========================================================================
	internal class BatchCommandSubTypeConverter : StringConverter
    {
        //---------------------------------------------------------------------
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[]{
                MenuXmlNode.CommandNodeSubType.Standard.ToString(), 
                MenuXmlNode.CommandNodeSubType.Wizard.ToString()});
        }
    }

    //=========================================================================
    [Serializable]
	internal class BatchMenuCommand : MenuCommand, IComponent
	{
        private string subType = "";

        //---------------------------------------------------------------------
        [TypeConverter(typeof(BatchCommandSubTypeConverter)),
		CategoryAttribute("Behavior"), AffectsAppearanceAttribute(true),
		LocalizedDescriptionAttribute("BatchMenuCommandSubTypePropertyDescription", typeof(MenuEditorStrings)),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE),
		Browsable(false)]
        public string SubType
        {
            get { return subType; }
            set
            {
                if (String.Compare(subType, value) == 0)
                    return;

                string currentValue = subType;
                subType = value;
                OnPropertyValueChanged("SubType", currentValue);
            }
        }
		
		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(DocumentBatchMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

		//---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return MenuEditorStrings.BatchCommandTypeDescription;
        }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public BatchMenuCommand()
        {
        }

        //---------------------------------------------------------------------
		public BatchMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsRunBatch)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the BatchMenuCommand constructor.");

            this.SubType = (aMenuXmlNode.CommandSubType != null) ? aMenuXmlNode.CommandSubType.GetXmlTag() : String.Empty;
        }

        //---------------------------------------------------------------------
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

			BatchMenuCommand aBatch = obj as BatchMenuCommand;
			if (aBatch == null)
				throw new ArgumentException("'obj' is not a BatchMenuCommand object");

			int result = base.CompareTo(aBatch,ignoreCase,culture);
			if (result != 0)
				return result;
					
			return String.Compare(subType, aBatch.subType  , ignoreCase, culture);
		}

        //---------------------------------------------------------------------
		[Browsable(false)]
        public bool IsWizardBatch
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_BATCH_SUBTYPE_WIZARD) == 0; 
            }
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_BATCH;
        }
    
        //---------------------------------------------------------------------
        public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement batchCommandNode = base.GetXmlElement(aMenuXmlDocument);
            if (batchCommandNode == null || batchCommandNode.OwnerDocument == null)
                return null;

            if (IsWizardBatch)
                batchCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_BATCH_SUBTYPE_WIZARD);
            else
                batchCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_BATCH_SUBTYPE_STANDARD);

            return batchCommandNode;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.BatchMenuCommandDefaultTitleFormat;
		}
    }

    //=========================================================================
	internal class FunctionCommandSubTypeConverter : StringConverter
    {
        //---------------------------------------------------------------------
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[]{
                MenuXmlNode.CommandNodeSubType.Form.ToString(), 
                MenuXmlNode.CommandNodeSubType.Batch.ToString(), 
                MenuXmlNode.CommandNodeSubType.Report.ToString(), 
                MenuXmlNode.CommandNodeSubType.Function.ToString(), 
                MenuXmlNode.CommandNodeSubType.Exe.ToString(), 
                MenuXmlNode.CommandNodeSubType.Text.ToString()});
        }
    }
    //=========================================================================
    [Serializable]
	internal class FunctionMenuCommand : MenuCommand, IComponent
	{
		private string subType = "";

        //---------------------------------------------------------------------
        [TypeConverter(typeof(FunctionCommandSubTypeConverter)),
		CategoryAttribute("Behavior"),
		AffectsAppearanceAttribute(true),
		LocalizedDescriptionAttribute("FunctionMenuCommandSubTypePropertyDescription", typeof(MenuEditorStrings)),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE),
		Browsable(false)]
        public string SubType
        {
            get { return subType; }
            set
            {
                if (String.Compare(subType, value) == 0)
                    return;

                string currentValue = subType;
                subType = value;
                OnPropertyValueChanged("SubType", currentValue);
            }
        }

		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(FunctionMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public FunctionMenuCommand()
        {
        }

        //---------------------------------------------------------------------
		public FunctionMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsRunFunction)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the FunctionMenuCommand constructor.");

            this.SubType = (aMenuXmlNode.CommandSubType != null) ? aMenuXmlNode.CommandSubType.GetXmlTag() : String.Empty;
        }

        //---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.FunctionCommandTypeDescription;
        }

        //---------------------------------------------------------------------
        public override BaseMenuItem Clone()
        {
            FunctionMenuCommand clonedMenuCommand = base.Clone() as FunctionMenuCommand;

            clonedMenuCommand.SubType = this.SubType;

            return clonedMenuCommand;
        }

        //---------------------------------------------------------------------
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

			FunctionMenuCommand aFunction = obj as FunctionMenuCommand;
			if (aFunction == null)
				throw new ArgumentException("'obj' is not a FunctionMenuCommand object.");

			int result = base.CompareTo(aFunction,ignoreCase,culture);
			if (result != 0)
				return result;
		
			return String.Compare(subType, aFunction.subType  , ignoreCase, culture);
		}

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunDocumentFunction
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_FORM) == 0; 
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunReportFunction
        {
                get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_REPORT) == 0; 
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunBatchFunction
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_BATCH) == 0; 
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunFunctionFunction
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_FUNCTION) == 0;
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunTextFunction
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_TEXT) == 0; 
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRunExecutableFunction
        {
            get
            {
                return String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_EXE) == 0; 
            }
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_FUNCTION;
        }

        //---------------------------------------------------------------------
        public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement functionCommandNode = base.GetXmlElement(aMenuXmlDocument);
            if (functionCommandNode == null || functionCommandNode.OwnerDocument == null)
                return null;

            if (IsRunDocumentFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_FORM);
            else if (IsRunBatchFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_BATCH);
            else if (IsRunReportFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_REPORT);
            else if (IsRunFunctionFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_FUNCTION);
            else if (IsRunExecutableFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_EXE);
            else if (IsRunTextFunction)
                functionCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, MenuXmlNode.MenuXmlNodeCommandSubType.XML_FUNCTION_SUBTYPE_TEXT);

            return functionCommandNode;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.FunctionMenuCommandDefaultTitleFormat;
		}
    }

    //=========================================================================
    [Serializable]
	internal class TextMenuCommand : MenuCommand, IComponent
	{
		//---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return MenuEditorStrings.TextCommandTypeDescription;
        }

		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(FileMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public TextMenuCommand()
        {
        }
	
    	//---------------------------------------------------------------------
		public TextMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {

        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_TEXT;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.TextMenuCommandDefaultTitleFormat;
		}
    }

	//=========================================================================
    [Serializable]
	internal class ExeMenuCommand : MenuCommand, IComponent
	{
		//---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.ExeCommandTypeDescription;
        }

		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(FileMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public ExeMenuCommand()
        {
        }

	    //---------------------------------------------------------------------
		public ExeMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_EXE;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.ExecutableMenuCommandDefaultTitleFormat;
		}
    }

	//=========================================================================
	internal class OfficeApplicationConverter : StringConverter
    {
        //---------------------------------------------------------------------
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
        //---------------------------------------------------------------------
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[]{
                MenuXmlNode.OfficeItemApplication.Word.ToString(), 
                MenuXmlNode.OfficeItemApplication.Excel.ToString()});
        }
    }

    //=========================================================================
    [Serializable]
	internal class OfficeItemMenuCommand : MenuCommand, IComponent
	{
        private string application = String.Empty;
        private string subType = String.Empty;

		[LocalizedDescriptionAttribute("MenuCommandObjectPropertyDescription", typeof(MenuEditorStrings)),
		CategoryAttribute("Behavior"),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Element),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_TAG_OBJECT),
		EditorAttribute(typeof(OfficeItemMenuCommandObjectTypeEditor), typeof(UITypeEditor))]
		public override string CommandObject { get { return base.CommandObject; } set { base.CommandObject = value; } }
        
        //---------------------------------------------------------------------
        [AffectsAppearanceAttribute(true),
		MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_OFFICE_ITEM_APP),
		Browsable(false)]
        public string Application
        {
            get { return application; }
            set 
            {
                if (String.Compare(application, value) == 0)
                    return;

                string currentValue = application;
                application = value; 
                OnPropertyValueChanged("Application", currentValue); 
            }
        }

        //---------------------------------------------------------------------
        [AffectsAppearanceAttribute(true),
        MenuItemXmlNodeTypeAttribute(XmlNodeType.Attribute),
		MenuItemXmlNodeNameAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE),
		Browsable(false)]
        public string SubType
        {
            get { return subType; }
            set 
            {
                if (String.Compare(subType, value) == 0)
                    return;

                string currentValue = subType;
                subType = value;
                OnPropertyValueChanged("SubType", currentValue);
            }
        }
       
		//---------------------------------------------------------------------
        public override string GetMenuCommandTypeDescription()
        {
			return Microarea.EasyBuilder.MenuEditor.MenuEditorStrings.OfficeCommandTypeDescription;
        }

        //---------------------------------------------------------------------
        // Il costruttore senza parametri è necessario per poter chiamare la CreateInstance sul tipo!!!
        //---------------------------------------------------------------------
        public OfficeItemMenuCommand()
        {
        }

        //---------------------------------------------------------------------
		public OfficeItemMenuCommand(MenuXmlNode aMenuXmlNode, IContainer container)
			: base(aMenuXmlNode, container)
        {
            if (aMenuXmlNode == null || !aMenuXmlNode.IsOfficeItem)
                throw new ArgumentException("Null or invalid MenuXmlNode passed as argument to the OfficeItemMenuCommand constructor.");

            this.SubType = (aMenuXmlNode.CommandSubType != null ) ? aMenuXmlNode.CommandSubType.GetXmlTag() : String.Empty;
            this.Application = aMenuXmlNode.GetOfficeApplication().ToString();
        }

        //---------------------------------------------------------------------
        public override BaseMenuItem Clone()
        {
            OfficeItemMenuCommand clonedMenuCommand = base.Clone() as OfficeItemMenuCommand;

            clonedMenuCommand.Application = this.Application;
            clonedMenuCommand.SubType = this.SubType;
            
			return clonedMenuCommand;
        }
        
        //---------------------------------------------------------------------
		public override int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

			OfficeItemMenuCommand aOfficeItem = obj as OfficeItemMenuCommand;

			if (aOfficeItem == null)
				throw new ArgumentException("'obj' is not a OfficeItemMenuCommand object.");

			int result = base.CompareTo(aOfficeItem,ignoreCase,culture);

			if (result != 0)
				return result;

			result = String.Compare(application, aOfficeItem.application,  ignoreCase, culture);

			if (result != 0)
				return result;
		
			return String.Compare(subType, aOfficeItem.subType  , ignoreCase, culture);

		}

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsWordItem
        {
            get
            {
                return (String.Compare(application, MenuXmlNode.OfficeItemApplication.Word.ToString()) == 0);
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsWordDocument
        {
            get
            {
                return IsWordItem &&
                    String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT) == 0;
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsWordTemplate
        {
            get
            {
                return IsWordItem &&
                    String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE) == 0;
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsExcelItem
        {
            get
            {
                return (String.Compare(application, MenuXmlNode.OfficeItemApplication.Excel.ToString()) == 0);
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsExcelDocument
        {
            get
            {
                return IsExcelItem &&
                    String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT) == 0;
            }
        }

        //---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsExcelTemplate
        {
            get
            {
                return IsExcelItem &&
                    String.Compare(subType, MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE) == 0;
            }
        }

        //---------------------------------------------------------------------
        public override string GetXmlTag()
        {
            return MenuXmlNode.XML_TAG_OFFICE_ITEM;
        }

        //---------------------------------------------------------------------
        public override XmlElement GetXmlElement(XmlDocument aMenuXmlDocument)
        {
            XmlElement officeItemCommandNode = base.GetXmlElement(aMenuXmlDocument);
            if (officeItemCommandNode == null || officeItemCommandNode.OwnerDocument == null)
                return null;

            if (IsWordItem)
                officeItemCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_OFFICE_ITEM_APP, MenuXmlNode.OfficeItemApplication.Word.ToString());
            else if (IsExcelItem)
                officeItemCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_OFFICE_ITEM_APP, MenuXmlNode.OfficeItemApplication.Excel.ToString());
            if (!String.IsNullOrEmpty(subType))
                officeItemCommandNode.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_COMMAND_SUBTYPE, subType);

            return officeItemCommandNode;
        }

		//---------------------------------------------------------------------
		public override string GetMenuCommandDefaultTitleFormat()
		{
			return MenuEditorStrings.OfficeItemMenuCommandDefaultTitleFormat;
		}
    }

	//=========================================================================
    [Serializable]
	internal class MenuItemTitle : EasyBuilderTypeDescriptor, IComparable, IComponent
	{	
		private string text = String.Empty;
		private bool localizable = true;

		public event EventHandler Disposed;
		public event EventHandler<MenuItemPropertyValueChangedEventArgs> PropertyChanged;
		private ISite site;

		//---------------------------------------------------------------------
		private void OnPropertyChanged(object sender, MenuItemPropertyValueChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(sender, e);
		}

        //---------------------------------------------------------------------
        public string Text 
        {
            get { return text; } 
            set 
            {
                string currentValue = text;

                text = value;
				OnPropertyChanged(this, new MenuItemPropertyValueChangedEventArgs(null, this.GetType().GetProperty("Text"), currentValue));
            } 
        }

        //---------------------------------------------------------------------
		[Browsable(false)]
        public bool Localizable 
        {
            get { return localizable; } 
            set 
            {
                bool currentValue = localizable;
                
                localizable = value;
				OnPropertyChanged(this, new MenuItemPropertyValueChangedEventArgs(null, this.GetType().GetProperty("Localizable"), currentValue));
            } 
        }

		//---------------------------------------------------------------------
		public MenuItemTitle(string aText, bool aLocalizable)
		{
            text = aText;
			localizable	= aLocalizable;
		}

        //---------------------------------------------------------------------
		public override string ToString()
        {
            return text;
        }

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			return CompareTo(obj, true, CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------
		public int CompareTo(object obj, bool ignoreCase, CultureInfo culture)
		{
			if (obj == null)
				return 1;

			MenuItemTitle aTitle = obj as MenuItemTitle;

			if (aTitle == null)
				throw new ArgumentException("'obj' is not a MenuItemTitle object.");

			return String.Compare(text, aTitle.Text, ignoreCase, culture);
		}

		//---------------------------------------------------------------------
		protected void OnDisposed()
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public ISite Site
		{
			get
			{
				return this.site;
			}
			set
			{
				this.site = value;
			}
		}

		//---------------------------------------------------------------------
		[Browsable(false)]
		public BaseMenuItem ContainerComponent { get; set; }

		//-----------------------------------------------------------------------------
		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			//Uso il TypeDescriptor sul tipo e non sull'istanza perchè usandolo sull'istanza non funziona:
			//infatti viene usato il DictionaryService ottenuto tramite il Site del Component in causa per memorizzare una cache per la collezione delle proprietà.
			//Questa cache è una per ogni istanza, e a seconda del controllo viene ritornata sempre la stessa causando problemi alla PropertyGrid
			//Ogni qualvolta si selezionavano oggetti di tipo diverso (per esempio combo box e tab)
			//Lavorando sui tipi invece viene utilizzata una cache per ogni tipo e non per ogni istanza, cosa che, oltre ad essere più efficiente, non causa
			//malfunzionamenti alla PropertyGrid nel nostro caso.
			PropertyDescriptorCollection defaultProperties;
			if (attributes == null)
				defaultProperties = TypeDescriptor.GetProperties(GetType());
			else
				defaultProperties = TypeDescriptor.GetProperties(GetType(), attributes);

			PropertyDescriptor[] props = new PropertyDescriptor[defaultProperties.Count];
			for (int i = 0; i < props.Length; i++)
				props[i] = new EasyBuilderPropertyDescriptor(defaultProperties[i]);

			PropertyDescriptorCollection properties = new PropertyDescriptorCollection(props);
			for (int i = properties.Count - 1; i >= 0; i--)
			{
				PropertyDescriptor descriptor = properties[i];

				// instance readonly 
				EasyBuilderPropertyDescriptor tbDescriptor = descriptor as EasyBuilderPropertyDescriptor;

				if (tbDescriptor != null && this.ContainerComponent != null)
				{
					tbDescriptor.IsReadOnlyForContext = String.IsNullOrWhiteSpace(this.ContainerComponent.Owner);
				}
			}

			return properties;
		}
	}

    //=========================================================================
	internal class MenuItemTitleConverter : ExpandableObjectConverter
    {
        //---------------------------------------------------------------------
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MenuItemTitle))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        //---------------------------------------------------------------------
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is MenuItemTitle)
            {

                MenuItemTitle menuItem = (MenuItemTitle)value;

                return menuItem.Text;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        //---------------------------------------------------------------------
        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        //---------------------------------------------------------------------
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {

            if (value is string)
            {
                string s = (string)value;

                int comma = s.IndexOf(',');
                if (comma == -1)
                    return new MenuItemTitle(s, true);
				else
                {
                    string titleText = (comma > 0) ? s.Substring(0, comma) : String.Empty;
                    string localizableText = (comma < (s.Length - 1)) ? s.Substring(comma + 1).Trim() : String.Empty;

                    bool isLocalizable = true;
                    if (!String.IsNullOrEmpty(localizableText))
                    {
                        try
                        {
                            isLocalizable = Boolean.Parse(localizableText);
                        }
                        catch
                        {}
                    }

                    return new MenuItemTitle(titleText, isLocalizable);
                }

            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    //====================================================================
	internal class MenuItemPropertyValueChangedEventArgs : EventArgs
    {
		private object changedItem = null;
        private PropertyInfo property = null;
        private object previousValue = null;

        //---------------------------------------------------------------------
        public MenuItemPropertyValueChangedEventArgs(
			object aChangedItem,
			PropertyInfo aPropertyInfo,
            object aValue
			)
        {
			changedItem = aChangedItem;
            property = aPropertyInfo;
            
            if (aValue == null || property.PropertyType == aValue.GetType())
                previousValue = aValue;
        }

        //---------------------------------------------------------------------
        public object ChangedItem { get { return changedItem;  } }
        //---------------------------------------------------------------------
        public PropertyInfo Property { get { return property; } }
        //---------------------------------------------------------------------
        public object PreviousValue { get { return previousValue; } }
    }

    //====================================================================
	internal class MenuItemEventArgs : EventArgs
    {
        private object item = null;
        private object parentItem = null;
        private object beforeItem = null;
        private object previousParentItem = null;
        private object previousBeforeItem = null;
    
        //-------------------------------------------------------------------------------------------------------
        public MenuItemEventArgs 
            (
            object aMenuItem,
            object aParentMenuItem,
            object aBeforeMenuItem,
            object aPreviousParentMenuItem,
            object aPreviousBeforeMenuItem
            )
        {
            item = aMenuItem;
            parentItem = aParentMenuItem;
            beforeItem = aBeforeMenuItem;
            previousParentItem = aPreviousParentMenuItem;
            previousBeforeItem = aPreviousBeforeMenuItem;
        }

        //-------------------------------------------------------------------------------------------------------
        public MenuItemEventArgs
            (
            object aMenuItem,
            object aParentMenuItem,
            object aBeforeMenuItem
            )
            : this (aMenuItem, aParentMenuItem, aBeforeMenuItem, null, null)
        {
        }

        //-------------------------------------------------------------------------------------------------------
        public object MenuItem { get { return item; } }
        //-------------------------------------------------------------------------------------------------------
        public object ParentItem { get { return parentItem; } }
        //-------------------------------------------------------------------------------------------------------
        public object BeforeItem { get { return beforeItem; } }
        //-------------------------------------------------------------------------------------------------------
        public object PreviousParentItem { get { return previousParentItem; } }
        //-------------------------------------------------------------------------------------------------------
        public object PreviousBeforeItem { get { return previousBeforeItem; } }
    }
}
