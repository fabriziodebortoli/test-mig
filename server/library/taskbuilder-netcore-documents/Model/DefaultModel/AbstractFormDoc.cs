using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class AbstractFormDoc : Document
    {
        public enum FormModeType { None, Browse, New, Edit, Find };

        private FormModeType mode;
        public FormModeType FormMode
        {
            get { return mode; }
            set
            {
                FormModeChanging?.Invoke(this, EventArgs.Empty);
                mode = value;
                FormModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #region events declarations

        public event CancelEventHandler NewingData;
        public event EventHandler DataNewed;
        public event CancelEventHandler EditingData;
        public event EventHandler DataEdited;
        public event CancelEventHandler Finding;
        public event EventHandler Finded;
        public event EventHandler FormModeChanging;
        public event EventHandler FormModeChanged;

        #endregion

        //-----------------------------------------------------------------------------------------------------
        protected AbstractFormDoc()
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
            return OnOpenDocument() && OnInitDocument();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnOpenDocument()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnInitDocument()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnAttachDataModel()
        {
            return OnAttachData();
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
            if (!CanDoNewRecord())
                return false;

            if (NewingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                NewingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            FormMode = FormModeType.New;

            if (!NewRecord())
                return false;

            DataNewed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool CanDoNewRecord()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool NewRecord()
        {
            ClearData();
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool EditData()
        {
            if (!CanDoEditRecord())
                return false;

            if (EditingData != null)
            {
                CancelEventArgs cancelEventArgs = new CancelEventArgs();
                EditingData(this, cancelEventArgs);

                if (cancelEventArgs.Cancel)
                    return false;
            }

            FormMode = FormModeType.Edit;

            if (!EditRecord())
                return false;

            DataEdited?.Invoke(this, EventArgs.Empty);

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool CanDoEditRecord()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool EditRecord()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool Find()
        {
            if (!CanDoFindRecord())
                return false;

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
        protected virtual bool CanDoFindRecord()
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
    }
}
