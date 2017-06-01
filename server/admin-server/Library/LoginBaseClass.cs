
namespace Microarea.AdminServer.Library
{
    //Classe per la verifica delle credenziali ed il ritorno dei token
    //contatterà il provider dei dati, se non presenti o scaduti dovrà chiederli al GWAM ottenendo una risposta da lui e salvandosi sul provider locale le informazioni.
    //nota: per ora il provider è un db detto microareaprovisioningdb
    public class LoginBaseClass
    {
        string loginId;
        string password;
       

        public LoginBaseClass(string loginid, string password) { }
        public LoginReturnCodes VerifyCredential()
        {
            return LoginReturnCodes.NoError;
        }

        public AuthenticationTokens CreateTokens()
        {
            return new AuthenticationTokens();
        }



    }
}
