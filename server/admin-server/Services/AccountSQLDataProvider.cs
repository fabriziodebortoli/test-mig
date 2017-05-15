﻿using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Microarea.AdminServer.Services
{
    public class AccountSQLDataProvider : IDataProvider
    {
        string connectionString;

        public AccountSQLDataProvider(string connString)
        {
            this.connectionString = connString;
            
        }

        public DateTime MinDateTimeValue
        {
            get { return (DateTime)SqlDateTime.MinValue; }
        }
        public bool Save(IAdminModel iModel)
        {
            Account account = (Account)iModel;

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
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

        public IAdminModel Load(IAdminModel iModel)
        {
            Account account;

            try
            {
                account = (Account)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.SelectAccountByUserName, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", account.UserName);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read()){
                                account.Name = dataReader["Name"] as string;
                                account.Description = dataReader["Description"] as string;
                                account.Email = dataReader["Email"] as string;
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return account;
        }

        public bool Update(IAdminModel iModel)
        {
            Account updateAccount = new Account();

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.UpdateAccount, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", updateAccount.UserName);
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

        public bool Delete(string userName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.DeleteAccount, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName);
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
