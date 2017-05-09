using Microarea.Common.DiagnosticManager;
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
        IOrchestrator orchestrator;
        List<IExtension> extensions;
        ICallerContext callerContext;
        IValidator validator;
        IDiagnostic diagnostic;
        string title;

        #region events declarations

        public event CancelEventHandler Initializing;
        public event EventHandler Initialized;
        public event CancelEventHandler AttachingDataModel;
        public event EventHandler DataModelAttached;
        public event CancelEventHandler DetachingDataModel;
        public event EventHandler DataModelDetached;
        public event CancelEventHandler DataClearing;
        public event EventHandler DataCleared;
        public event CancelEventHandler DataLoading;
        public event EventHandler DataLoaded;
        public event CancelEventHandler ValidatingData;
        public event EventHandler DataValidated;
        public event CancelEventHandler SavingData;
        public event EventHandler DataSaved;
        public event CancelEventHandler DeletingData;
        public event EventHandler DataDeleted;

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
        public IValidator Validator
        {
            get
            {
                return validator;
            }

            set
            {
                validator = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public IDiagnostic Diagnostic
        {
            get
            {
                return diagnostic;
            }

            set
            {
                diagnostic = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public ICallerContext CallerContext
        {
            get
            {
                return callerContext;
            }

            set
            {
                callerContext = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
            }
        }


        //-----------------------------------------------------------------------------------------------------
        protected Document()
        {
            diagnostic = new Diagnostic(NameSpace.FullNameSpace);
            Clear();
        }

        //-----------------------------------------------------------------------------------------------------
        public void Clear()
        {
            this.callerContext = null;
 
            diagnostic.Clear();

            ClearData();
        }

        /// <summary>
        /// Initialize document 
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <param name="callerContext"></param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------------------------------
        public bool Initialize(IOrchestrator orchestrator, ICallerContext callerContext)
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
        protected abstract bool OnInitialize();

        /// <summary>
        /// It attaches and initialize data model to document
        /// </summary>
        /// <returns>If data model has been attached</returns>
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
            
            ClearData();

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnAttachDataModel();

        /// <summary>
        /// It detaches data model from document
        /// </summary>
        /// <returns>If data model has been detached</returns>
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
        protected abstract bool OnDetachDataModel();

        /// <summary>
        /// It invokes data loding 
        /// </summary>
        /// <returns>If data model has been loaded</returns>
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

            // TODO invocazione del metodo di load del data model

            if (!OnLoadData())
                return false;

            DataLoaded?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnLoadData();

        /// <summary>
        /// It clears data 
        /// </summary>
        //-----------------------------------------------------------------------------------------------------
        public void ClearData()
        {
            if (DataClearing != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                DataClearing(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return;
            }

            // TODO invocazione del metodo di clear del data model

            OnClearData();

            DataCleared?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract void OnClearData();

        /// <summary>
        /// It validates data
        /// </summary>
        /// <returns>If data are valid</returns>
        //-----------------------------------------------------------------------------------------------------
        public bool ValidateData()
        {
            if (ValidatingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                ValidatingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (Validator != null)
                Validator.Validate(this);

            if (!OnValidateData())
                return false;

            DataValidated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnValidateData();

        //-----------------------------------------------------------------------------------------------------
        public bool SaveData()
        {
            // validation default called on data saving
            if (Validator == null || Validator.UsedValidationType == ValidationType.SavingData)
            {
                if (!ValidateData())
                    return false;
            }

            if (SavingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                SavingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            // TODO invocazione del save del data model
            if (!OnSaveData())
                return false;

            DataSaved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnSaveData();

        //-----------------------------------------------------------------------------------------------------
        public bool DeleteData()
        {
            if (DeletingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                DeletingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            // TODO invocazine della delete del data model
            if (!OnDeleteData())
                return false;

            DataDeleted?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected abstract bool OnDeleteData();

        //-----------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                DetachDataModel();
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
