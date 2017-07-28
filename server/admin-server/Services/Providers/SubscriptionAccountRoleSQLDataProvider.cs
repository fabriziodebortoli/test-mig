﻿using System;
using Microarea.AdminServer.Model.Interfaces;
using System.Data.SqlTypes;
using System.Data.SqlClient;

namespace Microarea.AdminServer.Services.Providers
{
    //================================================================================
    public class SubscriptionAccountRoleSQLDataProvider : IDataProvider
    {
        string connectionString;

        //---------------------------------------------------------------------
        public SubscriptionAccountRoleSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

        //---------------------------------------------------------------------
        public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

        //---------------------------------------------------------------------
        public bool Delete(IAdminModel iModel)
        {
            IAccountRoles role;

            try
            {
                role = (IAccountRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Queries.DeleteAccountRoles, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", role.RoleId);
                        command.Parameters.AddWithValue("@EntityKey", role.EntityKey);
                        command.Parameters.AddWithValue("@AccountName", role.AccountName);
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
        public IAdminModel Load(IAdminModel iModel)
        {
            IAccountRoles sar;

            try
            {
                sar = (IAccountRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Queries.SelectAccountRoles, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", sar.RoleId);
                        command.Parameters.AddWithValue("@EntityKey", sar.EntityKey);
                        command.Parameters.AddWithValue("@AccountName", sar.AccountName);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                sar.ExistsOnDB = true;
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

            return sar;
        }

        //---------------------------------------------------------------------
        public OperationResult Query(QueryInfo qi)
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        public OperationResult Save(IAdminModel iModel)
        {
            IAccountRoles sar;
            OperationResult opRes = new OperationResult();

            try
            {
                sar = (IAccountRoles)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    bool existSar= false;

                    using (SqlCommand command = new SqlCommand(Queries.ExistAccountRoles, connection))
                    {
                        command.Parameters.AddWithValue("@RoleId", sar.RoleId);
                        command.Parameters.AddWithValue("@EntityKey", sar.EntityKey);
                        command.Parameters.AddWithValue("@AccountName", sar.AccountName);
                        existSar = (int)command.ExecuteScalar() > 0;
                    }
                    if (existSar)
                    {
                        opRes.Result = false;
                        opRes.Message = "SubscriptionAccountRole already exists.";
                        return opRes;
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = Queries.InsertAccountRoles;
                        
                        command.Parameters.AddWithValue("@RoleId", sar.RoleId);
                        command.Parameters.AddWithValue("@EntityKey", sar.EntityKey);
                        command.Parameters.AddWithValue("@AccountName", sar.AccountName);
                        command.ExecuteNonQuery();
                    }

                    opRes.Result = true;
                    opRes.Content = sar;
                }
            }
            catch (Exception e)
            {
                opRes.Result = false;
                opRes.Message = String.Concat("An error occurred while saving SubscriptionAccountRole: ", e.Message);
                return opRes;
            }

            return opRes;
        }
    }
}
