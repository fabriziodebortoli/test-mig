using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.AdminServer.Model;

namespace Microarea.AdminServer.Libraries
{
    //=========================================================================
    public static class LoginBaseClass
    {
       // private IAccount account;
        private static int maxPasswordError = 5;

        //----------------------------------------------------------------------
        public static LoginReturnCodes VerifyCredential(Account account, string password, BurgerData burgerdata)
        {
            if (account.ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (account.Locked)
                return LoginReturnCodes.LoginLocked;

            if (account.Disabled)
                return LoginReturnCodes.UserNotAllowed;

			// calculating password hash
			Byte[] salt = account.Salt;

			string passwordToCheck = salt != null ? SecurityManager.HashThis(password, salt) : password;

            if (account.Password != passwordToCheck)
            {
                AddWrongPwdLoginCount(account);

                if (!account.Save(burgerdata).Result)
                    return LoginReturnCodes.ErrorSavingAccount;

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
        internal static LoginReturnCodes ChangePassword(Account account, ChangePasswordInfo  passwordInfo, BurgerData burgerdata)
        {
            if (account.ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (account.Locked)
                return LoginReturnCodes.LoginLocked;

            if (account.CannotChangePassword)
                return LoginReturnCodes.CannotChangePasswordError;

            if (account.Disabled)
                return LoginReturnCodes.UserNotAllowed;

            if (account.Password != Crypt(passwordInfo.Password))
            {
                AddWrongPwdLoginCount(account);
                return LoginReturnCodes.InvalidUserError;
            }

            account.Password = Crypt(passwordInfo.NewPassword);
            account.ResetPasswordExpirationDate();
            account.Ticks = DateTime.Now.Ticks;
            account.MustChangePassword = false;
            ClearWrongPwdLoginCount(account);

            if (!account.Save(burgerdata).Result)
                return LoginReturnCodes.ErrorSavingAccount;

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