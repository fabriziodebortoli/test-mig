
namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders
{
    /// <summary>
    /// public  class CRMInfinityRecoveryManager
    /// </summary>
    //================================================================================
    internal class CRMInfinityRecoveryManager : RecoveryManager
    {
        //---------------------------------------------------------------------
        public CRMInfinityRecoveryManager(CRMInfinitySynchroProvider provider, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString)
        {
            this.provider = provider;
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.loginWindowsAuthentication = loginWindowsAuthentication;
            this.companyName = companyName;
            this.companyConnectionString = companyConnectionString;
        }

        /// <summary>
        /// Metodo specifico per Infinity non ancora implementato
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="provider"></param>
        //---------------------------------------------------------------------
        protected override void InboundRecovery()
        {
        }
    }
}