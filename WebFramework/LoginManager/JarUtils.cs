using System.Diagnostics.CodeAnalysis;

namespace Microarea.InfoBusinessLicenseDomain//se cambio ns devo rifare dll, quindi me ne faccio una ragione
{
    //=========================================================================
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    public class AesCryptingAlgorithm
    {
        #region ICryptingAlgorithm Members

        //---------------------------------------------------------------------
        public static string Crypt(string toBeCrypted, string password)
        {
            return Crypto.CriptoLibrary.AEScryptString(toBeCrypted, password);
        }

        //---------------------------------------------------------------------
        public static string DeCrypt(string todeCrypted, string password)
        {
            return Crypto.CriptoLibrary.AESdecryptString(todeCrypted, password);
        }

        #endregion
    }
}

