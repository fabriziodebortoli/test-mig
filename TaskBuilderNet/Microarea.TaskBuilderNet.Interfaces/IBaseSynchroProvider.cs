using System;
using System.Xml.Serialization;

namespace Microarea.TaskBuilderNet.Interfaces
{
    ///<summary>
    /// Gestione DataSynchronizer
    /// Lasciata vuota perche' serve solo per dare la visibilita' del BaseSynchroProvider
    /// in TaskBuilderNet.Core ed evitare lo spostamento delle classi base
    /// Fatta una classe perche' l'interface non accetta il [Serializable]
    /// Il [Serializable] serve perche' questa classe e' referenziata in un oggetto
    /// utilizzato da LoginManager
    ///</summary>
    //================================================================================
    [Serializable]
    public abstract class IBaseSynchroProvider
    {
        /// qui vanno i metodi base di webservice :
        /// Notify, SynchronizeOutbound, SynchronizeInbound
        public bool IsProviderValid = false;

        //Properties
        //public string CompanyConnectionString { get; set; }
        public string CompanyName { get; set; }

        public IBaseSynchroProvider()
        {
        }

        [XmlIgnore]
        public IProviderLogWriter LogWriter { get; set; }

        private bool _isValidationEnabled = false;

        public bool IsValidationEnabled
        {
            get { return _isValidationEnabled; }
            protected set { _isValidationEnabled = value; }
        }

        private bool _isInPause = false;
        private bool _Abort = false;
        public bool IsInPause { get { return _isInPause; } set { _isInPause = value; } }
        public bool Abort { get { return _Abort; } set { _Abort = value; } }

        //To implement
        public string ProviderName { get { return this.GetType().Name.Replace("SynchroProvider", ""); } }

        public abstract bool TestProviderParameters(string url, string username, string password, bool skipCrtValidation, string parameters, out string message);

        public abstract bool SetProviderParameters(string authenticationToken, string url, string username, string password, bool skipCrtValidation, string parameters, string iafmodule, out string message);

        public abstract bool DoNotify(int logID, string onlyForDMS, string iMagoConfigurations);

        public abstract void SynchronizeInbound(string authenticationToken, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString);

        public abstract void SynchronizeOutbound(string startSynchroDate = "", bool bDelta = false);

        public abstract void SynchronizeErrorsRecovery();

        public abstract bool CreateExternalServer(string extservername, string connstr, out string message);

        public abstract bool CheckCompaniesToBeMapped(out string extservername, out string message);

        public abstract bool MapCompany(string appreg, int magocompany, string infinitycompany, string taxid, out string message);

        public abstract bool UploadActionPackage(string actionpath, out string message);

        public abstract void ValidateOutbound(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId);

        public abstract bool ValidateDocument(string nameSpace, string guidDoc, string SerializesErrors, int workerId, out string message, bool includeXsd = true);

        public abstract bool SetConvergenceCriteria(string xmlCriteria, out string message);

        public abstract bool GetConvergenceCriteria(string actionName, out string xmlCriteria, out string message);

        public abstract bool SetGadgetPerm(out string message);

        public abstract bool CheckVersion(string magoversion, out string message);
    }
}