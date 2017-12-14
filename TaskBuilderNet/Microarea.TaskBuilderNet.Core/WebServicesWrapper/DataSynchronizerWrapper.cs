using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.dsSync;
using Microarea.TaskBuilderNet.Core.Generic;
using System.ServiceModel;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
    /// <summary>;
    /// Wrapper per l'utilizzo di DataSynchronizer
    /// </summary>
    //============================================================================
    public class DataSynchronizer
    {
        private MicroareaDataSynchronizerSoapClient dataSynchronizer;

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="eaSyncUrl">Indirizzo di dataSynchronizer</param>
        //---------------------------------------------------------------------------
        public DataSynchronizer(string Url)
        {
            if (!String.IsNullOrWhiteSpace(Url))
                dataSynchronizer = new MicroareaDataSynchronizerSoapClient(new BasicHttpBinding(BasicHttpSecurityMode.None), new EndpointAddress(Url));
        }

        /// <summary>
        /// Inizializza DataSyncronizer, passando la lista delle aziende che hanno
        /// il flag UseDataSynchro a true
        /// </summary>
        //-----------------------------------------------------------------------
        public bool Init(string username, string password, bool windowsAuthentication, string company)
        {
            return dataSynchronizer.Init(username, password, windowsAuthentication, company);
        }

        /// <summary>
        /// Verifica se il web service risponde
        /// </summary>
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            return dataSynchronizer.IsAlive();
        }

        //-----------------------------------------------------------------------
        public bool Reboot(string rebootToken)
        {
            return dataSynchronizer.Reboot(rebootToken);
        }
    }


    //============================================================================
    /// <summary>
    /// Shared lock for CRMInfinitySynchroProvider e DMSInfinitySynchroProvider
    /// </summary>
    [Serializable]
    public sealed class SharedLock
    {
        private static SharedLock _instance;
        private static object _locker = "";
        public volatile object Obj = new object();
        private int _synchroRequestCount;
        private int _validationRequestCount;

        SharedLock()
        {
            _synchroRequestCount = 0;
            _validationRequestCount = 0;
        }
        public static SharedLock Instance
        {
            get
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new SharedLock();
                    }
                    return _instance;
                }
            }
        }

        private bool _isLocked = false;
        public bool IsLocked
        {
            get
            {
                lock (_locker)
                { return _isLocked; }
            }
            set
            {
                lock (_locker)
                { _isLocked = value; }
            }
        }

        public void PushSynchroRequest()
        {
            lock (_locker)
                _synchroRequestCount++;
        }
        public void PopSynchroRequest()
        {
            lock (_locker)
                _synchroRequestCount--;
        }
        public int GetSynchroRequestCount()
        {
            lock (_locker)
                return _synchroRequestCount;
        }

        public void PushValidationRequest()
        {
            lock (_locker)
                _validationRequestCount++;
        }
        public void PopValidationRequest()
        {
            lock (_locker)
                _validationRequestCount--;
        }
        public int GetValidationRequestCount()
        {
            lock (_locker)
                return _validationRequestCount;
        }

        private bool _isMassiveSynchronizing = false;
        public bool IsMassiveSynchronizing
        {
            get
            {
                lock (_locker)
                { return _isMassiveSynchronizing; }
            }
            set
            {
                lock (_locker)
                { _isMassiveSynchronizing = value; }
            }
        }


        private bool _isMassiveValidating = false;
        public bool IsMassiveValidating
        {
            get
            {
                lock (_locker)
                { return _isMassiveValidating; }
            }
            set
            {
                lock (_locker)
                { _isMassiveValidating = value; }
            }
        }

        private string _providerName = string.Empty;
        public string ProviderName
        {
            get
            {
                lock (_locker)
                { return _providerName; }
            }
            set
            {
                lock (_locker)
                { _providerName = value; }
            }
        }

        public bool IsMassiveOperation
        {
            get { return IsMassiveSynchronizing || IsMassiveValidating; }
        }

    }
}
