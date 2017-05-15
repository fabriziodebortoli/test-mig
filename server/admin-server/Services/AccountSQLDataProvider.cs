using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
