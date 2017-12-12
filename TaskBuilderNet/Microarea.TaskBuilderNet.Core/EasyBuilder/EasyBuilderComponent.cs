using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using EBEventInfo = Microarea.TaskBuilderNet.Core.EasyBuilder.EventInfo;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    public enum EDesignMode
		{
			None,  /*non sono in design mode*/ 
			Runtime, /*Easy Studio*/ 
			Static /*JSON designer*/

		};
    [PropertyTabAttribute(typeof(System.Windows.Forms.Design.EventsTab), PropertyTabScope.Component)]
    //=======================================================================
    public class EasyBuilderComponent : EasyBuilderTypeDescriptor, IComponent, IChangedEventsSource
    {
        //E` statico di thread perche` ogni documento abbia la sua cache
        [ThreadStatic]
        private static List<EBEventInfo> eventInfosCache;

        /// <remarks />
		public static IList<EBEventInfo> GetEventInfosFromCache(string methodName)
        {
            if (eventInfosCache == null)
            {
                eventInfosCache = new List<EBEventInfo>();
            }
            List<EBEventInfo> foundEventInfos = new List<EBEventInfo>();
            foreach (var eventInfo in eventInfosCache)
            {
                if (String.Compare(eventInfo.EventHandlerName, methodName, StringComparison.Ordinal) == 0)
                    foundEventInfos.Add(eventInfo);
            }

            return foundEventInfos;
        }

        /// <remarks />
        public static void AddEventInfoToCache(EBEventInfo ei)
        {
            if (eventInfosCache == null)
            {
                eventInfosCache = new List<EBEventInfo>();
            }
            if (ei == null || eventInfosCache.Contains(ei))
                return;

            eventInfosCache.Add(ei);
        }

        private bool isStretchable;
        private bool hasCodeBehind;
        private List<string> changedProperties;
        private List<EventInfo> changedEvents;
        private List<string> referencedBy;
        private ISite site;
        private EasyBuilderComponent parentComponent;
        private DiagnosticSession diagnosticSession;
        private bool usingWrappingSerialization;
        private bool isValidComponent = true;
        private bool showInObjectModel = true;
		protected bool isChanged = false;

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public Diagnostic Diagnostic { get { return DiagnosticSession.CurrentDiagnostic; } }
        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public DiagnosticSession DiagnosticSession
        {
            get
            {
                if (ParentComponent != null)
                    return ParentComponent.DiagnosticSession;
                if (diagnosticSession == null)
                {
                    diagnosticSession = new DiagnosticSession();
                    diagnosticSession.StartSession(Name);
                }
                return diagnosticSession;
            }
        }

        //-----------------------------------------------------------------------------
        [Browsable(false), ExcludeFromIntellisense]
        public virtual IComponent EventSourceComponent
		{
			get { return this; }
		}

		//-----------------------------------------------------------------------------
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsChanged { get { return isChanged; } set { isChanged = value; } }
 
		//-----------------------------------------------------------------------------
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string Name { get; set; }

        //-----------------------------------------------------------------------------
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool IsReferenceableType { get { return false; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public virtual bool HasCodeBehind { get { return hasCodeBehind; } set { hasCodeBehind = value; } }

        //------------------------------------------------------------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public virtual bool IsStretchable { get { return isStretchable; } set { isStretchable = value; } }

        public Version Version { get { return GetType().Assembly.GetName().Version; }  }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public bool UsingWrappingSerialization 
		{
			get { return usingWrappingSerialization; } 
			set 
			{
				usingWrappingSerialization = value; 
				IEasyBuilderContainer cnt = this as IEasyBuilderContainer;
				if (cnt == null)
					return;

				foreach (IComponent cmp in cnt.Components)
				{
					EasyBuilderComponent ebc = cmp as EasyBuilderComponent;
					if (ebc != null)
						ebc.UsingWrappingSerialization = value;
				}
			}
		}

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public IEnumerable<string> ChangedProperties { get { return changedProperties; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public int ChangedPropertiesCount { get { return (changedProperties == null) ? 0 : changedProperties.Count; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        protected void ClearChangedProperties () { if (changedProperties != null) changedProperties.Clear(); }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public IEnumerable<EventInfo> ChangedEvents { get { return changedEvents; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public int ChangedEventsCount { get { return (changedEvents == null) ? 0 : changedEvents.Count; } }
        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public int ReferencesCount { get { return referencedBy.Count; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public IEnumerable<string> ReferencedBy { get { return referencedBy; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
        virtual public ISite Site { get { return site; } set { site = value; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
        public virtual string SerializedName { get { return SerializedType; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public bool IsValidComponent { get { return isValidComponent; } set { isValidComponent = value; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
        public virtual string SerializedPropertyAccessorName { get { return SerializedType; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
        public virtual string SerializedType { get { return EasyBuilderSerializer.Escape(Name); } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ExcludeFromIntellisense]
        public virtual bool CanBeDeleted { get { return !HasCodeBehind || !IsValidComponent ; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public virtual List<string> InternalClasses { get { return null; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public virtual Int32 TbHandle { get { return 0; } }

        [XmlIgnore, Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        public virtual string ControllerType
        {
            get
            {
                return Document != null ? Document.ControllerType : EasyBuilderSerializer.DocumentControllerClassName;
            }
        }
        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
        public virtual EasyBuilderComponent ParentComponent { get { return parentComponent; } set { parentComponent = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
        [XmlIgnore]
		public virtual bool DesignMode { get { return ParentComponent != null && ParentComponent.DesignModeType != EDesignMode.None; } }
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ExcludeFromIntellisense]
		[XmlIgnore]
		public virtual EDesignMode DesignModeType { get { return ParentComponent == null ? EDesignMode.None : ParentComponent.DesignModeType; } }

        [XmlIgnore, Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IDocumentDataManager Document
        {
            get
            {
                if (this is IDocumentDataManager)
                    return (IDocumentDataManager)this;
                return ParentComponent?.Document;
            }
        }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public virtual bool EmptyComponent { get { return false; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), XmlIgnore, ExcludeFromIntellisense]
        public bool CanShowInObjectModel { get { return showInObjectModel; } set { showInObjectModel = value; } }

        //-----------------------------------------------------------------------------
        [Browsable(false), ExcludeFromIntellisense]
        public event EventHandler Disposed;

        //-----------------------------------------------------------------------------
        public EasyBuilderComponent()
        {
            changedProperties = new List<string>();
            changedEvents = new List<EventInfo>();
            referencedBy = new List<string>();
        }

        //--------------------------------------------------------------------------------
        [ExcludeFromIntellisense, Browsable(false)]
        public virtual bool IsAddonInstanceOfSharedSerializedClass
        {
            get
            {
                return false;
            }
        }

        //--------------------------------------------------------------------------------
        [ExcludeFromIntellisense]
        public static EasyBuilderComponent[] GetComponentsBySerializedName(IContainer rootContainer, string serializedName)
        {
            List<EasyBuilderComponent> list = new List<EasyBuilderComponent>();
            foreach (IComponent cmp in rootContainer.Components)
            {
                EasyBuilderComponent ebc = cmp as EasyBuilderComponent;
                if (ebc == null)
                    continue;

                if (ebc.SerializedName == serializedName)
                    list.Add(ebc);

                IContainer container = cmp as IContainer;
                if (container != null)
                    list.AddRange(GetComponentsBySerializedName(container, serializedName));
            }
            return list.ToArray();
        }

        //--------------------------------------------------------------------------------
        [ExcludeFromIntellisense]
        public static EasyBuilderComponent[] ClearReferences(IContainer rootContainer, string objectName)
        {
            List<EasyBuilderComponent> list = new List<EasyBuilderComponent>();
            foreach (IComponent cmp in rootContainer.Components)
            {
                EasyBuilderComponent ebc = cmp as EasyBuilderComponent;
                if (ebc == null)
                    continue;

                if (ebc.IsReferencedBy(objectName))
                {
                    ebc.RemoveReferencedBy(objectName);
                    list.Add(ebc);
                }
                IContainer container = cmp as IContainer;
                if (container != null)
                    list.AddRange(ClearReferences(container, objectName));
            }
            return list.ToArray();
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool IsToDelete()
		{
			return ChangedEventsCount == 0 && ChangedPropertiesCount == 0 && ReferencesCount == 0 && !IsChanged;
		}


		//-----------------------------------------------------------------------------
		[Browsable(false), ExcludeFromIntellisense]
		public virtual bool AreComponentsLoaded
        {
            get
            {
                return true;
            }
        }

        //-----------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                IDisposable disposable = site as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                    site = null;
                }
            }

            OnDisposed();
        }

        //-----------------------------------------------------------------------------
        protected virtual void OnDisposed()
        {
	        Disposed?.Invoke(this, EventArgs.Empty);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public override string GetComponentName()
        {
            return site?.Name;
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool CanChangeProperty(string propertyName)
        {
            return true;
        }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool CanShowEvent(string eventName, TBEventFilterAttribute filter)
        {
            return true;
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual void AddChangedEvent(EventInfo eventInfo)
        {
            if (!changedEvents.Contains(eventInfo))
                changedEvents.Add(eventInfo);

            AddEventInfoToCache(eventInfo);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual void RemoveChangedEvent(EventInfo eventInfo)
        {
            changedEvents.Remove(eventInfo);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool ContainsChangedEvent(EventInfo eventInfo)
        {
            return changedEvents.Contains(eventInfo);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual void AddChangedProperty(string propertyName)
        {
            if (!changedProperties.Contains(propertyName))
                changedProperties.Add(propertyName);
        }

        //-----------------------------------------------------------------------------
        public virtual void RemoveChangedProperty(string propertyName)
        {
            changedProperties.Remove(propertyName);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool ContainsChangedProperty(string propertyName)
        {
            return changedProperties.Contains(propertyName);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public void AddReferencedBy(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return;
            if (!referencedBy.Contains(objectName))
                referencedBy.Add(objectName);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual void RemoveReferencedBy(string objectName)
        {
            referencedBy.Remove(objectName);
        }

		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public virtual bool IsReferencedBy(string objectName)
        {
            return referencedBy.Contains(objectName);
        }
		
		//-----------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public string GetReferencedByList()
        {
            if (referencedBy.Count == 0)
                return string.Empty;

            StringBuilder list = new StringBuilder();
            foreach (string componentName in referencedBy)
                list.Append(componentName).Append(Environment.NewLine);

            return list.ToString();
        }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public bool IsValidName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return DesignModeType == EDesignMode.Static;

            if (!Char.IsLetter(name[0]))
                return false;

            foreach (Char ch in name)
                if (!Char.IsLetterOrDigit(ch) && ch != '_')
                    return false;

            return true;
        }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public static bool HasComponent(ComponentCollection components, string controlName)
        {
            return  HasComponent(components, controlName, false);
        }

		[ExcludeFromIntellisense]
		//--------------------------------------------------------------------------------
		public static bool HasComponent(ComponentCollection components, string controlName, bool recursive)
        {
            return GetComponent(components, controlName, recursive) != null;
        }

		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public static IComponent GetComponent(ComponentCollection components, string controlName)
        {
            return GetComponent(components, controlName, false);
        }
		
		//--------------------------------------------------------------------------------
		[ExcludeFromIntellisense]
		public static IComponent GetComponent(ComponentCollection components, string controlName, bool recursive)
        {
            foreach (IComponent current in components)
            {
                EasyBuilderComponent ebc = current as EasyBuilderComponent;
                if (ebc == null)
                    continue;
                if (controlName.CompareNoCase(ebc.Name))
                    return ebc;

                if (recursive)
                {
                    IContainer cnt = current as IContainer;
                    if (cnt != null)
                    {
                        IComponent childCmp = GetComponent(cnt.Components, controlName, true);
                        if (childCmp != null)
                            return childCmp;
                    }
                }
            }
            return null;
        }
	}
}
