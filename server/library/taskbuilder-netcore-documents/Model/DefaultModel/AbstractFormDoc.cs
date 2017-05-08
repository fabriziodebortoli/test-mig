using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    public abstract class AbstractFormDoc : Document
    {
        public enum FormModeType { None, Browse, New, Edit, Find };

        public FormModeType FormMode { get { return (FormModeType) Mode; } set { Mode = (DocumentMode) value; } }
        //-----------------------------------------------------------------------------------------------------
        protected AbstractFormDoc()
        {

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
        protected override sealed bool OnValidate()
        {
            return OnOkTransaction();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnOkTransaction()
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
        protected override sealed bool OnNew()
        {
            return CanDoNewRecord() && NewRecord();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool CanDoNewRecord()
        {
            return true;
        }
        //-----------------------------------------------------------------------------------------------------
        protected virtual bool NewRecord()
        {
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        protected override sealed bool OnEdit()
        {
            return CanDoEditRecord() && EditRecord();
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
        protected override sealed bool OnSave()
        {
            return OnSaveDocument();
        }

        //-----------------------------------------------------------------------------------------------------
        protected virtual bool OnSaveDocument()
        {
            return true;
        }
    }
}
