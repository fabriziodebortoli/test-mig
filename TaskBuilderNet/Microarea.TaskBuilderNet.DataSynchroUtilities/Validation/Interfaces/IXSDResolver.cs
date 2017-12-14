using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces
{
    public interface IBusinessObjectInfo
    {
        string Name { get; set; }
        object Param { get; set; }
    }

    public interface ILocalBusinessObject : IBusinessObjectInfo 
    {
        string LocalPath { get; }
    }

    public class LocalBusinessObjectInfo : ILocalBusinessObject
    {
        public string Name { get; set; }

        public object Param { get; set; }

        public string LocalPath
        {
            get 
            {
                if (Param == null)
                    return string.Empty;

                return Param.ToString();
            }
        }
    }

    public class MemoryBusinessObjectInfo : IBusinessObjectInfo
    {
        public string Name { get; set; }

        public object Param { get; set; }

        public System.Xml.Schema.XmlSchemaSet Cache
        {
            get
            {
                if (Param == null || Param.GetType() != typeof(System.Xml.Schema.XmlSchemaSet))
                    return null;

                return Param as System.Xml.Schema.XmlSchemaSet;
            }
        }
    }

    public interface IRemoteBusinessObjectInfo : IBusinessObjectInfo 
    {
        IBaseSynchroProvider ProviderImplementation { get; }
        bool SpecificFileRequest { get; set; }
    }

    public class RemoteBusinessObjectInfo : IRemoteBusinessObjectInfo
    {
        public RemoteBusinessObjectInfo() 
        {
            SpecificFileRequest = false;
        }

        public string Name { get; set; }

        public object Param { get; set; }

        public IBaseSynchroProvider ProviderImplementation
        {
            get
            {
                return Param as IBaseSynchroProvider;
            }
        }

       public bool SpecificFileRequest 
        { 
            get; 
            set; 
        }
    }

    public class RemoteBusinessObjectInfoWithCache : ILocalBusinessObject, IRemoteBusinessObjectInfo
    {

        public RemoteBusinessObjectInfoWithCache() 
        {
            SpecificFileRequest = false;
        }

        public bool SpecificFileRequest
        {
            get;
            set;
        }

        public string Name { get; set; }

        public object Param { get; set; }

        public IBaseSynchroProvider ProviderImplementation
        {
            get
            {
                if (ContextDictionary == null || ContextDictionary.Keys.Count == 0)
                    return null;

                object localObj = null;

                if (ContextDictionary.ContainsKey("REMOTE"))
                    localObj = ContextDictionary["REMOTE"];

                if (localObj == null)
                    return null;

                return localObj as IBaseSynchroProvider; 
            }
        }

        public Dictionary<string, object> ContextDictionary
        {
            get
            {
                return Param as Dictionary<string, object>;
            }
        }

        public string LocalPath
        {
            get 
            {
                if (ContextDictionary == null || ContextDictionary.Keys.Count == 0)
                    return string.Empty;

                object localObj = null;

                if(ContextDictionary.ContainsKey("LOCAL"))
                    localObj = ContextDictionary["LOCAL"];

                if (localObj == null)
                    return string.Empty;

                return localObj.ToString(); 
            }
        }

        public object RemoteConnectionObject
        {
            get 
            {
                if (ContextDictionary == null || ContextDictionary.Keys.Count == 0)
                    return null;

                if(ContextDictionary.ContainsKey("REMOTE"))
                    return ContextDictionary["REMOTE"];

                return null;
            }
        }
     
    }

    public interface IBusinessObjectXSDInfo
    {
        string XsdContenct { get; set; }
        bool Found { get; set; }
    }

    public class BusinessObjectXSDInfo : IBusinessObjectXSDInfo
    {
        public string XsdContenct { get; set; }
        public bool Found { get; set; }
    }

    public interface IXSDResolver
    {
        IBusinessObjectXSDInfo Resolve(IBusinessObjectInfo bOInfo);
    }
}
