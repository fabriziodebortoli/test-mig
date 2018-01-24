using Newtonsoft.Json;
using System;
using System.ComponentModel;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;
using System.Reflection;
using Microarea.Common.Generic;
using System.Linq;

namespace TaskBuilderNetCore.Documents.Model
{
    public class Component : Interfaces.IComponent
    {
        ICallerContext callerContext;
        IActivatorService activatorService;
        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public INameSpace NameSpace
        {
            get
            {
                if (CallerContext != null)
                    return CallerContext.NameSpace;

                var nameSpaceAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(NameSpaceAttribute), true).FirstOrDefault() as NameSpaceAttribute;
                return nameSpaceAttribute == null ? null : new NameSpace(nameSpaceAttribute.Namespace);
            }
        }

        public event CancelEventHandler Initializing;
        public event EventHandler Initialized;

        //-----------------------------------------------------------------------------------------------------
        public ICallerContext CallerContext { get => callerContext; set => callerContext = value; }
        public IActivatorService ActivatorService { get => activatorService; set => activatorService = value; }

        //-----------------------------------------------------------------------------------------------------
        public virtual void Clear()
        {
            callerContext = null;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool CanBeLoaded(ICallerContext callerContext)
        {
            return true;
        }

        /// <summary>
        /// Initialize component 
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <param name="callerContext"></param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------------------------------
        public bool Initialize (ICallerContext callerContext)
        {
            // it detach previous objects
            Clear();

            if (Initializing != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Initializing(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            this.callerContext = callerContext;
  
            if (!OnInitialize())
                return false;

            Initialized?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnInitialize()
        {
             return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public virtual void Dispose()
        {
        }

     
    }
}
