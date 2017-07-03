using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services.Providers
{
    //================================================================================
    public class AccountSQLDataProvider : IDataProvider, IAccountDataProvider
	{
        string connectionString;

		//---------------------------------------------------------------------
		public AccountSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue  { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			Account account;

			try
			{
				account = (Account)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
                   
                    using (SqlCommand command = new SqlCommand(Consts.SelectAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
                                account.FullName = dataReader["FullName"] as string;
								account.Notes = dataReader["Notes"] as string;
								account.Email = dataReader["Email"] as string;
								account.Password = dataReader["Password"] as string;
                                account.LoginFailedCount = (int)dataReader["LoginFailedCount"];
								account.PasswordNeverExpires = (bool)dataReader["PasswordNeverExpires"];
								account.MustChangePassword = (bool)dataReader["MustChangePassword"];
                                account.CannotChangePassword = (bool)dataReader["CannotChangePassword"];
                                account.PasswordExpirationDate = (DateTime)dataReader["PasswordExpirationDate"];
								account.PasswordDuration = (int)dataReader["PasswordDuration"];
								account.ProvisioningAdmin = (bool)dataReader["ProvisioningAdmin"];
                                account.IsWindowsAuthentication = (bool)dataReader["WindowsAuthentication"];
                                account.Disabled = (bool)dataReader["Disabled"];
								account.Locked = (bool)dataReader["Locked"];
								account.PreferredLanguage = dataReader["PreferredLanguage"] as string;
                                account.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								account.ExpirationDate = (DateTime)dataReader["ExpirationDate"];
								account.ExistsOnDB = true;
                            }
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}

			return account;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(IAdminModel iModel)
        {
			Account account;
			OperationResult opRes = new OperationResult();

            try
            {
				account = (Account)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					bool existAccount = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						existAccount = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existAccount ? Consts.UpdateAccount : Consts.InsertAccount;

						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						command.Parameters.AddWithValue("@FullName", account.FullName);
						command.Parameters.AddWithValue("@Password", account.Password);
                        command.Parameters.AddWithValue("@CloudAdmin", account.CloudAdmin);
                        command.Parameters.AddWithValue("@Notes", account.Notes);
						command.Parameters.AddWithValue("@Email", account.Email);
						command.Parameters.AddWithValue("@LoginFailedCount", account.LoginFailedCount);
						command.Parameters.AddWithValue("@PasswordNeverExpires", account.PasswordNeverExpires);
						command.Parameters.AddWithValue("@MustChangePassword", account.MustChangePassword);
						command.Parameters.AddWithValue("@CannotChangePassword", account.CannotChangePassword);
						command.Parameters.AddWithValue("@PasswordExpirationDate", account.PasswordExpirationDate);
						command.Parameters.AddWithValue("@PasswordDuration", account.PasswordDuration);
						command.Parameters.AddWithValue("@Disabled", account.Disabled);
						command.Parameters.AddWithValue("@Locked", account.Locked);
						command.Parameters.AddWithValue("@ProvisioningAdmin", account.ProvisioningAdmin);
						command.Parameters.AddWithValue("@WindowsAuthentication", account.IsWindowsAuthentication);
						command.Parameters.AddWithValue("@PreferredLanguage", account.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", account.ApplicationLanguage);
                        command.Parameters.AddWithValue("@Ticks", account.Ticks);
                        command.Parameters.AddWithValue("@ExpirationDate", account.ExpirationDate);

						command.ExecuteNonQuery();
					}

					opRes.Result = true;
					opRes.Content = account;
				}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving account: ", e.Message);
				return opRes;
			}

            return opRes;
        }

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
			Account account;

			try
			{
				account = (Account)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
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
		public List<Account> GetAccounts(string accountName)
		{
			List<Account> accountList = new List<Account>();

			string selectQuery = "SELECT * FROM MP_Accounts";
			if (!string.IsNullOrWhiteSpace(accountName))
				selectQuery += " WHERE AccountName = @AccountName";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						if (!string.IsNullOrWhiteSpace(accountName))
							command.Parameters.AddWithValue("@AccountName", accountName);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Account account = new Account();
								account.AccountName = dataReader["AccountName"] as string;
                                account.CloudAdmin = (bool)dataReader["CloudAdmin"];
                                account.FullName = dataReader["FullName"] as string;
								account.Notes = dataReader["Notes"] as string;
								account.Email = dataReader["Email"] as string;
								account.Password = dataReader["Password"] as string;
								account.LoginFailedCount = (int)dataReader["LoginFailedCount"];
								account.PasswordNeverExpires = (bool)dataReader["PasswordNeverExpires"];
								account.MustChangePassword = (bool)dataReader["MustChangePassword"];
								account.CannotChangePassword = (bool)dataReader["CannotChangePassword"];
								account.PasswordExpirationDate = (DateTime)dataReader["PasswordExpirationDate"];
								account.PasswordDuration = (int)dataReader["PasswordDuration"];
								account.ProvisioningAdmin = (bool)dataReader["ProvisioningAdmin"];
								account.IsWindowsAuthentication = (bool)dataReader["WindowsAuthentication"];
								account.Disabled = (bool)dataReader["Disabled"];
								account.Locked = (bool)dataReader["Locked"];
								account.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								account.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
                                account.Ticks = (long)dataReader["Ticks"];
                                account.ExpirationDate = (DateTime)dataReader["ExpirationDate"];
								accountList.Add(account);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}

			return accountList;
		}

	}
}
