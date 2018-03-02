using Newtonsoft.Json;
using System;
using System.ComponentModel;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Interfaces;
using System.Reflection;
using Microarea.Common.Generic;
using System.Linq;
using System.Collections.Generic;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class Component : Interfaces.IComponent
    {
        ICallerContext callerContext;
        IDocumentServices documentServices;
        Dictionary<object, IDataBag> dataBags;
        bool disposed;

        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        protected bool Disposed { get => disposed; }

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
        public IDocumentServices DocumentServices { get => documentServices; set => documentServices = value; }
        public Dictionary<object, IDataBag> DataBags
        {
            get
            {
                if (dataBags == null)
                    dataBags = new Dictionary<object, IDataBag>();
                return dataBags;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public virtual void Clear()
        {
            callerContext = null;
            DataBags.Clear();
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
        public bool Initialize (ICallerContext callerContext, IDocumentServices documentServices)
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
            this.documentServices = documentServices;
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
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
