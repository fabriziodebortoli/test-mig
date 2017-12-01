using System;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Collections.Generic;
using System.Data;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Controllers.Helpers;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Account : IAccount, IModelObject
    {       
        internal static int maxPasswordError = 5;
        // model attributes
        string accountName;
        string fullName = string.Empty;
		string password = string.Empty;
		byte[] salt = new byte[] { };
        int loginFailedCount = 0;
        string notes = string.Empty;
		string email = string.Empty;
		bool passwordNeverExpires = false;
		bool mustChangePassword = false;
		bool cannotChangePassword = false;
        DateTime passwordExpirationDate;
		int passwordDuration;
		bool disabled = false;
		bool locked = false;
		string regionalSettings = string.Empty;
        string language = string.Empty;
        bool isWindowsAuthentication = false;
        DateTime expirationDate = DateTime.Now.AddDays(3);// todo per ora scadenza 3 giorni per esempio
		string parentAccount = string.Empty;
		bool confirmed = false;
        int ticks = TicksHelper.GetTicks();
        string vATNr = string.Empty;

        //---------------------------------------------------------------------
        public string AccountName { get { return this.accountName; } set { this.accountName = value; } }
		public string FullName { get { return this.fullName; } set { this.fullName = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
		public byte[] Salt{ get { return this.salt; } set { this.salt = value; } }
		public int LoginFailedCount { get { return this.loginFailedCount; } set { this.loginFailedCount = value; } }
        public string Notes { get { return this.notes; } set { this.notes = value; } }
		public string Email { get { return this.email; } set { this.email = value; } }
		public bool PasswordNeverExpires { get { return this.passwordNeverExpires; } set { this.passwordNeverExpires = value; } }
		public bool MustChangePassword { get { return this.mustChangePassword; } set { this.mustChangePassword = value; } }
		public bool CannotChangePassword { get { return this.cannotChangePassword; } set { this.cannotChangePassword = value; } }
		public DateTime PasswordExpirationDate { get { return this.passwordExpirationDate; } set { this.passwordExpirationDate = value; } }
        public int PasswordDuration { get { return this.passwordDuration; } set { this.passwordDuration = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool Locked { get { return this.locked; } set { this.locked = value; } }
		public string Language { get { return this.language; } set { this.language = value; } }
		public string RegionalSettings { get { return this.regionalSettings; } set { this.regionalSettings = value; } }
        public bool IsWindowsAuthentication { get { return this.isWindowsAuthentication; } set { this.isWindowsAuthentication = value; } }
        public DateTime ExpirationDate { get { return this.expirationDate; } set { this.expirationDate = value; } }
        public int Ticks { get { return this.ticks; } set { this.ticks = value; } }
		public string ParentAccount { get { return this.parentAccount; } set { this.parentAccount = value; } }
		public bool Confirmed { get { return this.confirmed; } set { this.confirmed = value; } }
        public string VATNr { get => vATNr; set => vATNr = value; }

        //---------------------------------------------------------------------
        public Account()
        { }

        //---------------------------------------------------------------------
        internal static Account GetAccountByName(BurgerData burgerData, string accountName)
        {
            try
            {
                return burgerData.GetObject<Account, IAccount>(String.Empty, ModelTables.Accounts, SqlLogicOperators.AND, new WhereCondition[]
                      {
                    new WhereCondition("AccountName", accountName, QueryComparingOperators.IsEqual, false)
                      }) as Account;
            }
            catch {  return null; } // todo log

        }

        //--------------------------------------------------------------------------------
        public IModelObject Fetch(IDataReader dataReader)
        {
            Account account = new Account();
            account.AccountName = dataReader["AccountName"] as string;
            account.FullName = dataReader["FullName"] as string;
            account.Notes = dataReader["Notes"] as string;
            VATNr = dataReader["VATNr"] as string;
            account.Email = dataReader["Email"] as string;
            account.Password = dataReader["Password"] as string;
			account.Salt = dataReader["Salt"] as byte[];
			account.LoginFailedCount = (int)dataReader["LoginFailedCount"];
            account.PasswordNeverExpires = (bool)dataReader["PasswordNeverExpires"];
            account.MustChangePassword = (bool)dataReader["MustChangePassword"];
            account.CannotChangePassword = (bool)dataReader["CannotChangePassword"];
            account.PasswordExpirationDate = (DateTime)dataReader["PasswordExpirationDate"];
            account.PasswordDuration = (int)dataReader["PasswordDuration"];
            account.IsWindowsAuthentication = (bool)dataReader["WindowsAuthentication"];
            account.Disabled = (bool)dataReader["Disabled"];
            account.Locked = (bool)dataReader["Locked"];
            account.Language = dataReader["Language"] as string;
            account.RegionalSettings = dataReader["RegionalSettings"] as string;
            account.Ticks = (int)dataReader["Ticks"];
            account.ExpirationDate = (DateTime)dataReader["ExpirationDate"];
            account.ParentAccount = dataReader["ParentAccount"] as string;
            account.Confirmed = (bool)dataReader["Confirmed"];
            return account;
        }

        //---------------------------------------------------------------------
        public OperationResult Save(BurgerData burgerData)
        {
            OperationResult opRes = new OperationResult();

            List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
            burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.AccountName));
            burgerDataParameters.Add(new BurgerDataParameter("@FullName", this.FullName));
            burgerDataParameters.Add(new BurgerDataParameter("@Password", this.Password));

			if (this.Salt == null)
			{
				this.Salt = new byte[] { };
			}

			burgerDataParameters.Add(new BurgerDataParameter("@Salt", this.Salt));
			burgerDataParameters.Add(new BurgerDataParameter("@Notes", this.Notes));
            burgerDataParameters.Add(new BurgerDataParameter("@VATNr", this.VATNr));
            burgerDataParameters.Add(new BurgerDataParameter("@Email", this.Email));
            burgerDataParameters.Add(new BurgerDataParameter("@LoginFailedCount", this.LoginFailedCount));
            burgerDataParameters.Add(new BurgerDataParameter("@PasswordNeverExpires", this.PasswordNeverExpires));
            burgerDataParameters.Add(new BurgerDataParameter("@MustChangePassword", this.MustChangePassword));
            burgerDataParameters.Add(new BurgerDataParameter("@CannotChangePassword", this.CannotChangePassword));
            burgerDataParameters.Add(new BurgerDataParameter("@PasswordExpirationDate", this.PasswordExpirationDate));
            burgerDataParameters.Add(new BurgerDataParameter("@PasswordDuration", this.PasswordDuration));
            burgerDataParameters.Add(new BurgerDataParameter("@Disabled", this.Disabled));
            burgerDataParameters.Add(new BurgerDataParameter("@Locked", this.Locked));
            burgerDataParameters.Add(new BurgerDataParameter("@WindowsAuthentication", this.IsWindowsAuthentication));
            burgerDataParameters.Add(new BurgerDataParameter("@Language", this.Language));
            burgerDataParameters.Add(new BurgerDataParameter("@RegionalSettings", this.RegionalSettings));
            burgerDataParameters.Add(new BurgerDataParameter("@Ticks",this.Ticks));
            burgerDataParameters.Add(new BurgerDataParameter("@ExpirationDate", this.ExpirationDate));
            burgerDataParameters.Add(new BurgerDataParameter("@ParentAccount", this.ParentAccount));
            burgerDataParameters.Add(new BurgerDataParameter("@Confirmed", this.Confirmed));

            BurgerDataParameter keyColumnParameter = new BurgerDataParameter("@AccountName", this.accountName);

            opRes.Result = burgerData.Save(ModelTables.Accounts, keyColumnParameter, burgerDataParameters);
            opRes.Content = this;
            return opRes;
        }

        //--------------------------------------------------------------------------------
        public string GetKey()
        {
            return String.Concat(" ( Account = '", this.accountName, "' ) ");
        }

        //---------------------------------------------------------------------
        public Account(string accountName)
        {
            this.accountName = accountName;
        }

        //---------------------------------------------------------------------
        public bool CheckPassword(string password)
        {
            // calculating password hash
            Byte[] salt = Salt;
            //siccome il salt a null viene in save sostituito con  array vuoto devo controllare sia null sia array vuoto
            string passwordToCheck = (salt != null && salt.Length > 0) ? SecurityManager.HashThis(password, salt) : password;

            if (Password == passwordToCheck) return true;

            AddWrongPwdLoginCount();
            return false;

        }

        //---------------------------------------------------------------------
        public void ResetPassword(string pwd)
		{
            Password = pwd;
            ResetPasswordExpirationDate();
            MustChangePassword = false;
            ClearWrongPwdLoginCount();
        }

        //---------------------------------------------------------------------
        public bool IsPasswordExpirated()
		{
            // La data è inferiore ad adesso, ma comunque non è il min value che è il default
            return passwordExpirationDate < DateTime.Now &&
                passwordExpirationDate > BurgerData.MinDateTimeValue;
        }

        //---------------------------------------------------------------------
        public void ResetPasswordExpirationDate()
        {
            passwordExpirationDate = DateTime.Now.AddDays(passwordDuration);
        }

        //----------------------------------------------------------------------
        public void AddWrongPwdLoginCount()
        {
            Locked = (++LoginFailedCount >= maxPasswordError);
        }

        //----------------------------------------------------------------------
        public void ClearWrongPwdLoginCount()
        {
            Locked = false;
            LoginFailedCount = 0;

        }

        //----------------------------------------------------------------------
        public LoginReturnCodes VerifyCredential(string password, BurgerData burgerdata)
        {
            if (ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (Locked)
                return LoginReturnCodes.LoginLocked;

            if (Disabled)
                return LoginReturnCodes.UserNotAllowed;

            if (CheckPassword(password))
            {
                if (!Save(burgerdata).Result)
                    return LoginReturnCodes.ErrorSavingAccount;
            }
            else
                return LoginReturnCodes.InvalidUserError;

            ClearWrongPwdLoginCount();
            if (!Save(burgerdata).Result)
                return LoginReturnCodes.ErrorSavingAccount;

            if (MustChangePassword)
                return LoginReturnCodes.UserMustChangePasswordError;

            if (IsPasswordExpirated())
            {
                if (CannotChangePassword)
                    return LoginReturnCodes.CannotChangePasswordError;
                return LoginReturnCodes.UserMustChangePasswordError;
            }

            return LoginReturnCodes.NoError;
        }

        //----------------------------------------------------------------------
        internal LoginReturnCodes ChangePassword(ChangePasswordInfo passwordInfo, BurgerData burgerdata)
        {
            if (ExpirationDate < DateTime.Now)
                return LoginReturnCodes.UserExpired;

            if (Locked)
                return LoginReturnCodes.LoginLocked;

            if (CannotChangePassword)
                return LoginReturnCodes.CannotChangePasswordError;

            if (Disabled)
                return LoginReturnCodes.UserNotAllowed;

            if (Password != passwordInfo.Password)
            {
                AddWrongPwdLoginCount();
                if (!Save(burgerdata).Result)
                    return LoginReturnCodes.ErrorSavingAccount;

                return LoginReturnCodes.InvalidUserError;
            }
            ResetPassword(passwordInfo.NewPassword);

            if (!Save(burgerdata).Result)
                return LoginReturnCodes.ErrorSavingAccount;

            return LoginReturnCodes.NoError;
        }

		//----------------------------------------------------------------------
		public OperationResult Delete(BurgerData burgerData)
		{
			OperationResult opRes = new OperationResult();
			List<BurgerDataParameter> burgerDataParameters = new List<BurgerDataParameter>();
			burgerDataParameters.Add(new BurgerDataParameter("@AccountName", this.AccountName));
			opRes.Result = burgerData.Delete(ModelTables.Accounts, burgerDataParameters);
			return opRes;
		}
	}
}