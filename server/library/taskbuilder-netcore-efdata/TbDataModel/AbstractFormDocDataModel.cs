using System;
using Microsoft.EntityFrameworkCore;
using TaskBuilderNetCore.Model.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Data.EntityFramework.Interfaces;

namespace TaskBuilderNetCore.Data.EntityFramework
{

    //====================================================================================    
    public class AbstractFormDocDataModel : DocumentComponent, IDataModel
    {
        //-----------------------------------------------------------------------------------------------------
        private DbContext _dbContext;
        private object currentEntity;
        private Type entityType;

        //-----------------------------------------------------------------------------------------------------
        public DbContext DbContext
        {
            get
            {
                return _dbContext;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public object CurrentEntity
        {
            get
            {
                return currentEntity;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public AbstractFormDocDataModel(Type factoryType, Type entityType)
        {
            this.entityType = entityType;

            IDbContextFactory factory = Activator.CreateInstance(factoryType) as IDbContextFactory;
            _dbContext = factory?.CreateContext() as DbContext;
            currentEntity = Activator.CreateInstance(entityType);
        }

        //-----------------------------------------------------------------------------------------------------
        public bool NewData()
        {
            // decisione della chiave 
            try
            {
                using (var db = DbContext)
                {
                    currentEntity = Activator.CreateInstance(entityType);
                    db.Add(currentEntity);
                }
            }
            catch (Exception ex)
            {
                throw new EFException("EntityFramework data model: error newing data",  ex);
            }
                
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool EditData()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new EFException("EntityFramework data model: error editing data", ex);

            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool ClearData()
        {
            currentEntity = Activator.CreateInstance(entityType);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool DeleteData()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new EFException("EntityFramework data model: error deleting data", ex);
            }
            //TODO
            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool SaveData()
        {
            try
            {
                using (var db = DbContext)
                {
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new EFException("EntityFramework data model: error saving data", ex);
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------
        public bool LoadData()
        {
            return true;    
        }
    }
}
