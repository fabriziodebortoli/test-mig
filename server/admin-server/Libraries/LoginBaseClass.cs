using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Library
{
    //=========================================================================
    public class LoginBaseClass
    {
        private IAccount account;
        private int maxPasswordError = 5;
        public LoginBaseClass(IAccount account) { this.account = account; }

        //----------------------------------------------------------------------
        public LoginReturnCodes VerifyCredential(string password)
        {
            if (account.ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (account.Locked) return LoginReturnCodes.LoginLocked;

            if (account.Password != Crypt(password))
            {
                AddWrongPwdLoginCount();
                if (!account.Save().Result) return LoginReturnCodes.ErrorSavingAccount;
                return LoginReturnCodes.InvalidUserError;
            }

            ClearWrongPwdLoginCount();
           
            if (account.MustChangePassword)
                return LoginReturnCodes.UserMustChangePasswordError;

            if (account.IsPasswordExpirated())
            {
                if (account.CannotChangePassword)
                    return LoginReturnCodes.CannotChangePasswordError;
                return LoginReturnCodes.UserMustChangePasswordError;
			}

			return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        public LoginReturnCodes ChangePassword(string oldpassword, string newpassword)
        {
            if (account.ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (account.Locked)
                return LoginReturnCodes.LoginLocked;

            if (account.CannotChangePassword)
                return LoginReturnCodes.CannotChangePasswordError;
			
			if (account.PasswordExpirationDate < DateTime.Now)
			    return LoginReturnCodes.PasswordExpiredError;

			if (account.Password != Crypt(oldpassword))
            {
                AddWrongPwdLoginCount();
                return LoginReturnCodes.InvalidUserError;
            }

            account.Password = Crypt(newpassword);
            if (!account.Save().Result) return LoginReturnCodes.ErrorSavingAccount;
        
            ClearWrongPwdLoginCount();
       
            return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        private void AddWrongPwdLoginCount()
        {
            account.Locked = (++account.LoginFailedCount >= maxPasswordError);  
        }

        //----------------------------------------------------------------------
        private void ClearWrongPwdLoginCount()
        {
            account.Locked = false;
            account.LoginFailedCount = 0;
        }

        //----------------------------------------------------------------------
        [Obsolete ("TODO-Warning: Crypt Password")]
        private string Crypt(string password)
        {
            return password;//TODO CRYPT
        }
    }
}