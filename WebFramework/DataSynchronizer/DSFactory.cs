using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;
using System.Configuration;

namespace Microarea.WebServices.DataSynchronizer
{
    public class DSFactory
    {
        #region costruttore
        private static object locker = new object();
        private static DSFactory instance;
        
        private List<IBaseSynchroProvider> _providerListInbound = new List<IBaseSynchroProvider>();
        private CRMInfinitySynchroProvider _CRMInstance = null;
        private DMSInfinitySynchroProvider _DMSInstance = null;
        
        public static int instanceNumber = 1;
        private DSFactory()
        {
            instanceNumber++;
        }

        public static DSFactory Instance
        {
            get
            {
                lock (locker)
                {
                    if (instance == null)
                        instance = new DSFactory();
                    return instance;
                }
            }
        }
        #endregion
        
        public IBaseSynchroProvider GetProvider(string authenticationToken, ProviderConfiguration providerConfiguration, ConnectionStringManager connectionStringManager)
        {
            lock (locker)
            {
                if (providerConfiguration == null)
                    return null;

                switch (providerConfiguration.Name)
                {
                    case "CRMInfinity":
                    {
                            if (_CRMInstance == null)
                                _CRMInstance = new CRMInfinitySynchroProvider(authenticationToken, connectionStringManager, providerConfiguration);
                            _CRMInstance.NotifyDataSynch = ConfigurationManager.AppSettings["notifyDataSynch"];
                            return _CRMInstance;
                    }
                    case "DMSInfinity":
                    {
                        if (_DMSInstance == null)
                                _DMSInstance = new DMSInfinitySynchroProvider(authenticationToken, connectionStringManager, providerConfiguration);
                            _DMSInstance.NotifyDataSynch = ConfigurationManager.AppSettings["notifyDataSynch"];
                            return _DMSInstance;
                    }
                    default:
                        return null;
                }
            }
        }

        private IBaseSynchroProvider GetProviderInbound(string company)
        {
            foreach (IBaseSynchroProvider provider in _providerListInbound)
            {
                if (provider.CompanyName == company)
                    return provider;
            }
            return null;
        }

        private void AddProviderInBound(IBaseSynchroProvider provider)
        {
            if (_providerListInbound == null)
                _providerListInbound = new List<IBaseSynchroProvider>();

            _providerListInbound.Add(provider);
        }

        public IBaseSynchroProvider GetProviderInbound(string authenticationToken, ProviderConfiguration providerConfiguration, ConnectionStringManager connectionStringManager)
        {
            lock (locker)
            {
                switch (providerConfiguration.Name)
                {
                    case "CRMInfinity":
                    {
                        CRMInfinitySynchroProvider provider = (CRMInfinitySynchroProvider) GetProviderInbound(connectionStringManager.CompanyConnectionString.GetCompany());
                        if (provider == null)
                        {
                            provider = new CRMInfinitySynchroProvider(authenticationToken, connectionStringManager, providerConfiguration);
                            AddProviderInBound(provider);
                        }
                        return provider;
                    }
                    // TO DO: il DMS non lo stiamo considerando perche non gestisce la sincronizzazione inbound
                    //case "DMSInfinity":
                    //{
                    //    DMSInfinitySynchroProvider provider = (DMSInfinitySynchroProvider) GetProviderForThisCompany(connectionStringManager.CompanyConnectionString.GetCompany());
                    //    if (provider == null)
                    //    {
                    //        provider = new DMSInfinitySynchroProvider(connectionStringManager, providerConfiguration);
                    //        _providerListInbound.Add(provider);
                    //    }
                    //    return provider;
                    //}
                    default:
                        return null;
                }
            }
        }
    }
}