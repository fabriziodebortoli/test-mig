using System;
using TaskBuilderNetCore.Model.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DBTObject : DocumentComponent,  IDataModel
    {
        private object record;
        private object oldRecord;

        //---------------------------------------------------------------------
        protected object Record { get => record; set => record = value; }
        //---------------------------------------------------------------------
        protected object OldRecord { get => oldRecord; set => oldRecord = value; }

        #region IDataModel
        //---------------------------------------------------------------------
        public bool LoadData() => FindData();
        
        //---------------------------------------------------------------------
        public bool SaveData() => Update();

        //---------------------------------------------------------------------
        public bool DeleteData() => Delete();

        //---------------------------------------------------------------------
        public bool NewData() => AddNew();

        //---------------------------------------------------------------------
        public bool EditData() => Edit();

        #endregion

        //---------------------------------------------------------------------
        public DBTObject(Type recordType)
        {
            Record = Activator.CreateInstance(recordType);
            OldRecord = Activator.CreateInstance(recordType);
        }

        //---------------------------------------------------------------------
        public object GetRecord()
        {
            return Record;
        }

        //---------------------------------------------------------------------
        public object GetOldRecord()
        {
            return OldRecord;
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
        public bool FindData()
        {
            return true;
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
        //---------------------------------------------------------------------
        public bool AddNew()
        {
            return true;
        }

        //---------------------------------------------------------------------
        public bool Edit()
        {
            return true;
        }

        //---------------------------------------------------------------------
        public bool ClearData()
        {
            return true;
        }
    }

}
