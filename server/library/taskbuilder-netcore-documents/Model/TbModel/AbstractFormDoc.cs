using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using TaskBuilderNetCore.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model.TbModel
{
    public enum FormModeType { None, Browse, New, Edit, Find };
    //====================================================================================    
    public class AbstractFormDoc : Document
    {
        public FormModeType FormMode
        {
            get { return (FormModeType) DocumentState.State; }
            set  { DocumentState.State = (int) value; }
        }

        #region events declarations

        public event CancelEventHandler NewingData;
        public event EventHandler DataNewed;
        public event CancelEventHandler EditingData;
        public event EventHandler DataEdited;
        public event CancelEventHandler Finding;
        public event EventHandler Finded;

        #endregion

        //-----------------------------------------------------------------------------------------------------
        public AbstractFormDoc()
            :
            base()
        {

        }

        //-----------------------------------------------------------------------------------------------------
        public string GetTitle()
        {
            return Title;
        }

        //-----------------------------------------------------------------------------------------------------
        public void SetTitle(string title)
        {
            Title = title;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnInitialize()
        {
            ComponentsLoaded += AbstractFormDoc_ComponentsLoaded;
            DocumentState.StateChanging += DocumentState_StateChanging;
            DocumentState.StateChanged += DocumentState_StateChanged;

            return base.OnInitialize() && OnOpenDocument();
        }

        //-----------------------------------------------------------------------------------------------------
        private void DocumentState_StateChanging(object sender, EventArgs e)
        {
        }

        //-----------------------------------------------------------------------------------------------------
        private void DocumentState_StateChanged(object sender, StateChangedEventArgs e)
        {
            switch (FormMode)
            {
                case FormModeType.New:
                    NewData();
                    break;
                case FormModeType.Edit:
                    EditData();
                    break;
                case FormModeType.Find:
                    Find();
                    break;
                default:
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        private void AbstractFormDoc_ComponentsLoaded(object sender, EventArgs e)
        {
            OnInitDocument();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnOpenDocument()
        {
            return true;
        }

        
        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnInitDocument()
        {
            return AttachDataModel();
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnAttachDataModel()
        {
            return OnAttachData();
        }

        //-----------------------------------------------------------------------------------------------------
        protected bool Attach(DBTMaster dbtMaster)
        {
            Components.Add(dbtMaster);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnAttachData()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnDetachDataModel()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool BrowseRecord()
        {
            return LoadData();
        }
        
        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnLoadData()
        {
            return OnPrepareAuxData();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnPrepareAuxData()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed void OnClearData()
        {
            OnInitAuxData();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnInitAuxData()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnValidateData()
        {
            switch (FormMode)
            {
                case FormModeType.New:
                case FormModeType.Edit:
                    return OnOkTransaction();
                case FormModeType.Browse:
                    return OnOkDelete();
                default:
                    break;
            }
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnOkTransaction()
        {
            return true;
        }
        
        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnOkDelete()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool NewData()
        {
            if (NewingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                NewingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.NewData();
                }

            DataNewed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool EditData()
        {
            if (EditingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                EditingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            if (Components != null)
                foreach (IDocumentComponent component in Components)
                {
                    IDataModel dataModel = component as IDataModel;
                    if (dataModel != null)
                        dataModel.EditData();
                }

            DataEdited?.Invoke(this, EventArgs.Empty);

            return true;
        }

         //-----------------------------------------------------------------------------------------------------
        public bool Find()
        {
            if (Finding != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                Finding(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            FormMode = FormModeType.Find;

            if (!FindRecord())
                return false;

            Finded?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool FindRecord()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnSaveData()
        {
            return OnSaveDocument();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnSaveDocument()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override bool OnDeleteData()
        {
            return true;
        }

        // ce la mettiamo una codifica della diagnostica adesso? 
        //-----------------------------------------------------------------------------------------------------
        public void AddError(string text)
        {
            Diagnostic.SetError(text);
        }

        //-----------------------------------------------------------------------------------------------------
        public void AddWarning(string text)
        {
            Diagnostic.SetWarning(text);
        }

        //-----------------------------------------------------------------------------------------------------
        public void AddHint(string text)
        {
            Diagnostic.SetInformation(text);
        }

        //-----------------------------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            ComponentsLoaded -= AbstractFormDoc_ComponentsLoaded;
            DocumentState.StateChanging -= DocumentState_StateChanging;
            DocumentState.StateChanged -= DocumentState_StateChanged;
            base.Dispose(disposing);
        }
    }
}
