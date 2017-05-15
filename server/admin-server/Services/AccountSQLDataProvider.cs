using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Data.SqlClient;

namespace Microarea.AdminServer.Services
{
    public class AccountSQLDataProvider : IDataProvider
    {
        public bool Save(IAdminModel iModel, string connString)
        {
            Account account = new Account(); // refactor

            try
            {
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.InsertAccount, connection))
                    {
                        command.Parameters.AddWithValue("@Name", account.Name);
                        command.Parameters.AddWithValue("@Password", account.Password);
                        command.Parameters.AddWithValue("@Description", account.Description);
                        command.Parameters.AddWithValue("@Email", account.Email);
                        command.Parameters.AddWithValue("@PasswordNeverExpires", account.PasswordNeverExpires);
                        command.Parameters.AddWithValue("@MustChangePassword", account.MustChangePassword);
                        command.Parameters.AddWithValue("@CannotChangePassword", account.CannotChangePassword);
                        command.Parameters.AddWithValue("@ExpiryDateCannotChange", account.ExpiryDateCannotChange);
                        command.Parameters.AddWithValue("@ExpiryDatePassword", account.ExpiryDatePassword);
                        command.Parameters.AddWithValue("@Disabled", account.Disabled);
                        command.Parameters.AddWithValue("@Locked", account.Locked);
                        command.Parameters.AddWithValue("@ProvisioningAdmin", account.ProvisioningAdmin);
                        command.Parameters.AddWithValue("@WindowsAuthentication", account.IsWindowsAuthentication);
                        command.Parameters.AddWithValue("@PreferredLanguage", account.PreferredLanguage);
                        command.Parameters.AddWithValue("@ApplicationLanguage", account.ApplicationLanguage);
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

        public bool Update(IAdminModel iModel, string connectionString)
        {
            Account updateAccount = new Account();

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
