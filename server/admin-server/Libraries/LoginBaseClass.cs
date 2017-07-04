using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Library
{
    //=========================================================================
    public static class LoginBaseClass
    {
       // private IAccount account;
        private static int maxPasswordError = 5;

        //----------------------------------------------------------------------
        public static LoginReturnCodes VerifyCredential(IAccount account, string password)
        {
            if (account.ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (account.Locked) return LoginReturnCodes.LoginLocked;

            if (account.Password != Crypt(password))
            {
                AddWrongPwdLoginCount(account);
                if (!account.Save().Result) return LoginReturnCodes.ErrorSavingAccount;
                return LoginReturnCodes.InvalidUserError;
            }

            ClearWrongPwdLoginCount(account);
           
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
        public static LoginReturnCodes ChangePassword(IAccount account,string oldpassword, string newpassword)
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
                AddWrongPwdLoginCount(account);
                return LoginReturnCodes.InvalidUserError;
            }

            account.Password = Crypt(newpassword);
            if (!account.Save().Result) return LoginReturnCodes.ErrorSavingAccount;
        
            ClearWrongPwdLoginCount(account);
       
            return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        private static void AddWrongPwdLoginCount(IAccount account)
        {
            account.Locked = (++account.LoginFailedCount >= maxPasswordError);  
        }

        //----------------------------------------------------------------------
        private static void ClearWrongPwdLoginCount(IAccount account)
        {
            account.Locked = false;
            account.LoginFailedCount = 0;
        }

        //----------------------------------------------------------------------
        [Obsolete ("TODO-Warning: Crypt Password")]
        private static string Crypt(string password)
        {
            return password;//TODO CRYPT
        }
    }
}