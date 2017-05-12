using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Account : IAccount
    {
        int accountId;
        string name = string.Empty;
		string password = string.Empty;
		string description = string.Empty;
		string email = string.Empty;
		bool provisioningAdmin;
		bool passwordNeverExpires = false;
		bool mustChangePassword = false;
		bool cannotChangePassword = false;
		bool expiryDateCannotChange = false;
		DateTime expiryDatePassword = (DateTime)SqlDateTime.MinValue;
		bool disabled = false;
		bool locked = false;
		string applicationLanguage = string.Empty;
        string preferredLanguage = string.Empty;
        bool isWindowsAuthentication = false;
	
		//---------------------------------------------------------------------
		public int AccountId { get { return this.accountId; } set { this.accountId = value; } }
		public string Name { get { return this.name; } set { this.name = value; } }
		public string Password { get { return this.password; } set { this.password = value; } }
		public string Description { get { return this.description; } set { this.description = value; } }
		public string Email { get { return this.email; } set { this.email = value; } }
		public bool ProvisioningAdmin { get { return this.provisioningAdmin; } set { this.provisioningAdmin = value; } }
		public bool PasswordNeverExpires { get { return this.passwordNeverExpires; } set { this.passwordNeverExpires = value; } }
		public bool MustChangePassword { get { return this.mustChangePassword; } set { this.mustChangePassword = value; } }
		public bool CannotChangePassword { get { return this.cannotChangePassword; } set { this.cannotChangePassword = value; } }
		public bool ExpiryDateCannotChange { get { return this.expiryDateCannotChange; } set { this.expiryDateCannotChange = value; } }
		public DateTime ExpiryDatePassword { get { return this.expiryDatePassword; } set { this.expiryDatePassword = value; } }
		public bool Disabled { get { return this.disabled; } set { this.disabled = value; } }
		public bool Locked { get { return this.locked; } set { this.locked = value; } }
		public string PreferredLanguage { get { return this.preferredLanguage; } set { this.preferredLanguage = value; } }
		public string ApplicationLanguage { get { return this.applicationLanguage; } set { this.applicationLanguage = value; } }
        public bool IsWindowsAuthentication { get { return this.isWindowsAuthentication; } set { this.isWindowsAuthentication = value; } }

        //---------------------------------------------------------------------
        public bool Save(string connectionString)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.InsertAccount, connection))
					{
						command.Parameters.AddWithValue("@Name",					Name);
						command.Parameters.AddWithValue("@Password",				Password);
						command.Parameters.AddWithValue("@Description",				Description);
						command.Parameters.AddWithValue("@Email",					Email);
						command.Parameters.AddWithValue("@PasswordNeverExpires",	PasswordNeverExpires); 
						command.Parameters.AddWithValue("@MustChangePassword",		MustChangePassword);
						command.Parameters.AddWithValue("@CannotChangePassword",	CannotChangePassword);
						command.Parameters.AddWithValue("@ExpiryDateCannotChange",	ExpiryDateCannotChange);
						command.Parameters.AddWithValue("@ExpiryDatePassword",		ExpiryDatePassword);
						command.Parameters.AddWithValue("@Disabled",				Disabled);
						command.Parameters.AddWithValue("@Locked",					Locked);
						command.Parameters.AddWithValue("@ProvisioningAdmin",		ProvisioningAdmin);
						command.Parameters.AddWithValue("@WindowsAuthentication",	IsWindowsAuthentication);
						command.Parameters.AddWithValue("@PreferredLanguage",		PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage",		ApplicationLanguage);
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
			
			return true;
		}

		//---------------------------------------------------------------------
		public bool Update(Account updateAccount, string connectionString)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.UpdateAccount, connection))
					{
						command.Parameters.AddWithValue("@Name", updateAccount.Name);
						command.Parameters.AddWithValue("@Description", updateAccount.Description);
						command.Parameters.AddWithValue("@Email", updateAccount.Email);
						command.Parameters.AddWithValue("@PasswordNeverExpires", updateAccount.PasswordNeverExpires);
						command.Parameters.AddWithValue("@MustChangePassword", updateAccount.MustChangePassword);
						command.Parameters.AddWithValue("@CannotChangePassword", updateAccount.CannotChangePassword);
						command.Parameters.AddWithValue("@ExpiryDateCannotChange", updateAccount.ExpiryDateCannotChange);
						command.Parameters.AddWithValue("@ExpiryDatePassword", updateAccount.ExpiryDatePassword);
						command.Parameters.AddWithValue("@Disabled", updateAccount.Disabled);
						command.Parameters.AddWithValue("@Locked", updateAccount.Locked);
						command.Parameters.AddWithValue("@ProvisioningAdmin", updateAccount.ProvisioningAdmin);
						command.Parameters.AddWithValue("@WindowsAuthentication", updateAccount.IsWindowsAuthentication);
						command.Parameters.AddWithValue("@PreferredLanguage", updateAccount.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", updateAccount.ApplicationLanguage);
						command.Parameters.AddWithValue("@AccountId", updateAccount.AccountId);
						command.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool Delete(int accountId, string connectionString)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountId", accountId);
						command.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			return true;
		}
	}
}
