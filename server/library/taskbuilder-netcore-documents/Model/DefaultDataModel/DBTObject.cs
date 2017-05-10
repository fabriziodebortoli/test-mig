using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DBTObject
    {
        protected object record;
        protected object oldRecord;

        //---------------------------------------------------------------------
        public DBTObject(Type recordType)
        {
            record = Activator.CreateInstance(recordType);
            oldRecord = Activator.CreateInstance(recordType);
        }

        //---------------------------------------------------------------------
        public object GetRecord()
        {
            return record;
        }

        //---------------------------------------------------------------------
        public object GetOldRecord()
        {
            return oldRecord;
        }

        //---------------------------------------------------------------------
        public virtual bool OnCheckPrimaryKey()
        {
            return true;
        }

        //---------------------------------------------------------------------
        public virtual bool OnPreparePrimaryKey()
        {
            return true;
        }

        //---------------------------------------------------------------------
        public virtual void OnDefineQuery()
        {
        } 

        //---------------------------------------------------------------------
        public virtual void OnPrepareQuery()
        {
        }

        //---------------------------------------------------------------------
        public void FindData()
        {
        }

        //---------------------------------------------------------------------
        public bool Update()
        {
            return true;
        }

        //---------------------------------------------------------------------
        public bool Delete()
        {
            return true;
        }
    }

}
