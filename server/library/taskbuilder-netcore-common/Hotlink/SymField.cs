using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;
using Microarea.Common.ExpressionManager;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;

namespace Microarea.Common.Hotlink
{
    public enum DataLevel { Rules, GroupBy, Events }

    public class SymField : Variable, IDisposable
    {
        protected  bool ruleDataFetched = false;
        protected int len = 0;              // they need for eventual store on
        public string Title = string.Empty;

        public SymField(string name) : base(name)
        {
        }

        public SymField(string dataType, string name, ushort enumTag = 0) 
            : 
            base(name) 
        {
            EnumTag = enumTag;
            this.data = ObjectHelper.CreateObject(dataType);
        }

        //----------------------------------------------------------------------------
        public int Len { get { return len; } set { len = value; } }

        //----------------------------------------------------------------------------
        virtual public void Dispose()
        {
        }

        //----------------------------------------------------------------------------
        virtual public void Assign(string svalue)
        {
            object o = this.Data;
            ObjectHelper.Assign(ref o, svalue);
            Data = o;
        }

        //----------------------------------------------------------------------------
        virtual public object GetData(DataLevel l)
        {
            return this.data;
        }

        virtual public void SetData(DataLevel l, object value)
        {
            if (DataType == "Boolean")
                value = ObjectHelper.CastBool(value);

            this.data = value;
         }

        //----------------------------------------------------------------------------
        public override object Data
        {
            get
            {
                return this.data;
            }
            set
            {
                if (DataType == "Boolean")
                    value = ObjectHelper.CastBool(value);

                //An.17985 
                if (WoormType == "Date" && value is DateTime)
                    value = ((DateTime)value).Date;

               this.data = value;
            }
        }

        //----------------------------------------------------------------------------
        public override bool Valid
        {
            get
            {
                return this.valid;
            }
            set
            {
                this.valid = value;
            }
        }

        public bool ValidRuleData { get { return this.valid; } set { this.valid = value; } }
        public void ClearRuleData() { ObjectHelper.Clear(ref this.data); this.valid = true; }
        public bool RuleDataFetched { get { return ruleDataFetched; } set { ruleDataFetched = value; } }

        //----------------------------------------------------------------------------
        virtual public void ClearAllData()
        {
            ClearRuleData();

            RuleDataFetched = true;
        }

    }

    //=============================================================================
    public class FieldException : Exception
    {
        public FieldException(string message) : base(message) { }
        public FieldException(string message, Exception inner) : base(message, inner) { }
    }
}
