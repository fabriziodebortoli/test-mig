
using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Library
{
    //Classe per la verifica delle credenziali ed il ritorno dei token
    //contatterà il provider dei dati, se non presenti o scaduti dovrà chiederli al GWAM ottenendo una risposta da lui e salvandosi sul provider locale le informazioni.
    //nota: per ora il provider è un db detto microareaprovisioningdb
    public class LoginBaseClass
    {
        IAccount account;

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
                if (!account.Save()) return LoginReturnCodes.ErrorSavingAccount;
                return LoginReturnCodes.InvalidUserError;
            }

            ClearWrongPwdLoginCount();
            if (!account.Save()) return LoginReturnCodes.ErrorSavingAccount;

            if (account.MustChangePassword)
                return LoginReturnCodes.UserMustChangePasswordError;

            if (account.PasswordExpirationDate < DateTime.Now)
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
            if (!account.Save()) return LoginReturnCodes.ErrorSavingAccount;

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
        public void CreateTokens()
        {
            account.Tokens = new UserTokens(account.ProvisioningAdmin);//non sono sicura todo
        }
    }
}
