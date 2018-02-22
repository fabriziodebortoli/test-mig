using System;
using System.ComponentModel;
using TaskBuilderNetCore.Interfaces;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Model.Interfaces;
using System.Collections.ObjectModel;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class Document : Component, IDocument
    {
        ObservableCollection<IDocumentComponent> components;
        ComponentState documentState;
        string title;

        #region events declarations

        public event CancelEventHandler LoadingComponents;
        public event EventHandler ComponentsLoaded;
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
        public ObservableCollection<IDocumentComponent> Components { get => components; set => components = value; }

        //-----------------------------------------------------------------------------------------------------
        public IDiagnostic Diagnostic
        {
            get
            {
                if (CallerContext == null)
                    return null;
                if (CallerContext.Diagnostic == null)
                    CallerContext.Diagnostic = new Microarea.Common.DiagnosticManager.Diagnostic(CallerContext.Identity);

                return CallerContext.Diagnostic;
            }

            set
            {
                CallerContext.Diagnostic = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public string Title { get => title;  set => title = value; }

        //-----------------------------------------------------------------------------------------------------
        public ComponentState DocumentState { get => documentState; set => documentState = value; }

        //-----------------------------------------------------------------------------------------------------
        public IDataModel DataModel { get => Components[0] as IDataModel; }

        //-----------------------------------------------------------------------------------------------------
        public Document()
        {
            Components = new ObservableCollection<IDocumentComponent>();
            DocumentState = new ComponentState(this);
            Clear();
        }

        //-----------------------------------------------------------------------------------------------------
        public override void Clear()
        {
            ClearData();
            if (Diagnostic != null)
                Diagnostic.Clear();

            base.Clear();
        }

        //-----------------------------------------------------------------------------------------------------
        protected override bool OnInitialize()
        {
             return true;
        }

        /// <summary>
        /// It attaches and initialize data model to document
        /// </summary>
        /// <returns>If data model has been attached</returns>
        //-----------------------------------------------------------------------------------------------------
        public bool LoadComponents()
        {
            if (LoadingComponents != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                LoadingComponents(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (!OnLoadingCoponents())
                return false;
            
            // ora inizializzo i compomenti
            foreach (DocumentComponent component in Components)
            {
                component.Document = this;
                component.Initialize(CallerContext, DocumentServices);
            }

            ComponentsLoaded?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnLoadingCoponents()
        {
            return true;
        }

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
        protected virtual bool OnAttachDataModel()
        {
            return true;

        }

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
        protected virtual bool OnDetachDataModel()
        {
            return true;

        }

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

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.LoadData();
                }

            if (!OnLoadData())
                return false;

            DataLoaded?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnLoadData() { return true; }

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

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.ClearData();
                }
            OnClearData();

            DataCleared?.Invoke(this, EventArgs.Empty);
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual void OnClearData() { }

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
            bool validated = true;
            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IValidator validator = component as IValidator;
                    if (validator != null)
                        validated = validated && validator.Validate(this);
                }

            if (!validated || !OnValidateData())
                return false;

            DataValidated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnValidateData() { return true; }

        //-----------------------------------------------------------------------------------------------------
        public bool SaveData()
        {      
            if (!ValidateData())
                return false;

            if (SavingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                SavingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.SaveData();
                }

            if (!OnSaveData())
                return false;

            DataSaved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnSaveData() { return true; }

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

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.DeleteData();
                }

            if (!OnDeleteData())
                return false;

            DataDeleted?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnDeleteData() { return true; }

        //-----------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                DetachDataModel();
            }
        }

    }
}
