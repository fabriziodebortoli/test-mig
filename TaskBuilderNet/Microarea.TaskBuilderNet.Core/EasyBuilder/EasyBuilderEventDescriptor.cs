using System;
using System.ComponentModel;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
    //================================================================================
    public class EasyBuilderEventDescriptor : EventDescriptor, IEventNameGenerator
    {
        private EventDescriptor descriptor = null;
  
        //-----------------------------------------------------------------------------
        public override Type ComponentType { get { return descriptor.ComponentType; } }
        //-----------------------------------------------------------------------------
        public override Type EventType { get { return descriptor.EventType; } }
        //-----------------------------------------------------------------------------
        public override bool IsMulticast { get { return descriptor.IsMulticast; } }

        //-----------------------------------------------------------------------------
        public EasyBuilderEventDescriptor(EventDescriptor descriptor)
            :
            base(descriptor)
        {
        this.descriptor = descriptor;
        }

        //-----------------------------------------------------------------------------
        protected EasyBuilderEventDescriptor(MemberDescriptor descr) : base(descr)
        {
        }

        //-----------------------------------------------------------------------------
        protected EasyBuilderEventDescriptor(MemberDescriptor descr, Attribute[] attrs) 
            : 
            base(descr, attrs) 
        { 
        }

        //-----------------------------------------------------------------------------
        protected EasyBuilderEventDescriptor(string name, Attribute[] attrs) 
            : 
            base(name, attrs)
        { 
        }

        //-----------------------------------------------------------------------------
        public override void AddEventHandler(object component, Delegate value) 
        { 
            descriptor.AddEventHandler(component, value); 
        }

        //-----------------------------------------------------------------------------
        public override void RemoveEventHandler(object component, Delegate value) 
        { 
            descriptor.RemoveEventHandler(component, value); 
        }

        //-----------------------------------------------------------------------------
        public virtual string GenerateEventName(IComponent component)
        {
            string prefix = string.Empty;
            IComponent containerComponent = component.Site.Container as IComponent;
            if (containerComponent != null && containerComponent.Site != null)
                prefix = String.Concat(containerComponent.Site.Name, "_");

            return String.Concat(prefix, component.Site.Name, "_", Name);
        }

        //-----------------------------------------------------------------------------
        virtual public IComponent GetComponent(IComponent component) { return component; }
    };
}
