using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using System.Collections;
using System.Threading;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities
{
    // enumerativo Tipo azione sincronizzazione
    //---------------------------------------------------------------------
    public enum SynchroActionType
    {
        Insert = 31588352, // default
        Update = 31588353,
        Delete = 31588354,
        Massive = 31588355,
        Exclude = 31588356,
        NewAttachment = 31588357, // nuovi enum per DMS
        DeleteAttachment = 31588358,
        NewCollection = 31588359,
        UpdateCollection = 31588360,
        UpdateProvider = 31588361,
        MassiveIncremental = 31588362
    }

    // enumerativo Stato sincronizzazione
    //---------------------------------------------------------------------
    public enum SynchroStatusType
    {
        ToSynchro = 31457280, // default
        Wait = 31457281,
        Synchro = 31457282,
        NoSynchro = 31457283, // in Pat serve ad identificare i record da escludere da tabella DS_SynchInfo / in Infinity serve per escludere le righe del recovery
        Error = 31457284,
        Excluded = 31457285
    }

    // enumerativo Direzione sincronizzazione
    //---------------------------------------------------------------------
    public enum SynchroDirectionType
    {
        Outbound = 31522816, // default
        Inbound = 31522817
    }

    //================================================================================
    public static class DSUtils
    {
        ///<summary>
        /// Dato un valore estratto dalla colonna ActionType della tabella DS_ActionsLog ritorna l'enum alla C#
        ///</summary>
        //--------------------------------------------------------------------------------
        public static SynchroActionType GetSyncroActionType(int actionType)
        {
            SynchroActionType sat = SynchroActionType.Insert;
            Enum.TryParse(actionType.ToString(), out sat);
            return sat;
        }

        ///<summary>
        /// Dato un valore estratto dalla colonna SynchStatus della tabella DS_ActionsLog ritorna l'enum alla C#
        ///</summary>
        //--------------------------------------------------------------------------------
        public static SynchroStatusType GetSynchroStatusType(int synchStatus)
        {
            SynchroStatusType sst = SynchroStatusType.ToSynchro;
            Enum.TryParse(synchStatus.ToString(), out sst);
            return sst;
        }

        ///<summary>
        /// Dato un valore estratto dalla colonna SynchDirection della tabella DS_ActionsLog ritorna l'enum alla C#
        ///</summary>
        //--------------------------------------------------------------------------------
        public static SynchroDirectionType GetSynchroDirectionType(int synchDirection)
        {
            SynchroDirectionType sdt = SynchroDirectionType.Outbound;
            Enum.TryParse(synchDirection.ToString(), out sdt);
            return sdt;
        }
    }

    //================================================================================
    public static class ConnectionStrings
    {
        public static LoginManager loginManager = new LoginManager();

        private static Dictionary<string, string> companyConnections = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static Dictionary<string, string> dmsConnections = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        //---------------------------------------------------------------------
        public static string GetConnectionString(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                return string.Empty;

            string connectionString = string.Empty;
            companyConnections.TryGetValue(authenticationToken, out connectionString);

            if (String.IsNullOrWhiteSpace(connectionString) && loginManager.GetLoginInformation(authenticationToken))//todo gestire errori
            {
                connectionString = loginManager.NonProviderCompanyConnectionString;

                if (!String.IsNullOrWhiteSpace(connectionString))
                    companyConnections.Add(authenticationToken, connectionString);
            }

            return connectionString;
        }

        //---------------------------------------------------------------------
        public static string GetDMSConnectionString(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                return string.Empty;

            string connectionString = string.Empty;
            dmsConnections.TryGetValue(authenticationToken, out connectionString);

            if (String.IsNullOrWhiteSpace(connectionString) && loginManager.GetLoginInformation(authenticationToken))
            {
                connectionString = loginManager.GetDMSConnectionString(authenticationToken);

                if (!String.IsNullOrWhiteSpace(connectionString))
                    dmsConnections.Add(authenticationToken, connectionString);
            }

            return connectionString;
        }
    }


    public interface ITBGuidCache<T>
    {
        void AddTbGuid(string tbGuid, T value);
        bool TryGetValue(string tbGuid, out T value);

        bool Exist(string tbGuid);
    }

    public class TBGuidFakeCache<T> : ITBGuidCache<T>
    {
        private static object _locker = "";
        private static TBGuidFakeCache<T> _instance = null;

        public void AddTbGuid(string tbGuid, T value)
        {
        }
        public bool TryGetValue(string tbGuid, out T value)
        {
            value = default(T);
            return false;
        }
        public bool Exist(string tbGuid)
        {
            return false;
        }
        protected TBGuidFakeCache()
        {
        }
        public static TBGuidFakeCache<T> GetInstance()
        {
            lock (_locker)
            {
                if (_instance == null)
                {
                    _instance = new TBGuidFakeCache<T>();
                }
                return _instance;
            }
        }
    }

    public class TBGuidSimpleCache<T> : ITBGuidCache<T>
    {
        private static object _locker = "";
        private static TBGuidSimpleCache<T> _instance = null;
        protected TBGuidSimpleCache(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            LeftoverCapacity = MaxCapacity;
            Map = new Hashtable(Convert.ToInt32(Math.Ceiling((decimal)(maxCapacity / 10))));
        }

        public static TBGuidSimpleCache<T> GetInstance(int maxCapacity)
        {
            lock (_locker)
            {
                if (_instance == null)
                    _instance = new TBGuidSimpleCache<T>(maxCapacity);
                return _instance;
            }
        }

        private Hashtable _map = null;
        protected Hashtable Map {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
            }
        }

        protected int MaxCapacity { get; set; }
        protected int LeftoverCapacity { get; set; }

        protected void Free()
        {
            Map.Clear();
            LeftoverCapacity = MaxCapacity;
        }

        public void AddTbGuid(string tbGuid, T value)
        {
            lock(_locker)
            {
                if (LeftoverCapacity == 0)
                    Free();

                Map.Add(tbGuid, value);
                LeftoverCapacity--;
            }
        }

        public bool Exist(string tbGuid)
        {
            lock(_locker)
                return Map.ContainsKey(tbGuid);
        }

        public bool TryGetValue(string tbGuid, out T value)
        {
            lock(_locker)
            {
                if (Exist(tbGuid))
                {
                    value = (T)Map[tbGuid];
                    return true;
                }
                value = default(T);
                return false;
            }
        }
    }

    ///<summary>
    /// Classe per la gestione delle eccezioni interne del DataSynchronizer
    ///</summary>
    //================================================================================
    public class DSException : Exception
    {
        public string ExtendedInfo = string.Empty;

        //---------------------------------------------------------------------
        public DSException(string source, string message)
            : base(message)
        {
            Source = source;

        }

        //---------------------------------------------------------------------
        public DSException(string source, string message, string extendedInfo)
            : base(message)
        {
            Source = source;
            ExtendedInfo = extendedInfo;
        }
    }

    ///<summary>
    /// Classe che implementa funzionalità di Enumerazione in un contesto thread safe
    ///</summary>
    //================================================================================
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> m_Inner;
        // this is the object we shall lock on. 
        private readonly object m_Lock;

        public SafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            m_Inner = inner;
            m_Lock = @lock;
            // entering lock in constructor
            Monitor.Enter(m_Lock);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            Monitor.Exit(m_Lock);
    }

        #endregion

        #region Implementation of IEnumerator

        // we just delegate actual implementation
        // to the inner enumerator, that actually iterates
        // over some collection

        public bool MoveNext()
        {
            return m_Inner.MoveNext();
        }

        public void Reset()
        {
            m_Inner.Reset();
        }

        public T Current
        {
            get { return m_Inner.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }

    //================================================================================
    public interface IFKToFixInfo : IEquatable<IFKToFixInfo>
    {
        string ProviderName { get; }
        string DocNamespace { get; }
        string QualifiedField { get; }
        string Value { get; }
        string Id { get; }
    }

    //--------------------------------------------------------------------------------
    public class FKToFixInfo : IFKToFixInfo
    {
        public FKToFixInfo(string providerName, string docNamespace, string qualifiedField, string value)
        {
            ProviderName    = providerName;
            DocNamespace    = docNamespace;
            QualifiedField  = qualifiedField;
            Value           = value;
        }
        public string ProviderName { get; set; }
        public string DocNamespace { get; }
        public string QualifiedField { get; }
        public string Value { get; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IFKToFixInfo);
        }

        public string Id
        {
            get { return $"{ProviderName}{DocNamespace}{QualifiedField}{Value}"; }
        }

        public virtual bool Equals(IFKToFixInfo other)
        {
            if (other == null)
                return false;

            return Id.Equals(other.Id, StringComparison.InvariantCultureIgnoreCase);

        }
    }

    //--------------------------------------------------------------------------------
    public interface IFKToFixErrors : IEnumerable<IFKToFixInfo>
    {
        void Add(IFKToFixInfo fkToFixInfo);

        int Get(IFKToFixInfo fkToFixInfo);
        void Update(IFKToFixInfo fkToFixInfo);
        void Free();
    }

    //--------------------------------------------------------------------------------
    public class FKToFixErrors : IFKToFixErrors
    {
        private static object _locker = "";

        private static FKToFixErrors _instance = null;

        public static FKToFixErrors GetInstance()
        {
            lock (_locker)
            {
                if (_instance == null)
                {
                    _instance = new FKToFixErrors();
                }
                return _instance;
            }
        }

        private Dictionary<IFKToFixInfo, int> _mapFKToFix = null;
        protected Dictionary<IFKToFixInfo, int> MapFKToFix
        {
            get
            {
                return _mapFKToFix;
            }
            set
            {
                _mapFKToFix = value;
            }
        }

        protected FKToFixErrors()
        {
            lock (_locker)
            {
                MapFKToFix = new Dictionary<IFKToFixInfo, int>();
            }
        }

        public void Free()
        {
            lock (_locker)
            {
                MapFKToFix.Clear();
            }
        }

        public void Add(IFKToFixInfo fkToFixInfo)
        {
            lock (_locker)
            {
                MapFKToFix[fkToFixInfo] = 1;
            }
        }

        public int Get(IFKToFixInfo fkToFixInfo)
        {
            return MapFKToFix[fkToFixInfo];
        }

        public void Update(IFKToFixInfo fkToFixInfo)
        {
            lock (_locker)
            {
                if (!MapFKToFix.ContainsKey(fkToFixInfo))
                {
                    Add(fkToFixInfo);
                    return;
                }

                int nErrorsCorrected = MapFKToFix[fkToFixInfo];
                MapFKToFix[fkToFixInfo] = nErrorsCorrected + 1;
            }
        }

        public IEnumerator<IFKToFixInfo> GetEnumerator()
        {
            return new SafeEnumerator<IFKToFixInfo>(MapFKToFix.Keys.GetEnumerator(), _locker);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

//================================================================================
public interface IValidationFiltersInfo
{
    string Query     { get; }
    string Namespace { get; }
    string MasterTable { get; }
    string SetType   { get; }
}

//--------------------------------------------------------------------------------
public class ValidationFiltersInfo : IValidationFiltersInfo
{
    public ValidationFiltersInfo(string query, string docNamespace, string masterTable, string setType)
    {
        Query       = query;
        Namespace   = docNamespace;
        MasterTable = masterTable;
        SetType     = setType;
    }
    public string Query     { get; set; }
    public string Namespace { get; }
    public string MasterTable { get; }
    public string SetType   { get; }
}

//================================================================================
public interface IValidationFiltersInfoByNamespace
{
    string MasterTable { get; }
    string SetType { get; }
}

//--------------------------------------------------------------------------------
public class ValidationFiltersInfoByNamespace : IValidationFiltersInfoByNamespace
{
    public ValidationFiltersInfoByNamespace(string masterTable, string setType)
    {
        MasterTable = masterTable;
        SetType = setType;
    }
    public string MasterTable { get; }
    public string SetType { get; }
}
public class ProviderConfiguration
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Url { get; private set; }
    public string User { get; private set; }
    public string Password { get; private set; }
    public bool SkipCrtValidation { get; private set; }
    public string Parameters { get; private set; }
    public bool IsEAProvider { get; private set; }
    public string IAFModule { get; private set; }

    public ProviderConfiguration(string name, string description, string url, string user, string password, string parameters, bool isEAProvider, string iafModule, bool skipCrtValidation)
    {
        Name = name;
        Description = description;
        Url = url;
        User = user;
        Password = password;
        SkipCrtValidation = skipCrtValidation;
        Parameters = parameters;
        IsEAProvider = isEAProvider;
        IAFModule = iafModule;
    }
}

