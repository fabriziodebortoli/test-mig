using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    public abstract class AbstractFormDoc : Document
    {
        public enum FormModeType { None, Browse, New, Edit, Find };

        public FormModeType FormMode { get { return (FormModeType) Action; } set { Action = (DocumentAction) value; } }
        protected AbstractFormDoc()
        {

        }
        protected override sealed bool OnInitialize()
        {
            return OnInitDocument();
        }

        protected virtual bool OnInitDocument()
        {
            return true;
        }

        protected override sealed bool OnLoadData()
        {
            return OnPrepareAuxData();
        }

        protected virtual bool OnPrepareAuxData()
        {
            return true;
        }
    }
}
