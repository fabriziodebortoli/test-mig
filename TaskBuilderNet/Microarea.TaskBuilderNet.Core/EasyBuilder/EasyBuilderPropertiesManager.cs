using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    //=============================================================================
    public enum TBPropertyFilters
    {
        None = 0,
        IsCodeBehindInvolved = 1,
        StringTypes = 2,
        NumericTypes = 4,
        RealTypes = 8,
        EnumTypes = 16,
        BoolTypes = 32,
        AllTypes = StringTypes | NumericTypes | RealTypes | EnumTypes | BoolTypes,
        ComponentState = 64,
		DesignerStatic = 128,
		DesignerRuntime = 256,
        Stretchable = 512
    };

    //=============================================================================
    public enum EasyBuilderPropertiesOrderBy
    {
        None = 0,
        AlphabeticalOrder
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class TBPropertyFilterAttribute : Attribute
    {
        private int filter = 0;

        //--------------------------------------------------------------------------------
        public int Filter { get { return filter; } set { filter = value; } }
        public bool HasCodeBehindInvolved { get { return (filter & (int)TBPropertyFilters.IsCodeBehindInvolved) == (int)TBPropertyFilters.IsCodeBehindInvolved; } }
        public bool HasStringTypes { get { return (filter & (int)TBPropertyFilters.StringTypes) == (int)TBPropertyFilters.StringTypes; } }
        public bool HasNumericTypes { get { return (filter & (int)TBPropertyFilters.NumericTypes) == (int)TBPropertyFilters.NumericTypes; } }
        public bool HasRealTypes { get { return (filter & (int)TBPropertyFilters.RealTypes) == (int)TBPropertyFilters.RealTypes; } }
        public bool HasEnumTypes { get { return (filter & (int)TBPropertyFilters.EnumTypes) == (int)TBPropertyFilters.EnumTypes; } }
        public bool HasBoolTypes { get { return (filter & (int)TBPropertyFilters.BoolTypes) == (int)TBPropertyFilters.BoolTypes; } }
        public bool ComponentStateDependent { get { return (filter & (int)TBPropertyFilters.ComponentState) == (int)TBPropertyFilters.ComponentState; } }
		public bool DesignerStatic { get { return (filter & (int)TBPropertyFilters.DesignerStatic) == (int)TBPropertyFilters.DesignerStatic; } }
		public bool DesignerRuntime { get { return (filter & (int)TBPropertyFilters.DesignerRuntime) == (int)TBPropertyFilters.DesignerRuntime; } }
        public bool IsStretchable { get { return (filter & (int)TBPropertyFilters.Stretchable) == (int)TBPropertyFilters.Stretchable; } }

        //--------------------------------------------------------------------------------
        public TBPropertyFilterAttribute(int filter)
        {
            this.filter = filter;
        }

        //--------------------------------------------------------------------------------
        public TBPropertyFilterAttribute(TBPropertyFilters filter)
        {
            this.filter = (int)filter;
        }
    };

     //=======================================================================
    internal class InstancePropertyChange
    {
        private bool readOnly = false;
        private bool browsable = true;

        //-----------------------------------------------------------------------------
        internal bool ReadOnly { get { return readOnly; } set { readOnly = value; } }
        internal bool Browsable { get { return browsable; } set { browsable = value; } }
        //-----------------------------------------------------------------------------
        internal InstancePropertyChange()
        {
        }
    }

    //=======================================================================
    public class EasyBuilderPropertiesManager
    {
        readonly Type enumTagType = typeof(Applications.EnumTag);
        readonly Type enumItemType = typeof(Applications.EnumItem);

        //-----------------------------------------------------------------------------
        private ICustomTypeDescriptor component;
        private EasyBuilderPropertiesOrderBy orderBy;
        
        internal Dictionary<string, InstancePropertyChange> instancePropertyChanges = new Dictionary<string, InstancePropertyChange>();
        
        public EasyBuilderPropertiesOrderBy OrderBy { get { return orderBy; } set { orderBy = value; } }
 
        //-----------------------------------------------------------------------------
        public EasyBuilderPropertiesManager(ICustomTypeDescriptor component)
        {
            this.component = component;
        }

        //-----------------------------------------------------------------------------
        internal virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return GetPropertiesInternal(attributes, true);
		}

		//-----------------------------------------------------------------------------
		private PropertyDescriptorCollection GetPropertiesInternal(Attribute[] attributes, bool removeExtenderAccessor)
		{
			// crea una nuova collezione modificabile
			PropertyDescriptorCollection properties = CreateNewCollection(attributes);

			// inizializza il filtro x data Type se è gestito
			IEasyBuilderDataTypeProperties dataTypeProps = component as IEasyBuilderDataTypeProperties;
			TBPropertyFilters dataTypeFilter = dataTypeProps == null ? TBPropertyFilters.None : GetDataTypeFilter(dataTypeProps.FilteredDataType);

			for (int i = properties.Count - 1; i >= 0; i--)
			{
				PropertyDescriptor descriptor = properties[i];

				if (descriptor.Name == "Handle")
				{
					properties.RemoveAt(i);
					continue;
				}

				if (removeExtenderAccessor && component is IEasyBuilderComponentExtender && descriptor.Name == ((IEasyBuilderComponentExtender)component).AccessorPropertyName)
				{
					properties.RemoveAt(i);
					continue;
				}

				TBPropertyFilterAttribute tbAttribute = descriptor.Attributes[typeof(TBPropertyFilterAttribute)] as TBPropertyFilterAttribute;
				if (tbAttribute != null &&
					(IsRemovedByDataTypeFilter(dataTypeFilter, tbAttribute.Filter) || IsRemovedByDesignerState(tbAttribute)))
				{
					properties.RemoveAt(i);
					continue;
				}

				// instance browsing
				InstancePropertyChange change = instancePropertyChanges.ContainsKey(descriptor.Name) ? instancePropertyChanges[descriptor.Name] : null;
				if (change != null && !change.Browsable)
				{
					properties.RemoveAt(i);
					continue;
				}

                if (tbAttribute != null && tbAttribute.IsStretchable && !((EasyBuilderComponent)component).IsStretchable)
                {
                    properties.RemoveAt(i);
                    continue;
                }

                // instance readonly 
                EasyBuilderPropertyDescriptor tbDescriptor = descriptor as EasyBuilderPropertyDescriptor;

                if (tbDescriptor != null)
					tbDescriptor.IsReadOnlyForContext = (change != null && change.ReadOnly) ||
													   (
														   component is EasyBuilderComponent &&
														   tbAttribute != null &&
														   (
															   (tbAttribute.HasCodeBehindInvolved && ((EasyBuilderComponent)component).HasCodeBehind) ||
															   (tbAttribute.ComponentStateDependent && !((EasyBuilderComponent)component).CanChangeProperty(descriptor.Name))
														   )
													   );



			}

			switch (OrderBy)
			{
				case EasyBuilderPropertiesOrderBy.AlphabeticalOrder:
					return properties.Sort();
			}

			return properties;
		}

		//-----------------------------------------------------------------------------
		internal EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            EventDescriptorCollection newEvents = new EventDescriptorCollection(null);

            // devo creare una nuova collection perchè quella che arriva non si lascia aggiungere elementi
            EasyBuilderEventDescriptor easyBuilderEventDescriptor = null;
            foreach (EventDescriptor evnt in TypeDescriptor.GetEvents(component.GetType(), attributes))
            {
                easyBuilderEventDescriptor = new EasyBuilderEventDescriptor(evnt);
                newEvents.Add(easyBuilderEventDescriptor);
            }

			// aggiungo quelle degli extender
			IEasyBuilderComponentExtendable extendable = component as IEasyBuilderComponentExtendable;
			if (extendable != null && extendable.Extensions != null)
			{
				foreach (IEasyBuilderComponentExtender ext in extendable.Extensions)
				{
					EventDescriptorCollection defaultEvents = TypeDescriptor.GetEvents(ext, attributes);

					//nel caso non abbia un nome per accedere all'oggetto,
					//questo non ha una sua individualità come oggetto a sé stante, e le sue proprietà vengono
					//mischiate a quelle dell'oggetto ospitante
					/*if (string.IsNullOrEmpty(ext.AccessorPropertyName))
					{
						foreach (PropertyDescriptor item in defaultProperties)
							newList.Add(new ExtenderPropertyDescriptor(item, ext));
					}
					else*/
					{
						foreach (EventDescriptor childEvnt in defaultEvents)
							newEvents.Add(new EasyBuilderExtenderEventDescriptor(childEvnt, ext.Name));
					}

				}
			}
            return newEvents;
        }
    
        //--------------------------------------------------------------------------------
        public void AddReadOnlyToProperty(string name, bool value = true)
        {
            InstancePropertyChange change = instancePropertyChanges.ContainsKey(name) ?
                                                instancePropertyChanges[name] :
                                                new InstancePropertyChange();

            change.ReadOnly = value;
            instancePropertyChanges[name] = change;
        }

        //--------------------------------------------------------------------------------
        public void RemoveBrowsableToProperty(string name, bool value = false)
        {
            InstancePropertyChange change = instancePropertyChanges.ContainsKey(name) ?
                                                instancePropertyChanges[name] :
                                                new InstancePropertyChange();

            change.Browsable = value;
            instancePropertyChanges[name] = change;
        }

        //-----------------------------------------------------------------------------
        public void SetAllPropertiesReadOnly(bool readOnly)
        {
             foreach (PropertyDescriptor propDesc in GetProperties(null))
                AddReadOnlyToProperty(propDesc.Name, readOnly);
        }

        //-----------------------------------------------------------------------------
        private TBPropertyFilters GetDataTypeFilter(IDataType dataType)
        {
            DataType dt = (DataType) dataType;

            if (dt == DataType.String || dt == DataType.Text || dt == DataType.Guid)
                return TBPropertyFilters.StringTypes;

            if (dt == DataType.Bool)
                return TBPropertyFilters.BoolTypes;

            if (dt == DataType.Integer || dt.Type == DataType.Long.Type || dt.Type == DataType.Date.Type)    // tutti i long e tutti i tipi di data
                return TBPropertyFilters.NumericTypes;
            
            if (dt == DataType.Double || dt == DataType.Money || dt == DataType.Quantity || dt == DataType.Percent)
                return TBPropertyFilters.RealTypes;
            
            if (dt.Type == DataType.Enum.Type)
                return TBPropertyFilters.EnumTypes;

            return TBPropertyFilters.None;
        }

        //-----------------------------------------------------------------------------
        private bool IsRemovedByDataTypeFilter(TBPropertyFilters dataTypeFilter, int attributeFilter)
        {
            // data type filter without code behind
            int currentFilter = attributeFilter & (int)TBPropertyFilters.AllTypes;
            if (currentFilter != (int)TBPropertyFilters.None)
            {
                if (!((currentFilter & (int)dataTypeFilter) == (int)dataTypeFilter))
                    return true;
            }
            return false;
        }

		//-----------------------------------------------------------------------------
		private bool IsRemovedByDesignerState(TBPropertyFilterAttribute tbAttribute)
		{
			EasyBuilderComponent cmp = component as EasyBuilderComponent;
			if (cmp == null)
				return false;
			if (cmp.DesignModeType == EDesignMode.None) //introdotto per il problema di mancanza del parent per PropertyItem(propertyGrid type)
				return false;
			//nel caso in cui su una proprietà non ci siano esplicitati filtri, oppure esplicitati sia runtime che static, non devo escluderli
			if (tbAttribute.DesignerRuntime == tbAttribute.DesignerStatic)
				return false;

			return (tbAttribute.DesignerRuntime && cmp.DesignModeType != EDesignMode.Runtime) ||
			   (tbAttribute.DesignerStatic && cmp.DesignModeType != EDesignMode.Static);
			
		}
        //-----------------------------------------------------------------------------
        private PropertyDescriptorCollection CreateNewCollection (Attribute[] attributes)
        {
            //Uso il TypeDescriptor sul tipo e non sull'istanza perchè usandolo sull'istanza non funziona:
            //infatti viene usato il DictionaryService ottenuto tramite il Site del Component in causa per memorizzare una cache per la collezione delle proprietà.
            //Questa cache è una per ogni istanza, e a seconda del controllo viene ritornata sempre la stessa causando problemi alla PropertyGrid
            //Ogni qualvolta si selezionavano oggetti di tipo diverso (per esempio combo box e tab)
            //Lavorando sui tipi invece viene utilizzata una cache per ogni tipo e non per ogni istanza, cosa che, oltre ad essere più efficiente, non causa
            //malfunzionamenti alla PropertyGrid nel nostro caso.
            PropertyDescriptorCollection defaultProperties;
            if (attributes == null)
                defaultProperties = TypeDescriptor.GetProperties(component.GetType());
            else
                defaultProperties = TypeDescriptor.GetProperties(component.GetType(), attributes);


            IList<PropertyDescriptor> newList = new List<PropertyDescriptor>();
            Type componentType = null;
            foreach (PropertyDescriptor des in defaultProperties)
            {
                componentType = des.ComponentType;
                if (componentType == enumTagType || componentType == enumItemType)
                {
                    newList.Add(new EnumsPropertyDescriptor(des));
                }
                else
                {
                    newList.Add(new EasyBuilderPropertyDescriptor(des));
                }
            }

            // aggiungo quelle degli extender
            IEasyBuilderComponentExtendable extendable = component as IEasyBuilderComponentExtendable;
			if (extendable != null && extendable.Extensions != null)
			{
				PropertyDescriptorCollection extenderDefaultProperties;
				foreach (IEasyBuilderComponentExtender ext in extendable.Extensions)
				{
					if (ext is EasyBuilderTypeDescriptor)
					{
						extenderDefaultProperties = ((EasyBuilderTypeDescriptor)ext).PropertiesManager.GetPropertiesInternal(attributes, false);
					}
					else
					{
						if (attributes == null)
							extenderDefaultProperties = TypeDescriptor.GetProperties(ext);
						else
							extenderDefaultProperties = TypeDescriptor.GetProperties(ext, attributes);
					}
					//nel caso non abbia un nome per accedere all'oggetto,
					//questo non ha una sua individualità come oggetto a sé stante, e le sue proprietà vengono
					//mischiate a quelle dell'oggetto ospitante

					if (string.IsNullOrEmpty(ext.AccessorPropertyName))
					{
						foreach (PropertyDescriptor item in extenderDefaultProperties)
							newList.Add(new ExtenderPropertyDescriptor(item, ext));
					}
					else
					{
						PropertyDescriptor prop = extenderDefaultProperties[ext.AccessorPropertyName];
						if (prop != null)
						{
                            componentType = prop.ComponentType;
                            if (componentType == enumTagType || componentType == enumItemType)
                            {
                                newList.Add(new EnumsPropertyDescriptor(ext.Name, prop));
                            }
                            else
                            {
                                newList.Add(new EasyBuilderPropertyDescriptor(ext.Name, prop));
                            }
						}
					}
					
				}
			}

            return new PropertyDescriptorCollection(newList.ToArray());
        }     
    }
}
