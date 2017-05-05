using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    
    public abstract class Document : IDocument
    {
        public enum DocumentMode { None, DataLoaded, AddNew, Edit, Find };

        IOrchestrator orchestrator;
        List<IExtension> extensions;
        CallerContext callerContext;
        DocumentMode mode;

        #region events declarations

        public event CancelEventHandler Initializing;
        public event EventHandler Initialized;
        public event CancelEventHandler AttachingData;
        public event EventHandler DataAttached;
        public event CancelEventHandler DetachingData;
        public event EventHandler DataDetached;
        public event CancelEventHandler DataLoading;
        public event EventHandler DataLoaded;

        #endregion

        public IOrchestrator Orchestrator
        {
            get
            {
                return orchestrator;
            }
        }

        public List<IExtension> Extensions
        {
            get
            {
                return extensions;
            }

            set
            {
                extensions = value;
            }
        }

        public INameSpace NameSpace
        {
            get
            {
                var nameSpaceAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(NameSpaceAttribute), true).FirstOrDefault() as NameSpaceAttribute;
                return nameSpaceAttribute == null ? null : nameSpaceAttribute.NameSpace;
            }
        }

        public DocumentMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        protected Document()
        {
            Clear();
        }

        public void Clear()
        {
            this.callerContext = null;
            this.orchestrator = null;
            
        }

        /// <summary>
        /// It initialize document 
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <param name="callerContext"></param>
        /// <returns></returns>
        public bool Initialize(IOrchestrator orchestrator, CallerContext callerContext)
        {
            // it detach previous objects
            Clear();

            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            Initializing(this, cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            this.callerContext = callerContext;
            this.orchestrator = orchestrator;

            if (!OnInitialize())
                return false;

            Initialized(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool OnInitialize()
        {
            return AttachData();
        }


        /// <summary>
        /// It attaches and initialize data model to document
        /// </summary>
        /// <returns></returns>
        public bool AttachData ()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            AttachingData(this, cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            if (!OnAttachData())
                return false;

            DataAttached(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool OnAttachData()
        {
            return true;
        }

        public bool DetachData()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            DetachingData(this, cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            if (!OnDetachData())
                return false;

            DataDetached(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool OnDetachData()
        {
            return true;
        }

        public bool LoadData()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs();
            DataLoading(this, cancelEventArgs);

            if (cancelEventArgs.Cancel)
                return false;

            if (!OnLoadData())
                return false;

            DataLoaded(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool OnLoadData()
        {
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // cose di chiusura
                Clear();
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
