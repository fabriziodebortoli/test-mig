using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
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
        public event CancelEventHandler AttachingDataModel;
        public event EventHandler DataModelAttached;
        public event CancelEventHandler DetachingDataModel;
        public event EventHandler DataModelDetached;
        public event CancelEventHandler DataLoading;
        public event EventHandler DataLoaded;
        public event CancelEventHandler Validating;
        public event EventHandler Validated;
        public event CancelEventHandler Saving;
        public event EventHandler Saved;
        public event CancelEventHandler Newing;
        public event EventHandler Newed;
        public event CancelEventHandler Editing;
        public event EventHandler Edited;

        #endregion

        //-----------------------------------------------------------------------------------------------------
        public IOrchestrator Orchestrator
        {
            get
            {
                return orchestrator;
            }
        }

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        public INameSpace NameSpace
        {
            get
            {
                var nameSpaceAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(NameSpaceAttribute), true).FirstOrDefault() as NameSpaceAttribute;
                return nameSpaceAttribute == null ? null : nameSpaceAttribute.NameSpace;
            }
        }

        //-----------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------
        protected Document()
        {
            Clear();
        }

        //-----------------------------------------------------------------------------------------------------
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
        //-----------------------------------------------------------------------------------------------------
        public bool Initialize(IOrchestrator orchestrator, CallerContext callerContext)
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
            this.orchestrator = orchestrator;

            if (!OnInitialize())
                return false;

            Initialized?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnInitialize()
        {
            return AttachDataModel();
        }


        /// <summary>
        /// It attaches and initialize data model to document
        /// </summary>
        /// <returns></returns>
        //-----------------------------------------------------------------------------------------------------
        public bool AttachDataModel ()
        {
            if (AttachingDataModel != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                AttachingDataModel(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnAttachDataModel())
                return false;

            DataModelAttached?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnAttachDataModel()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool DetachDataModel()
        {
            if (DetachingDataModel != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                DetachingDataModel(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnDetachDataModel())
                return false;

            DataModelDetached?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnDetachDataModel()
        {
            return true;
        }


        //-----------------------------------------------------------------------------------------------------
        public bool New()
        {
            if (Newing != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Newing(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnNew())
                return false;

            Newed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnNew()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool Edit()
        {
            if (Editing != null)
            {

                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Editing(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnEdit())
                return false;

            Edited?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnEdit()
        {
            return true;
        }


        //-----------------------------------------------------------------------------------------------------
        public bool Validate()
        {
            if (Validating != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Validating(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }
            if (!OnValidate())
                return false;

            Validated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnValidate()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool Save()
        {
            if (Saving != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Saving(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnSave())
                return false;

            Saved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnSave()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool LoadData()
        {
            if (DataLoading != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                DataLoading(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnLoadData())
                return false;

            DataLoaded?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnLoadData()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // cose di chiusura
                Clear();
            }
        }

        //-----------------------------------------------------------------------------------------------------
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
