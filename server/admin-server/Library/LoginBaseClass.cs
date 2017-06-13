
using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Controllers.Helpers;

namespace Microarea.AdminServer.Library
{
    //Classe per la verifica delle credenziali ed il ritorno dei token
    //contatterà il provider dei dati, se non presenti o scaduti dovrà chiederli al GWAM ottenendo una risposta da lui e salvandosi sul provider locale le informazioni.
    //nota: per ora il provider è un db detto microareaprovisioningdb
    public class LoginBaseClass
    {
        IAccount account;
        UserTokens tokens;
        public UserTokens Tokens {get  {return tokens;} }
        public LoginBaseClass(IAccount account) { this.account = account; }
        public LoginReturnCodes VerifyCredential(string password)
        {
            //verifica login lockata
            //Verifica password errata

            //se corretta:
            //verifica password da cambiare
            //verifica password scaduta

            if (account.Locked) return LoginReturnCodes.LoginLocked;

            if (account.Password != Crypt(password))
            {
                AddWrongPwdLoginCount();
                if (!account.Save()) return LoginReturnCodes.ErrorSavingTokens;
                return LoginReturnCodes.InvalidUserError;
            }

            ClearWrongPwdLoginCount();
            if (!CreateTokens())
                 return LoginReturnCodes.ErrorSavingTokens;
           

            if (account.MustChangePassword)
                return LoginReturnCodes.UserMustChangePasswordError;

            if (account.IsPasswordExpirated())
            {
                if (account.CannotChangePassword)
                    return LoginReturnCodes.CannotChangePasswordError;
                return account.PasswordExpirationDateCannotChange ? LoginReturnCodes.PasswordExpiredError : LoginReturnCodes.UserMustChangePasswordError;
            } 

            return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        public LoginReturnCodes ChangePasssword(string oldpassword, string newpassword)
        {
            if (account.Locked)
                return LoginReturnCodes.LoginLocked;
            if (account.CannotChangePassword)
                return LoginReturnCodes.CannotChangePasswordError;
            if (account.PasswordExpirationDateCannotChange && account.PasswordExpirationDate < DateTime.Now)
                return LoginReturnCodes.PasswordExpiredError;
            if (account.Password != Crypt(oldpassword))
            {
                AddWrongPwdLoginCount();
                return LoginReturnCodes.InvalidUserError;
            }
            account.Password = Crypt(newpassword);
            if (!account.Save()) return LoginReturnCodes.ErrorSavingTokens;

            ClearWrongPwdLoginCount();
            CreateTokens();
            return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        private void AddWrongPwdLoginCount()
        {
            int x = 5;//todo parametri server connection config?

            if (++account.LoginFailedCount >= x)
                account.Locked = true;
        }

        //----------------------------------------------------------------------
        private void ClearWrongPwdLoginCount()
        {
            account.Locked = false;
            account.LoginFailedCount = 0;
        }

        //----------------------------------------------------------------------
        private string Crypt(string password)
        {
            return password;//TODO
        }

        //----------------------------------------------------------------------
        public bool CreateTokens()
        {
            tokens = new UserTokens(account.ProvisioningAdmin, account.AccountId);
            return tokens.Save();
        }
    }
}
